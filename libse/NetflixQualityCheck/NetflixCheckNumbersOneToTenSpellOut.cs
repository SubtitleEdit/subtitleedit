﻿using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckNumbersOneToTenSpellOut : INetflixQualityChecker
    {
        private static readonly Regex NumberOneToNine = new Regex(@"\b\d\b", RegexOptions.Compiled);
        private static readonly Regex NumberTen = new Regex(@"\b10\b", RegexOptions.Compiled);

        /// <summary>
        /// From 1 to 10, numbers should be written out: one, two, three, etc.
        /// </summary>
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string newText = p.Text;
                var m = NumberOneToNine.Match(newText);
                while (m.Success)
                {
                    bool ok = newText.Length > m.Index + 1 && !":.".Contains(newText[m.Index + 1].ToString()) || newText.Length <= m.Index + 1;
                    if (!ok && newText.Length > m.Index + 1)
                    {
                        var rest = newText.Substring(m.Index + 1);
                        if (rest == "." || rest == "?" || rest == "!" ||
                            rest == ".</i>" || rest == "?</i>" || rest == "!</i>" ||
                            rest == "." + Environment.NewLine || rest == "?" + Environment.NewLine || rest == "!" + Environment.NewLine ||
                            rest == ".</i>" + Environment.NewLine || rest == "?</i>" + Environment.NewLine || rest == "!</i>" + Environment.NewLine)
                        {
                            ok = true;
                        }
                    }
                    if (ok && m.Index > 0 && ":.".Contains(newText[m.Index - 1].ToString()))
                        ok = false;
                    if (ok)
                        newText = newText.Remove(m.Index, 1).Insert(m.Index, NetflixHelper.ConvertNumberToString(m.Value.Substring(0, 1), false, controller.Language));
                    m = NumberOneToNine.Match(newText, m.Index + 1);
                }

                m = NumberTen.Match(newText);
                while (m.Success)
                {
                    bool ok = newText.Length > m.Index + 2 && newText[m.Index + 2] != ':' || newText.Length <= m.Index + 2;
                    if (ok && m.Index > 0 && ":.".Contains(newText[m.Index - 1].ToString()))
                        ok = false;
                    if (ok)
                        newText = newText.Remove(m.Index, 2).Insert(m.Index, "ten");
                    m = NumberTen.Match(newText, m.Index + 1);
                }

                if (newText != p.Text)
                {
                    var fixedParagraph = new Paragraph(p, false) { Text = newText };
                    string comment = "From 1 to 10, numbers should be written out: one, two, three, etc";
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }

    }
}
