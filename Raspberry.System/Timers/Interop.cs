using System.Runtime.InteropServices;

namespace Raspberry.Timers
{
    internal static class Interop
    {
        #region Methods

        public struct timespec
        {
            public long tv_sec;
            public long tv_nsec;
        };

        [DllImport("libc.so.6")]
        public static extern void nanosleep(ref timespec timespec);

        #endregion
    }
}