using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class MultipleReplace : IFixCommonError
    {
        public static class Language
        {
            public static string FixViaMultipleReplace { get; set; } = "Fix Via Multiple Replace Rules";
        }

        private readonly ICollection<ReplaceExpression> _replaceExpressions;
        private readonly Dictionary<string, Regex> _compiledRegExList = new Dictionary<string, Regex>();

        public MultipleReplace() => _replaceExpressions = LoadReplaceExpressions();

        private bool IsLoaded() => _replaceExpressions.Count > 0;

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            if (!IsLoaded())
            {
                return;
            }

            foreach (var paragraph in subtitle.Paragraphs)
            {
                var text = Replace(paragraph.Text);
                if (paragraph.Text != text)
                {
                    var beforeChanges = paragraph.Text;
                    paragraph.Text = text;
                    callbacks.AddFixToListView(paragraph, Language.FixViaMultipleReplace, beforeChanges, text);
                }
            }
        }

        private string Replace(string newText)
        {
            foreach (ReplaceExpression item in _replaceExpressions)
            {
                if (item.SearchType == ReplaceExpression.SearchCaseSensitive)
                {
                    if (newText.Contains(item.FindWhat))
                    {
                        newText = newText.Replace(item.FindWhat, item.ReplaceWith);
                    }
                }
                else if (item.SearchType == ReplaceExpression.SearchRegEx)
                {
                    Regex r = _compiledRegExList[item.FindWhat];
                    if (r.IsMatch(newText))
                    {
                        newText = RegexUtils.ReplaceNewLineSafe(r, newText, item.ReplaceWith);
                    }
                }
                else
                {
                    int index = newText.IndexOf(item.FindWhat, StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        do
                        {
                            newText = newText.Remove(index, item.FindWhat.Length).Insert(index, item.ReplaceWith);
                            index = newText.IndexOf(item.FindWhat, index + item.ReplaceWith.Length, StringComparison.OrdinalIgnoreCase);
                        } while (index >= 0);
                    }
                }
            }

            return newText;
        }

        private ICollection<ReplaceExpression> LoadReplaceExpressions()
        {
            var replaceExpressions = new HashSet<ReplaceExpression>();
            foreach (var group in Configuration.Settings.MultipleSearchAndReplaceGroups)
            {
                if (!group.Enabled)
                {
                    continue;
                }

                foreach (var rule in group.Rules)
                {
                    if (!rule.Enabled)
                    {
                        continue;
                    }

                    string findWhat = rule.FindWhat;
                    if (!string.IsNullOrEmpty(findWhat)) // allow space or spaces
                    {
                        string replaceWith = RegexUtils.FixNewLine(rule.ReplaceWith);
                        findWhat = RegexUtils.FixNewLine(findWhat);
                        string searchType = rule.SearchType;
                        var ruleInfo = string.IsNullOrEmpty(rule.Description) ? $"Group name: {group.Name} - Rule number: {group.Rules.IndexOf(rule) + 1}" : $"Group name: {group.Name} - Rule number: {group.Rules.IndexOf(rule) + 1}. {rule.Description}";
                        var mpi = new ReplaceExpression(findWhat, replaceWith, searchType, ruleInfo);
                        replaceExpressions.Add(mpi);
                        if (mpi.SearchType == ReplaceExpression.SearchRegEx && !_compiledRegExList.ContainsKey(findWhat))
                        {
                            _compiledRegExList.Add(findWhat, new Regex(findWhat, RegexOptions.Compiled | RegexOptions.Multiline));
                        }
                    }
                }
            }

            return replaceExpressions;
        }
    }
}