using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    internal static class StringBuilderPool
    {
        private const int MinCapacity = 1024;
        private const int MaxPoolSize = 3;

        private static readonly object Lock = new object();
        private static readonly Stack<StringBuilder> Pool = new Stack<StringBuilder>();

        private static int _maxCapacity = 85000;

        internal static int MaxCapacity
        {
            get => _maxCapacity;
            set => _maxCapacity = Math.Max(MinCapacity, value);
        }

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

        internal static string ToPool(this StringBuilder sb)
        {
            var content = sb.ToString();
            ReturnToPool(sb);
            return content;
        }

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
                if (Pool.Peek().Capacity < sb.Capacity)
                {
                    Pool.Push(sb);
                }

                return true;
            }
        }
    }
}