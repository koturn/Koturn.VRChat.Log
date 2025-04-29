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
        /// <typeparam name="TDelegate">Type of <see cref="Delegate"/></typeparam>
        /// <param name="targetEventHandler">Target <see cref="VRCLogEventHandler{TEventArgs}"/>.</param>
        /// <param name="val">An <typeparamref name="TDelegate"/> to combine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<TDelegate>(ref TDelegate? targetEventHandler, TDelegate? val)
            where TDelegate : Delegate
        {
            var eventHandler = targetEventHandler;
            TDelegate? eventHandler2;
            do
            {
                eventHandler2 = eventHandler;
                eventHandler = Interlocked.CompareExchange(
                    ref targetEventHandler,
                    (TDelegate?)Delegate.Combine(eventHandler2, val),
                    eventHandler2);
            }
            while (eventHandler != eventHandler2);
        }

        /// <summary>
        /// Remove a delegate with thread-safety guarantees.
        /// </summary>
        /// <typeparam name="TDelegate">Type of <see cref="Delegate"/></typeparam>
        /// <param name="targetEventHandler">Target <see cref="VRCLogEventHandler{TEventArgs}"/>.</param>
        /// <param name="val">An <typeparamref name="TDelegate"/> to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove<TDelegate>(ref TDelegate? targetEventHandler, TDelegate? val)
            where TDelegate : Delegate
        {
            var eventHandler = targetEventHandler;
            TDelegate? eventHandler2;
            do
            {
                eventHandler2 = eventHandler;
                eventHandler = Interlocked.CompareExchange(
                    ref targetEventHandler,
                    (TDelegate?)Delegate.Remove(eventHandler2, val),
                    eventHandler2);
            }
            while (eventHandler != eventHandler2);
        }
    }
}
