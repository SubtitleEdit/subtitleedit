using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic
{
    internal class DisplayableSubtitleHelper
    {
        // Map associating a time stamp with a number of paragraphs that start at that time stamp.
        private readonly Dictionary<double, int> _startParagraphCounts = new Dictionary<double, int>();
        // Map associating a time stamp with a number of paragraphs that end at that time stamp.
        private readonly Dictionary<double, int> _endParagraphCounts = new Dictionary<double, int>();


        private readonly List<Paragraph> _paragraphs = new List<Paragraph>();


        private readonly double _startThresholdMilliseconds;
        private readonly double _endThresholdMilliseconds;

        private readonly double _startVisibleMilliseconds;
        private readonly double _endVisibleMilliseconds;

        public DisplayableSubtitleHelper(double startMilliseconds, double endMilliseconds, double additionalSeconds)
        {
            _startThresholdMilliseconds = startMilliseconds - additionalSeconds * 1000;
            _endThresholdMilliseconds = endMilliseconds + additionalSeconds * 1000;

            _startVisibleMilliseconds = startMilliseconds;
            _endVisibleMilliseconds = endMilliseconds;
        }

        public void Add(Paragraph p)
        {
            // This should take in all paragraphs and figure out which are important enough to render.
            // TODO: Filter out visible / invisible paragraphs.

            if (IsInThreshold(p))
            {
                AddStart(p.StartTime.TotalMilliseconds);
                AddEnd(p.EndTime.TotalMilliseconds);
                _paragraphs.Add(p);
            }
        }

        private void AddStart(double startMilliseconds)
        {
            if (_startParagraphCounts.TryGetValue(startMilliseconds, out int startCount))
            {
                startCount++;
            }
            else
            {
                startCount = 1;
            }

            _startParagraphCounts[startMilliseconds] = startCount;
        }

        private void AddEnd(double endMilliseconds)
        {
            if (_endParagraphCounts.TryGetValue(endMilliseconds, out int endCount))
            {
                endCount++;
            }
            else
            {
                endCount = 1;
            }

            _endParagraphCounts[endMilliseconds] = endCount;
        }

        private bool IsVisible(Paragraph p)
        {
            return IsInRange(p, _startVisibleMilliseconds, _endVisibleMilliseconds);
        }

        private bool IsInThreshold(Paragraph p)
        {
            return IsInRange(p, _startThresholdMilliseconds, _endThresholdMilliseconds);
        }

        private bool IsInRange(Paragraph p, double start, double end)
        {
            return p.StartTime.TotalMilliseconds <= end && p.EndTime.TotalMilliseconds >= start;
        }

        private double CalculateAverageParagraphCoverage()
        {
            // Average coverage is average number of layers of paragraphs at any moment of the visible timeline.
            // A single paragraph covering the entire visible timeline is equivalent to two layers of paragraphs
            // covering one half of the timeline with no paragraphs covering the other half.
            double average = 0;

            int numberOfVisibleParagraphs = 0;

            foreach (Paragraph p in _paragraphs)
            {
                if (IsVisible(p) && p.DurationTotalMilliseconds > 0)
                {
                    numberOfVisibleParagraphs++;
                    average += CalculateVisibleDurationOfParagraph(p);
                }
            }
            //Console.WriteLine($"Found {numberOfVisibleParagraphs} visible paragraphs with total {average} coverage of timeline.");

            return average;
        }

        private double CalculateVisibleDurationOfParagraph(Paragraph p)
        {
            double startClamped = Math.Max(p.StartTime.TotalMilliseconds, _startVisibleMilliseconds);
            double endClamped = Math.Min(p.EndTime.TotalMilliseconds, _endVisibleMilliseconds);
            return (endClamped - startClamped) / (_endVisibleMilliseconds - _startVisibleMilliseconds);
        }

        private double CalculateCoverageInRange(List<CoverageRecord> currentCoverage, double startRange, double endRange)
        {
            // TODO: This whole method is ugly, needs refactoring.
            CoverageRecord startRecord = new CoverageRecord(startRange);

            int startIndex = currentCoverage.BinarySearch(startRecord, new TimestampRecordComparer());
            double weightedCoverage = 0;
            double previousTimestamp = startRange;
            double previousNumberOfParagraphs = 0;

            if (startIndex < 0)
            {
                // Start of range has no record, need to add the overlap from the previous record.
                startIndex = ~startIndex;
                if (startIndex > 0)
                {
                    if (startIndex >= currentCoverage.Count)
                    {
                        // The start index comes after all paragraphs have ended, so there can't be any coverage.
                        return 0;
                    }
                    CoverageRecord r = currentCoverage[startIndex - 1];
                    previousTimestamp = startRange;
                    previousNumberOfParagraphs = r.numberOfParagraphs;
                }
                else
                {
                    // Start happened before the first record - there is no overlap to count.
                    startIndex = 0;
                    previousTimestamp = startRange;
                    previousNumberOfParagraphs = 0;
                }
            }
            else
            {
                if (startIndex >= currentCoverage.Count)
                {
                    // The start index comes after all paragraphs have ended, so there can't be any coverage.
                    return 0;
                }
                CoverageRecord previousRecord = currentCoverage[startIndex];
                previousTimestamp = previousRecord.timestamp;
                previousNumberOfParagraphs = previousRecord.numberOfParagraphs;
            }


            if (startIndex < currentCoverage.Count - 1)
            {
                int currentIndex = startIndex + 1;
                while (currentIndex < currentCoverage.Count && currentCoverage[currentIndex].timestamp < endRange)
                {
                    CoverageRecord currentRecord = currentCoverage[currentIndex];
                    weightedCoverage += previousNumberOfParagraphs * (currentRecord.timestamp - previousTimestamp);

                    previousTimestamp = currentRecord.timestamp;
                    previousNumberOfParagraphs = currentRecord.numberOfParagraphs;
                    currentIndex++;
                }
            }

            if (previousTimestamp != endRange)
            {
                // There was no record exactly matching the end range, so there was a little bit left over.
                // It is also possible that no start record matched either, so this is calculating the time between startRange and endRange.
                weightedCoverage += previousNumberOfParagraphs * (endRange - previousTimestamp);
            }

            return weightedCoverage / (endRange - startRange);
        }

        private Paragraph ChooseOneParagaph(double averageCoverage,double currentVisibleCoverage, int lowestCoverage, List<Paragraph> candidates, List<CoverageRecord> currentCoverage)
        {

            double minimumCoverage = double.MaxValue;
            int indexOfMinimum = -1;
            Paragraph bestParagraph = null;

            for(var i =  0; i < candidates.Count;i++)
            {
                Paragraph p = candidates[i];
                // TODO: If existing coverage > averageCoverage, allow invisible paragraph.
                double existingCoverage = CalculateCoverageInRange(currentCoverage, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds);
                if (existingCoverage < minimumCoverage)
                {
                    if ((currentVisibleCoverage > averageCoverage / 2 && IsInThreshold(p)) || IsVisible(p))
                    {
                        // Prefer visible paragraphs until the visible range has at least reached the average coverage for the range
                        minimumCoverage = existingCoverage;
                        bestParagraph = p;
                        indexOfMinimum = i;
                        if (existingCoverage <= lowestCoverage)
                        {
                            break;
                        }
                    }
                }
            }
            if (bestParagraph != null)
            {
                candidates.RemoveAt(indexOfMinimum);
            }

            return bestParagraph;
        }

        private Paragraph FindLeastOverlap(SortedDictionary<double, int> overlaps, List<Paragraph> paragraphs)
        {
            double lowestAverageOverlap = double.MaxValue;
            Paragraph leastOverlappingParagraph = null;

            /*
             * This algorithm tries to maximize the percentage of the timeline that is covered with paragraphs, so it will tend to cover everything as evenly as possible.
             * Algorithm:
             *  1. Select a candidate paragraph
             *  2. Calculate average length of time this paragraph overlaps with already selected paragraphs.
             *  3. If the number of overlaps is equal to the minimum overlap on the whole timeline, return early (this indicates the paragraph has minimum overlap and is a good candidate).
             *  4. If not, try the next paragraph
             *  5. Any time a paragraph is found, update the current overlaps with the new paragraph
             *             
             * As the paragraph list is sorted with longest paragraph first, this ensures it chooses longest paragraphs first, with each new paragraph overlapping as few others as possible.
             * This avoids situations where the displayed paragraphs:
             *  - Always start and end at the same time (stacked many layers deep).
             *  - Leave gaps between non-overlapping paragraphs (a problem with buckets and wrong bucket size)
             *  - Tend to become pruned unpredictably, especially when scrolling the timeline: any paragraphs of equal duration are sorted first-to-last
             *             
             * The only drawback is that to the algorithm, there is no difference in priority between a paragraph that starts and ends at the same time as another paragraph vs. 
             * one that "straddles" two other paragraphs (assuming the two paragraphs have no gap between them).
             * 
             * The only solution is to hope that paragraphs don't start and end at the exact same time... or to add a condition that starting or ending at the same time as
             * another paragraph is less preferred. This should only take effect if there is a tie for least overlap, otherwise it may select a paragraph with no common
             * start/end times but creating more overlaps.
             *             
             */

            /*
             *  Desired algorithm:
             *  - Compute average number of subtitles covering each second of the visible timeline (sum all paragraph time within the visible range, divide by visible time)
             *  - Sort all paragraphs by length
             *  - Select N paragraphs, choose a candidate, starting at the longest paragraph. For each candidate:
             *    - Compute average number of subtitles already existing for the length of the candidate. (number of subtitles * duration of overlap / duration of candidate)
             *    - Candidate must be in the visible range, unless the current amount of subtitle coverage is greater than or equal to the total average paragraph coverage generated in step 1.
             *    - Candidate with the lowest average overlap wins
             *    - If there is a tie, the candidate with fewest shared start and end time wins (i.e. prefer to not choose stacked subtitles).
             *  - After choosing a candidate, update the current paragraph coverage.
             * 
             * Benefits:
             *  - No explicit choice of visible vs. invisible paragraphs
             *  - Allows choosing more invisible paragraphs when the visible range is exhausted (without risking setting too high of a limit on visible or invisible paragraphs)
             *  - Will only choose visible paragraphs until enough of them have been selected to cover the visible range, then allows invisible ones to be chosen if they are the best candidate
             *  - No special treatment or logic for invisible paragraphs (aside from checking that they are visible)
             */

            foreach (Paragraph p in paragraphs)
            {

                double start = p.StartTime.TotalMilliseconds;
                double end = p.EndTime.TotalMilliseconds;

                // These are guaranteed to exist because all paragraphs have been added to start / end Dictionaries.
                int startOverlap = overlaps[start];
                int endOverlap = overlaps[end];

                SortedDictionary<double, int>.KeyCollection keys = overlaps.Keys;
                List<double> keyList = keys.ToList();

                int startIndex = keyList.IndexOf(start);
                int endIndex = keyList.IndexOf(end);

                double previousTime = start;
                double previousOverlaps = overlaps[start];
                double averageOverlap = 0;

                for (int overlapIndex = startIndex + 1; overlapIndex <= endIndex; overlapIndex++)
                {
                    double currentTime = keyList[overlapIndex];
                    int currentOverlaps = overlaps[currentTime];
                    double timeDelta = currentTime - previousTime;
                    averageOverlap += previousOverlaps / timeDelta;

                    previousOverlaps = currentOverlaps;
                    previousTime = currentTime;
                }

                if (averageOverlap < lowestAverageOverlap)
                {
                    lowestAverageOverlap = averageOverlap;
                    leastOverlappingParagraph = p;
                }
            }

            return leastOverlappingParagraph;
        }

        public List<Paragraph> GetParagraphs(int limit, int invisibleLimit)
        {
            //Console.WriteLine($"Getting {limit} paragraphs.");
            if (limit >= _paragraphs.Count)
            {
                return _paragraphs;
            }

            List<Paragraph> result = new List<Paragraph>();
            Dictionary<Paragraph, double> coverageCache = new Dictionary<Paragraph, double>();

            double averageCoverage = CalculateAverageParagraphCoverage();
            double currentVisibleCoverage = 0;
            List<CoverageRecord> records = new List<CoverageRecord>();
            // How to figure out what to remove from cache?

            // Ensure that longer paragraphs are preferred.
            _paragraphs.Sort(new ParagraphComparer());

            int lowestCoverage = 0;

            while (result.Count < limit && _paragraphs.Count > 0)
            {
                Paragraph selection = ChooseOneParagaph(averageCoverage,currentVisibleCoverage,lowestCoverage, _paragraphs, records);
                if (selection != null)
                {
                    result.Add(selection);
                    UpdateCoverageRecords(records, selection);
                    if (IsVisible(selection))
                    {
                        double coveragePercent = CalculateVisibleDurationOfParagraph(selection);
                        currentVisibleCoverage += coveragePercent;
                        //Console.WriteLine($"Paragraph selected, adding {coveragePercent} to current coverage. (for a total of {currentVisibleCoverage})");
                        lowestCoverage = FindLowestCoverage(records);
                    }
                }
            }

            return result;
        }

        private void UpdateCoverageRecords(List<CoverageRecord> records, Paragraph newParagraph)
        {
            int startIndex = CreateAndGetRecordIndex(records, newParagraph.StartTime.TotalMilliseconds);
            int endIndex = CreateAndGetRecordIndex(records,newParagraph.EndTime.TotalMilliseconds);
            for (int i = startIndex; i < endIndex; i++)
            {
                records[i].numberOfParagraphs++;
            }
        }

        private int FindLowestCoverage(List<CoverageRecord> records)
        {
            int min = int.MaxValue;
            foreach (CoverageRecord record in records)
            {
                if (record.numberOfParagraphs < min)
                {
                    min = record.numberOfParagraphs;
                    if (min == 0)
                    {
                        return 0;
                    }
                }
            }
            return min;
        }

        private int CreateAndGetRecordIndex(List<CoverageRecord> records, double timestamp)
        {
            CoverageRecord newRecord = new CoverageRecord(timestamp);

            int recordIndex = records.BinarySearch(newRecord, new TimestampRecordComparer());
            if (recordIndex < 0)
            {
                recordIndex = ~recordIndex;
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
                }else if (lengthComparison < 0)
                {
                    return -1;
                }

                // Calculation is (x.start - y.start) so that if x comes first, difference is < 0.
                return Math.Sign(x.StartTime.TotalMilliseconds - y.StartTime.TotalMilliseconds);
            }
        }


    }
}
