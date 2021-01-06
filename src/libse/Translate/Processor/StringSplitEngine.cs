using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Translate.Processor
{
    public class StringSplitEngine
    {
        private char[] DelimitersToSplitAfter { get; }
        private char[] DelimitersToSplitBefore { get; }
        private readonly List<IUnsplittableMarker> _unsplittableMarkers = new List<IUnsplittableMarker>();

        public StringSplitEngine(char[] delimitersToSplitAfter, char[] delimitersToSplitBefore)
        {
            DelimitersToSplitAfter = delimitersToSplitAfter;
            DelimitersToSplitBefore = delimitersToSplitBefore;
            InitUnsplittableMarkers();
        }

        public StringSplitEngine(char[] delimitersToSplitAfter) : this(delimitersToSplitAfter, new char[0])
        {
        }

        private void InitUnsplittableMarkers()
        {
            AddUnsplittableMarker(new UrlUnsplittableMarker());
            AddUnsplittableMarker(new XmlTaggedUnsplittableMarker());
        }

        public void AddUnsplittableMarker(IUnsplittableMarker unsplittableMarker)
        {
            _unsplittableMarkers.Add(unsplittableMarker);
        }

        public IEnumerable<int> FindAllSplitPositions(string text)
        {
            var unsplittablePositions = EvaluateUnsplittablePositions(text);
            for (int i = 0; i < text.Length; i++)
            {
                if (IsSplittable(text, i, unsplittablePositions))
                {
                    yield return i;
                }
            }
        }

        public static string[] SplitAt(string source, int[] positions)
        {
            string[] output = new string[positions.Length + 1];
            int pos = 0;

            for (int i = 0; i < positions.Length; pos = positions[i++])
                output[i] = source.Substring(pos, positions[i] - pos);

            output[positions.Length] = source.Substring(pos);
            return output;
        }

        public List<string> Split(string text)
        {
            var splitPositions = FindAllSplitPositions(text);
            var splits = SplitAt(text, splitPositions.ToArray()).ToList();
            splits.RemoveAll(x => x.Trim().Length == 0); //remove potential empty splits
            return splits;
        }

        public HashSet<int> EvaluateUnsplittablePositions(string text)
        {
            var unsplittablePositions = new HashSet<int>();
            foreach (var unsplittableMarker in _unsplittableMarkers)
            {
                unsplittablePositions.UnionWith(unsplittableMarker.FindUnsplittablePositions(text));
            }
            return unsplittablePositions;
        }

        private bool IsSplittable(string text, int potentialSplitPosition, HashSet<int> unsplittablePositions)
        {
            if (unsplittablePositions.Contains(potentialSplitPosition))
            {
                return false;
            }
            if (potentialSplitPosition>0 && potentialSplitPosition < text.Length  && DelimitersToSplitAfter.Contains(text[potentialSplitPosition-1]))
            {
                return true;
            }
            if (potentialSplitPosition < text.Length && DelimitersToSplitBefore.Contains(text[potentialSplitPosition ]))
            {
                return true;
            }
            return false;
        }

        public bool IsSplittable(string text, int potentialSplitPosition)
        {
            var unsplittablePositions = EvaluateUnsplittablePositions(text);
            return IsSplittable(text, potentialSplitPosition, unsplittablePositions);
        }
    }

    /// <summary>
    /// concept to define rules for StringSplitEngine to exclude specific string patterns
    /// from splitting (despite the presence of potentiality included split delimiters)
    /// </summary>
    public interface IUnsplittableMarker
    {
        HashSet<int> FindUnsplittablePositions(string text);
    }

    /**
     * marks opening & following closing xml tags together with encapsulated text as unsplittable 
     * (at the moment nested xml tags are NOT supported.)
     */
    public class XmlTaggedUnsplittableMarker : IUnsplittableMarker
    {
        private bool IsOpenTag(Match xmlTag)
        {
            return !xmlTag.Value.StartsWith("</");
        }
        private bool IsCloseTag(Match xmlTag)
        {
            return xmlTag.Value.StartsWith("</");
        }

        public HashSet<int> FindUnsplittablePositions(string text)
        {
            HashSet<int> unsplittablePositions = new HashSet<int>();
            MatchCollection matchesXmlTags = Regex.Matches(text, @"<[^>]+>"); //regex to find tags encapsulated by angle brackets
            Match openingTag = null;
            foreach (Match xmlTag in matchesXmlTags)
            {
                if (openingTag == null && IsOpenTag(xmlTag))
                {
                    openingTag = xmlTag;
                }

                if (openingTag != null && IsCloseTag(xmlTag))
                {
                    var closeTag = xmlTag;
                    for (int i = openingTag.Index +1; i < closeTag.Index + closeTag.Length; i++)
                    {
                        unsplittablePositions.Add(i);
                    }

                    openingTag = null;
                }
            }
            return unsplittablePositions;
        }
    }

    public class UrlUnsplittableMarker : IUnsplittableMarker
    {
        public HashSet<int> FindUnsplittablePositions(string text)
        {
            var unsplittablePositions = new HashSet<int>();
            var matches = Regex.Matches(text, @"\b(?:https?://|www\.)\S+\b"); //regex to find URLs
            foreach (Match match in matches)
            {
                for (int i = match.Index +1; i < match.Index + match.Length; i++)
                {
                    unsplittablePositions.Add(i);
                }
            }
            return unsplittablePositions;
        }
    }
}