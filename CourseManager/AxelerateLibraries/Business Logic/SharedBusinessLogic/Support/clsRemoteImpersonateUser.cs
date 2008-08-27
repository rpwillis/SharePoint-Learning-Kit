using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Support
{
    /// <summary>
    /// This class allows to impersonate in a remote machine.
    /// </summary>
    public class clsRemoteImpersonateUser
    {
        private const uint RPC_C_AUTHN_LEVEL_NONE = 1;
        private const uint RPC_C_IMP_LEVEL_IMPERSONATE = 3;
        private const uint COINIT_MULTITHREADED = 0;
        private const uint COINIT_APARTMENTTHREADED = 2;
        /// <summary>
        /// Threading Type
        /// </summary>
        public enum APARTMENTTYPE
        {
            /// <summary>Multi-threaded</summary>
            Multi = 0,
            /// <summary>Single-threaded</summary>
            Single = 1
        }

        // Note: PreserveSig=false allows .NET interop to handle processing the returned HRESULT and throw an exception on failure
        [DllImport("Ole32.dll", ExactSpelling = true, EntryPoint = "CoInitializeEx", CallingConvention = CallingConvention.StdCall, SetLastError = false, PreserveSig = false)]
        static extern void CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

        [DllImport("Ole32.dll", ExactSpelling = true, EntryPoint = "CoUninitialize", CallingConvention = CallingConvention.StdCall, SetLastError = false, PreserveSig = false)]
        static extern void CoUninitialize();

        [DllImport("Ole32.dll", ExactSpelling = true, EntryPoint = "CoInitializeSecurity", CallingConvention = CallingConvention.StdCall, SetLastError = false, PreserveSig = false)]
        static extern void CoInitializeSecurity(IntPtr pVoid, uint cAuthSvc, IntPtr asAuthSvc, IntPtr pReserved1, uint dwAuthnLevel,
            uint dwImpLevel, IntPtr pAuthList, uint dwCapabilities, IntPtr pReserved3);

        /// <summary>
        /// Initialize COM Security settings on current thread
        /// </summary>
        public static void Initialize()
        {
            // By default set to Single Threaded Apartment (STA)
            Initialize(APARTMENTTYPE.Single);
        }


        
        /// <summary>
        /// Initialize COM Security settings on current thread with threading choice
        /// </summary>
        /// <param name="threadType">The type of the thread</param>
        public static void Initialize(APARTMENTTYPE threadType)
        {

            uint coinit = COINIT_APARTMENTTHREADED;
            // if Multi Threaded Apartment (MTA)
            if (threadType == APARTMENTTYPE.Multi)
                coinit = COINIT_MULTITHREADED;
            // Initializes the COM library for use by the calling thread
            CoInitializeEx(IntPtr.Zero, coinit);
            // use SecurityImpersonation
            CoInitializeSecurity(IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, RPC_C_AUTHN_LEVEL_NONE, RPC_C_IMP_LEVEL_IMPERSONATE, IntPtr.Zero, 0, IntPtr.Zero);
        }


        /// <summary>
        /// Uninitializes COM on the current thread
        /// </summary>
        public static void Uninitialize()
        {
            CoUninitialize();
        }
    }
}
