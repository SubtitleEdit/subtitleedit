using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixContinuationStyle : IFixCommonError
    {
        private ContinuationUtilities.ContinuationProfile continuationProfile = null;
        private List<string> names = null;

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = string.Format(language.FixContinuationStyleX, ContinuationUtilities.GetContinuationStyleName(Configuration.Settings.General.ContinuationStyle));
            int fixCount = 0;

            // Check continuation profile
            if (this.continuationProfile == null)
            {
                this.SetContinuationProfile(Configuration.Settings.General.ContinuationStyle);
            }

            // Prepare variables
            string addEnd = (this.continuationProfile.SuffixAddSpace ? " " : "") + this.continuationProfile.Suffix;
            string addStart = this.continuationProfile.Prefix + (this.continuationProfile.PrefixAddSpace ? " " : "");
            bool replaceComma = this.continuationProfile.SuffixReplaceComma;

            int minGapMs = ContinuationUtilities.GetMinimumGapMs();

            string gapAddEnd = addEnd;
            string gapAddStart = addStart;
            bool gapReplaceComma = replaceComma;

            if (this.continuationProfile.UseDifferentStyleGap)
            {
                gapAddEnd = (this.continuationProfile.GapSuffixAddSpace ? " " : "") + this.continuationProfile.GapSuffix;
                gapAddStart = this.continuationProfile.GapPrefix + (this.continuationProfile.GapPrefixAddSpace ? " " : "");
                gapReplaceComma = this.continuationProfile.GapSuffixReplaceComma;
            }

            bool inSentence = false;
            bool? inItalicSentence = null;

            // Loop paragraphs
            for (int i = 0; i < subtitle.Paragraphs.Count - 1; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                Paragraph pNext = subtitle.Paragraphs[i + 1];
                var oldText = p.Text;
                var oldTextNext = pNext.Text;
                var text = ContinuationUtilities.SanitizeString(p.Text);
                var textNext = ContinuationUtilities.SanitizeString(pNext.Text);
                var isChecked = true;

                // Check if we should fix this paragraph
                if (ShouldFixParagraph(text))
                {
                    // If ends with nothing...
                    if (!ContinuationUtilities.IsEndOfSentence(text))
                    {
                        // ...ignore inserts
                        if (Configuration.Settings.General.FixContinuationStyleUncheckInsertsAllCaps)
                        {
                            if (ContinuationUtilities.IsAllCaps(text))
                            {
                                isChecked = false;
                            }
                        }

                        // ...and italic lyrics    
                        if (Configuration.Settings.General.FixContinuationStyleUncheckInsertsItalic)
                        {
                            if (ContinuationUtilities.IsItalic(oldText) && !ContinuationUtilities.IsNewSentence(text) && inItalicSentence == false)
                            {
                                isChecked = false;
                            }
                        }

                        // ...and smallcaps inserts or non-italic lyrics
                        if (Configuration.Settings.General.FixContinuationStyleUncheckInsertsLowercase)
                        {
                            if (!ContinuationUtilities.IsNewSentence(text) && !inSentence)
                            {
                                isChecked = false;
                            }
                        }
                    }


                    // Remove any previous suffixes from first paragraph
                    var textWithoutSuffix = text;
                    foreach (string suffix in ContinuationUtilities.Suffixes.Union(new List<string> { "," }))
                    {
                        if (textWithoutSuffix.EndsWith(suffix)) textWithoutSuffix = textWithoutSuffix.Substring(0, textWithoutSuffix.Length - suffix.Length);
                    }
                    textWithoutSuffix = textWithoutSuffix.Trim();

                    // Remove any previous prefixes from second paragraph
                    var textNextWithoutPrefix = textNext;
                    foreach (string prefix in ContinuationUtilities.Prefixes)
                    {
                        if (textNextWithoutPrefix.StartsWith(prefix)) textNextWithoutPrefix = textNextWithoutPrefix.Substring(prefix.Length);
                    }
                    textNextWithoutPrefix = textNextWithoutPrefix.Trim();

                    // Get last word of this paragraph                    
                    string lastWord = text.Split(Convert.ToChar(" ")).Last();
                    string newLastWord = textWithoutSuffix.Split(Convert.ToChar(" ")).Last().Trim();

                    // Get first word of the next paragraph
                    string firstWord = textNext.Split(Convert.ToChar(" ")).First();
                    string newFirstWord = textNextWithoutPrefix.Split(Convert.ToChar(" ")).First().Trim();

                    // Detect gap
                    bool gap = pNext.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds > minGapMs;

                    
                    // If ends with dots (possible interruptions), check if next sentence is new sentence, otherwise don't check by default
                    if (text.EndsWith("..") || text.EndsWith("…"))
                    {
                        if (!HasPrefix(textNext) && ContinuationUtilities.IsNewSentence(textNext))
                        {
                            isChecked = false;

                            // If set, we'll hide interruption continuation candidates that don't start with a name,
                            // to prevent clogging up the window with a lot of unchecked items.
                            // For example, a candidate we DO want to list:  But wait...  Marty is still there!
                            if (Configuration.Settings.General.FixContinuationStyleHideInterruptionContinuationCandidatesWithoutName
                                && !StartsWithName(textNextWithoutPrefix, callbacks.Language))
                            {
                                goto skipThisLine;
                            }
                        }
                    }


                    // First paragraph...

                    // If first paragraphs ends with a suffix,
                    // and profile states to NOT replace comma,
                    // and next sentence starts with conjunction,
                    // try to re-add comma
                    var addComma = false;
                    if (!lastWord.EndsWith(",")
                        && HasSuffix(text) 
                        && (gap ? !gapReplaceComma : !replaceComma)
                        && ContinuationUtilities.StartsWithConjunction(textNextWithoutPrefix, callbacks.Language))
                    {
                        addComma = true;
                    }

                    // Make new last word
                    if (gap)
                    {
                        newLastWord = newLastWord + ((lastWord.EndsWith(",") || addComma) && !gapReplaceComma ? "," : "") + gapAddEnd;
                    }
                    else
                    {
                        newLastWord = newLastWord + ((lastWord.EndsWith(",") || addComma) && !replaceComma ? "," : "") + addEnd;
                    }

                    // Replace it
                    var newText = ContinuationUtilities.ReplaceLastOccurrence(oldText, lastWord, newLastWord);

                    // Commit if changed
                    if (oldText != newText && callbacks.AllowFix(p, fixAction))
                    {
                        // Don't apply fix when it's checked in step 1
                        if ((IsPreviewStep() && isChecked) || !IsPreviewStep())
                        {
                            p.Text = newText;
                        }
                        fixCount++;
                        callbacks.AddFixToListView(p, fixAction, oldText, newText, isChecked);
                    }
                    

                    // Second paragraph...

                    // Make new first word
                    if (gap)
                    {
                        newFirstWord = gapAddStart + newFirstWord;
                    }
                    else
                    {
                        newFirstWord = addStart + newFirstWord;
                    }

                    // Replace it
                    var newTextNext = ContinuationUtilities.ReplaceFirstOccurrence(oldTextNext, firstWord, newFirstWord);
                                        
                    // Commit if changed
                    if (oldTextNext != newTextNext && callbacks.AllowFix(pNext, fixAction + " "))
                    {
                        // Don't apply fix when it's checked in step 1
                        if ((IsPreviewStep() && isChecked) || !IsPreviewStep())
                        {
                            pNext.Text = newTextNext;
                        }
                        fixCount++;
                        callbacks.AddFixToListView(pNext, fixAction + " ", oldTextNext, newTextNext, isChecked);
                    }
                }

                skipThisLine:

                // Detect new sentence
                if (ContinuationUtilities.IsNewSentence(text))
                {
                    inSentence = true;

                    if (ContinuationUtilities.IsItalic(oldText))
                    {
                        inItalicSentence = true;
                    }
                    else
                    {
                        inItalicSentence = null;
                    }
                }

                // Detect end of sentence
                if (ContinuationUtilities.IsEndOfSentence(text))
                {
                    inSentence = false;

                    if (ContinuationUtilities.IsItalic(oldText))
                    {
                        inItalicSentence = false;
                    }
                    else
                    {
                        inItalicSentence = null;
                    }
                }
            }

            callbacks.UpdateFixStatus(fixCount, language.FixUnnecessaryLeadingDots, language.XFixContinuationStyle);

            bool IsPreviewStep()
            {
                return callbacks.AllowFix(new Paragraph() { Number = -1 }, string.Empty);
            }
        }

        private bool ShouldFixParagraph(string input)
        {
            return ContinuationUtilities.ShouldAddSuffix(input, this.continuationProfile, false);
        }

        private bool HasPrefix(string input)
        {
            return ContinuationUtilities.HasPrefix(input, this.continuationProfile);
        }

        private bool HasSuffix(string input)
        {
            return ContinuationUtilities.HasSuffix(input, this.continuationProfile);
        }
                
        private bool StartsWithName(string input, string language)
        {
            if (this.names == null)
            {
                NameList nameList = new NameList(Configuration.DictionariesDirectory, language, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
                this.names = nameList.GetAllNames();
            }

            if (this.names != null)
            {
                foreach (string name in this.names)
                {
                    if (input.StartsWith(name + " ") || input.StartsWith(name + ",") || input.StartsWith(name + ":"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void SetContinuationProfile(ContinuationStyle continuationStyle)
        {
            this.continuationProfile = ContinuationUtilities.GetContinuationProfile(continuationStyle);
        }
    }
}
