using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public class TimeCodesBeautifier
    {
        private readonly Subtitle _subtitle;
        private readonly double _frameRate;

        private List<double> _timeCodes;
        private List<double> _shotChanges;
        private List<int> _shotChangesFrames;

        public ProgressChangedDelegate ProgressChanged { get; set; }

        public TimeCodesBeautifier(Subtitle subtitle, double frameRate, List<double> timeCodes, List<double> shotChanges)
        {
            _subtitle = subtitle;
            _frameRate = frameRate;
            _timeCodes = timeCodes;
            _shotChanges = shotChanges;

            // Convert shot changes to frame numbers
            _shotChangesFrames = _shotChanges.Select(d => SubtitleFormat.MillisecondsToFrames(d * 1000, _frameRate)).ToList();
        }

        public void Beautify()
        {
            var amountOfPasses = 2;

            for (int pass = 0; pass < amountOfPasses; pass++)
            {
                for (int p = 0; p < _subtitle.Paragraphs.Count; p++)
                {
                    // === In cue ===
                    
                    var paragraph = _subtitle.Paragraphs.ElementAtOrDefault(p);
                    var previousParagraph = _subtitle.Paragraphs.ElementAtOrDefault(p - 1);

                    // Check if we have connected subtitles
                    var result = FixConnectedSubtitles(true, previousParagraph, paragraph);

                    // Check if we have chainable subtitles
                    if (!result)
                    {
                        result = FixChainableSubtitles(true, previousParagraph, paragraph);
                    }

                    // Then we have a free in cue
                    if (!result)
                    {
                        FixInCue(paragraph);
                    }


                    // === Out cue ===

                    var nextParagraph = _subtitle.Paragraphs.ElementAtOrDefault(p + 1);

                    // Check if we have connected subtitles
                    result = FixConnectedSubtitles(false, paragraph, nextParagraph);

                    // Check if we have chainable subtitles
                    if (!result)
                    {
                        result = FixChainableSubtitles(true, paragraph, nextParagraph);
                    }

                    // Then we have a free out cue
                    if (!result)
                    {
                        FixOutCue(paragraph);
                    }

                    // Report progress
                    var progress = ((pass + 1) / (double)amountOfPasses) * (p / (double)_subtitle.Paragraphs.Count);
                    ProgressChanged.Invoke(progress);
                }
            }
        }

        private bool FixConnectedSubtitles(bool isInCue, Paragraph leftParagraph = null, Paragraph rightParagraph = null)
        {
            if (leftParagraph == null || rightParagraph == null)
            {
                return false;
            }

            var distance = rightParagraph.StartTime.TotalMilliseconds - leftParagraph.EndTime.TotalMilliseconds;
            var subtitlesAreConnected = distance < Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesTreatConnected;

            if (subtitlesAreConnected)
            {
                var newLeftOutCueFrame = SubtitleFormat.MillisecondsToFrames(leftParagraph.EndTime.TotalMilliseconds, _frameRate);
                var newRightInCueFrame = SubtitleFormat.MillisecondsToFrames(rightParagraph.StartTime.TotalMilliseconds, _frameRate);

                // Check if we should do something with shot changes
                if (_shotChangesFrames.Count > 0)
                {
                    // TODO
                }
                else
                {
                    // Just chain them
                    newLeftOutCueFrame = newRightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                }

                // Align and update cues
                AlignAndSetCue(leftParagraph, false, newLeftOutCueFrame);
                AlignAndSetCue(rightParagraph, true, newRightInCueFrame);
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool FixChainableSubtitles(bool isInCue, Paragraph leftParagraph = null, Paragraph rightParagraph = null)
        {
            if (leftParagraph == null || rightParagraph == null)
            {
                return false;
            }

            var newLeftOutCueFrame = SubtitleFormat.MillisecondsToFrames(leftParagraph.EndTime.TotalMilliseconds, _frameRate);
            var newRightInCueFrame = SubtitleFormat.MillisecondsToFrames(rightParagraph.StartTime.TotalMilliseconds, _frameRate);

            // Check if we should do something with shot changes
            if (_shotChangesFrames.Count > 0)
            {
                // TODO
            }
            else
            {
                // No, so we will be using the "General" settings

                // Check if zones are being used
                if (Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralUseZones)
                {
                    var rightInCueWithGreenZone = newRightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralLeftGreenZone;
                    var isInGreenZone = newLeftOutCueFrame > rightInCueWithGreenZone && newLeftOutCueFrame < (newRightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralLeftRedZone);
                    var isInRedZone = newLeftOutCueFrame >= (newRightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralLeftRedZone) && newLeftOutCueFrame <= newRightInCueFrame;

                    if (isInRedZone)
                    {
                        // Chain them
                        newLeftOutCueFrame = newRightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                    } 
                    else if (isInGreenZone)
                    {
                        // Enforce green zone
                        newLeftOutCueFrame = rightInCueWithGreenZone;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    // No, so just check the distance in milliseconds
                    var distance = rightParagraph.StartTime.TotalMilliseconds - leftParagraph.EndTime.TotalMilliseconds;
                    if (distance < Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralMaxGap)
                    {
                        // Chain them
                        newLeftOutCueFrame = newRightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            // Align and update cues
            AlignAndSetCue(leftParagraph, false, newLeftOutCueFrame);
            AlignAndSetCue(rightParagraph, true, newRightInCueFrame);

            return true;
        }

        private void FixInCue(int index)
        {
            FixCue(index, true);
        }

        private void FixOutCue(int index)
        {
            FixCue(index, false);
        }

        private void FixCue(int index, bool isInCue)
        {
            var paragraph = _subtitle.Paragraphs[index];
            var newCueFrame = SubtitleFormat.MillisecondsToFrames(isInCue ? paragraph.StartTime.TotalMilliseconds : paragraph.EndTime.TotalMilliseconds, _frameRate);

            // Check if we should do something with shot changes
            if (_shotChangesFrames.Count > 0)
            {
                var bestCueFrame = FindNewBestCueFrame(newCueFrame, isInCue);

                // Check if adjacent subtitle is not in the way
                if (isInCue)
                {
                    var previousParagraph = _subtitle.Paragraphs.ElementAtOrDefault(index - 1);
                    if (previousParagraph != null)
                    {
                        var previousOutCueFrame = SubtitleFormat.MillisecondsToFrames(previousParagraph.EndTime.TotalMilliseconds, _frameRate);
                        newCueFrame = Math.Max(bestCueFrame, previousOutCueFrame + Configuration.Settings.BeautifyTimeCodes.Profile.Gap);
                    }
                    else
                    {
                        newCueFrame = bestCueFrame;
                    }
                }
                else
                {
                    var nextParagraph = _subtitle.Paragraphs.ElementAtOrDefault(index + 1);
                    if (nextParagraph != null)
                    {
                        var nextInCueFrame = SubtitleFormat.MillisecondsToFrames(nextParagraph.StartTime.TotalMilliseconds, _frameRate);
                        newCueFrame = Math.Min(bestCueFrame, nextInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.Gap);
                    }
                    else
                    {
                        newCueFrame = bestCueFrame;
                    }
                }
            }

            // Align and update cue
            AlignAndSetCue(paragraph, isInCue, newCueFrame);
        }

        private int FindNewBestCueFrame(int cueFrame, bool isInCue)
        {
            var previousShotChange = new List<int> { -1 }.Concat(_shotChangesFrames).Last(x => x <= cueFrame); // will return -1 if none found
            var nextShotChange = _shotChangesFrames.Concat(new List<int> { int.MaxValue }).First(x => x >= cueFrame); // will return maxValue if none found

            // If both not found, return self
            if (previousShotChange < 0 && nextShotChange == int.MaxValue)
            {
                return cueFrame;
            }

            // Do logic
            int previousShotChangeWithGreenZone;
            int nextShotChangeWithGreenZone;

            bool isInPreviousShotChangeGreenZone;
            bool isInPreviousShotChangeRedZone;
            bool isInNextShotChangeGreenZone;
            bool isInNextShotChangeRedZone;

            int previousShotChangeWithGap;
            int nextShotChangeWithGap;

            if (isInCue)
            {
                previousShotChangeWithGreenZone = previousShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.InCuesRightGreenZone;
                nextShotChangeWithGreenZone = nextShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.InCuesLeftGreenZone;
                
                isInPreviousShotChangeGreenZone = cueFrame < previousShotChangeWithGreenZone && cueFrame > (previousShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.InCuesRightRedZone);
                isInPreviousShotChangeRedZone = cueFrame <= (previousShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.InCuesRightRedZone) && cueFrame >= previousShotChange;
                isInNextShotChangeGreenZone = cueFrame > nextShotChangeWithGreenZone && cueFrame < (nextShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.InCuesLeftRedZone);
                isInNextShotChangeRedZone = cueFrame >= (nextShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.InCuesLeftRedZone) && cueFrame <= nextShotChange;

                previousShotChangeWithGap = previousShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap;
                nextShotChangeWithGap = nextShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap;
            }
            else
            {
                previousShotChangeWithGreenZone = previousShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesRightGreenZone;
                nextShotChangeWithGreenZone = nextShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesLeftGreenZone;
                
                isInPreviousShotChangeGreenZone = cueFrame < previousShotChangeWithGreenZone && cueFrame > (previousShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesRightRedZone);
                isInPreviousShotChangeRedZone = cueFrame <= (previousShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesRightRedZone) && cueFrame >= previousShotChange;
                isInNextShotChangeGreenZone = cueFrame > nextShotChangeWithGreenZone && cueFrame < (nextShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesLeftRedZone);
                isInNextShotChangeRedZone = cueFrame >= (nextShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesLeftRedZone) && cueFrame <= nextShotChange;

                previousShotChangeWithGap = previousShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap;
                nextShotChangeWithGap = nextShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap;
            }
            
            if (isInPreviousShotChangeRedZone && isInNextShotChangeRedZone)
            {
                // We are in both red zones! Snap to closest shot change
                if (Math.Abs(cueFrame - previousShotChange) < Math.Abs(cueFrame - nextShotChange))
                {
                    return previousShotChangeWithGap;
                }
                else
                {
                    return nextShotChangeWithGap;
                }
            }
            else if (isInPreviousShotChangeGreenZone && isInNextShotChangeGreenZone)
            {
                // We are in both green zones! Take one with least "violation"
                if (Math.Abs(previousShotChangeWithGreenZone - nextShotChange) > Math.Abs(nextShotChangeWithGreenZone - previousShotChange))
                {
                    return previousShotChangeWithGreenZone;
                }
                else
                {
                    return nextShotChangeWithGreenZone;
                }
            }
            else
            {
                if (isInPreviousShotChangeRedZone)
                {
                    // Snap to previous shot change
                    return previousShotChangeWithGap;
                }
                else if (isInNextShotChangeRedZone)
                {
                    // Snap to next shot change
                    return nextShotChangeWithGap;
                }
                else if (isInPreviousShotChangeGreenZone)
                {
                    // Enforce green zone from previous shot change, but shouldn't exceed next shot change
                    return Math.Min(previousShotChangeWithGreenZone, nextShotChange);
                }
                else if (isInNextShotChangeGreenZone)
                {
                    // Enforce green zone from next shot change, but shouldn't exceed previous shot change
                    return Math.Max(nextShotChangeWithGreenZone, previousShotChange);
                }
            }

            return cueFrame;
        }

        private void AlignAndSetCue(Paragraph paragraph, bool isInCue, int newFrame)
        {
            double newTime = SubtitleFormat.FramesToMilliseconds(newFrame, _frameRate);

            // Check if we have extracted exact time codes
            if (_timeCodes.Count > 0)
            {
                newTime = _timeCodes.Aggregate((x, y) => Math.Abs(x - newTime) < Math.Abs(y - newTime) ? x : y);
            }

            // Finally, update time
            if (isInCue)
            {
                paragraph.StartTime.TotalMilliseconds = newTime;
            }
            else
            {
                paragraph.EndTime.TotalMilliseconds = newTime;
            }

            // Duration cannot be negative
            if (paragraph.Duration.TotalMilliseconds < 0)
            {
                paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds;
            }
        }


        // Delegates

        public delegate void ProgressChangedDelegate(double progress);
    }

    public static class TimeCodesBeautifierUtils
    {
        public static double GetFrameDurationMs(double? frameRate = null)
        {
            return TimeCode.BaseUnit / (frameRate ?? Configuration.Settings.General.CurrentFrameRate);
        }

        public static double GetInCuesGapMs(double? frameRate = null)
        {
            return GetFrameDurationMs(frameRate) * Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap;
        }

        public static double GetOutCuesGapMs(double? frameRate = null)
        {
            return GetFrameDurationMs(frameRate) * Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap;
        }
    }
}
