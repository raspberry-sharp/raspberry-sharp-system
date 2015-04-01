#region References

using System;

#endregion

namespace Raspberry.Timers
{
    /// <summary>
    /// Provides an interface for a timer.
    /// </summary>
    public interface ITimer
    {
        #region Properties

        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        /// <value>
        /// The interval.
        /// </value>
        TimeSpan Interval { get; set; }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        Action Action { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <param name="startDelay">The delay before the first occurence.</param>
        void Start(TimeSpan startDelay);

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();

        #endregion
    }
}