#region References

using System;
using System.Linq;
using System.Threading;

#endregion

namespace Raspberry.Timers
{
    /// <summary>
    /// Represents a high-resolution timer.
    /// </summary>
    public class HighResolutionTimer : ITimer
    {
        #region Fields

        private decimal delay;
        private decimal interval;
        private Action action;

        private Thread thread;

        private static readonly int nanoSleepOffset = Calibrate();

        #endregion

        #region Instance Management

        /// <summary>
        /// Initializes a new instance of the <see cref="HighResolutionTimer"/> class.
        /// </summary>
        public HighResolutionTimer()
        {
            if (!Board.Current.IsRaspberryPi)
                throw new NotSupportedException("Cannot use HighResolutionTimer on a platform different than Raspberry Pi");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the interval, in milliseconds.
        /// </summary>
        /// <value>
        /// The interval, in milliseconds.
        /// </value>
        public decimal Interval
        {
            get { return interval; }
            set
            {
                if (value > uint.MaxValue/1000)
                    throw new ArgumentOutOfRangeException("value", interval, "Interval must be lower than or equal to uint.MaxValue / 1000");

                interval = value;
            }
        }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public Action Action
        {
            get { return action; }
            set
            {
                if (value == null)
                    Stop();

                action = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sleeps the specified delay.
        /// </summary>
        /// <param name="delay">The delay.</param>
        public static void Sleep(decimal delay)
        {
            // Based on [BCM2835 C library](http://www.open.com.au/mikem/bcm2835/)

            // Calling nanosleep() takes at least 100-200 us, so use it for
            // long waits and use a busy wait on the hires timer for the rest.
            var start = DateTime.Now.Ticks;

            if (delay >= 100)
            {
                // Do not use high resolution timer for long interval (>= 100ms)
                Thread.Sleep((int) delay);
            }
            else if (delay > 0.450m)
            {
                var t1 = new Interop.timespec();
                var t2 = new Interop.timespec();

                // Use nanosleep if interval is higher than 450µs
                t1.tv_sec = (IntPtr)0;
                t1.tv_nsec = (IntPtr)((long) (delay * 1000000) - nanoSleepOffset);

                Interop.nanosleep(ref t1, ref t2);
            }
            else
            {
                while (true)
                {
                    if ((DateTime.Now.Ticks - start) * 0.0001m >= delay)
                        break;
                }
            }
        }


        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <param name="startDelay">The delay before the first occurence, in milliseconds.</param>
        public void Start(decimal startDelay)
        {
            if (startDelay > uint.MaxValue/1000)
                throw new ArgumentOutOfRangeException("startDelay", startDelay, "Delay must be lower than or equal to uint.MaxValue / 1000");

            lock (this)
            {
                if (thread == null)
                {
                    delay = startDelay;
                    thread = new Thread(ThreadProcess);
                    thread.Start();
                }
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                if (thread != null)
                {
                    if (thread != Thread.CurrentThread)
                        thread.Abort();
                    thread = null;
                }
            }
        }

        #endregion

        #region Private Helpers

        private static int Calibrate()
        {
            const int referenceCount = 1000;
            return Enumerable.Range(0, referenceCount)
                .Aggregate(
                    (long) 0,
                    (a, i) =>
                        {
                            var t1 = new Interop.timespec();
                            var t2 = new Interop.timespec();

                            t1.tv_sec = (IntPtr) 0;
                            t1.tv_nsec = (IntPtr) 1000000;

                            var start = DateTime.Now.Ticks;
                            Interop.nanosleep(ref t1, ref t2);

                            return a + ((DateTime.Now.Ticks - start) * 100 - 1000000);
                        },
                    a => (int)(a / referenceCount));
        }

        private void ThreadProcess()
        {
            var thisThread = thread;

            Sleep(delay);
            while (thread == thisThread)
            {
                (Action ?? NoOp)();
                Sleep(interval);
            }
        }

        private void NoOp(){}

        #endregion
    }
}