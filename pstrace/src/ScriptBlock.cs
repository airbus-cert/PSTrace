using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace PSTrace
{
    /// <summary>
    /// ScriptBlock hooking
    /// Cover all compile execution mode like Invoke-Expression (use a lot in framework like empire for obfuscation)
    /// Hook:
    ///     System.Management.Automation.ScriptBlock.InvokeWithPipe instance method
    /// </summary>
    internal class ScriptBlock
    {
        /// <summary>
        /// Handler method of on loading assembly event from app manager
        /// </summary>
        /// <param name="name">name of assembly</param>
        /// <param name="assembly">associate assembly</param>
        internal static void OnLoad(string name, Assembly assembly)
        {
            if (name != "System.Management.Automation")
            {
                return;
            }

            // retrieve method info of target function
            var target = assembly
                .GetType("System.Management.Automation.ScriptBlock")
                .GetMethod(
                    "InvokeWithPipe",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[] {
                        typeof(bool),
                        typeof(bool),
                        typeof(object),
                        typeof(object),
                        typeof(object),
                        assembly.GetType("System.Management.Automation.Internal.Pipe"),
                        typeof(ArrayList).MakeByRefType(),
                        typeof(object[])
                    },
                    null
            );

            var handler = typeof(ScriptBlock)
                .GetMethod(
                    "Handler",
                    BindingFlags.Static | BindingFlags.NonPublic
                );

            
            var trampoline = typeof(ScriptBlock)
                .GetMethod(
                    "Trampoline",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

            MinHook.Hook(target, handler, trampoline);
        }

        /// <summary>
        /// Handler for InvokeWithPipe method of ScriptBlock class
        /// </summary>
        /// <param name="This">this context</param>
        /// <param name="useLocalScope"></param>
        /// <param name="writeErrors">if it's an interesting script true</param>
        /// <param name="dollarUnder"></param>
        /// <param name="input"></param>
        /// <param name="scriptThis"></param>
        /// <param name="outputPipe"></param>
        /// <param name="resultList"></param>
        /// <param name="args"></param>
        private static void Handler(ScriptBlock This, bool useLocalScope, bool writeErrors, object dollarUnder, object input, object scriptThis, object outputPipe, ref ArrayList resultList, object[] args)
        {
            using (EventLog log = new EventLog())
            {
                log.Source = "Powershell";
                log.WriteEntry(This.ToString(), EventLogEntryType.SuccessAudit, 701);
            }
            This.Trampoline(useLocalScope, writeErrors, dollarUnder, input, scriptThis, outputPipe, ref resultList, args);
        }

        /// <summary>
        /// A trampoline erased to original trampoline of minhook
        /// </summary>
        /// <param name="useLocalScope"></param>
        /// <param name="writeErrors"></param>
        /// <param name="dollarUnder"></param>
        /// <param name="input"></param>
        /// <param name="scriptThis"></param>
        /// <param name="outputPipe"></param>
        /// <param name="resultList"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private object Trampoline(bool useLocalScope, bool writeErrors, object dollarUnder, object input, object scriptThis, object outputPipe, ref ArrayList resultList, object[] args)
        {
            throw new Exception("It is a bug. Fix it bro!");
        }
    }
}
