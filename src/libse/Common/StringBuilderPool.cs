using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    internal static class StringBuilderPool
    {
        private const int MinLimit = 1024;

        private static readonly object _lock = new object();
        private static readonly Stack<StringBuilder> Pool = new Stack<StringBuilder>();

        private static int _maxPoolSize = 3;
        private static int _maxLimit = 85000;

        internal static int MaxPoolSize
        {
            get => _maxPoolSize;
            set => _maxPoolSize = Math.Max(1, value);
        }

        internal static int MaxLimit
        {
            get => _maxLimit;
            set => _maxLimit = Math.Max(MinLimit, value);
        }

        internal static StringBuilder Retrieve()
        {
            lock (_lock)
            {
                // nothing in the pool
                if (Pool.Count == 0)
                {
                    return new StringBuilder(MinLimit);
                }

                // clear and return from the pool
                return Pool.Pop().Clear();
            }
        }

        internal static void ReturnToPool(this StringBuilder sb)
        {
            lock (_lock)
            {
                var currentPoolSize = Pool.Count;
                
                // capacity passes the max allowed limit in the pool
                if (sb.Capacity > _maxLimit)
                {
                    return;
                }

                if (currentPoolSize == _maxPoolSize)
                {
                    // drop min from the bag
                }
                else if (currentPoolSize < Math.Min(2, _maxPoolSize) || Pool.Peek().Capacity < sb.Capacity)
                {
                    Pool.Push(sb);
                }
            }
        }
    }
}