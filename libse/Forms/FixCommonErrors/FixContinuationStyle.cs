﻿using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Interfaces;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixContinuationStyle : IFixCommonError
    {
        private ContinuationUtilities.ContinuationProfile _continuationProfile;
        private List<string> _names;

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = string.Format(language.FixContinuationStyleX, ContinuationUtilities.GetContinuationStyleName(Configuration.Settings.General.ContinuationStyle));
            int fixCount = 0;

            // Check continuation profile
            if (_continuationProfile == null)
            {
                SetContinuationProfile(Configuration.Settings.General.ContinuationStyle);
            }

            int minGapMs = ContinuationUtilities.GetMinimumGapMs();

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
                var shouldProcess = true;

                // Check if we should fix this paragraph
                if (ShouldFixParagraph(text))
                {
                    // If ends with nothing...
                    if (!ContinuationUtilities.IsEndOfSentence(text))
                    {
                        // ...ignore inserts
                        if (Configuration.Settings.General.FixContinuationStyleUncheckInsertsAllCaps
                            && (ContinuationUtilities.IsAllCaps(text) || ContinuationUtilities.IsAllCaps(textNext)))
                        {
                            isChecked = false;
                        }

                        // ...and italic lyrics    
                        if (Configuration.Settings.General.FixContinuationStyleUncheckInsertsItalic && ContinuationUtilities.IsItalic(oldText)
                            && !ContinuationUtilities.IsNewSentence(text, true) && inItalicSentence == false)
                        {
                            isChecked = false;
                        }

                        // ...and smallcaps inserts or non-italic lyrics
                        if (Configuration.Settings.General.FixContinuationStyleUncheckInsertsLowercase
                            && !ContinuationUtilities.IsNewSentence(text, true) && !inSentence)
                        {
                            isChecked = false;
                        }
                    }


                    // Detect gap
                    bool gap = pNext.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds > minGapMs;

                    // Remove any suffixes and prefixes
                    var oldTextWithoutSuffix = ContinuationUtilities.RemoveSuffix(oldText, _continuationProfile, new List<string> { "," }, false).Trim();
                    var oldTextNextWithoutPrefix = ContinuationUtilities.RemovePrefix(oldTextNext, _continuationProfile, true, gap);
                    var textNextWithoutPrefix = ContinuationUtilities.SanitizeString(oldTextNextWithoutPrefix, true);

                    // Get last word of this paragraph                    
                    string lastWord = ContinuationUtilities.GetLastWord(text);

                    // If ends with dots (possible interruptions), or nothing, check if next sentence is new sentence, otherwise don't check by default
                    if (text.EndsWith("..") || text.EndsWith("…") || ContinuationUtilities.EndsWithNothing(text, _continuationProfile))
                    {
                        if (!HasPrefix(textNext) && (ContinuationUtilities.IsNewSentence(textNext, true) || string.IsNullOrEmpty(textNext)))
                        {
                            isChecked = false;

                            // If set, we'll hide interruption continuation candidates that don't start with a name,
                            // to prevent clogging up the window with a lot of unchecked items.
                            // For example, a candidate we DO want to list:  But wait...   Marty is still there!
                            //                                          or:  This is something   Marty can do.
                            // If both sentences are all caps, DO show them.
                            if (Configuration.Settings.General.FixContinuationStyleHideContinuationCandidatesWithoutName
                                && !(textNextWithoutPrefix.StartsWith("I ") || textNextWithoutPrefix.StartsWith("I'"))
                                && !StartsWithName(textNextWithoutPrefix, callbacks.Language)
                                && !(ContinuationUtilities.IsAllCaps(text) && ContinuationUtilities.IsAllCaps(textNext)))
                            {
                                shouldProcess = false;
                            }
                        }
                    }

                    if (shouldProcess)
                    {
                        // First paragraph...

                        // If first paragraphs ends with a suffix,
                        // and profile states to NOT replace comma,
                        // and next sentence starts with conjunction,
                        // try to re-add comma
                        bool addComma = lastWord.EndsWith(",") || (HasSuffix(text)
                                                                   && (gap ? !_continuationProfile.GapSuffixReplaceComma : !_continuationProfile.SuffixReplaceComma)
                                                                   && ContinuationUtilities.StartsWithConjunction(textNextWithoutPrefix, callbacks.Language));

                        // Make new last word
                        var newText = ContinuationUtilities.AddSuffixIfNeeded(oldTextWithoutSuffix, _continuationProfile, gap, addComma);

                        // Commit if changed
                        if (oldText != newText && callbacks.AllowFix(p, fixAction))
                        {
                            // Don't apply fix when it's checked in step 1
                            if ((IsPreviewStep(callbacks) && isChecked) || !IsPreviewStep(callbacks))
                            {
                                p.Text = newText;
                            }
                            fixCount++;
                            callbacks.AddFixToListView(p, fixAction, oldText, newText, isChecked);
                        }

                        // Second paragraph...

                        // Make new first word
                        var newTextNext = ContinuationUtilities.AddPrefixIfNeeded(oldTextNextWithoutPrefix, _continuationProfile, gap);

                        // Commit if changed
                        if (oldTextNext != newTextNext && callbacks.AllowFix(pNext, fixAction + " "))
                        {
                            // Don't apply fix when it's checked in step 1
                            if ((IsPreviewStep(callbacks) && isChecked) || !IsPreviewStep(callbacks))
                            {
                                pNext.Text = newTextNext;
                            }
                            fixCount++;
                            callbacks.AddFixToListView(pNext, fixAction + " ", oldTextNext, newTextNext, isChecked);
                        }
                    }
                }

                // Detect new sentence
                if (ContinuationUtilities.IsNewSentence(text, true))
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
        }

        private static bool IsPreviewStep(IFixCallbacks callbacks)
        {
            return callbacks.AllowFix(new Paragraph { Number = -1 }, string.Empty);
        }

        private bool ShouldFixParagraph(string input)
        {
            return ContinuationUtilities.ShouldAddSuffix(input, _continuationProfile, false);
        }

        private bool HasPrefix(string input)
        {
            return ContinuationUtilities.HasPrefix(input, _continuationProfile);
        }

        private bool HasSuffix(string input)
        {
            return ContinuationUtilities.HasSuffix(input, _continuationProfile);
        }

        private bool StartsWithName(string input, string language)
        {
            if (_names == null)
            {
                NameList nameList = new NameList(Configuration.DictionariesDirectory, language, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
                _names = nameList.GetAllNames();
            }

            if (_names != null)
            {
                foreach (string name in _names)
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
            _continuationProfile = ContinuationUtilities.GetContinuationProfile(continuationStyle);
        }
    }
}
