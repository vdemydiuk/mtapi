using System;

namespace MtApi.Monitors.Triggers
{
    /// <summary>
    /// Interface for triggers which can be used to trigger a <see cref="MtMonitorBase"/>.
    /// </summary>
    public interface IMonitorTrigger
    {
        /// <summary>
        /// Event will be called if the trigger raised.
        /// </summary>
        event EventHandler Raised;
        /// <summary>
        /// Returns whether the trigger is started
        /// </summary>
        bool IsStarted { get; }
        /// <summary>
        /// Stops the trigger and prevents further calls of the <see cref="Raised"/> event.
        /// </summary>
        void Stop();
        /// <summary>
        /// Starts the trigger.
        /// </summary>
        void Start();
    }
}
