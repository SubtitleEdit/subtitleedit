using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nikse.SubtitleEdit.Logic
{
    /**
     * <summary>
     * <para>
     * A class that helps determine which paragraphs should be displayed when there may be too many to
     * efficiently render on the timeline at the same time.
     * </para>
     * <para>
     * It assumes that:
     * <list type="bullet">
     * <item>It is good to select paragraphs slightly outside the visible area so that there is something to see while scrolling, 
     * but the timeline will be stationary most of the time, so it is better to select paragraphs that are currently visible.</item>
     * <item>It is more useful to have paragraphs that cover a large area of the timeline rather than a small area with 10 paragraphs layered on top of each other.</item>
     * <item>There are situtations where paragraphs may overlap, but it is useful to see most or all of them
     * (such as dialogue shown at the same time as a paragraph shown next to text in the video).</item>
     * <item>More predictable behavior is better - pruning a large paragraph is more noticeable than pruning a small one, and the visible paragraphs should stay
     * constant as much as possible while scrolling.</item>
     * </list>
     * </para>
     * <para>
     * Therefore, this class aims to maximize the amount of coverage of the timeline.
     * Non-overlapping paragraphs are preferred first to prevent a stack of overlapping paragraphs with a large blank space. Paragraphs outside the visible area are only
     * choosen once enough visible paragraphs have been chosen, to prevent a blank timeline. This class may select paragraphs that are very close together, if all other preferred
     * paragraphs have been chosen already.
     * </para>
     * </summary>
     */
    internal class DisplayableParagraphHelper
    {
        /// <summary>
        /// The percentage of the visible timeline that must be covered by paragraphs before a paragraph outside the visible area may be chosen.
        /// <para>
        /// Note that this is cumulative: two paragraphs stacked on top of each other count exactly the same as the same two paragraphs with no overlap.
        /// </para>
        /// </summary>
        private const double VisibleSelectionRequirement = 0.5;

        /// <summary>
        /// Paragraphs that may be chosen when requested later.
        /// </summary>
        private readonly List<Paragraph> _paragraphs = new List<Paragraph>();

        private TimelinePartition _cachedParagraphPartitions;

        /// <summary>
        /// The beginning of the invisible area that paragraphs may be chosen from to improve scrolling.
        /// </summary>
        private readonly double _startThresholdMilliseconds;
        /// <summary>
        /// The end of the invisible area.
        /// </summary>
        private readonly double _endThresholdMilliseconds;

        /// <summary>
        /// The beginning of the visible area of the timeline.
        /// </summary>
        private readonly double _startVisibleMilliseconds;
        /// <summary>
        /// The end of the visible area of the timeline.
        /// </summary>
        private readonly double _endVisibleMilliseconds;

        /// <summary>
        /// Creates a new displayable paragraph helper that will choose paragraphs between the start and end time.
        /// </summary>
        /// <param name="startMilliseconds">The start of the visible area of the timeline in milliseconds.</param>
        /// <param name="endMilliseconds">The end of the visible area of the timeline in milliseconds.</param>
        /// <param name="additionalMilliseconds">Additional time outside of the visible area to include, to improve rendering while scrolling.</param>
        public DisplayableParagraphHelper(double startMilliseconds, double endMilliseconds, double additionalMilliseconds)
        {
            _startThresholdMilliseconds = startMilliseconds - additionalMilliseconds;
            _endThresholdMilliseconds = endMilliseconds + additionalMilliseconds;

            _startVisibleMilliseconds = startMilliseconds;
            _endVisibleMilliseconds = endMilliseconds;

            _cachedParagraphPartitions = new TimelinePartition(_startThresholdMilliseconds, _endThresholdMilliseconds, 500);
        }

        /// <summary>
        /// Adds a paragraph to the pool of available paragraphs the helper will choose from.
        /// </summary>
        /// <param name="p"></param>
        public void Add(Paragraph p)
        {
            if (IsInThreshold(p))
            {
                _paragraphs.Add(p);
                //paragraphPartition.Add(p);
            }
        }

        /// <summary>
        /// Determines whether the paragraph is in the visible area.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsVisible(Paragraph p)
        {
            return IsInRange(p, _startVisibleMilliseconds, _endVisibleMilliseconds);
        }

        /// <summary>
        /// Determines whether the paragraph is visible in the area just outside the visible area.
        /// Note that a paragraph that passes this test may also be in the visible area, so
        /// <code>!IsInThreshold(p)</code> is not necessarily the same as <code>IsVisible(p)</code>.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsInThreshold(Paragraph p)
        {
            return IsInRange(p, _startThresholdMilliseconds, _endThresholdMilliseconds);
        }

        /// <summary>
        /// Determines whether any portion of a paragraph is within the start and end range.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="startMilliseconds">Start time of range, in milliseconds.</param>
        /// <param name="endMilliseconds">End time of range, in milliseconds.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsInRange(Paragraph p, double startMilliseconds, double endMilliseconds)
        {
            return p.StartTime.TotalMilliseconds <= endMilliseconds && p.EndTime.TotalMilliseconds >= startMilliseconds;
        }

        /// <summary>
        /// Determines whether two paragraphs overlap.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ParagraphsOverlap(Paragraph p1, Paragraph p2)
        {
            return IsInRange(p1, p2.StartTime.TotalMilliseconds, p2.EndTime.TotalMilliseconds);
        }

        /**
         * Calculates the average number of layers of paragraphs in the visible portion of the timeline.
         * Two paragraphs covering the left half of the timeline count the same as a single paragraph covering the whole time.
         * This has the benefit of being easier to calculate, and allows coverage percentage to be built up over time (when selecting paragraphs).
         */
        private double CalculateAverageParagraphCoverage()
        {
            double average = 0;
            foreach (Paragraph p in _paragraphs)
            {
                if (IsVisible(p) && p.DurationTotalMilliseconds > 0)
                {
                    average += CalculateVisiblePercentOfTimeline(p);
                }
            }
            return average;
        }

        /// <summary>
        /// Calculates the percent of the visible timeline that a paragraph is visible.
        /// If the paragraph is partially invisible, parts outside the visible area are not considered.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private double CalculateVisiblePercentOfTimeline(Paragraph p)
        {
            double startClamped = Math.Max(p.StartTime.TotalMilliseconds, _startVisibleMilliseconds);
            double endClamped = Math.Min(p.EndTime.TotalMilliseconds, _endVisibleMilliseconds);
            return (endClamped - startClamped) / (_endVisibleMilliseconds - _startVisibleMilliseconds);
        }

        /// <summary>
        /// Calculates the amount of paragraph coverage in the given range.
        /// <para>
        /// Amount of coverage is defined as the number of layers of paragraphs times the duration of the overlap, divided by
        /// the duration of the paragraph.
        /// This is computed piecewise - a time range that contains a single paragraph for 1 millisecond and no paragraphs
        /// for the other 1000 milliseconds will be very close to 0.
        /// A time range half covered with one paragraph and half covered with two paragraphs will have a coverage of 1.5.
        /// </para>
        /// 
        /// </summary>
        /// <param name="currentCoverage">An ordered list indicating how many paragraphs are visible at each location on the timeline.</param>
        /// <param name="startMillis"></param>
        /// <param name="endMillis"></param>
        /// <returns>The average amount of paragraph coverage in the given range. This is a floating point number greater than 0.</returns>
        private double CalculateCoverageInRange(List<CoverageRecord> currentCoverage, double startMillis, double endMillis)
        {

            if (currentCoverage.Count == 0)
            {
                // There are no coverage records, so by default the answer is 0.
                // Prevents array out-of-bounds exceptions as well.
                return 0;
            }

            double previousTimestamp;
            double previousNumberOfParagraphs;

            // The sum of number of paragraphs times the duration of the overlap.
            double weightedCoverage = 0;

            CoverageRecord startRecord = new CoverageRecord(startMillis);
            int startIndex = currentCoverage.BinarySearch(startRecord, new TimestampRecordComparer());
            if (startIndex < 0)
            {
                // Start of range has no record, need to build the information from the record we would have found.
                startIndex = ~startIndex;
                if (startIndex > 0)
                {
                    if (startIndex >= currentCoverage.Count)
                    {
                        // The start index comes after all paragraphs have ended, so there can't be any coverage.
                        return 0;
                    }
                    // Any start record that would have existed at startIndex would have the same number of paragraphs
                    // as the previous record.
                    previousNumberOfParagraphs = currentCoverage[startIndex - 1].numberOfParagraphs;
                    if (endMillis <= currentCoverage[startIndex].timestamp)
                    {
                        // The start and end both happen before the same record. Average coverage over the entire range is trivial.
                        return previousNumberOfParagraphs;
                    }
                    weightedCoverage = previousNumberOfParagraphs * (currentCoverage[startIndex].timestamp - startMillis);
                    previousNumberOfParagraphs = currentCoverage[startIndex].numberOfParagraphs;
                }
                else
                {
                    // startIndex is 0.
                    // The start range is before the first record - there cannot be any paragraph coverage yet.
                    previousNumberOfParagraphs = 0;
                }
                // We are guaranteed to have at least one item in the array because of the checks above.
                previousTimestamp = currentCoverage[startIndex].timestamp;
            }
            else
            {
                // The start timestamp matches an existing record, so there is no leading coverage to calculate.
                if (startIndex >= currentCoverage.Count)
                {
                    // We can't combine with the above check because building the previous record data is
                    // very different depending on the value of startIndex.
                    return 0;
                }
                CoverageRecord previousRecord = currentCoverage[startIndex];
                previousTimestamp = previousRecord.timestamp;
                previousNumberOfParagraphs = previousRecord.numberOfParagraphs;
            }


            if (startIndex < currentCoverage.Count - 1)
            {
                int currentIndex = startIndex + 1;
                while (currentIndex < currentCoverage.Count && currentCoverage[currentIndex].timestamp <= endMillis)
                {
                    CoverageRecord currentRecord = currentCoverage[currentIndex];
                    weightedCoverage += previousNumberOfParagraphs * (currentRecord.timestamp - previousTimestamp);

                    previousTimestamp = currentRecord.timestamp;
                    previousNumberOfParagraphs = currentRecord.numberOfParagraphs;
                    currentIndex++;
                }
            }

            if (previousTimestamp != endMillis)
            {
                // There was no record exactly matching the end range, so there was a little bit left over.
                weightedCoverage += previousNumberOfParagraphs * (endMillis - previousTimestamp);
            }

            return weightedCoverage / (endMillis - startMillis);
        }

        /// <summary>
        /// Chooses the best paragraph from the list of candidate paragraphs.
        /// </summary>
        /// <param name="averageCoverage">The total coverage of the visible timeline, to prefer visible paragraphs
        /// until the timeline has enough coverage.</param>
        /// <param name="currentVisibleCoverage">The current coverage of the visible timeline. If this is high enough,
        /// invisible paragraphs may be chosen.</param>
        /// <param name="lowestCoverage">The minimum amount of coverage in the </param>
        /// <param name="candidates"></param>
        /// <param name="currentCoverage"></param>
        /// <param name="coverageCache"></param>
        /// <returns></returns>
        private int GetBestParagraphIndex(double averageCoverage, double currentVisibleCoverage, double lowestCoverage,
            List<Paragraph> candidates, List<CoverageRecord> currentCoverage, Dictionary<Paragraph, double> coverageCache)
        {
            double minimumCoverage = double.MaxValue;
            int indexOfMinimum = -1;

            for (var i = 0; i < candidates.Count; i++)
            {
                Paragraph p = candidates[i];
                bool paragraphVisible = IsVisible(p);
                // Only consider visible paragraphs until a minimum portion of the visible area is covered.
                if (currentVisibleCoverage > averageCoverage * VisibleSelectionRequirement || paragraphVisible)
                {
                    if (!coverageCache.TryGetValue(p, out double existingCoverage))
                    {
                        existingCoverage = CalculateCoverageInRange(currentCoverage, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds);
                        coverageCache.Add(p, existingCoverage);
                        _cachedParagraphPartitions.Add(p);
                    }
                    if (existingCoverage < minimumCoverage)
                    {
                        minimumCoverage = existingCoverage;
                        indexOfMinimum = i;
                        if (existingCoverage <= lowestCoverage)
                        {
                            return indexOfMinimum;
                        }
                    }
                }
            }

            return indexOfMinimum;
        }

        public List<Paragraph> GetParagraphs(int limit)
        {
            if (limit >= _paragraphs.Count)
            {
                return _paragraphs;
            }

            List<Paragraph> result = new List<Paragraph>();
            Dictionary<Paragraph, double> coverageCache = new Dictionary<Paragraph, double>();

            double averageCoverage = CalculateAverageParagraphCoverage();
            double currentVisibleCoverage = 0;
            List<CoverageRecord> records = new List<CoverageRecord>(limit * 2);

            // Ensure that longer paragraphs are preferred.
            _paragraphs.Sort(new ParagraphComparer());

            double lowestCoverage = 0;

            while (result.Count < limit && _paragraphs.Count > 0)
            {
                int bestParagraphIndex = GetBestParagraphIndex(averageCoverage, currentVisibleCoverage, lowestCoverage, _paragraphs, records, coverageCache);
                if (bestParagraphIndex != -1)
                {
                    Paragraph selection = _paragraphs[bestParagraphIndex];
                    _paragraphs.RemoveAt(bestParagraphIndex);
                    lowestCoverage = coverageCache[selection];

                    result.Add(selection);
                    UpdateCoverageRecords(records, selection);
                    if (IsVisible(selection))
                    {
                        double coveragePercent = CalculateVisiblePercentOfTimeline(selection);
                        currentVisibleCoverage += coveragePercent;
                    }
                    if (result.Count < limit)
                    {
                        UpdateCacheForParagraph(coverageCache, selection);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Given a paragraph that has just been selected, find cached coverage values for paragraphs that overlap with the
        /// new paragraph and adjust the value.
        /// <para>
        /// This is an O(1) operation per cache value, versus an O(n log n) operation to calculate from scratch.
        /// As the cache fills up, the number of values to update increases, making the scan for overlapping paragraphs slower.
        /// This is optimized by checking only those paragraphs from the same partition as the newly added paragraph. This
        /// is a much smaller set than the set of all cached paragraphs.
        /// </para>
        /// </summary>
        /// <param name="coverageCache"></param>
        /// <param name="p"></param>
        private void UpdateCacheForParagraph(Dictionary<Paragraph, double> coverageCache, Paragraph p)
        {
            HashSet<Paragraph> partitionedParagraphs = _cachedParagraphPartitions.GetPartitionedParagraphs(p);
            // We don't want to update the cache entry for the selected paragraph on this iteration or any further iterations.
            partitionedParagraphs.Remove(p);
            coverageCache.Remove(p);

            foreach (Paragraph key in partitionedParagraphs)
            {
                // The partition may contain paragraphs that have been selected and evicted from the cache,
                // so this isn't guaranteed to exist.
                if (ParagraphsOverlap(key, p) && coverageCache.TryGetValue(key, out double coverage))
                {
                    double overlapMillis = CalculateOverlapLength(key, p);
                    coverageCache[key] = coverage + overlapMillis / key.DurationTotalMilliseconds;
                }
            }
        }


        private double CalculateOverlapLength(Paragraph p1, Paragraph p2)
        {
            double overlapStart = Math.Max(p1.StartTime.TotalMilliseconds, p2.StartTime.TotalMilliseconds);
            double overlapEnd = Math.Min(p1.EndTime.TotalMilliseconds, p2.EndTime.TotalMilliseconds);
            return overlapEnd - overlapStart;
        }

        private void UpdateCoverageRecords(List<CoverageRecord> records, Paragraph newParagraph)
        {
            int startIndex = CreateAndGetRecordIndex(records, newParagraph.StartTime.TotalMilliseconds);
            int endIndex = CreateAndGetRecordIndex(records, newParagraph.EndTime.TotalMilliseconds);
            for (int i = startIndex; i < endIndex; i++)
            {
                records[i].numberOfParagraphs++;
            }
        }

        private int CreateAndGetRecordIndex(List<CoverageRecord> records, double timestamp)
        {
            CoverageRecord newRecord = new CoverageRecord(timestamp);

            int recordIndex = records.BinarySearch(newRecord, new TimestampRecordComparer());
            if (recordIndex < 0)
            {
                recordIndex = ~recordIndex;

                if (recordIndex > 0)
                {
                    // Carry over the overlap from the previous item to keep layers correct.
                    newRecord.numberOfParagraphs = records[recordIndex - 1].numberOfParagraphs;
                }

                records.Insert(recordIndex, newRecord);
            }

            return recordIndex;
        }

        private class CoverageRecord
        {
            public double timestamp { get; }
            public int numberOfParagraphs { get; set; }
            private int numberOfStartMarks;
            private int numberOfEndMarks;

            public CoverageRecord(double timestamp)
            {
                this.timestamp = timestamp;
            }

            public override string ToString()
            {
                return $"Record - {timestamp} millis / {numberOfParagraphs} paragraphs";
            }

        }

        private class TimestampRecordComparer : IComparer<CoverageRecord>
        {
            public int Compare(CoverageRecord x, CoverageRecord y)
            {
                return x.timestamp.CompareTo(y.timestamp);
            }
        }

        /**
         * A comparer for paragraphs, prioritizing those that are:
         *   1. Longer
         *   2. Have a smaller (earlier) start time.
         */
        private class ParagraphComparer : IComparer<Paragraph>
        {
            public int Compare(Paragraph x, Paragraph y)
            {
                // Calculation is (y.duration - x.duration) so that if x.duration is larger, the difference is < 0 and x comes first.
                double lengthComparison = y.DurationTotalMilliseconds - x.DurationTotalMilliseconds;
                if (lengthComparison > 0)
                {
                    return 1;
                }
                else if (lengthComparison < 0)
                {
                    return -1;
                }

                // Calculation is (x.start - y.start) so that if x comes first, difference is < 0.
                return Math.Sign(x.StartTime.TotalMilliseconds - y.StartTime.TotalMilliseconds);
            }
        }

        private class TimelinePartition
        {

            private double _startMillis;
            private double _endMillis;
            private int _partitionCount;

            private HashSet<Paragraph>[] _partitions;

            public TimelinePartition(double startMillis, double endMillis, int partitionCount)
            {
                _startMillis = startMillis;
                _endMillis = endMillis;
                _partitionCount = partitionCount;

                _partitions = new HashSet<Paragraph>[_partitionCount];
            }

            public void Add(Paragraph p)
            {
                PartitionRange insertRange = GetPartitionRange(p);
                for (var i = insertRange.StartIndex; i <= insertRange.EndIndex; i++)
                {
                    if (_partitions[i] == null)
                    {
                        _partitions[i] = new HashSet<Paragraph>();
                    }
                    _partitions[i].Add(p);
                }
            }

            public HashSet<Paragraph> GetPartitionedParagraphs(Paragraph p)
            {
                PartitionRange range = GetPartitionRange(p);
                HashSet<Paragraph> result = new HashSet<Paragraph>();
                for (var i = range.StartIndex; i < range.EndIndex; i++)
                {
                    HashSet<Paragraph> partition = _partitions[i];
                    if (partition != null)
                    {
                        result.UnionWith(partition);
                    }
                }
                return result;
            }

            private int GetPartitionNumber(double timestampMillis, bool roundUp)
            {
                double timeSpan = _endMillis - _startMillis;
                double partitionWidth = timeSpan / _partitionCount;
                double partitionNumberFraction = (timestampMillis - _startMillis) / partitionWidth;
                int partitionNumber;
                if (roundUp)
                {
                    partitionNumber = (int)Math.Ceiling(partitionNumberFraction);
                }
                else
                {
                    partitionNumber = (int)Math.Floor(partitionNumberFraction);
                }

                if (partitionNumber < 0)
                {
                    partitionNumber = 0;
                }
                else if (partitionNumber >= _partitionCount)
                {
                    partitionNumber = _partitionCount - 1;
                }

                return partitionNumber;
            }

            public PartitionRange GetPartitionRange(Paragraph p)
            {
                int startPartition = GetPartitionNumber(p.StartTime.TotalMilliseconds, false);
                int endPartition = GetPartitionNumber(p.EndTime.TotalMilliseconds, true);
                return new PartitionRange(startPartition, endPartition);
            }


            public class PartitionRange
            {
                public PartitionRange(int startIndex, int endIndex)
                {
                    StartIndex = startIndex;
                    EndIndex = endIndex;
                }
                public int StartIndex { get; }
                public int EndIndex { get; }
            }

        }


    }
}
