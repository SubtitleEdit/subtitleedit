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
        private readonly bool _alignTimeCodes;

        private List<double> _timeCodes;
        private List<double> _shotChanges;
        private List<int> _shotChangesFrames;

        public ProgressChangedDelegate ProgressChanged { get; set; }

        public TimeCodesBeautifier(Subtitle subtitle, double frameRate, bool alignTimeCodes, List<double> timeCodes, List<double> shotChanges)
        {
            _subtitle = subtitle;
            _frameRate = frameRate;
            _alignTimeCodes = alignTimeCodes;
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
                        FixInCue(p);
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
                        FixOutCue(p);
                    }

                    // Report progress
                    var progress = ((pass + 1) / (double)amountOfPasses) * (p / (double)_subtitle.Paragraphs.Count);
                    ProgressChanged.Invoke(progress);
                }
            }
        }

        private bool FixConnectedSubtitles(bool isInCue, Paragraph left = null, Paragraph right = null)
        {
            if (left == null || right == null)
            {
                return false;
            }

            var distance = right.StartTime.TotalMilliseconds - left.EndTime.TotalMilliseconds;
            var subtitlesAreConnected = distance < Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesTreatConnected;

            if (subtitlesAreConnected)
            {
                // TODO fix
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool FixChainableSubtitles(bool isInCue, Paragraph left = null, Paragraph right = null)
        {
            if (left == null || right == null)
            {
                return false;
            }

            // TODO fix


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
            var newTime = isInCue ? _subtitle.Paragraphs[index].StartTime.TotalMilliseconds : _subtitle.Paragraphs[index].EndTime.TotalMilliseconds;

            // Check if we should do something with shot changes
            if (_shotChangesFrames.Count > 0)
            {
                newTime = FindNewBestCueTime(newTime, isInCue);
            }

            // Check if we should align to time codes
            if (_alignTimeCodes)
            {
                // Check if we have extracted exact time codes
                if (_timeCodes.Count > 0)
                {
                    newTime = _timeCodes.Aggregate((x, y) => Math.Abs(x - newTime) < Math.Abs(y - newTime) ? x : y);
                }
                else
                {
                    var newTimeFrames = SubtitleFormat.MillisecondsToFrames(newTime, _frameRate);
                    newTime = SubtitleFormat.FramesToMilliseconds(newTimeFrames, _frameRate);
                }
            }

            // Finally, update time
            if (isInCue)
            {
                _subtitle.Paragraphs[index].StartTime.TotalMilliseconds = newTime;
            }
            else
            {
                _subtitle.Paragraphs[index].EndTime.TotalMilliseconds = newTime;
            }
        }

        private double FindNewBestCueTime(double cueTime, bool isInCue)
        {
            var cueTimeFrame = SubtitleFormat.MillisecondsToFrames(cueTime, _frameRate);
            var previousShotChange = new List<int> { -1 }.Concat(_shotChangesFrames).Last(x => x <= cueTimeFrame); // will return -1 if non found
            var nextShotChange = _shotChangesFrames.Concat(new List<int> { int.MaxValue }).First(x => x >= cueTimeFrame); // will return maxValue if non found

            // If both not found, return self
            if (previousShotChange < 0 && nextShotChange == int.MaxValue)
            {
                return cueTime;
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
                
                isInPreviousShotChangeGreenZone = cueTimeFrame < previousShotChangeWithGreenZone && cueTimeFrame > (previousShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.InCuesRightRedZone);
                isInPreviousShotChangeRedZone = cueTimeFrame <= (previousShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.InCuesRightRedZone) && cueTimeFrame >= previousShotChange;
                isInNextShotChangeGreenZone = cueTimeFrame > nextShotChangeWithGreenZone && cueTimeFrame < (nextShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.InCuesLeftRedZone);
                isInNextShotChangeRedZone = cueTimeFrame >= (nextShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.InCuesLeftRedZone) && cueTimeFrame <= nextShotChange;

                previousShotChangeWithGap = previousShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap;
                nextShotChangeWithGap = nextShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap;
            }
            else
            {
                previousShotChangeWithGreenZone = previousShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesRightGreenZone;
                nextShotChangeWithGreenZone = nextShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesLeftGreenZone;
                
                isInPreviousShotChangeGreenZone = cueTimeFrame < previousShotChangeWithGreenZone && cueTimeFrame > (previousShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesRightRedZone);
                isInPreviousShotChangeRedZone = cueTimeFrame <= (previousShotChange + Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesRightRedZone) && cueTimeFrame >= previousShotChange;
                isInNextShotChangeGreenZone = cueTimeFrame > nextShotChangeWithGreenZone && cueTimeFrame < (nextShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesLeftRedZone);
                isInNextShotChangeRedZone = cueTimeFrame >= (nextShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesLeftRedZone) && cueTimeFrame <= nextShotChange;

                previousShotChangeWithGap = previousShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap;
                nextShotChangeWithGap = nextShotChange - Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap;
            }
            
            if (isInPreviousShotChangeRedZone && isInNextShotChangeRedZone)
            {
                // We are in both red zones! Snap to closest shot change
                if (Math.Abs(cueTimeFrame - previousShotChange) < Math.Abs(cueTimeFrame - nextShotChange))
                {
                    return SubtitleFormat.FramesToMilliseconds(previousShotChangeWithGap, _frameRate);
                }
                else
                {
                    return SubtitleFormat.FramesToMilliseconds(nextShotChangeWithGap, _frameRate);
                }
            }
            else if (isInPreviousShotChangeGreenZone && isInNextShotChangeGreenZone)
            {
                // We are in both green zones! Take one with least "violation"
                if (Math.Abs(previousShotChangeWithGreenZone - nextShotChange) > Math.Abs(nextShotChangeWithGreenZone - previousShotChange))
                {
                    return SubtitleFormat.FramesToMilliseconds(previousShotChangeWithGreenZone, _frameRate);
                }
                else
                {
                    return SubtitleFormat.FramesToMilliseconds(nextShotChangeWithGreenZone, _frameRate);
                }
            }
            else
            {
                if (isInPreviousShotChangeRedZone)
                {
                    // Snap to previous shot change
                    return SubtitleFormat.FramesToMilliseconds(previousShotChangeWithGap, _frameRate);
                }
                else if (isInNextShotChangeRedZone)
                {
                    // Snap to next shot change
                    return SubtitleFormat.FramesToMilliseconds(nextShotChangeWithGap, _frameRate);
                }
                else if (isInPreviousShotChangeGreenZone)
                {
                    // Enforce green zone from previous shot change, but shouldn't exceed next shot change
                    return SubtitleFormat.FramesToMilliseconds(Math.Min(previousShotChangeWithGreenZone, nextShotChange), _frameRate);
                }
                else if (isInNextShotChangeGreenZone)
                {
                    // Enforce green zone from next shot change, but shouldn't exceed previous shot change
                    return SubtitleFormat.FramesToMilliseconds(Math.Max(nextShotChangeWithGreenZone, previousShotChange), _frameRate);
                }
            }

            return cueTime;
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
