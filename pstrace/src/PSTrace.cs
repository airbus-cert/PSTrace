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
    /// Define you own domain manager to be injected in any .NET application
    /// </summary>
    public class PSTrace : AppDomainManager
    {
        /// <summary>
        /// Event handler for on assembly lioad event
        /// </summary>
        private static readonly AssemblyLoadEventHandler onAssemblyLoadEvent = new AssemblyLoadEventHandler(OnAssemblyLoad);

        /// <summary>
        /// Delegate signature of event handler
        /// </summary>
        /// <param name="name">name of assembly</param>
        /// <param name="assembly">associate assembly object</param>
        /// <returns></returns>
        internal delegate void OnAssemblyLoadEventHandler(string name, Assembly assembly);

        /// <summary>
        /// Static event listener
        /// </summary>
        internal static event OnAssemblyLoadEventHandler OnAssemblyLoadEvent;

        /// <summary>
        /// Static ctor call when assembly was loaded
        /// </summary>
        static PSTrace()
        {
            // only default appdomain loading
            if (!AppDomain.CurrentDomain.IsDefaultAppDomain())
            {
                return;
            }

            // Start listening assembly loading
            AppDomain.CurrentDomain.AssemblyLoad += onAssemblyLoadEvent;


            // Bind all hookers
            PSTrace.OnAssemblyLoadEvent += new OnAssemblyLoadEventHandler(ScriptBlock.OnLoad);
            PSTrace.OnAssemblyLoadEvent += new OnAssemblyLoadEventHandler(Pipeline.OnLoad);
        }

        /// <summary>
        /// Forwarded event
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="Args"></param>
        private static void OnAssemblyLoad(object Sender, AssemblyLoadEventArgs Args)
        {
            // forward event to subscriber
            PSTrace.OnAssemblyLoadEvent(Args.LoadedAssembly.GetName().Name, Args.LoadedAssembly);
        }
    }
}
