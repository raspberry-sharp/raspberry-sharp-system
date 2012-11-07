#region References

using System;
using System.Runtime.InteropServices;

#endregion

namespace Raspberry.Timers
{
    internal static class Interop
    {
        #region Constants

        public static int CLOCK_MONOTONIC_RAW = 4;

        #endregion

        #region Classes

        public struct timespec
        {
            public IntPtr tv_sec; /* seconds */
            public IntPtr tv_nsec; /* nanoseconds */
        }

        #endregion

        #region Methods

        [DllImport("libc.so.6")]
        public static extern int nanosleep(ref timespec req, ref timespec rem); 

        #endregion
    }
}