using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class ActorConverter
    {
        public const int NormalCase = 0;
        public const int UpperCase = 1;
        public const int LowerCase = 2;
        public const int ProperCase = 3;

        private SubtitleFormat _subtitleFormat;
        private string _languageCode;

        private NameList _namesList;
        private List<string> _nameListInclMulti;

        public bool ToSquare { get; set; }
        public bool ToParentheses { get; set; }
        public bool ToColon { get; set; }
        public bool ToActor { get; set; }


        public ActorConverter(SubtitleFormat subtitleFormat, string languageCode)
        {
            _subtitleFormat = subtitleFormat;
            _languageCode = languageCode;
            _namesList = new NameList(Configuration.DictionariesDirectory, languageCode, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
            _nameListInclMulti = _namesList.GetAllNames();
        }

        public string FixActorsFromActor(Paragraph p, int? changeCasing, SKColor? color)
        {
            var actor = p.Actor;
            if (changeCasing.HasValue)
            {
                actor = SetCasing(_subtitleFormat, changeCasing, actor);
            }

            if (ToSquare)
            {
                actor = "[" + actor + "]";
            }
            else if (ToParentheses)
            {
                actor = "(" + actor + ")";
            }
            else if (ToColon)
            {
                actor = actor + ":";
            }
            else if (ToActor)
            {
                return p.Text;
            }

            if (color.HasValue && !ToActor)
            {
                actor = SetColor(_subtitleFormat, color.Value, actor);
            }

            p.Text = actor + " " + p.Text.TrimStart(' ');

            return p.Text;
        }

        public string FixActorsFromBeforeColon(Paragraph p, char ch, int? changeCasing, SKColor? color)
        {
            var sb = new StringBuilder();
            foreach (var line in p.Text.SplitToLines())
            {
                var s = line.Trim();
                var startIdx = line.IndexOf(ch);
                if (startIdx > 0)
                {
                    var actor = s.Substring(0, startIdx).Trim(' ', '-', '"');
                    if (changeCasing.HasValue)
                    {
                        actor = SetCasing(_subtitleFormat, changeCasing, actor);
                    }

                    if (ToSquare)
                    {
                        actor = "[" + actor + "]";
                    }
                    else if (ToParentheses)
                    {
                        actor = "(" + actor + ")";
                    }
                    else if (ToColon)
                    {
                        actor = actor + ":";
                    }
                    else if (ToActor)
                    {
                    }

                    if (color.HasValue && !ToActor)
                    {
                        SetColor(_subtitleFormat, color.Value, actor);
                    }

                    if (ToSquare)
                    {
                        s = actor + " " + s.Substring(startIdx + 1).TrimStart(' ');
                    }
                    else if (ToParentheses)
                    {
                        s = actor + " " + s.Substring(startIdx + 1).TrimStart(' ');
                    }
                    else if (ToColon)
                    {
                        s = actor + " " + s.Substring(startIdx + 1).TrimStart(' ');
                    }
                    else if (ToActor)
                    {
                        s = s.Substring(startIdx + 1);
                    }
                }

                sb.AppendLine(s);
            }

            return sb.ToString().Trim();
        }

        public ActorConverterResult FixActors(Paragraph paragraph, char start, char end, int? changeCasing, SKColor? color)
        {
            var p = new Paragraph(paragraph, false);
            Paragraph nextParagraph = null;
            var lines = p.Text.SplitToLines();
            if (lines.Count > 2)
            {
                return new ActorConverterResult { Paragraph = paragraph, Skip = true };
            }

            var lineIdx = 0;
            p.Text = string.Empty;
            var selectFix = true;
            foreach (var line in lines)
            {
                var s = line;
                var startIdx = s.IndexOf(start);
                var endIdx = s.IndexOf(end);
                if (startIdx != -1 && endIdx != -1)
                {
                    if (endIdx < startIdx)
                    {
                        break;
                    }

                    var actor = s.Substring(startIdx + 1, endIdx - startIdx - 1).Trim(' ', '-', '"');
                    selectFix = IsActor(actor);
                    if (changeCasing.HasValue)
                    {
                        actor = SetCasing(_subtitleFormat, changeCasing, actor);
                    }

                    if (ToSquare)
                    {
                        actor = "[" + actor + "]";
                    }
                    else if (ToParentheses)
                    {
                        actor = "(" + actor + ")";
                    }
                    else if (ToColon)
                    {
                        actor = actor + ":";
                    }
                    else if (ToActor)
                    {
                        s = s.Substring(0, startIdx) + s.Substring(endIdx + 1).Trim();
                    }

                    if (color.HasValue && !ToActor)
                    {
                        actor = SetColor(_subtitleFormat, color.Value, actor);
                    }

                    if (ToSquare)
                    {
                        s = s.Substring(0, startIdx) + actor + " " + s.Substring(endIdx + 1).TrimStart(' ');
                    }
                    else if (ToParentheses)
                    {
                        s = s.Substring(0, startIdx) + actor + " " + s.Substring(endIdx + 1).TrimStart(' ');
                    }
                    else if (ToColon)
                    {
                        s = s.Substring(0, startIdx) + actor + " " + s.Substring(endIdx + 1).TrimStart(' ');
                    }

                    if (lineIdx == 0)
                    {
                        if (ToActor)
                        {
                            p.Actor = actor;
                        }

                        p.Text = s;
                    }
                    else if (lineIdx == 1 && ToActor)
                    {
                        nextParagraph = new Paragraph(p);
                        nextParagraph.Text = s.Trim();
                        nextParagraph.Actor = actor;
                    }
                    else if (lineIdx == 1)
                    {
                        p.Text += Environment.NewLine + s.Trim();
                    }

                }
                else
                {
                    p.Text = (p.Text + Environment.NewLine + s).Trim();
                }

                lineIdx++;
            }

            return new ActorConverterResult
            {
                Paragraph = p,
                NextParagraph = nextParagraph,
                Selected = selectFix,
            };
        }

        private static string SetCasing(SubtitleFormat format, int? changeCasing, string actor)
        {
            switch (changeCasing.Value)
            {
                case NormalCase:
                    actor = actor.ToLower().CapitalizeFirstLetter();
                    break;
                case UpperCase:
                    actor = actor.ToUpper();
                    break;
                case LowerCase:
                    actor = actor.ToLower();
                    break;
                case ProperCase:
                    actor = actor.ToProperCase(format);
                    break;
            }

            return actor;
        }

        private static string SetColor(SubtitleFormat format, SKColor color, string actor)
        {
            if (format.FriendlyName == AdvancedSubStationAlpha.NameOfFormat)
            {
                actor = "{\\" + AdvancedSubStationAlpha.GetSsaColorStringForEvent(color, "c") + "}" + actor + "{\\c}";
            }
            else
            {
                actor = "<font color=\"" + Settings.Settings.ToHtml(color) + "\">" + actor + "</font>";
            }

            return actor;
        }

        private bool IsActor(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            var words = s.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                return false;
            }

            if (_nameListInclMulti.Contains(s, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            foreach (var word in words)
            {
                if (word.Length < 2)
                {
                    return false;
                }

                if (word.Any(c => char.IsDigit(c) || (!char.IsLetter(c) && c != '-' && c != '\'')))
                {
                    return false;
                }

                var commonTitles = new[] { "Mr.", "Mrs.", "Dr.", };
                if (commonTitles.Contains(word))
                {
                    continue;
                }

                if (!_nameListInclMulti.Contains(word, StringComparer.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
