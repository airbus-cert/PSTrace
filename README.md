# PSTrace

Enable script-block logging for PowerShell v2+.

Log every script and command of any `powershell.exe` launched on target to the Windows Event Log (even PowerShell executables pushed by an attacker 😉)

## Why?

In older versions of Powershell, there is no way to trace all called scripts as we can see on modern Powershell implementation through AMSI (Anti Malware Scan Interface), or via ETW provider (`Microsoft-Windows-Powershell`). 

This is a huge advantage for attackers on platforms like Windows 7 or Windows Server 2008.

To monitor this kind of attack, we explored some solutions proposed by security researchers : 
* https://github.com/tandasat/DotNetHooking from Crowdstrike
* https://cansecwest.com/slides/2017/CSW2017_Amanda_Rousseau_.NETHijackingPowerShell.pdf from Endgame

These were our main sources of inspiration for writing PSTrace.

## Build

PSTrace massively uses Cmake to do the job, and it is mandatory to install it before the build step:
https://github.com/Kitware/CMake/releases/download/v3.13.4/cmake-3.13.4-win64-x64.msi

We need `wix` to build the installer part:
http://wixtoolset.org/releases/v3.11.1/stable

Now do the following magic commands:
```bash
git clone https://github.com/CERT/PSTrace --recursive
mkdir build_ptrace
cd build_ptrace
cmake -G "Visual Studio 15 2017 Win64" ..\ptrace
cmake --build . --target package --config release
```

Enjoy your `pstrace-1.0.0-win64.msi` file!

Adapt "Visual Studio 15 2017 Win64" to your target compiler and platform.

For prebuilt releases, see Release page:
https://github.com/airbus-cert/PSTrace/releases

## How

PStrace wants to log all scripts executed through Powershell. But Powershell exposes lots of ways to execute a script, and many interfaces to obfuscate it :
* Open `powershell.exe` and execute commands directly through console input
* Execute via `powershell.exe` command line parameter
* Execute an encoded command via the `-e` command line parameter
* Execute an obfuscated script via the `Invoke-Expression` (alias `iex`) cmdlet
* Execute a command via the `Invoke-Command` cmdlet

PSTrace must trace all these kinds of execution.

`Powershell.exe` is just an exe which launches CLR and loads the main Powershell assembly :
* System.Management.Automation

We chose to apply the solution presented by Crowdstrike and Endgame, and injected a .NET assembly to hook some methods from `powershell.exe`, more precisely `System.Management.Automation`.
But after trying both solutions, not all execution modes were covered. We had to determine a better way to hook. 

After a hard work of reversing 😉 (via [ILSpy](https://github.com/icsharpcode/ILSpy)), we determined two target methods :
* Instance method *InvokeWithPipe* from `System.Management.Automation.ScriptBlock` class
* Instance method *Invoke* from `System.Management.Automation.Runspaces.Pipeline`

The first method covers tracing of any invoked script which needs to be compiled beforehand, like `Invoke-Expression` or `Invoke-Command`, or encoded command line.
Second method covers tracing of input from console directly.

We will achieve this goal in two steps :
* Inject an assembly in target process
* Hook method before the app starts

### Inject assembly

First of all, we need to inject our assembly before the execution of Powershell. To do that, we will create a custom domain manager which is in charge of resolving assembly on loading. To force CLR (Common Language Runtime) to use our custom domain manager, there are two environments variables to set before executing powershell :

```
set APPDOMAIN_MANAGER_ASM=PSTrace, Version=0.0.0.0, Culture=neutral, PublicKeyToken=cba672b68346b966, processorArchitecture=MSIL
set APPDOMAIN_MANAGER_TYPE=PSTrace.PSTrace
```

Note: the assembly must be signed to be a valid candidate for domain manager.

### Hook method

Once we are loaded into the target application as an application domain manager, we can control the assembly loading step. When an assembly is loaded, an event is emitted. We just have to wait for the target assembly, and find the target method using reflection.
Then we just use x86/x64 inline hooking, because in the fabulous world of .NET we can also control the JIT compiler through the `RuntimeHelpers` class. `RuntimeHelpers.PrepareMethod` just compiles it, and `GetFunctionPointer` returns a valid virtual address, which can be directly manipulated in assembly.

Once methods are hooked, we implement a handler that will log into the Windows Event Log, using Powershell as a source, because it is already present.

### Install

To monitor all `powershell.exe` processes, we use the Global Assembly Cache (GAC) and system Environment Variable. This is done by the msi file generated by cmake.
