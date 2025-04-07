using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Koturn.VRChat.Log.Events;


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
        /// <typeparam name="T">Type of <see cref="VRCLogEventHandler{T}"/></typeparam>
        /// <param name="targetEventHandler">Target <see cref="VRCLogEventHandler{TEventArgs}"/>.</param>
        /// <param name="val">An <see cref="VRCLogEventHandler{T}"/> to combine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(ref VRCLogEventHandler<T>? targetEventHandler, VRCLogEventHandler<T>? val)
        {
            var eventHandler = targetEventHandler;
            VRCLogEventHandler<T>? eventHandler2;
            do
            {
                eventHandler2 = eventHandler;
                eventHandler = Interlocked.CompareExchange(
                    ref targetEventHandler,
                    (VRCLogEventHandler<T>?)Delegate.Combine(eventHandler2, val),
                    eventHandler2);
            }
            while (eventHandler != eventHandler2);
        }

        /// <summary>
        /// Remove a delegate with thread-safety guarantees.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="VRCLogEventHandler{T}"/></typeparam>
        /// <param name="targetEventHandler">Target <see cref="VRCLogEventHandler{TEventArgs}"/>.</param>
        /// <param name="val">An <see cref="VRCLogEventHandler{T}"/> to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove<T>(ref VRCLogEventHandler<T>? targetEventHandler, VRCLogEventHandler<T>? val)
        {
            var eventHandler = targetEventHandler;
            VRCLogEventHandler<T>? eventHandler2;
            do
            {
                eventHandler2 = eventHandler;
                eventHandler = Interlocked.CompareExchange(
                    ref targetEventHandler,
                    (VRCLogEventHandler<T>?)Delegate.Remove(eventHandler2, val),
                    eventHandler2);
            }
            while (eventHandler != eventHandler2);
        }
    }
}
