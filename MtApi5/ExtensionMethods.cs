using System;

namespace MtApi5
{
    internal static class ExtensionMethods
    {
        #region Event Methods
        public static void FireEvent(this EventHandler eventHandler, object sender)
        {
            eventHandler?.Invoke(sender, EventArgs.Empty);
        }

        public static void FireEvent<T>(this EventHandler<T> eventHandler, object sender, T e)
            where T : EventArgs
        {
            eventHandler?.Invoke(sender, e);
        }

        #endregion
    }
}
