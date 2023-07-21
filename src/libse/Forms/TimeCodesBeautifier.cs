using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public class TimeCodesBeautifier
    {
        private readonly Subtitle _subtitle;
        private readonly double _frameRate;

        private readonly List<double> _timeCodes;
        private readonly List<int> _shotChangesFrames;

        public ProgressChangedDelegate ProgressChanged { get; set; }

        public TimeCodesBeautifier(Subtitle subtitle, double frameRate, List<double> timeCodes, List<double> shotChanges)
        {
            _subtitle = subtitle;
            _frameRate = frameRate;

            // Convert time codes to milliseconds
            _timeCodes = timeCodes.Select(d => d * 1000).ToList();

            // Convert shot changes to frame numbers
            _shotChangesFrames = shotChanges.Select(d => SubtitleFormat.MillisecondsToFrames(d * 1000, _frameRate)).ToList();
        }

        public void Beautify()
        {
            var amountOfPasses = 2;
            var skipNextInCue = false;

            for (int pass = 0; pass < amountOfPasses; pass++)
            {
                for (int p = 0; p < _subtitle.Paragraphs.Count; p++)
                {
                    // Gather relevant paragraphs
                    var paragraph = _subtitle.Paragraphs.ElementAtOrDefault(p);
                    var previousParagraph = _subtitle.Paragraphs.ElementAtOrDefault(p - 1);
                    var nextParagraph = _subtitle.Paragraphs.ElementAtOrDefault(p + 1);

                    var result = false;

                    // === In cue ===

                    // Check if the in cue should be processed
                    if (!skipNextInCue)
                    {
                        // Check if we have connected subtitles
                        result = FixConnectedSubtitles(previousParagraph, paragraph);

                        // If not, check if we have chainable subtitles
                        if (!result)
                        {
                            result = FixChainableSubtitles(previousParagraph, paragraph);
                        }

                        // If not, then we have a free in cue
                        if (!result)
                        {
                            FixInCue(p);
                        }
                    }
                    else
                    {
                        // Reset flag for next iteration
                        skipNextInCue = false;
                    }

                    // === Out cue ===

                    // Check if we have connected subtitles
                    result = FixConnectedSubtitles(paragraph, nextParagraph);
                    if (result)
                    {
                        // Yes, this means the next subtitle's in cue is now also processed. Skipping in next iteration
                        skipNextInCue = true;
                    }
                    else
                    {
                        // If not, then we have a free out cue
                        FixOutCue(p);
                    }

                    // Report progress
                    var progress = (double)(pass * _subtitle.Paragraphs.Count + p) / (_subtitle.Paragraphs.Count * amountOfPasses);
                    if (ProgressChanged != null)
                    {
                        ProgressChanged.Invoke(progress);
                    }
                }
            }
        }

        private bool FixConnectedSubtitles(Paragraph leftParagraph = null, Paragraph rightParagraph = null)
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

                    // Gather subtitles' other cue for length calculations
                    var leftInCueFrame = SubtitleFormat.MillisecondsToFrames(leftParagraph.StartTime.TotalMilliseconds, _frameRate);
                    var rightOutCueFrame = SubtitleFormat.MillisecondsToFrames(rightParagraph.EndTime.TotalMilliseconds, _frameRate);

                    // Check result
                    if (bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToRedZone && bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToRedZone)
                    {
                        var fixInfoForLeft = GetFixedConnectedSubtitlesCueFrames(leftParagraph, rightParagraph, bestLeftOutCueFrameInfo.cueFrame);
                        var fixInfoForRight = GetFixedConnectedSubtitlesCueFrames(leftParagraph, rightParagraph, bestRightInCueFrameInfo.cueFrame);

                        // Both are in red zones! We will use the closest shot change to align the cues around
                        if (Math.Abs(newLeftOutCueFrame - bestLeftOutCueFrameInfo.cueFrame) <= Math.Abs(newRightInCueFrame - bestRightInCueFrameInfo.cueFrame))
                        {
                            // Align around the left shot change
                            // Except, when the left subtitle now becomes invalid (negative duration) and the right subtitle won't, we will use the right shot change anyway
                            var newLeftDuration = fixInfoForLeft.newLeftOutCueFrame - leftInCueFrame;
                            var newRightDuration = rightOutCueFrame - fixInfoForLeft.newRightInCueFrame;
                            if (newLeftDuration <= 0 && newRightDuration > 0)
                            {
                                newLeftOutCueFrame = fixInfoForRight.newLeftOutCueFrame;
                                newRightInCueFrame = fixInfoForRight.newRightInCueFrame;
                            }
                            else
                            {
                                newLeftOutCueFrame = fixInfoForLeft.newLeftOutCueFrame;
                                newRightInCueFrame = fixInfoForLeft.newRightInCueFrame;
                            }
                        }
                        else
                        {
                            // Align around the right shot change
                            // Except, when the right subtitle now becomes invalid (negative duration) and the left subtitle won't we will use the left shot change anyway
                            var newLeftDuration = fixInfoForRight.newLeftOutCueFrame - leftInCueFrame;
                            var newRightDuration = rightOutCueFrame - fixInfoForRight.newRightInCueFrame;
                            if (newRightDuration <= 0 && newLeftDuration > 0)
                            {
                                newLeftOutCueFrame = fixInfoForLeft.newLeftOutCueFrame;
                                newRightInCueFrame = fixInfoForLeft.newRightInCueFrame;
                            }
                            else
                            {
                                newLeftOutCueFrame = fixInfoForRight.newLeftOutCueFrame;
                                newRightInCueFrame = fixInfoForRight.newRightInCueFrame;
                            }
                        }
                    }
                    else if ((bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToLeftGreenZone || bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToRightGreenZone) && 
                             (bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToLeftGreenZone || bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToRightGreenZone))
                    {
                        // Both are in green zones! Check cases...
                        if (bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToLeftGreenZone && bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToLeftGreenZone)
                        {
                            // Both cues want to go backward. Most likely both are in the same shot change's green zone
                            // For now, assume that, and put the right in cue on the edge of the green zone, and push the previous subtitle backward
                            newRightInCueFrame = bestRightInCueFrameInfo.cueFrame;
                            newLeftOutCueFrame = newRightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                        }
                        else if (bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToLeftGreenZone && bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToRightGreenZone)
                        {
                            // Cues want to be pulled apart. Highly unlikely. There is probably a shot change in the middle of them and no red zone set
                            // For now, try get the first shot change in between and align the cues around that, ignoring the zones
                            var firstShotChangeInBetween = GetFirstShotChangeFrameInBetween(newLeftOutCueFrame, newRightInCueFrame);
                            if (firstShotChangeInBetween != null)
                            {
                                var fixInfo = GetFixedConnectedSubtitlesCueFrames(leftParagraph, rightParagraph, firstShotChangeInBetween.Value);
                                newLeftOutCueFrame = fixInfo.newLeftOutCueFrame;
                                newRightInCueFrame = fixInfo.newRightInCueFrame;
                            }
                            else
                            {
                                // No shot change found... Then just chain them then as fallback
                                newLeftOutCueFrame = newRightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                            }
                        }
                        else if (bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToRightGreenZone && bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToLeftGreenZone)
                        {
                            // Cues want to be pushed together. The connect subtitles are most likely between two shot changes that are close together
                            // For now, check which shot change is closer and align the cues around that, ignoring the zones
                            var previousShotChange = new List<int> { int.MinValue }.Concat(_shotChangesFrames).Last(x => x <= newLeftOutCueFrame); // will return minValue if none found
                            var nextShotChange = _shotChangesFrames.Concat(new List<int> { int.MaxValue }).First(x => x >= newRightInCueFrame); // will return maxValue if none found
                            if (previousShotChange >= 0 && nextShotChange != int.MaxValue)
                            {
                                if (Math.Abs(previousShotChange - newLeftOutCueFrame) <= Math.Abs(nextShotChange - newRightInCueFrame))
                                {
                                    var fixInfo = GetFixedConnectedSubtitlesCueFrames(leftParagraph, rightParagraph, previousShotChange);
                                    newLeftOutCueFrame = fixInfo.newLeftOutCueFrame;
                                    newRightInCueFrame = fixInfo.newRightInCueFrame;
                                }
                                else
                                {
                                    var fixInfo = GetFixedConnectedSubtitlesCueFrames(leftParagraph, rightParagraph, nextShotChange);
                                    newLeftOutCueFrame = fixInfo.newLeftOutCueFrame;
                                    newRightInCueFrame = fixInfo.newRightInCueFrame;
                                }
                            }
                            else
                            {
                                // No shot changes found on either sides... Then just chain them as fallback
                                newLeftOutCueFrame = newRightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                            }
                        }
                        else if (bestLeftOutCueFrameInfo.result == FindBestCueResult.SnappedToRightGreenZone && bestRightInCueFrameInfo.result == FindBestCueResult.SnappedToRightGreenZone)
                        {
                            // Both cues want to go forward. Most likely both are in the same shot change's green zone
                            // For now, assume that, and put the left out cue on the edge of the green zone, and push the next subtitle forward
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
            var previousShotChange = new List<int> { int.MinValue }.Concat(_shotChangesFrames).Last(x => x <= cueFrame); // will return minValue if none found
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
                if (Math.Abs(cueFrame - previousShotChange) <= Math.Abs(cueFrame - nextShotChange))
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

            if (Math.Abs(leftParagraph.EndTime.TotalMilliseconds - shotChangeMs) < Math.Abs(rightParagraph.StartTime.TotalMilliseconds - shotChangeMs))
            {
                // Left subtitle's out cue is closest
                var newLeftOutCueFrame = shotChangeFrame - Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesOutCueClosestLeftGap;
                var newRightInCueFrame = shotChangeFrame + Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesOutCueClosestRightGap;
                return (newLeftOutCueFrame, newRightInCueFrame);
            }
            else
            {
                // Right subtitle's in cue is closest (priority if same)
                var newLeftOutCueFrame = shotChangeFrame - Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesInCueClosestLeftGap;
                var newRightInCueFrame = shotChangeFrame + Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesInCueClosestRightGap;
                return (newLeftOutCueFrame, newRightInCueFrame);
            }
        }

        private bool FixChainableSubtitles(Paragraph leftParagraph = null, Paragraph rightParagraph = null)
        {
            if (leftParagraph == null || rightParagraph == null)
            {
                return false;
            }

            var newLeftOutCueFrame = SubtitleFormat.MillisecondsToFrames(leftParagraph.EndTime.TotalMilliseconds, _frameRate);
            var newRightInCueFrame = SubtitleFormat.MillisecondsToFrames(rightParagraph.StartTime.TotalMilliseconds, _frameRate);

            var shouldFixConnectedSubtitles = false;

            // Check if we should do something with shot changes
            if (_shotChangesFrames.Count > 0)
            {
                // Find best cues for left out cue and right in cue
                var bestLeftOutCueFrameInfo = FindBestCueFrame(newLeftOutCueFrame, false);
                var bestLeftOutCueFrame = bestLeftOutCueFrameInfo.cueFrame;

                var bestRightInCueFrameInfo = FindBestCueFrame(newRightInCueFrame, true);
                var bestRightInCueFrame = bestRightInCueFrameInfo.cueFrame;

                // Check cases
                var isLeftOutCueOnShotChange = IsCueOnShotChange(bestLeftOutCueFrame, false);
                var isRightInCueOnShotChange = IsCueOnShotChange(bestRightInCueFrame, true);

                if (isRightInCueOnShotChange)
                {
                    // The right in cue is on a shot change
                    // Try to chain the subtitles
                    var fixedLeftOutCueFrame = GetFixedChainableSubtitlesLeftOutCueFrameInCueOnShot(bestLeftOutCueFrame, bestRightInCueFrame);
                    if (fixedLeftOutCueFrame != null)
                    {
                        newLeftOutCueFrame = fixedLeftOutCueFrame.Value;
                        newRightInCueFrame = bestRightInCueFrame;

                        // Make sure the newly connected subtitles get fixed
                        shouldFixConnectedSubtitles = true;
                    }
                    else
                    {
                        // Chaining wasn't needed
                        return false;
                    }
                }
                else if (isLeftOutCueOnShotChange)
                {
                    // The left out cue in on a shot change
                    // Try to chain the subtitles
                    var fixedRightInCueFrame = GetFixedChainableSubtitlesRightInCueFrameOutCueOnShot(bestLeftOutCueFrame, bestRightInCueFrame);
                    if (fixedRightInCueFrame != null)
                    {
                        newLeftOutCueFrame = bestLeftOutCueFrame;
                        newRightInCueFrame = fixedRightInCueFrame.Value;

                        // Make sure the newly connected subtitles get fixed
                        shouldFixConnectedSubtitles = true;
                    }
                    else
                    {
                        // Chaining wasn't needed
                        return false;
                    }
                }
                else
                {
                    // The cues are not on shot changes
                    // Try to chain the subtitles already, maybe chaining is not needed
                    var fixedLeftOutCueFrame = GetFixedChainableSubtitlesLeftOutCueFrameGeneral(bestLeftOutCueFrame, bestRightInCueFrame);
                    if (fixedLeftOutCueFrame != null)
                    {
                        // Check if there are any shot changes in between them
                        var firstShotChangeInBetween = GetFirstShotChangeFrameInBetween(bestLeftOutCueFrame, bestRightInCueFrame);
                        if (firstShotChangeInBetween != null)
                        {
                            // There are shot changes in between. Check behaviors
                            switch (Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralShotChangeBehavior)
                            {
                                case BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.ChainingGeneralShotChangeBehaviorEnum.DontChain:
                                    // Don't do anything
                                    return false;
                                case BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.ChainingGeneralShotChangeBehaviorEnum.ExtendCrossingShotChange:
                                    // Apply the chaining
                                    newLeftOutCueFrame = fixedLeftOutCueFrame.Value;
                                    newRightInCueFrame = bestRightInCueFrame;

                                    // Make sure the newly connected subtitles get fixed
                                    shouldFixConnectedSubtitles = true;
                                    break;
                                case BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.ChainingGeneralShotChangeBehaviorEnum.ExtendUntilShotChange:
                                    // Put the left out cue on the shot change, minus gap
                                    newLeftOutCueFrame = firstShotChangeInBetween.Value - Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap;
                                    newRightInCueFrame = bestRightInCueFrame;
                                    break;
                            }
                        }
                        else
                        {
                            // Apply the chaining
                            newLeftOutCueFrame = fixedLeftOutCueFrame.Value;
                            newRightInCueFrame = bestRightInCueFrame;
                        }
                    }
                    else
                    {
                        // Chaining not needed
                        return false;
                    }
                }
            }
            else
            {
                // No, so we will be using the "General" settings
                // Try to chain the subtitles (we can pass the original paragraphs because we haven't moved the cues)
                var fixedLeftOutCueFrame = GetFixedChainableSubtitlesLeftOutCueFrameGeneral(newLeftOutCueFrame, newRightInCueFrame, leftParagraph, rightParagraph);
                if (fixedLeftOutCueFrame != null)
                {
                    newLeftOutCueFrame = fixedLeftOutCueFrame.Value;
                }
                else
                {
                    // Chaining wasn't needed
                    return false;
                }
            }

            // Align and update cues
            AlignAndSetCue(leftParagraph, false, newLeftOutCueFrame);
            AlignAndSetCue(rightParagraph, true, newRightInCueFrame);

            // Update connected subtitles if requested
            if (shouldFixConnectedSubtitles)
            {
                FixConnectedSubtitles(leftParagraph, rightParagraph);
            }

            return true;
        }
        
        private int? GetFixedChainableSubtitlesLeftOutCueFrameGeneral(int leftOutCueFrame, int rightInCueFrame, Paragraph leftParagraph = null, Paragraph rightParagraph = null)
        {
            // Check if zones are being used
            if (Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralUseZones)
            {
                var rightInCueWithGreenZone = rightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralLeftGreenZone;
                var isInGreenZone = leftOutCueFrame > rightInCueWithGreenZone && leftOutCueFrame < (rightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralLeftRedZone);
                var isInRedZone = leftOutCueFrame >= (rightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralLeftRedZone) && leftOutCueFrame <= rightInCueFrame;

                if (isInRedZone)
                {
                    // Chain them
                    return rightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                }
                else if (isInGreenZone)
                {
                    // Enforce green zone
                    return rightInCueWithGreenZone;
                }
                else
                {
                    // Chaining not needed
                    return null;
                }
            }
            else
            {
                // No, so just check the distance in milliseconds (use original milliseconds if passed)
                double distance;
                if (leftParagraph != null && rightParagraph != null)
                {
                    distance = rightParagraph.StartTime.TotalMilliseconds - leftParagraph.EndTime.TotalMilliseconds;
                }
                else
                {
                    distance = SubtitleFormat.FramesToMilliseconds(rightInCueFrame, _frameRate) - SubtitleFormat.FramesToMilliseconds(leftOutCueFrame, _frameRate);
                }
                
                if (distance < Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralMaxGap)
                {
                    // Chain them
                    return rightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                }
                else
                {
                    // Chaining not needed
                    return null;
                }
            }
        }

        private int? GetFixedChainableSubtitlesLeftOutCueFrameInCueOnShot(int leftOutCueFrame, int rightInCueFrame)
        {
            // Check if zones are being used
            if (Configuration.Settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotUseZones)
            {
                var rightInCueWithGreenZone = rightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotLeftGreenZone;
                var isInGreenZone = leftOutCueFrame > rightInCueWithGreenZone && leftOutCueFrame < (rightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotLeftRedZone);
                var isInRedZone = leftOutCueFrame >= (rightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotLeftRedZone) && leftOutCueFrame <= rightInCueFrame;

                if (isInRedZone)
                {
                    // Chain them
                    return rightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                }
                else if (isInGreenZone)
                {
                    // Enforce green zone
                    return rightInCueWithGreenZone;
                }
                else
                {
                    // Chaining not needed
                    return null;
                }
            }
            else
            {
                // No, so just check the distance in milliseconds
                var distance = SubtitleFormat.FramesToMilliseconds(rightInCueFrame, _frameRate) - SubtitleFormat.FramesToMilliseconds(leftOutCueFrame, _frameRate);
                if (distance < Configuration.Settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotMaxGap)
                {
                    // Chain them
                    return rightInCueFrame - Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                }
                else
                {
                    // Chaining not needed
                    return null;
                }
            }
        }

        private int? GetFixedChainableSubtitlesRightInCueFrameOutCueOnShot(int leftOutCueFrame, int rightInCueFrame)
        {
            // Check if zones are being used
            if (Configuration.Settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotUseZones)
            {
                var leftOutCueWithGreenZone = leftOutCueFrame + Configuration.Settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotRightGreenZone;
                var isInGreenZone = rightInCueFrame < leftOutCueWithGreenZone && rightInCueFrame > (leftOutCueFrame + Configuration.Settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotRightRedZone);
                var isInRedZone = rightInCueFrame <= (leftOutCueFrame + Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralLeftRedZone) && rightInCueFrame >= leftOutCueFrame;

                if (isInRedZone)
                {
                    // Chain them
                    return leftOutCueFrame + Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                }
                else if (isInGreenZone)
                {
                    // Enforce green zone
                    return leftOutCueWithGreenZone;
                }
                else
                {
                    // Chaining not needed
                    return null;
                }
            }
            else
            {
                // No, so just check the distance in milliseconds
                var distance = SubtitleFormat.FramesToMilliseconds(rightInCueFrame, _frameRate) - SubtitleFormat.FramesToMilliseconds(leftOutCueFrame, _frameRate);
                if (distance < Configuration.Settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotMaxGap)
                {
                    // Chain them
                    return leftOutCueFrame + Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
                }
                else
                {
                    // Chaining not needed
                    return null;
                }
            }
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
                var bestCueFrameInfo = FindBestCueFrame(newCueFrame, isInCue);
                var bestCueFrame = bestCueFrameInfo.cueFrame;

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

        private (int cueFrame, FindBestCueResult result) FindBestCueFrame(int cueFrame, bool isInCue)
        {
            var previousShotChange = new List<int> { int.MinValue }.Concat(_shotChangesFrames).Last(x => x <= cueFrame); // will return minValue if none found
            var nextShotChange = _shotChangesFrames.Concat(new List<int> { int.MaxValue }).First(x => x >= cueFrame); // will return maxValue if none found

            // If both not found, return self
            if (previousShotChange < 0 && nextShotChange == int.MaxValue)
            {
                return (cueFrame, FindBestCueResult.NoShotChangeFound);
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
                if (Math.Abs(cueFrame - previousShotChange) <= Math.Abs(cueFrame - nextShotChange))
                {
                    return (previousShotChangeWithGap, FindBestCueResult.SnappedToRedZone);
                }
                else
                {
                    return (nextShotChangeWithGap, FindBestCueResult.SnappedToRedZone);
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
                // Other cases... Red zone snapping has priority
                if (isInPreviousShotChangeRedZone)
                {
                    // Snap to previous shot change
                    return (previousShotChangeWithGap, FindBestCueResult.SnappedToRedZone);
                }
                else if (isInNextShotChangeRedZone)
                {
                    // Snap to next shot change
                    return (nextShotChangeWithGap, FindBestCueResult.SnappedToRedZone);
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
            if (paragraph.DurationTotalMilliseconds < 0)
            {
                paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds;
            }
        }


        // Helpers

        private int? GetFirstShotChangeFrameInBetween(int leftCueFrame, int rightCueFrame)
        {
            try
            {
                return _shotChangesFrames.First(x => x >= leftCueFrame && x <= rightCueFrame);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private int? GetClosestShotChangeFrame(int cueFrame)
        {
            try
            {
                return _shotChangesFrames.Aggregate((x, y) => Math.Abs(x - cueFrame) < Math.Abs(y - cueFrame) ? x : y);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private bool IsCueOnShotChange(int cueFrame, bool isInCue)
        {
            var closestShotChangeFrame = GetClosestShotChangeFrame(cueFrame);
            if (closestShotChangeFrame != null)
            {
                if (isInCue)
                {
                    return cueFrame >= closestShotChangeFrame.Value && cueFrame <= closestShotChangeFrame.Value + Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap;
                }
                else
                {
                    return cueFrame <= closestShotChangeFrame.Value && cueFrame >= closestShotChangeFrame.Value - Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap;
                }
            }
            else
            {
                return false;
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
