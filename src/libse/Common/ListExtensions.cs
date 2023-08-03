using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class ListExtensions
    {
        // First on or after / before (generic)

        public static T FirstOnOrAfter<T>(this List<T> orderedList, T target, T defaultValue)
        {
            int index = orderedList.BinarySearch(target);
            if (index < 0)
            {
                index = ~index; // Get the bitwise complement to find the index of the first element greater than the target
            }

            if (index < orderedList.Count)
            {
                return orderedList[index];
            }

            return defaultValue;
        }

        public static T FirstOnOrBefore<T>(this List<T> orderedList, T target, T defaultValue)
        {
            int index = orderedList.BinarySearch(target);
            if (index < 0)
            {
                index = ~index - 1; // Get the bitwise complement to find the index of the last element smaller than the target
            }

            if (index >= 0)
            {
                return orderedList[index];
            }

            return defaultValue;
        }


        // First on or after / before with max difference (double)

        public static double FirstOnOrAfter(this List<double> orderedList, double target, double maxDifference, double defaultValue)
        {
            int index = orderedList.BinarySearch(target);
            if (index < 0)
            {
                index = ~index; // Get the bitwise complement to find the index of the first element greater than the target
            }

            if ((index - 1) >= 0 && target - orderedList[index - 1] <= maxDifference)
            {
                return orderedList[index - 1];
            }

            if (index < orderedList.Count)
            {
                return orderedList[index];
            }

            return defaultValue;
        }

        public static double FirstOnOrBefore(this List<double> orderedList, double target, double maxDifference, double defaultValue)
        {
            int index = orderedList.BinarySearch(target);
            if (index < 0)
            {
                index = ~index - 1; // Get the bitwise complement to find the index of the last element smaller than the target
            }

            if (index + 1 < orderedList.Count && orderedList[index + 1] - target <= maxDifference)
            {
                return orderedList[index + 1];
            }

            if (index >= 0)
            {
                return orderedList[index];
            }

            return defaultValue;
        }


        // Closest to (int)

        public static int ClosestIndexTo(this List<int> orderedList, int target)
        {
            int closestIndex = orderedList.BinarySearch(target);

            // If the value is found, BinarySearch returns the index of the exact match.
            // Otherwise, it returns a negative value that indicates the bitwise complement of the first index
            // greater than the search value. So, we need to convert it to the index of the closest value.
            if (closestIndex < 0)
            {
                closestIndex = ~closestIndex;

                if (closestIndex >= orderedList.Count)
                {
                    return orderedList.Count - 1;
                }
                else if ((closestIndex - 1) >= 0)
                {
                    // If both indices are valid, return which one's value is closer.
                    return Math.Abs(orderedList[closestIndex - 1] - target) < Math.Abs(orderedList[closestIndex] - target) ? closestIndex - 1 : closestIndex;
                }
            }

            return closestIndex;
        }

        public static int ClosestTo(this List<int> orderedList, int target)
        {
            int closestIndex = orderedList.ClosestIndexTo(target);
            return orderedList[closestIndex];
        }


        // Closest to (double)

        public static int ClosestIndexTo(this List<double> orderedList, double target)
        {
            int closestIndex = orderedList.BinarySearch(target);

            // If the value is found, BinarySearch returns the index of the exact match.
            // Otherwise, it returns a negative value that indicates the bitwise complement of the first index
            // greater than the search value. So, we need to convert it to the index of the closest value.
            if (closestIndex < 0)
            {
                closestIndex = ~closestIndex;

                if (closestIndex >= orderedList.Count)
                {
                    return orderedList.Count - 1;
                }
                else if ((closestIndex - 1) >= 0)
                {
                    // If both indices are valid, return which one's value is closer.
                    return Math.Abs(orderedList[closestIndex - 1] - target) < Math.Abs(orderedList[closestIndex] - target) ? closestIndex - 1 : closestIndex;
                }
            }

            return closestIndex;
        }

        public static double ClosestTo(this List<double> orderedList, double target)
        {
            int closestIndex = orderedList.ClosestIndexTo(target);
            return orderedList[closestIndex];
        }


        // First within (int)

        public static int? FirstWithin(this List<int> orderedList, int start, int end)
        {
            int startIndex = orderedList.BinarySearch(start);
            int endIndex = orderedList.BinarySearch(end);

            if (startIndex < 0)
            {
                startIndex = ~startIndex;
            }

            if (endIndex < 0)
            {
                endIndex = ~endIndex - 1;
            }

            if (startIndex > endIndex || startIndex >= orderedList.Count || orderedList[startIndex] > end)
            {
                return null;
            }

            return orderedList[startIndex];
        }
    }
}