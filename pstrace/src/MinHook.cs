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
    /// MinHook function wrapper
    /// </summary>
    internal class MinHook
    {

        [DllImport("MinHook.dll")]
        private static extern int MH_Initialize();

        [DllImport("MinHook.dll")]
        private static extern int MH_CreateHook(IntPtr Target, IntPtr Detour, out IntPtr Original);

        [DllImport("MinHook.dll")]
        private static extern int MH_EnableHook(IntPtr Target);

        /// <summary>
        /// Init minhoo in static ctor of class
        /// </summary>
        static MinHook()
        {
            if (MH_Initialize() != 0)
                throw new Exception("MinHook init failed");
        }

        /// <summary>
        /// Hook !!! Hook every where ;-)
        /// </summary>
        /// <param name="target">target method (To Hook method)</param>
        /// <param name="handler">handler method (Hooked method)</param>
        /// <param name="trampoline">original method</param>
        internal static void Hook(MethodInfo target, MethodInfo handler, MethodInfo trampoline)
        {
            // call JIT compiler
            RuntimeHelpers.PrepareMethod(target.MethodHandle);
            RuntimeHelpers.PrepareMethod(handler.MethodHandle);
            RuntimeHelpers.PrepareMethod(trampoline.MethodHandle);

            IntPtr original = IntPtr.Zero;
            if (MH_CreateHook(target.MethodHandle.GetFunctionPointer(), handler.MethodHandle.GetFunctionPointer(), out original) != 0)
                throw new Exception("MinHook create hook failed for " + target.Name);

            if (MH_CreateHook(trampoline.MethodHandle.GetFunctionPointer(), original, out original) != 0)
                throw new Exception("MinHook create hook trampoline failed for " + target.Name);

            // enable all hooks
            MH_EnableHook(IntPtr.Zero);
        }
    }
}
