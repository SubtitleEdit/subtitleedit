using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixContinuationStyle : IFixCommonError
    {
        public static class Language
        {
            public static string FixUnnecessaryLeadingDots { get; set; } = "Remove unnecessary leading dots";
        }

        private ContinuationUtilities.ContinuationProfile _continuationProfile;
        private List<string> _names;
        public string FixAction { get; set; }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            int fixCount = 0;

            var isLanguageWithoutCaseDistinction = ContinuationUtilities.IsLanguageWithoutCaseDistinction(callbacks.Language);

            // Check continuation profile
            if (_continuationProfile == null)
            {
                SetContinuationProfile(Configuration.Settings.General.ContinuationStyle);
            }

            // Quick fix for Portuguese
            if (callbacks.Language == "pt")
            {
                _continuationProfile.SuffixApplyIfComma = false;
                _continuationProfile.GapSuffixApplyIfComma = false;

                if (_continuationProfile.Prefix == "..." || _continuationProfile.Prefix == "…")
                {
                    _continuationProfile.PrefixAddSpace = true;
                }

                if (_continuationProfile.GapPrefix == "..." || _continuationProfile.GapPrefix == "…")
                {
                    _continuationProfile.GapPrefixAddSpace = true;
                }
            }

            int minGapMs = ContinuationUtilities.GetMinimumGapMs();

            bool inSentence = false;
            bool? inItalicSentence = null;

            for (int i = 0; i < subtitle.Paragraphs.Count - 1; i++)
            {
                var p = subtitle.Paragraphs[i];
                var pNext = subtitle.Paragraphs[i + 1];
                var oldText = p.Text;
                var oldTextNext = pNext.Text;
                var text = ContinuationUtilities.SanitizeString(p.Text);
                var textNext = ContinuationUtilities.SanitizeString(pNext.Text);
                var isChecked = true;
                var shouldProcess = true;

                // Detect gap
                bool gap = pNext.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds >= minGapMs;

                // Convert for Arabic
                if (callbacks.Language == "ar")
                {
                    oldText = ContinuationUtilities.ConvertToForArabic(oldText);
                    oldTextNext = ContinuationUtilities.ConvertToForArabic(oldTextNext);
                    text = ContinuationUtilities.ConvertToForArabic(text);
                    textNext = ContinuationUtilities.ConvertToForArabic(textNext);
                }

                // Check if we should fix this paragraph
                if (ShouldFixParagraph(text, gap))
                {
                    // If ends with nothing...
                    if (!ContinuationUtilities.IsEndOfSentence(text))
                    {
                        if (!isLanguageWithoutCaseDistinction)
                        {
                            // ...ignore inserts
                            if (Configuration.Settings.General.FixContinuationStyleUncheckInsertsAllCaps)
                            {
                                if (ContinuationUtilities.IsAllCaps(text) || ContinuationUtilities.IsAllCaps(textNext))
                                {
                                    isChecked = false;
                                }
                            }

                            // ...and italic lyrics
                            if (Configuration.Settings.General.FixContinuationStyleUncheckInsertsItalic)
                            {
                                if (ContinuationUtilities.IsItalic(oldText) && !ContinuationUtilities.IsNewSentence(text, true) && inItalicSentence == false)
                                {
                                    isChecked = false;
                                }
                            }

                            // ...and small caps inserts or non-italic lyrics
                            if (Configuration.Settings.General.FixContinuationStyleUncheckInsertsLowercase)
                            {
                                if (!ContinuationUtilities.IsNewSentence(text, true) && !inSentence)
                                {
                                    isChecked = false;
                                }
                            }

                            // ...ignore bold tags for Portuguese
                            if (callbacks.Language == "pt")
                            {
                                if (ContinuationUtilities.IsBold(oldText) || ContinuationUtilities.IsBold(oldTextNext))
                                {
                                    isChecked = false;
                                }
                            }
                        }

                        // ...ignore Arabic inserts
                        if (callbacks.Language == "ar")
                        {
                            if (ContinuationUtilities.IsArabicInsert(oldText, text) || ContinuationUtilities.IsArabicInsert(oldTextNext, textNext))
                            {
                                isChecked = false;
                            }
                        }
                    }

                    // Remove any suffixes and prefixes
                    var oldTextWithoutSuffix = ContinuationUtilities.RemoveSuffix(oldText, _continuationProfile, new List<string> { "," }, false).Trim();
                    var oldTextNextWithoutPrefix = ContinuationUtilities.RemovePrefix(oldTextNext, _continuationProfile, true, gap);
                    var textNextWithoutPrefix = ContinuationUtilities.SanitizeString(oldTextNextWithoutPrefix, true);

                    // Get last word of this paragraph
                    string lastWord = ContinuationUtilities.GetLastWord(text);


                    // If ends with dots (possible interruptions), or nothing, check if next sentence is new sentence, otherwise don't check by default
                    if (text.EndsWith("..") || text.EndsWith("…") || ContinuationUtilities.EndsWithNothing(text, _continuationProfile))
                    {
                        if (!HasPrefix(textNext) && ((!isLanguageWithoutCaseDistinction && ContinuationUtilities.IsNewSentence(textNext, true)) || string.IsNullOrEmpty(textNext)))
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
                        bool addComma = lastWord.EndsWith(",") || HasSuffix(text)
                                        && (gap ? !_continuationProfile.GapSuffixReplaceComma : !_continuationProfile.SuffixReplaceComma)
                                        && ContinuationUtilities.StartsWithConjunction(textNextWithoutPrefix, callbacks.Language);

                        // Make new last word
                        var newText = ContinuationUtilities.AddSuffixIfNeeded(oldTextWithoutSuffix, _continuationProfile, gap, addComma);

                        // Commit if changed
                        if (oldText != newText && callbacks.AllowFix(p, FixAction))
                        {
                            // Convert back for Arabic
                            if (callbacks.Language == "ar")
                            {
                                newText = ContinuationUtilities.ConvertBackForArabic(newText);
                            }

                            // Don't apply fix when it's checked in step 1
                            if (IsPreviewStep(callbacks) && isChecked || !IsPreviewStep(callbacks))
                            {
                                p.Text = newText;
                            }
                            fixCount++;
                            callbacks.AddFixToListView(p, FixAction, oldText, newText, isChecked);
                        }


                        // Second paragraph...

                        // Make new first word
                        var newTextNext = ContinuationUtilities.AddPrefixIfNeeded(oldTextNextWithoutPrefix, _continuationProfile, gap);

                        // Commit if changed
                        if (oldTextNext != newTextNext && callbacks.AllowFix(pNext, FixAction + " "))
                        {
                            // Convert back for Arabic
                            if (callbacks.Language == "ar")
                            {
                                newTextNext = ContinuationUtilities.ConvertBackForArabic(newTextNext);
                            }

                            // Don't apply fix when it's checked in step 1
                            if (IsPreviewStep(callbacks) && isChecked || !IsPreviewStep(callbacks))
                            {
                                pNext.Text = newTextNext;
                            }
                            fixCount++;
                            callbacks.AddFixToListView(pNext, FixAction + " ", oldTextNext, newTextNext, isChecked);
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

            callbacks.UpdateFixStatus(fixCount, Language.FixUnnecessaryLeadingDots);
        }

        private static bool IsPreviewStep(IFixCallbacks callbacks)
        {
            return callbacks.AllowFix(new Paragraph { Number = -1 }, string.Empty);
        }

        private bool ShouldFixParagraph(string input, bool gap)
        {
            return ContinuationUtilities.ShouldAddSuffix(input, _continuationProfile, false, gap);
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
                var nameList = new NameList(Configuration.DictionariesDirectory, language, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
                _names = nameList.GetAllNames();
            }

            if (_names != null)
            {
                foreach (var name in _names)
                {
                    if (input.StartsWith(name + " ", StringComparison.Ordinal) || input.StartsWith(name + ",", StringComparison.Ordinal) || input.StartsWith(name + ":", StringComparison.Ordinal))
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
