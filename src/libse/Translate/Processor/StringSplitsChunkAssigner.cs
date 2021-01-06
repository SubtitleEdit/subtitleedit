using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Translate.Processor
{
    /// <summary>
    /// breaks up a (target) string and assigns the resulting individual components
    /// proportionally to the length of predefined (source) chunks.
    ///
    /// brummochse: I am not a pro in theoretical informatics, but I assume it is a NP-completeness problem to find the best suitable solution
    /// </summary>
    public abstract class AbstractStringSplitsChunkAssigner
    {
        protected StringSplitEngine StringSplitEngine;
        
        public class RateResult
        {
            public double Fitness;
            public int[] BreakPositions;
        }

        public AbstractStringSplitsChunkAssigner(StringSplitEngine stringSplitEngine)
        {
            StringSplitEngine = stringSplitEngine;
        }

        public abstract int[] GetSplitPositions(List<int> sourceChunksTextLength, string targetText);

        /// <summary>
        /// assess how good the targetBreakPositions are. the smaller the Fitness-value the better is the result.
        /// </summary>
        public static RateResult CalculateRateResult(List<int> sourceChunksTextLength, int targetTextLength, int[] targetBreakPositions)
        {
            var targetChunksTextLength = EvaluateTargetChunksTextLength(sourceChunksTextLength, targetTextLength, targetBreakPositions);
            int sourceTextLength = sourceChunksTextLength.Sum();

            double overallPercentageDifferenceSum = 0;
            for (int i = 0; i < sourceChunksTextLength.Count; i++)
            {
                double sourceChunkTextLength = sourceChunksTextLength[i];
                double currentSourceChunkPercentageLength = sourceChunkTextLength / sourceTextLength;
                double targetChunkTextLength = targetChunksTextLength[i];
                double currentTargetChunkPercentageLength = targetChunkTextLength / targetTextLength;

                double currentPercentageDifference = Math.Abs(currentTargetChunkPercentageLength - currentSourceChunkPercentageLength);
                overallPercentageDifferenceSum += currentPercentageDifference;
            }

            return new RateResult { Fitness = overallPercentageDifferenceSum, BreakPositions = targetBreakPositions };
        }

        private static int[] EvaluateTargetChunksTextLength(List<int> sourceChunksTextLength, int targetTextLength, int[] targetBreakPositions)
        {
            int[] targetChunksTextLength = new int[sourceChunksTextLength.Count];
            int lastBreakPosition;
            for (int i = 0; i < targetBreakPositions.Length; i++)
            {
                var currentBreakPosition = targetBreakPositions[i];
                lastBreakPosition = (i > 0) ? targetBreakPositions[i - 1] : 0;
                targetChunksTextLength[i] = currentBreakPosition - lastBreakPosition;
            }

            lastBreakPosition = ((targetBreakPositions.Length > 0) ? targetBreakPositions[targetBreakPositions.Length - 1] : 0);
            targetChunksTextLength[sourceChunksTextLength.Count - 1] = targetTextLength - lastBreakPosition;
            return targetChunksTextLength;
        }
    }

    /// <summary>
    /// Simple approach, that assign the individual components of the target string to the chunks
    /// one after the other and checks after the allocation whether this chunk is already disproportionately served.
    /// </summary>
    public class GreedyStringSplitsChunkAssigner : AbstractStringSplitsChunkAssigner
    {
        public GreedyStringSplitsChunkAssigner(StringSplitEngine stringSplitEngine) : base(stringSplitEngine)
        {
        }

        public override int[] GetSplitPositions(List<int> sourceChunksTextLength, string targetText)
        {
            int overallSourceLength = sourceChunksTextLength.Sum();

            int currentSentenceParagraph = 0;
            int currentSourceChunkEndPosition = sourceChunksTextLength[0];
            int[] splitPositions = new int[sourceChunksTextLength.Count-1];
            int splitPositionCount = 0;
            for (int i = 0; i < targetText.Length; i++)
            {
                var c = targetText[i];

                if (StringSplitEngine.IsSplittable(targetText,i))
                {
                    double currentTargetPositionPercentage = (double)i / targetText.Length;
                    double currentSourceChunkEndPositionPercentage = (double)currentSourceChunkEndPosition / overallSourceLength;
                    if (currentTargetPositionPercentage > currentSourceChunkEndPositionPercentage)
                    {
                        currentSentenceParagraph++;
                        currentSourceChunkEndPosition += sourceChunksTextLength[currentSentenceParagraph];

                        splitPositions[splitPositionCount] = i;
                        splitPositionCount++;

                    }
                }
            }

            //ensure that there is always to correct amount of resulting splitPositions
            while (splitPositionCount < sourceChunksTextLength.Count - 1)
            {
                splitPositions[splitPositionCount] = splitPositionCount > 0 ? splitPositions[splitPositionCount-1] : 0;
                splitPositionCount++;
            }
            return splitPositions;
        }
    }

    /// <summary>
    /// Uses random diced break positions (of which the best ones are returned at the end as solution).
    /// </summary>
    public class RandomizedStringSplitsChunkAssigner : AbstractStringSplitsChunkAssigner
    {
        //how many times the dice is rolled to find a good random assignment
        public const int DefaultIterations = 10000;
        private readonly Random _randomizer = new Random();
        private List<int> _sourceChunksTextLength;
        private List<string> _targetSplits;
        private List<int> _targetSplitsLength;
        private readonly int _iterations;

        public RandomizedStringSplitsChunkAssigner(StringSplitEngine stringSplitEngine, int iterations = DefaultIterations) : base(stringSplitEngine)
        {
            _iterations = iterations;
        }

        public override int[] GetSplitPositions(List<int> sourceChunksTextLength, string targetText)
        {
            _sourceChunksTextLength = sourceChunksTextLength;
            _targetSplits = StringSplitEngine.Split(targetText);
            _targetSplitsLength = _targetSplits.ConvertAll(x => x.Length);

            var rateResults = new List<RateResult>();
            for (int i = 0; i < _iterations; i++)
            {
                int[] breakPositions = GetRandomBreakPositions(targetText, _sourceChunksTextLength);

                var rateResult  = CalculateRateResult(_sourceChunksTextLength, targetText.Length, breakPositions);
                rateResults.Add(rateResult);
            }
            rateResults = rateResults.OrderBy(x => x.Fitness).ToList();
            return rateResults.First().BreakPositions;
        }

        private int[] GetRandomBreakPositions(string targetText, List<int> sourceChunksTextLength)
        {
            int [] splitPositions = new int[sourceChunksTextLength.Count-1];
            

            for (int i = 0; i < sourceChunksTextLength.Count - 1; i++)
            {
                int targetBreakSplitIndex = _randomizer.Next(0, _targetSplitsLength.Count);
                int splitPosition = _targetSplitsLength.GetRange(0, targetBreakSplitIndex).Sum();
                splitPositions[i]=splitPosition;
            }
            Array.Sort(splitPositions);
            return splitPositions;
        }
    }

}
