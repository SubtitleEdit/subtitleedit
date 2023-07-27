using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Represents a pool of StringBuilder objects that can be reused to mitigate the
    /// performance cost of frequently instantiating StringBuilders.
    /// </summary>
    internal static class StringBuilderPool
    {
        private const int MinCapacity = 1024;
        private const int MaxPoolSize = 3;

        private static readonly object Lock = new object();
        private static readonly Stack<StringBuilder> Pool = new Stack<StringBuilder>();

        private static int _maxCapacity = 85000;

        /// <summary>
        /// Get or set the maximum capacity of StringBuilder that can be stored in
        /// the pool.
        /// </summary>
        internal static int MaxCapacity
        {
            get => _maxCapacity;
            set => _maxCapacity = Math.Max(MinCapacity, value);
        }

        /// <summary>
        /// Returns a StringBuilder instance from the pool. If the pool is empty,
        /// a new instance is created with a default minimum capacity.
        /// </summary>
        /// <returns>A StringBuilder instance.</returns>
        internal static StringBuilder Get()
        {
            lock (Lock)
            {
                // nothing in the pool
                if (Pool.Count == 0)
                {
                    return new StringBuilder(MinCapacity);
                }

                // clear and return from the pool
                return Pool.Pop().Clear();
            }
        }

        /// <summary>
        /// Converts the StringBuilder into a string, returns the StringBuilder
        /// to the pool, and then returns the string contents of the StringBuilder.
        /// </summary>
        /// <param name="sb">The StringBuilder instance to return to the pool.</param>
        /// <returns>The string content of the StringBuilder.</returns>
        internal static string ToPool(this StringBuilder sb)
        {
            var content = sb.ToString();
            ReturnToPool(sb);
            return content;
        }

        /// <summary>
        /// Tries to return a StringBuilder to the pool. If the pool is full or
        /// the capacity of the StringBuilder is too large, it is not added to the pool.
        /// </summary>
        /// <param name="sb">The StringBuilder instance to return to the pool.</param>
        /// <returns>A boolean value indicating whether the operation was successful.</returns>
        private static bool ReturnToPool(this StringBuilder sb)
        {
            lock (Lock)
            {
                var currentPoolSize = Pool.Count;

                // capacity passes the max allowed limit in the pool
                if (sb.Capacity > _maxCapacity)
                {
                    return false;
                }

                // pool already at is max capacity
                if (currentPoolSize == MaxPoolSize)
                {
                    return false;
                }

                // the incoming stringbuilder will enter the pool only if it's capacity
                // is greater than the available one on the top
                if (currentPoolSize < MaxPoolSize || Pool.Peek().Capacity < sb.Capacity)
                {
                    Pool.Push(sb);
                }

                return true;
            }
        }
    }
}