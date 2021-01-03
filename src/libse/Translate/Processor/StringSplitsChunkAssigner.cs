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
            public List<int> BreakPositions;
        }

        public AbstractStringSplitsChunkAssigner(StringSplitEngine stringSplitEngine)
        {
            StringSplitEngine = stringSplitEngine;
        }

        public abstract List<int> GetSplitPositions(List<int> sourceChunksTextLength, string targetText);

        /// <summary>
        /// assess how good the targetBreakPositions are. the smaller the Fitness-value the better is the result.
        /// </summary>
        public static RateResult CalculateRateResult(List<int> sourceChunksTextLength, string targetText, List<int> targetBreakPositions)
        {
            string[] targetChunks = StringSplitEngine.SplitAt(targetText, targetBreakPositions);
            List<int> targetChunksTextLength = targetChunks.ToList().ConvertAll(x => x.Length);

            if (sourceChunksTextLength.Count != targetChunksTextLength.Count)
            {
                throw new Exception("sourceChunksTextLength and targetChunksTextLength must be the same length");
            }

            int overallSourceLength = sourceChunksTextLength.Sum();
            int overallTargetLength = targetChunksTextLength.Sum();

            double overallPercentageDifferenceSum = 0;
            for (int i = 0; i < sourceChunksTextLength.Count; i++)
            {
                double sourceChunkTextLength = (double)sourceChunksTextLength[i];
                double currentSourceChunkPercentageLength = sourceChunkTextLength / overallSourceLength;
                double targetChunkTextLength = (double)targetChunksTextLength[i];
                double currentTargetChunkPercentageLength = targetChunkTextLength / overallTargetLength;

                double currentPercentageDifference = Math.Abs(currentTargetChunkPercentageLength - currentSourceChunkPercentageLength);
                overallPercentageDifferenceSum += currentPercentageDifference;
            }

            return new RateResult { Fitness = overallPercentageDifferenceSum, BreakPositions = targetBreakPositions };
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

        public override List<int> GetSplitPositions(List<int> sourceChunksTextLength, string targetText)
        {
            int overallSourceLength = sourceChunksTextLength.Sum();

            int currentSentenceParagraph = 0;
            int currentSourceChunkEndPosition = sourceChunksTextLength[0];
            var splitPositions = new List<int>();
            for (int i = 0; i < targetText.Length; i++)
            {
                if (StringSplitEngine.IsSplittable(targetText,i))
                {
                    double currentTargetPositionPercentage = (double)i / targetText.Length;
                    double currentSourceChunkEndPositionPercentage = (double)currentSourceChunkEndPosition / overallSourceLength;
                    if (currentTargetPositionPercentage > currentSourceChunkEndPositionPercentage)
                    {
                        currentSentenceParagraph++;
                        currentSourceChunkEndPosition += sourceChunksTextLength[currentSentenceParagraph];

                        splitPositions.Add(i);
                    }
                }
            }

            //ensure that there is always to correct amount of resulting splitPositions
            while (splitPositions.Count < sourceChunksTextLength.Count - 1)
            {
                splitPositions.Add(splitPositions.Count>0 ? splitPositions.Last() : 0);
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
        public const int DefaultIterations = 1000;
        private readonly Random _randomizer = new Random();
        private List<int> _sourceChunksTextLength;
        private List<string> _targetSplits;
        private List<int> _targetSplitsLength;
        private readonly int _iterations;

        public RandomizedStringSplitsChunkAssigner(StringSplitEngine stringSplitEngine, int iterations = DefaultIterations) : base(stringSplitEngine)
        {
            _iterations = iterations;
        }

        public override List<int> GetSplitPositions(List<int> sourceChunksTextLength, string targetText)
        {
            _sourceChunksTextLength = sourceChunksTextLength;
            _targetSplits = StringSplitEngine.Split(targetText);
            _targetSplitsLength = _targetSplits.ConvertAll(x => x.Length);

            List<RateResult> rateResults = new List<RateResult>();
            for (int i = 0; i < _iterations; i++)
            {
                List<int> breakPositions = GetRandomBreakPositions(targetText, _sourceChunksTextLength);

                var rateResult  = CalculateRateResult(_sourceChunksTextLength, targetText, breakPositions);
                rateResults.Add(rateResult);
            }
            rateResults = rateResults.OrderBy(x => x.Fitness).ToList();
            return rateResults.First().BreakPositions;
        }

        private List<int> GetRandomBreakPositions(string targetText, List<int> sourceChunksTextLength)
        {
            List<int> splitPositions = new List<int>();

            for (int i = 0; i < sourceChunksTextLength.Count - 1; i++)
            {
                int targetBreakSplitIndex = _randomizer.Next(0, _targetSplitsLength.Count);
                int splitPosition = _targetSplitsLength.GetRange(0, targetBreakSplitIndex).Sum();
                splitPositions.Add(splitPosition);
            }
            splitPositions.Sort();
            return splitPositions;
        }
    }

}
