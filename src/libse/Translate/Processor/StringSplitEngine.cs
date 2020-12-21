using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Nikse.SubtitleEdit.Core.NetflixQualityCheck;

namespace Nikse.SubtitleEdit.Core.Translate.Processor
{
    public interface IInseparableMarker
    {
        HashSet<int> FindInseparablePositions(string text);
    }

    /**
     * at the moments no nested xml tags are supported.
     */
    public class XmlTaggedInseparableMarker : IInseparableMarker
    {
        private bool IsOpenTag(Match xmlTag)
        {
            return !xmlTag.Value.StartsWith("</");
        }
        private bool IsCloseTag(Match xmlTag)
        {
            return xmlTag.Value.StartsWith("</");
        }

        public HashSet<int> FindInseparablePositions(string text)
        {
            HashSet<int> inseparablePositions = new HashSet<int>();
            MatchCollection matchesXmlTags = Regex.Matches(text, @"<[^>]+>");
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
                    for (int i = openingTag.Index; i < closeTag.Index + closeTag.Length; i++)
                    {
                        inseparablePositions.Add(i);
                    }

                    openingTag = null;
                }
            }
            return inseparablePositions;

        }
    }

    public class UrlInseparableMarker : IInseparableMarker
    {
        public HashSet<int> FindInseparablePositions(string text)
        {
            HashSet<int> inseparablePositions = new HashSet<int>();
            MatchCollection matches = Regex.Matches(text, @"\b(?:https?://|www\.)\S+\b");
            foreach (Match match in matches)
            {
                for (int i = match.Index; i < match.Index + match.Length; i++)
                {
                    inseparablePositions.Add(i);
                }
            }
            return inseparablePositions;
        }
    }


    public class StringSplitEngine
    {
        private readonly char[] _delimiters;
        private List<IInseparableMarker> _inseparableMarkers = new List<IInseparableMarker>();

        public StringSplitEngine(char[] delimiters)
        {
            this._delimiters = delimiters;
            AddInseparableMarker(new UrlInseparableMarker());
            AddInseparableMarker(new XmlTaggedInseparableMarker());
        }

        public void AddInseparableMarker(IInseparableMarker inseparableMarker)
        {
            _inseparableMarkers.Add(inseparableMarker);
        }

        public IEnumerable<int> FindAllPositionsOf(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (_delimiters.Contains(text[i]))
                {
                    yield return i;
                }
            }
        }

        public static string[] SplitAt(string source, List<int> positions)
        {
            string[] output = new string[positions.Count + 1];
            int pos = 0;

            for (int i = 0; i < positions.Count; pos = positions[i++])
                output[i] = source.Substring(pos, positions[i] - pos);

            output[positions.Count] = source.Substring(pos);
            return output;
        }

        public List<string> split(string text)
        {
            var splitPositions = FindAllPositionsOf(text);
            var inseparablePositions = EvaluateInseparablePositions(text);
            splitPositions =splitPositions.Except(inseparablePositions);

            List<string> sentenceChunks = SplitAt(text, splitPositions.ToList()).ToList();
            sentenceChunks.RemoveAll(x => x.Trim().Length == 0); //remove empty chunks

            return sentenceChunks;
        }

        public HashSet<int> EvaluateInseparablePositions(string text)
        {
            HashSet<int> inseparablePositions = new HashSet<int>();
            foreach (var inseparableMarker in _inseparableMarkers)
            {
                inseparablePositions.UnionWith(inseparableMarker.FindInseparablePositions(text));
            }
            return inseparablePositions;
        }
    }
}