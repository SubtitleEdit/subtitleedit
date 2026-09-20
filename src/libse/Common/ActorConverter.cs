using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private HashSet<string> _nameListInclMulti;

        public bool ToSquare { get; set; }
        public bool ToParentheses { get; set; }
        public bool ToColon { get; set; }
        public bool ToActor { get; set; }


        public ActorConverter(SubtitleFormat subtitleFormat, string languageCode)
        {
            _subtitleFormat = subtitleFormat;
            _languageCode = languageCode;
            _namesList = new NameList(Configuration.DictionariesDirectory, languageCode, false, string.Empty);
            _nameListInclMulti = new HashSet<string>(_namesList.GetAllNames(), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Moves the actor column into the text. The converted paragraph is returned in the result -
        /// the actor column is cleared, as the name now lives in the text and would otherwise be
        /// written twice (#14077).
        /// </summary>
        public ActorConverterResult FixActorsFromActor(Paragraph paragraph, int? changeCasing, SKColor? color)
        {
            var p = new Paragraph(paragraph, false);
            if (ToActor)
            {
                return new ActorConverterResult { Paragraph = p, Selected = true };
            }

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

            if (color.HasValue)
            {
                actor = SetColor(_subtitleFormat, color.Value, actor);
            }

            p.Text = actor + " " + p.Text.TrimStart(' ');
            p.Actor = string.Empty;

            return new ActorConverterResult { Paragraph = p, Selected = true };
        }

        /// <summary>
        /// Converts "Actor: text" lines. The converted paragraph is returned in the result: converting
        /// to the actor column writes <see cref="Paragraph.Actor"/> from whichever line carries the
        /// name, and a second speaker in the same paragraph becomes
        /// <see cref="ActorConverterResult.NextParagraph"/> - the same shape <see cref="FixActors"/>
        /// returns for the bracket formats (#14077).
        /// </summary>
        public ActorConverterResult FixActorsFromBeforeColon(Paragraph paragraph, char ch, int? changeCasing, SKColor? color)
        {
            var p = new Paragraph(paragraph, false);
            var lines = p.Text.SplitToLines();

            // Only one extra paragraph can be split off, so a third speaker would be lost.
            if (ToActor && lines.Count(line => HasActor(line, ch)) > 2)
            {
                return new ActorConverterResult { Paragraph = paragraph, Skip = true };
            }

            Paragraph nextParagraph = null;
            var selectFix = true;
            var actorAssigned = false;
            var textLines = new List<string>();
            foreach (var line in lines)
            {
                // index into the trimmed line - leading whitespace goes with the actor it precedes
                var s = line.Trim();
                var startIdx = s.IndexOf(ch);
                if (startIdx <= 0)
                {
                    // A line without an actor belongs to the paragraph the previous line went to.
                    if (nextParagraph != null)
                    {
                        nextParagraph.Text = (nextParagraph.Text + Environment.NewLine + s).Trim();
                    }
                    else
                    {
                        textLines.Add(s);
                    }

                    continue;
                }

                var actor = s.Substring(0, startIdx).Trim(' ', '-', '"');
                // AND, not assignment: selectFix is declared outside the per-line loop, so a later line's valid speaker used to mask an earlier line whose "actor" was really a clock ("It's 12:30 now.") - pre-checking a fix that destroys text.
                selectFix &= IsActor(actor);
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

                if (color.HasValue && !ToActor)
                {
                    actor = SetColor(_subtitleFormat, color.Value, actor);
                }

                var text = s.Substring(startIdx + 1).TrimStart(' ');
                if (!ToActor)
                {
                    textLines.Add(actor + " " + text);
                }
                else if (!actorAssigned)
                {
                    // The first name found goes in the actor column...
                    p.Actor = actor;
                    actorAssigned = true;
                    textLines.Add(text);
                }
                else
                {
                    // ...a second one needs a paragraph of its own, as the column holds one name.
                    nextParagraph = new Paragraph(p) { Text = text, Actor = actor };
                }
            }

            p.Text = string.Join(Environment.NewLine, textLines).Trim();

            return new ActorConverterResult
            {
                Paragraph = p,
                NextParagraph = nextParagraph,
                Selected = selectFix,
            };
        }

        private static bool HasActor(string line, char ch)
        {
            return line.Trim().IndexOf(ch) > 0;
        }

        /// <summary>
        /// A line splitter can break a tag in half - "Princess Peach on (Speaker\n2) Super Smash
        /// Bros." - and the per-line scan in <see cref="FixActors"/> then finds no complete pair on
        /// either line, so the paragraph was silently left alone. Line breaks inside a bracket pair
        /// are folded to a space, pulling the two half-lines together. Capped at name length: a
        /// bracket pair spanning that much text is a parenthetical remark, not a speaker tag.
        /// </summary>
        private static string JoinTagBrokenOverLineBreak(string text, char start, char end)
        {
            const int maxTagLength = 50;

            var startIdx = text.IndexOf(start);
            while (startIdx >= 0)
            {
                var endIdx = text.IndexOf(end, startIdx + 1);
                if (endIdx < 0)
                {
                    break;
                }

                var inner = text.Substring(startIdx + 1, endIdx - startIdx - 1);
                if (inner.Length <= maxTagLength &&
                    inner.IndexOf(start) < 0 &&
                    (inner.Contains('\n') || inner.Contains('\r')))
                {
                    var joined = string.Join(" ", inner.Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
                    text = text.Substring(0, startIdx + 1) + joined + text.Substring(endIdx);
                    endIdx = startIdx + 1 + joined.Length;
                }

                startIdx = text.IndexOf(start, endIdx + 1);
            }

            return text;
        }

        public ActorConverterResult FixActors(Paragraph paragraph, char start, char end, int? changeCasing, SKColor? color)
        {
            var p = new Paragraph(paragraph, false);
            Paragraph nextParagraph = null;
            p.Text = JoinTagBrokenOverLineBreak(p.Text, start, end);
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

                // A closing bracket before the opening one is not an actor - the line is kept as it
                // is. Giving up on the whole paragraph here dropped this line and every line after
                // it from the text.
                if (startIdx != -1 && endIdx > startIdx)
                {
                    var actor = s.Substring(startIdx + 1, endIdx - startIdx - 1).Trim(' ', '-', '"');
                    // AND, not assignment: selectFix is declared outside the per-line loop, so a later line's valid speaker used to mask an earlier line whose "actor" was really a clock ("It's 12:30 now.") - pre-checking a fix that destroys text.
                    selectFix &= IsActor(actor);
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
                        if (string.IsNullOrEmpty(p.Actor))
                        {
                            // Only the second line names a speaker, so it belongs to this paragraph -
                            // splitting off a paragraph with no actor at all would leave the name
                            // nowhere (#14077).
                            p.Actor = actor;
                            p.Text = (p.Text + Environment.NewLine + s.Trim()).Trim();
                        }
                        else
                        {
                            nextParagraph = new Paragraph(p);
                            nextParagraph.Text = s.Trim();
                            nextParagraph.Actor = actor;
                        }
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

        private static readonly string[] CommonTitles = { "Mr.", "Mrs.", "Dr." };

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

            if (_nameListInclMulti.Contains(s))
            {
                return true;
            }

            foreach (var word in words)
            {
                if (word.Length < 2)
                {
                    return false;
                }

                if (CommonTitles.Contains(word))
                {
                    continue;
                }

                if (word.Any(c => char.IsDigit(c) || (!char.IsLetter(c) && c != '-' && c != '\'')))
                {
                    return false;
                }

                if (!_nameListInclMulti.Contains(word))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
