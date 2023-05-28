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

                    // If not, check if we have chainable subtitles
                    if (!result)
                    {
                        result = FixChainableSubtitles(true, previousParagraph, paragraph);
                    }

                    // If not, then we have a free in cue
                    if (!result)
                    {
                        FixInCue(p);
                    }


                    // === Out cue ===

                    var nextParagraph = _subtitle.Paragraphs.ElementAtOrDefault(p + 1);

                    // Check if we have connected subtitles
                    result = FixConnectedSubtitles(false, paragraph, nextParagraph);

                    // If not, check if we have chainable subtitles
                    if (!result)
                    {
                        result = FixChainableSubtitles(true, paragraph, nextParagraph);
                    }

                    // If not, then we have a free out cue
                    if (!result)
                    {
                        FixOutCue(p);
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
                    // Find best cues for left out cue and right in cue
                    var bestLeftOutCueFrameInfo = FindConnectedSubtitlesBestCueFrame(newLeftOutCueFrame);
                    var bestRightInCueFrameInfo = FindConnectedSubtitlesBestCueFrame(newRightInCueFrame);

                    // Check result
                    if (bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToRedZone && bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToRedZone)
                    {
                        // Both are in red zones! We will use the closest shot change to align the cues around
                        if (Math.Abs(newLeftOutCueFrame - bestLeftOutCueFrameInfo.cueFrame) < Math.Abs(newRightInCueFrame - bestRightInCueFrameInfo.cueFrame))
                        {
                            var fixInfo = GetFixedConnectedSubtitlesCueFrames(leftParagraph, rightParagraph, bestLeftOutCueFrameInfo.cueFrame);
                            newLeftOutCueFrame = fixInfo.newLeftOutCueFrame;
                            newRightInCueFrame = fixInfo.newRightInCueFrame;
                        }
                        else
                        {
                            var fixInfo = GetFixedConnectedSubtitlesCueFrames(leftParagraph, rightParagraph, bestRightInCueFrameInfo.cueFrame);
                            newLeftOutCueFrame = fixInfo.newLeftOutCueFrame;
                            newRightInCueFrame = fixInfo.newRightInCueFrame;
                        }
                    }
                    else if ((bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToLeftGreenZone || bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToRightGreenZone) && 
                             (bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToLeftGreenZone || bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToRightGreenZone))
                    {
                        // Both are in green zones! Check cases...
                        if (bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToLeftGreenZone && bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToLeftGreenZone)
                        {
                            // Both cues want to go backward. Most likely both are in the same shot change's green zone
                            // For now, put the right in cue on the edge of the green zone, and push the previous subtitle backward
                            newRightInCueFrame = bestRightInCueFrameInfo.cueFrame;
                            newLeftOutCueFrame = newRightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                        }
                        else if (bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToLeftGreenZone && bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToRightGreenZone)
                        {
                            // Cues want to be pulled apart. Highly unlikely. There is probably a shot change in the middle of them and no red zone set
                            // For now, let the left cue take priority, so put the right in cue on the edge of the green zone, and push the previous subtitle backward
                            newRightInCueFrame = bestLeftOutCueFrameInfo.cueFrame;
                            newLeftOutCueFrame = newRightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                        }
                        else if (bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToRightGreenZone && bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToLeftGreenZone)
                        {
                            // Cues want to be pushed together. The connect subtitles are most likely between two shot changes that are close together
                            // For now, let the left cue take priority, so put the left out cue on the edge of the green zone, and push the next subtitle forward
                            newLeftOutCueFrame = bestLeftOutCueFrameInfo.cueFrame;
                            newRightInCueFrame = newLeftOutCueFrame + Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                        }
                        else if (bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToRightGreenZone && bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToRightGreenZone)
                        {
                            // Both cues want to go forward. Most likely both are in the same shot change's green zone
                            // For now, put the left out cue on the edge of the green zone, and push the next subtitle forward
                            newLeftOutCueFrame = bestLeftOutCueFrameInfo.cueFrame;
                            newRightInCueFrame = newLeftOutCueFrame + Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                        }
                    }
                    else if (bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToRedZone) // Other cases... Red zone snapping has priority
                    {
                        var fixInfo = GetFixedConnectedSubtitlesCueFrames(leftParagraph, rightParagraph, bestLeftOutCueFrameInfo.cueFrame);
                        newLeftOutCueFrame = fixInfo.newLeftOutCueFrame;
                        newRightInCueFrame = fixInfo.newRightInCueFrame;
                    }
                    else if (bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToRedZone)
                    {
                        var fixInfo = GetFixedConnectedSubtitlesCueFrames(leftParagraph, rightParagraph, bestRightInCueFrameInfo.cueFrame);
                        newLeftOutCueFrame = fixInfo.newLeftOutCueFrame;
                        newRightInCueFrame = fixInfo.newRightInCueFrame;
                    }
                    else if (bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToLeftGreenZone)
                    {
                        throw new InvalidOperationException("The left out cue cannot be snapped to the left side of a green zone while the right in cue is unaffected at the same time.");
                    }
                    else if (bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToRightGreenZone)
                    {
                        // Put the left out cue on the edge of the green zone, and push the next subtitle forward
                        newLeftOutCueFrame = bestLeftOutCueFrameInfo.cueFrame;
                        newRightInCueFrame = newLeftOutCueFrame + Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                    }
                    else if (bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToLeftGreenZone)
                    {
                        // Put the right in cue on the edge of the green zone, and push the previous subtitle backward
                        newRightInCueFrame = bestRightInCueFrameInfo.cueFrame;
                        newLeftOutCueFrame = newRightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                    }
                    else if (bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToRightGreenZone)
                    {
                        throw new InvalidOperationException("The right in cue cannot be snapped to the right side of a green zone while the left out cue is unaffected at the same time.");
                    }
                    else
                    {
                        // Fallback when no shot changes were apparently found: just chain them
                        newLeftOutCueFrame = newRightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                    }
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
        
        private (int cueFrame, FindBestCueResult result) FindConnectedSubtitlesBestCueFrame(int cueFrame)
        {
            var previousShotChange = new List<int> { -1 }.Concat(_shotChangesFrames).Last(x => x <= cueFrame); // will return -1 if none found
            var nextShotChange = _shotChangesFrames.Concat(new List<int> { int.MaxValue }).First(x => x >= cueFrame); // will return maxValue if none found

            // If both not found, return self
            if (previousShotChange < 0 && nextShotChange == int.MaxValue)
            {
                return (cueFrame, FindBestCueResult.NoShotChangeFound);
            }

            // Do logic
            var previousShotChangeWithGreenZone = previousShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesRightGreenZone;
            var nextShotChangeWithGreenZone = nextShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesLeftGreenZone;

            var isInPreviousShotChangeGreenZone = cueFrame < previousShotChangeWithGreenZone && cueFrame > (previousShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesRightRedZone);
            var isInPreviousShotChangeRedZone = cueFrame <= (previousShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesRightRedZone) && cueFrame >= previousShotChange;
            var isInNextShotChangeGreenZone = cueFrame > nextShotChangeWithGreenZone && cueFrame < (nextShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesLeftRedZone);
            var isInNextShotChangeRedZone = cueFrame >= (nextShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesLeftRedZone) && cueFrame <= nextShotChange;

            if (isInPreviousShotChangeRedZone && isInNextShotChangeRedZone)
            {
                // We are in both red zones! Snap to closest shot change
                if (Math.Abs(cueFrame - previousShotChange) < Math.Abs(cueFrame - nextShotChange))
                {
                    return (previousShotChange, FindBestCueResult.SnappedToRedZone);
                }
                else
                {
                    return (nextShotChange, FindBestCueResult.SnappedToRedZone);
                }
            }
            else if (isInPreviousShotChangeGreenZone && isInNextShotChangeGreenZone)
            {
                // We are in both green zones! Take one with least "violation"
                if (Math.Abs(previousShotChangeWithGreenZone - nextShotChange) > Math.Abs(nextShotChangeWithGreenZone - previousShotChange))
                {
                    return (previousShotChangeWithGreenZone, FindBestCueResult.SnappedToRightGreenZone);
                }
                else
                {
                    return (nextShotChangeWithGreenZone, FindBestCueResult.SnappedToLeftGreenZone);
                }
            }
            else
            {
                if (isInPreviousShotChangeRedZone)
                {
                    // Snap to previous shot change
                    return (previousShotChange, FindBestCueResult.SnappedToRedZone);
                }
                else if (isInNextShotChangeRedZone)
                {
                    // Snap to next shot change
                    return (nextShotChange, FindBestCueResult.SnappedToRedZone);
                }
                else if (isInPreviousShotChangeGreenZone)
                {
                    // Enforce green zone from previous shot change, but shouldn't exceed next shot change
                    return (Math.Min(previousShotChangeWithGreenZone, nextShotChange), FindBestCueResult.SnappedToRightGreenZone);
                }
                else if (isInNextShotChangeGreenZone)
                {
                    // Enforce green zone from next shot change, but shouldn't exceed previous shot change
                    return (Math.Max(nextShotChangeWithGreenZone, previousShotChange), FindBestCueResult.SnappedToLeftGreenZone);
                }
            }

            return (cueFrame, FindBestCueResult.NoShotChangeFound);
        }

        private enum FindBestCueResult
        {
            NoShotChangeFound,
            SnappedToRedZone,
            SnappedToRightGreenZone,
            SnappedToLeftGreenZone
        }

        private (int newLeftOutCueFrame, int newRightInCueFrame) GetFixedConnectedSubtitlesCueFrames(Paragraph leftParagraph, Paragraph rightParagraph, int shotChangeFrame)
        {
            // Check which cue is closest (use milliseconds to check original unaligned positions)
            var shotChangeMs = SubtitleFormat.FramesToMilliseconds(shotChangeFrame, _frameRate);

            if (Math.Abs(leftParagraph.EndTime.Milliseconds - shotChangeMs) < Math.Abs(rightParagraph.StartTime.Milliseconds - shotChangeMs))
            {
                // Left subtitle's out cue is closest
                var newLeftOutCueFrame = shotChangeFrame - Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesOutCueClosestLeftGap;
                var newRightInCueFrame = shotChangeFrame + Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesOutCueClosestRightGap;
                return (newLeftOutCueFrame, newRightInCueFrame);
            }
            else
            {
                // Right subtitle's in cue is closest
                var newLeftOutCueFrame = shotChangeFrame - Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesInCueClosestLeftGap;
                var newRightInCueFrame = shotChangeFrame + Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesInCueClosestRightGap;
                return (newLeftOutCueFrame, newRightInCueFrame);
            }
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
                // Other cases... Red zone snapping has priority
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
