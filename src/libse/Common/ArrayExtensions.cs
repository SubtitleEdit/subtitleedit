using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class ArrayExtensions
    {
        public static T FirstOnOrAfter<T>(this List<T> list, T target, T defaultValue)
        {
            int index = list.BinarySearch(target);
            if (index < 0)
            {
                index = ~index; // Get the bitwise complement to find the index of the first element greater than the target
            }

            if (index < list.Count)
            {
                return list[index];
            }

            return defaultValue;
        }

        public static T FirstOnOrBefore<T>(this List<T> list, T target, T defaultValue)
        {
            int index = list.BinarySearch(target);
            if (index < 0)
            {
                index = ~index - 1; // Get the bitwise complement to find the index of the last element smaller than the target
            }

            if (index >= 0)
            {
                return list[index];
            }

            return defaultValue;
        }

        public static int ClosestIndexTo(this List<int> list, int target)
        {
            int closestIndex = list.BinarySearch(target);

            // If the value is found, BinarySearch returns the index of the exact match.
            // Otherwise, it returns a negative value that indicates the bitwise complement of the first index
            // greater than the search value. So, we need to convert it to the index of the closest value.
            if (closestIndex < 0)
            {
                closestIndex = ~closestIndex;

                if (closestIndex >= list.Count)
                {
                    return list.Count - 1;
                }
                else if ((closestIndex - 1) >= 0)
                {
                    // If both indices are valid, return which one's value is closer.
                    return Math.Abs(list[closestIndex - 1] - target) < Math.Abs(list[closestIndex] - target) ? closestIndex - 1 : closestIndex;
                }
            }

            return closestIndex;
        }

        public static int ClosestTo(this List<int> list, int target)
        {
            int closestIndex = list.ClosestIndexTo(target);
            return list[closestIndex];
        }

        public static int ClosestIndexTo(this List<double> list, double target)
        {
            int closestIndex = list.BinarySearch(target);

            // If the value is found, BinarySearch returns the index of the exact match.
            // Otherwise, it returns a negative value that indicates the bitwise complement of the first index
            // greater than the search value. So, we need to convert it to the index of the closest value.
            if (closestIndex < 0)
            {
                closestIndex = ~closestIndex;

                if (closestIndex >= list.Count)
                {
                    return list.Count - 1;
                }
                else if ((closestIndex - 1) >= 0)
                {
                    // If both indices are valid, return which one's value is closer.
                    return Math.Abs(list[closestIndex - 1] - target) < Math.Abs(list[closestIndex] - target) ? closestIndex - 1 : closestIndex;
                }
            }

            return closestIndex;
        }

        public static double ClosestTo(this List<double> list, double target)
        {
            int closestIndex = list.ClosestIndexTo(target);
            return list[closestIndex];
        }

        public static int? FirstWithin(this List<int> list, int start, int end)
        {
            int startIndex = list.BinarySearch(start);
            int endIndex = list.BinarySearch(end);

            if (startIndex < 0)
            {
                startIndex = ~startIndex;
            }

            if (endIndex < 0)
            {
                endIndex = ~endIndex - 1;
            }

            if (startIndex > endIndex || startIndex >= list.Count || list[startIndex] > end)
            {
                return null;
            }

            return list[startIndex];
        }
    }
}
