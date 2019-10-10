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
    /// Pipeline Hooker
    /// Encompass all command line logging argument
    /// Hook :
    ///     System.Management.Automation.Runspaces.Pipeline.Invoke method
    ///     Retrieve pipeline argument from collection Commands to log command
    /// Artifacts:
    ///     prompt command
    ///     Out-Default to write into console
    /// </summary>
    public class Pipeline : AppDomainManager
    {
        /// <summary>
        /// On load assembly event from app manager
        /// </summary>
        /// <param name="name">name of assembly</param>
        /// <param name="assembly">associate assembly</param>
        /// <returns></returns>
        internal static void OnLoad(string name, Assembly assembly)
        {
            if (name != "System.Management.Automation")
            {
                return;
            }

            // retrieve method info of target function
            var target = assembly
                .GetType("System.Management.Automation.Runspaces.Pipeline")
                .GetMethod(
                    "Invoke",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new Type[] { },
                    null
            );

            if(target == null)
            {
                throw new Exception("unable to locate InvokeWithPipe method in System.Management.Automation.Runspaces.Pipeline assembly");
            }

            var handler = typeof(Pipeline)
                .GetMethod(
                    "Handler",
                    BindingFlags.Static | BindingFlags.NonPublic
                );


            var trampoline = typeof(Pipeline)
                .GetMethod(
                    "Trampoline",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

            MinHook.Hook(target, handler, trampoline);
        }

        /// <summary>
        /// Handler of pipeline Invoke method
        /// </summary>
        /// <param name="This">fake oject just for typing of compiler</param>
        /// <returns>i don't known</returns>
        private static object Handler(Pipeline This)
        {
            PropertyInfo property = This.GetType().GetProperty("Commands", BindingFlags.Instance | BindingFlags.Public);
            var commands = (IList)property.GetValue(This, null);

            string inputCommand = null;
            foreach(object o in commands)
            {
                if(inputCommand == null)
                {
                    inputCommand = "";
                }
                else
                {
                    inputCommand += " | ";
                }

                inputCommand += o.ToString();
            }

            using (EventLog log = new EventLog())
            {
                log.Source = "Powershell";
                log.WriteEntry(inputCommand, EventLogEntryType.SuccessAudit, 702);
            }

            return This.Trampoline();
        }

        /// <summary>
        /// A trampoline erased to original trampoline of minhook
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private object Trampoline()
        {
            throw new Exception("It is a bug. Fix it bro!");
        }
    }
}
