using System;
using System.Runtime.CompilerServices;
using System.Threading;


namespace Koturn.VRChat.Log.Internals
{
    /// <summary>
    /// Provides some utility methods about event.
    /// </summary>
    internal static class EventHelper
    {
        /// <summary>
        /// Combine two delegates with thread-safety guarantees.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="EventHandler{T}"/></typeparam>
        /// <param name="targetEventHandler">Target <see cref="EventHandler{TEventArgs}"/>.</param>
        /// <param name="val">An <see cref="EventHandler{T}"/> to combine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(ref EventHandler<T>? targetEventHandler, EventHandler<T>? val)
        {
            var eventHandler = targetEventHandler;
            EventHandler<T>? eventHandler2;
            do
            {
                eventHandler2 = eventHandler;
                eventHandler = Interlocked.CompareExchange(
                    ref targetEventHandler,
                    (EventHandler<T>?)Delegate.Combine(eventHandler2, val),
                    eventHandler2);
            }
            while (eventHandler != eventHandler2);
        }

        /// <summary>
        /// Remove a delegate with thread-safety guarantees.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="EventHandler{T}"/></typeparam>
        /// <param name="targetEventHandler">Target <see cref="EventHandler{TEventArgs}"/>.</param>
        /// <param name="val">An <see cref="EventHandler{T}"/> to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove<T>(ref EventHandler<T>? targetEventHandler, EventHandler<T>? val)
        {
            var eventHandler = targetEventHandler;
            EventHandler<T>? eventHandler2;
            do
            {
                eventHandler2 = eventHandler;
                eventHandler = Interlocked.CompareExchange(
                    ref targetEventHandler,
                    (EventHandler<T>?)Delegate.Remove(eventHandler2, val),
                    eventHandler2);
            }
            while (eventHandler != eventHandler2);
        }
    }
}
