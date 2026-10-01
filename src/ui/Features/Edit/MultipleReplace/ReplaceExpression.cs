using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class ReplaceExpression
    {
        public const int SearchNormal = 0;
        public const int SearchRegEx = 1;
        public const int SearchCaseSensitive = 2;

        public const string SearchTypeNormal = "Normal";
        public const string SearchTypeCaseSensitive = "CaseSensitive";
        public const string SearchTypeRegularExpression = "RegularExpression";

        public string FindWhat { get; set; }
        public string ReplaceWith { get; set; }
        public int SearchType { get; set; }
        public string RuleInfo { get; set; }
        public RuleTreeNode? RuleTreeNode { get; set; }

        /// <summary>
        /// Set for a "Normal" or "CaseSensitive" rule with "Whole word" ticked: the find text,
        /// escaped and wrapped in word boundaries, honouring the rule's case setting (#15510).
        /// </summary>
        public Regex? WholeWordRegex { get; set; }

        public ReplaceExpression(string findWhat, string replaceWith, string searchType, string ruleInfo)
        {
            FindWhat = findWhat;
            ReplaceWith = replaceWith;
            RuleInfo = ruleInfo;
            if (string.CompareOrdinal(searchType, SearchTypeRegularExpression) == 0)
            {
                SearchType = SearchRegEx;
            }
            else if (string.CompareOrdinal(searchType, SearchTypeCaseSensitive) == 0)
            {
                SearchType = SearchCaseSensitive;
            }
        }
    }
}
