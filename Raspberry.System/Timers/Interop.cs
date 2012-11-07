using System.Runtime.InteropServices;

namespace Raspberry.Timers
{
    internal static class Interop
    {
        #region Methods

        [DllImport("libbcm2835.so", EntryPoint = "bcm2835_delayMicroseconds")]
        public static extern void bcm2835_delayMicroseconds(uint microseconds);

        #endregion
    }
}