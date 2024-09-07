﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class Subtitle
    {
        public const int MaxFileSize = 1024 * 1024 * 20; // 20 MB

        public List<Paragraph> Paragraphs { get; private set; }

        public string Header { get; set; } = string.Empty;
        public string Footer { get; set; } = string.Empty;

        public string FileName { get; set; }

        public static int MaximumHistoryItems => 100;

        public SubtitleFormat OriginalFormat { get; set; }
        public Encoding OriginalEncoding { get; private set; }

        public List<HistoryItem> HistoryItems { get; }
        public bool CanUndo => HistoryItems.Count > 0;

        public Subtitle() : this(new List<Paragraph>(), new List<HistoryItem>())
        {
        }

        public Subtitle(List<Paragraph> paragraphs) : this(paragraphs, new List<HistoryItem>())
        {
        }

        public Subtitle(List<HistoryItem> historyItems) : this(new List<Paragraph>(), historyItems)
        {
        }

        public Subtitle(List<Paragraph> paragraphs, List<HistoryItem> historyItems)
        {
            HistoryItems = historyItems;
            Paragraphs = paragraphs;
            FileName = "Untitled";
        }

        /// <summary>
        /// Copy constructor (without history).
        /// </summary>
        /// <param name="subtitle">Subtitle to copy</param>
        /// <param name="generateNewId">Generate new ID (guid) for paragraphs</param>
        public Subtitle(Subtitle subtitle, bool generateNewId = true)
        {
            HistoryItems = new List<HistoryItem>();

            if (subtitle == null)
            {
                FileName = "Untitled";
                Paragraphs = new List<Paragraph>();
                return;
            }

            Paragraphs = new List<Paragraph>(subtitle.Paragraphs.Count);
            foreach (var p in subtitle.Paragraphs)
            {
                Paragraphs.Add(new Paragraph(p, generateNewId));
            }

            Header = subtitle.Header;
            Footer = subtitle.Footer;
            FileName = subtitle.FileName;
            OriginalFormat = subtitle.OriginalFormat;
            OriginalEncoding = subtitle.OriginalEncoding;
        }


        /// <summary>
        /// Get the paragraph of index, null if out of bounds
        /// </summary>
        /// <param name="index">Index of wanted paragraph</param>
        /// <returns>Paragraph, null if index is index is out of bounds</returns>
        public Paragraph GetParagraphOrDefault(int index)
        {
            if (Paragraphs == null || Paragraphs.Count <= index || index < 0)
            {
                return null;
            }

            return Paragraphs[index];
        }

        public Paragraph GetParagraphOrDefaultById(string id)
        {
            return Paragraphs.Find(p => p.Id == id);
        }

        public SubtitleFormat ReloadLoadSubtitle(List<string> lines, string fileName, SubtitleFormat format, SubtitleFormat format2 = null, SubtitleFormat format3 = null)
        {
            Paragraphs.Clear();

            if (format != null && format.IsMine(lines, fileName))
            {
                format.LoadSubtitle(this, lines, fileName);
                OriginalFormat = format;
                return format;
            }

            if (format2 != null && format2.IsMine(lines, fileName))
            {
                format2.LoadSubtitle(this, lines, fileName);
                OriginalFormat = format2;
                return format2;
            }

            if (format3 != null && format3.IsMine(lines, fileName))
            {
                format3.LoadSubtitle(this, lines, fileName);
                OriginalFormat = format3;
                return format3;
            }

            foreach (var subtitleFormat in SubtitleFormat.AllSubtitleFormats)
            {
                var name = subtitleFormat.Name;
                if (format?.Name == name || format2?.Name == name || format3?.Name == name)
                {
                    continue;
                }

                if (subtitleFormat.IsMine(lines, fileName))
                {
                    subtitleFormat.LoadSubtitle(this, lines, fileName);
                    OriginalFormat = subtitleFormat;
                    return subtitleFormat;
                }
            }

            return null;
        }

        /// <summary>
        /// Load a subtitle from position 0 to the end of a stream.
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="fileExtension">File extension, e.g. ".srt".</param>
        /// <returns>A Subtitle if a valid subtitle format is found, otherwise null.</returns>
        public static Subtitle Parse(Stream stream, string fileExtension)
        {
            stream.Position = 0;
            using (var reader = new StreamReader(stream))
            {
                var lines = reader.ReadToEnd().SplitToLines();
                return Parse(lines, fileExtension);
            }
        }

        /// <summary>
        /// Load a subtitle for a list of text lines.
        /// </summary>
        /// <param name="lines">A list of text lines.</param>
        /// <param name="fileExtension">File extension, e.g. ".srt".</param>
        /// <returns>A Subtitle if a valid subtitle format is found, otherwise null.</returns>
        public static Subtitle Parse(List<string> lines, string fileExtension)
        {
            var subtitle = new Subtitle();
            var fileName = string.Empty;
            var ext = $".{fileExtension.ToLowerInvariant().TrimStart('.')}";

            foreach (var subtitleFormat in SubtitleFormat.AllSubtitleFormats.Where(p => p.Extension == ext))
            {
                if (subtitleFormat.IsMine(lines, string.Empty))
                {
                    subtitleFormat.LoadSubtitle(subtitle, lines, fileName);
                    return subtitle;
                }
            }

            foreach (var subtitleFormat in SubtitleFormat.AllSubtitleFormats.Where(p => p.Extension != ext))
            {
                if (subtitleFormat.IsMine(lines, string.Empty))
                {
                    subtitleFormat.LoadSubtitle(subtitle, lines, fileName);
                    return subtitle;
                }
            }

            return null;
        }

        /// <summary>
        /// Load a subtitle from a file.
        /// Check "OriginalFormat" to see what subtitle format was used.
        /// Check "OriginalEncoding" to see what text encoding was used.
        /// </summary>
        /// <param name="fileName">File name of subtitle to load.</param>
        /// <returns>Loaded subtitle, null if file is not known subtitle format.</returns>
        public static Subtitle Parse(string fileName) => Parse(fileName, useThisEncoding: null);

        /// <summary>
        /// Load a subtitle from a file.
        /// Check "OriginalFormat" to see what subtitle format was used.
        /// Check "OriginalEncoding" to see what text encoding was used.
        /// </summary>
        /// <param name="fileName">File name of subtitle to load.</param>
        /// <param name="useThisEncoding">Encoding to read file with.</param>
        /// <returns>Loaded subtitle, null if file is not known subtitle format.</returns>
        public static Subtitle Parse(string fileName, Encoding useThisEncoding)
        {
            var subtitle = new Subtitle();
            var format = subtitle.LoadSubtitle(fileName, out var encodingUsed, useThisEncoding);
            if (format == null)
            {
                return null;
            }

            subtitle.OriginalEncoding = encodingUsed;
            return subtitle;
        }

        /// <summary>
        /// Load a subtitle from a file.
        /// Check "OriginalFormat" to see what subtitle format was used.
        /// Check "OriginalEncoding" to see what text encoding was used.
        /// </summary>
        /// <param name="fileName">File name of subtitle to load.</param>
        /// <param name="format">SubtitleFormat to use.</param>
        /// <returns>Loaded subtitle, null if format does not match the file.</returns>
        public static Subtitle Parse(string fileName, SubtitleFormat format)
        {
            return Parse(fileName, new List<SubtitleFormat> { format });
        }

        /// <summary>
        /// Load a subtitle from a file.
        /// Check "OriginalFormat" to see what subtitle format was used.
        /// Check "OriginalEncoding" to see what text encoding was used.
        /// </summary>
        /// <param name="fileName">File name of subtitle to load.</param>
        /// <param name="formatsToLookFor">List of subtitle formats to use.</param>
        /// <returns>Loaded subtitle, null if file is not matched by any format in formatsToLookFor.</returns>
        public static Subtitle Parse(string fileName, List<SubtitleFormat> formatsToLookFor)
        {
            var subtitle = new Subtitle
            {
                FileName = fileName
            };

            List<string> lines;
            try
            {
                lines = ReadLinesFromFile(fileName, null, out var encoding);
                subtitle.OriginalEncoding = encoding;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                return null;
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            foreach (var subtitleFormat in formatsToLookFor.Where(p => p.Extension == ext && !p.Name.StartsWith("Unknown", StringComparison.Ordinal)))
            {
                if (subtitleFormat.IsMine(lines, fileName))
                {
                    subtitle.FinalizeFormat(fileName, false, null, lines, subtitleFormat, true);
                    return subtitle;
                }
            }

            foreach (var subtitleFormat in formatsToLookFor.Where(p => p.Extension != ext || p.Name.StartsWith("Unknown", StringComparison.Ordinal)))
            {
                if (subtitleFormat.IsMine(lines, fileName))
                {
                    subtitle.FinalizeFormat(fileName, false, null, lines, subtitleFormat, true);
                    return subtitle;
                }
            }

            return null;
        }

        public SubtitleFormat LoadSubtitle(string fileName, out Encoding encoding, Encoding useThisEncoding)
        {
            return LoadSubtitle(fileName, out encoding, useThisEncoding, false);
        }

        public SubtitleFormat LoadSubtitle(string fileName, out Encoding encoding, Encoding useThisEncoding, bool batchMode, double? sourceFrameRate = null, bool loadSubtitle = true)
        {
            FileName = fileName;
            Paragraphs = new List<Paragraph>();
            List<string> lines;
            try
            {
                lines = ReadLinesFromFile(fileName, useThisEncoding, out encoding);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                encoding = Encoding.UTF8;
                return null;
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            foreach (var subtitleFormat in SubtitleFormat.AllSubtitleFormats.Where(p => p.Extension == ext && !p.Name.StartsWith("Unknown", StringComparison.Ordinal)))
            {
                if (subtitleFormat.IsMine(lines, fileName))
                {
                    return FinalizeFormat(fileName, batchMode, sourceFrameRate, lines, subtitleFormat, loadSubtitle);
                }
            }

            foreach (var subtitleFormat in SubtitleFormat.AllSubtitleFormats.Where(p => p.Extension != ext || p.Name.StartsWith("Unknown", StringComparison.Ordinal)))
            {
                if (subtitleFormat.IsMine(lines, fileName))
                {
                    return FinalizeFormat(fileName, batchMode, sourceFrameRate, lines, subtitleFormat, loadSubtitle);
                }
            }

            if (useThisEncoding == null)
            {
                return LoadSubtitle(fileName, out encoding, Encoding.Unicode);
            }

            return null;
        }

        private static List<string> ReadLinesFromFile(string fileName, Encoding useThisEncoding, out Encoding encoding)
        {
            StreamReader sr;
            if (useThisEncoding != null)
            {
                sr = new StreamReader(fileName, useThisEncoding);
            }
            else
            {
                try
                {
                    sr = new StreamReader(fileName, LanguageAutoDetect.GetEncodingFromFile(fileName), true);
                }
                catch
                {
                    var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    sr = new StreamReader(fs);
                }
            }

            encoding = sr.CurrentEncoding;
            var lines = sr.ReadToEnd().SplitToLines();
            sr.Close();
            sr.Dispose();
            return lines;
        }

        private SubtitleFormat FinalizeFormat(string fileName, bool batchMode, double? sourceFrameRate, List<string> lines, SubtitleFormat subtitleFormat, bool loadSubtitle)
        {
            Header = null;
            subtitleFormat.BatchMode = batchMode;
            subtitleFormat.BatchSourceFrameRate = sourceFrameRate;
            if (loadSubtitle)
            {
                subtitleFormat.LoadSubtitle(this, lines, fileName);
            }
            OriginalFormat = subtitleFormat;
            return subtitleFormat;
        }

        public void MakeHistoryForUndo(string description, string subtitleFormatFriendlyName, DateTime fileModified, Subtitle original, string originalSubtitleFileName, int lineNumber, int linePosition, int linePositionOriginal)
        {
            // don't fill memory with history - use a max rollback points
            if (HistoryItems.Count > MaximumHistoryItems)
            {
                HistoryItems.RemoveAt(0);
            }

            HistoryItems.Add(new HistoryItem(HistoryItems.Count, this, description, FileName, fileModified, subtitleFormatFriendlyName, original, originalSubtitleFileName, lineNumber, linePosition, linePositionOriginal));
        }

        public string UndoHistory(int index, out string subtitleFormatFriendlyName, out DateTime fileModified, out Subtitle originalSubtitle, out string originalSubtitleFileName)
        {
            Paragraphs.Clear();
            foreach (var p in HistoryItems[index].Subtitle.Paragraphs)
            {
                Paragraphs.Add(new Paragraph(p));
            }

            subtitleFormatFriendlyName = HistoryItems[index].SubtitleFormatFriendlyName;
            FileName = HistoryItems[index].FileName;
            fileModified = HistoryItems[index].FileModified;
            originalSubtitle = new Subtitle(HistoryItems[index].OriginalSubtitle);
            originalSubtitleFileName = HistoryItems[index].OriginalSubtitleFileName;
            Header = HistoryItems[index].Subtitle.Header;

            return FileName;
        }

        /// <summary>
        /// Creates subtitle as text in its native format.
        /// </summary>
        /// <param name="format">Format to output</param>
        /// <returns>Native format as text string</returns>
        public string ToText(SubtitleFormat format)
        {
            return format.ToText(this, Path.GetFileNameWithoutExtension(FileName));
        }

        public void AddTimeToAllParagraphs(TimeSpan time)
        {
            double milliseconds = time.TotalMilliseconds;
            foreach (var p in Paragraphs)
            {
                p.StartTime.TotalMilliseconds += milliseconds;
                p.EndTime.TotalMilliseconds += milliseconds;
            }
        }

        public void ChangeFrameRate(double oldFrameRate, double newFrameRate)
        {
            var factor = SubtitleFormat.GetFrameForCalculation(oldFrameRate) / SubtitleFormat.GetFrameForCalculation(newFrameRate);
            foreach (var p in Paragraphs)
            {
                p.StartTime.TotalMilliseconds *= factor;
                p.EndTime.TotalMilliseconds *= factor;
            }
        }

        public void AdjustDisplayTimeUsingPercent(double percent, List<int> selectedIndexes, List<double> shotChanges = null, bool enforceDurationLimits = true)
        {
            for (int i = 0; i < Paragraphs.Count; i++)
            {
                if (selectedIndexes == null || selectedIndexes.Contains(i))
                {
                    double nextStartMilliseconds = double.MaxValue;
                    if (i + 1 < Paragraphs.Count)
                    {
                        nextStartMilliseconds = Paragraphs[i + 1].StartTime.TotalMilliseconds;
                    }

                    double newEndMilliseconds = Paragraphs[i].EndTime.TotalMilliseconds;
                    newEndMilliseconds = Paragraphs[i].StartTime.TotalMilliseconds + (((newEndMilliseconds - Paragraphs[i].StartTime.TotalMilliseconds) * percent) / 100.0);

                    // fix too short duration
                    if (enforceDurationLimits)
                    {
                        var minDur = Math.Max(Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds, 100);
                        if (Paragraphs[i].StartTime.TotalMilliseconds + minDur > newEndMilliseconds)
                        {
                            newEndMilliseconds = Paragraphs[i].StartTime.TotalMilliseconds + minDur;
                        }
                    }

                    // handle overlap with next
                    if (newEndMilliseconds > nextStartMilliseconds)
                    {
                        newEndMilliseconds = nextStartMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }

                    // handle shot change if supplied -- keep earliest time
                    if (shotChanges != null)
                    {
                        double nextShotChangeMilliseconds = ShotChangeHelper.GetNextShotChangeMinusGapInMs(shotChanges, Paragraphs[i].EndTime) ?? double.MaxValue;
                        if (newEndMilliseconds > nextShotChangeMilliseconds)
                        {
                            newEndMilliseconds = nextShotChangeMilliseconds;
                        }
                    }

                    // max duration
                    if (enforceDurationLimits)
                    {
                        var dur = newEndMilliseconds - Paragraphs[i].StartTime.TotalMilliseconds;
                        if (dur > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                        {
                            newEndMilliseconds = Paragraphs[i].StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                        }
                    }

                    if (percent > 100 && newEndMilliseconds > Paragraphs[i].EndTime.TotalMilliseconds || percent < 100)
                    {
                        Paragraphs[i].EndTime.TotalMilliseconds = newEndMilliseconds;
                    }
                }
            }
        }

        public void AdjustDisplayTimeUsingSeconds(double seconds, List<int> selectedIndexes, List<double> shotChanges = null, bool enforceDurationLimits = true)
        {
            if (Math.Abs(seconds) < 0.001)
            {
                return;
            }

            var adjustMs = seconds * TimeCode.BaseUnit;
            if (selectedIndexes != null)
            {
                foreach (var idx in selectedIndexes)
                {
                    AdjustDisplayTimeUsingMilliseconds(idx, adjustMs, shotChanges, enforceDurationLimits);
                }
            }
            else
            {
                for (var idx = 0; idx < Paragraphs.Count; idx++)
                {
                    AdjustDisplayTimeUsingMilliseconds(idx, adjustMs, shotChanges, enforceDurationLimits);
                }
            }
        }

        private void AdjustDisplayTimeUsingMilliseconds(int idx, double ms, List<double> shotChanges = null, bool enforceDurationLimits = true)
        {
            var p = Paragraphs[idx];
            var nextStartTimeInMs = double.MaxValue;
            if (idx + 1 < Paragraphs.Count)
            {
                nextStartTimeInMs = Paragraphs[idx + 1].StartTime.TotalMilliseconds;
            }
            var newEndTimeInMs = p.EndTime.TotalMilliseconds + ms;

            // fix too short duration
            if (enforceDurationLimits)
            {
                var minDur = Math.Max(Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds, 100);
                if (p.StartTime.TotalMilliseconds + minDur > newEndTimeInMs)
                {
                    newEndTimeInMs = p.StartTime.TotalMilliseconds + minDur;
                }
            }

            // handle overlap with next
            if (newEndTimeInMs > nextStartTimeInMs - Configuration.Settings.General.MinimumMillisecondsBetweenLines)
            {
                newEndTimeInMs = nextStartTimeInMs - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
            }

            // handle shot change if supplied -- keep earliest time
            if (shotChanges != null)
            {
                newEndTimeInMs = Math.Min(newEndTimeInMs, ShotChangeHelper.GetNextShotChangeMinusGapInMs(shotChanges, p.EndTime) ?? double.MaxValue);
            }

            // max duration
            if (enforceDurationLimits)
            {
                var dur = newEndTimeInMs - p.StartTime.TotalMilliseconds;
                if (dur > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    newEndTimeInMs = p.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                }
            }

            if (ms > 0 && newEndTimeInMs < p.EndTime.TotalMilliseconds || ms < 0 && newEndTimeInMs > p.EndTime.TotalMilliseconds)
            {
                return; // do not adjust wrong way
            }

            p.EndTime.TotalMilliseconds = newEndTimeInMs;
        }

        public void RecalculateDisplayTimes(double maxCharPerSec, List<int> selectedIndexes, double optimalCharPerSec, bool extendOnly = false, List<double> shotChanges = null, bool enforceDurationLimits = true)
        {
            if (selectedIndexes != null)
            {
                foreach (var index in selectedIndexes)
                {
                    RecalculateDisplayTime(maxCharPerSec, index, optimalCharPerSec, extendOnly, false, shotChanges, enforceDurationLimits);
                }
            }
            else
            {
                for (int i = 0; i < Paragraphs.Count; i++)
                {
                    RecalculateDisplayTime(maxCharPerSec, i, optimalCharPerSec, extendOnly, false, shotChanges, enforceDurationLimits);
                }
            }
        }

        public void RecalculateDisplayTime(double maxCharactersPerSecond, int index, double optimalCharactersPerSeconds, bool extendOnly = false, bool onlyOptimal = false, List<double> shotChanges = null, bool enforceDurationLimits = true)
        {
            var p = GetParagraphOrDefault(index);
            if (p == null)
            {
                return;
            }

            var originalEndTime = p.EndTime.TotalMilliseconds;

            var duration = Utilities.GetOptimalDisplayMilliseconds(p.Text, optimalCharactersPerSeconds, onlyOptimal, enforceDurationLimits);
            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + duration;
            while (p.GetCharactersPerSecond() > maxCharactersPerSecond)
            {
                duration++;
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + duration;
            }

            if (extendOnly && p.EndTime.TotalMilliseconds < originalEndTime)
            {
                p.EndTime.TotalMilliseconds = originalEndTime;
            }

            var next = GetParagraphOrDefault(index + 1);
            var wantedEndMs = p.EndTime.TotalMilliseconds;
            var bestEndMs = double.MaxValue;

            // First check for next subtitle
            if (next != null)
            {
                bestEndMs = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
            }

            // Then check for next shot change (if option is checked, and if any are supplied) -- keeping earliest time
            if (shotChanges != null)
            {
                bestEndMs = Math.Min(bestEndMs, ShotChangeHelper.GetNextShotChangeMinusGapInMs(shotChanges, new TimeCode(originalEndTime)) ?? double.MaxValue);
            }

            p.EndTime.TotalMilliseconds = wantedEndMs <= bestEndMs ? wantedEndMs : bestEndMs;

            if (p.DurationTotalMilliseconds <= 0)
            {
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 1;
            }
        }

        public void SetFixedDuration(List<int> selectedIndexes, double fixedDurationMilliseconds, List<double> shotChanges = null)
        {
            for (var i = 0; i < Paragraphs.Count; i++)
            {
                if (selectedIndexes == null || selectedIndexes.Contains(i))
                {
                    var p = GetParagraphOrDefault(i);
                    if (p == null)
                    {
                        continue;
                    }

                    var next = GetParagraphOrDefault(i + 1);
                    var wantedEndMs = p.StartTime.TotalMilliseconds + fixedDurationMilliseconds;
                    var bestEndMs = double.MaxValue;

                    // First check for next subtitle
                    if (next != null)
                    {
                        bestEndMs = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }

                    // Then check for next shot change (if option is checked, and if any are supplied) -- keeping earliest time
                    if (shotChanges != null)
                    {
                        bestEndMs = Math.Min(bestEndMs, ShotChangeHelper.GetNextShotChangeMinusGapInMs(shotChanges, p.EndTime) ?? double.MaxValue);
                    }

                    p.EndTime.TotalMilliseconds = wantedEndMs <= bestEndMs ? wantedEndMs : bestEndMs;

                    if (p.DurationTotalMilliseconds <= 0)
                    {
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 1;
                    }
                }
            }
        }

        public void Renumber(int startNumber = 1)
        {
            var number = startNumber;
            var l = Paragraphs.Count + number;
            while (number < l)
            {
                var p = Paragraphs[number - startNumber];
                p.Number = number++;
            }
        }

        public int GetIndex(Paragraph p)
        {
            if (p == null)
            {
                return -1;
            }

            var index = Paragraphs.IndexOf(p);
            if (index >= 0)
            {
                return index;
            }

            for (var i = 0; i < Paragraphs.Count; i++)
            {
                if (p.Id == Paragraphs[i].Id)
                {
                    return i;
                }

                if (i < Paragraphs.Count - 1 && p.Id == Paragraphs[i + 1].Id)
                {
                    return i + 1;
                }

                if (Math.Abs(p.StartTime.TotalMilliseconds - Paragraphs[i].StartTime.TotalMilliseconds) < 0.1 &&
                    Math.Abs(p.EndTime.TotalMilliseconds - Paragraphs[i].EndTime.TotalMilliseconds) < 0.1)
                {
                    return i;
                }

                if (p.Number == Paragraphs[i].Number && (Math.Abs(p.StartTime.TotalMilliseconds - Paragraphs[i].StartTime.TotalMilliseconds) < 0.1 ||
                    Math.Abs(p.EndTime.TotalMilliseconds - Paragraphs[i].EndTime.TotalMilliseconds) < 0.1))
                {
                    return i;
                }

                if (p.Text == Paragraphs[i].Text && (Math.Abs(p.StartTime.TotalMilliseconds - Paragraphs[i].StartTime.TotalMilliseconds) < 0.1 ||
                    Math.Abs(p.EndTime.TotalMilliseconds - Paragraphs[i].EndTime.TotalMilliseconds) < 0.1))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Get paragraph index by time in seconds
        /// </summary>
        public int GetIndex(double seconds)
        {
            var totalMilliseconds = seconds * TimeCode.BaseUnit;
            for (var i = 0; i < Paragraphs.Count; i++)
            {
                var p = Paragraphs[i];
                if (totalMilliseconds >= p.StartTime.TotalMilliseconds && totalMilliseconds <= p.EndTime.TotalMilliseconds)
                {
                    return i;
                }
            }

            return -1;
        }

        public Paragraph GetFirstAlike(Paragraph p)
        {
            foreach (var item in Paragraphs)
            {
                if (Math.Abs(p.StartTime.TotalMilliseconds - item.StartTime.TotalMilliseconds) < 0.1 &&
                    Math.Abs(p.EndTime.TotalMilliseconds - item.EndTime.TotalMilliseconds) < 0.1 &&
                    p.Text == item.Text)
                {
                    return item;
                }
            }

            return null;
        }

        public Paragraph GetNearestAlike(Paragraph p)
        {
            foreach (var item in Paragraphs)
            {
                if (Math.Abs(p.StartTime.TotalMilliseconds - item.StartTime.TotalMilliseconds) < 0.1 &&
                    Math.Abs(p.EndTime.TotalMilliseconds - item.EndTime.TotalMilliseconds) < 0.1 &&
                    p.Text == item.Text)
                {
                    return item;
                }
            }

            foreach (var item in Paragraphs)
            {
                if (Math.Abs(p.StartTime.TotalMilliseconds - item.StartTime.TotalMilliseconds) < 0.1 &&
                    Math.Abs(p.EndTime.TotalMilliseconds - item.EndTime.TotalMilliseconds) < 0.1)
                {
                    return item;
                }
            }

            return Paragraphs.OrderBy(s => Math.Abs(s.StartTime.TotalMilliseconds - p.StartTime.TotalMilliseconds)).FirstOrDefault();
        }

        public int RemoveEmptyLines()
        {
            var count = Paragraphs.Count;
            if (count <= 0)
            {
                return 0;
            }

            var firstNumber = Paragraphs[0].Number;
            for (var i = Paragraphs.Count - 1; i >= 0; i--)
            {
                var p = Paragraphs[i];
                if (p.Text.IsOnlyControlCharactersOrWhiteSpace())
                {
                    Paragraphs.RemoveAt(i);
                }
            }

            if (count != Paragraphs.Count)
            {
                Renumber(firstNumber);
            }

            return count - Paragraphs.Count;
        }

        /// <summary>
        /// Removes paragraphs by a list of indices
        /// </summary>
        /// <param name="indices">Indices of paragraphs/lines to delete</param>
        /// <returns>Number of lines deleted</returns>
        public int RemoveParagraphsByIndices(IEnumerable<int> indices)
        {
            var count = 0;
            foreach (var index in indices.OrderByDescending(p => p))
            {
                if (index >= 0 && index < Paragraphs.Count)
                {
                    Paragraphs.RemoveAt(index);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Removes paragraphs by a list of IDs
        /// </summary>
        /// <param name="ids">IDs of paragraphs/lines to delete</param>
        /// <returns>Number of lines deleted</returns>
        public int RemoveParagraphsByIds(IEnumerable<string> ids)
        {
            var beforeCount = Paragraphs.Count;
            Paragraphs = Paragraphs.Where(p => !ids.Contains(p.Id)).ToList();
            return beforeCount - Paragraphs.Count;
        }

        /// <summary>
        /// Sort subtitle paragraphs
        /// </summary>
        /// <param name="sortCriteria">Paragraph sort criteria</param>
        public void Sort(SubtitleSortCriteria sortCriteria)
        {
            switch (sortCriteria)
            {
                case SubtitleSortCriteria.Number:
                    Paragraphs = Paragraphs.OrderBy(p => p.Number).ThenBy(p => p.StartTime.TotalMilliseconds).ToList();
                    break;
                case SubtitleSortCriteria.StartTime:
                    Paragraphs = Paragraphs.OrderBy(p => p.StartTime.TotalMilliseconds).ThenBy(p => p.Number).ToList();
                    break;
                case SubtitleSortCriteria.EndTime:
                    Paragraphs = Paragraphs.OrderBy(p => p.EndTime.TotalMilliseconds).ThenBy(p => p.Number).ToList();
                    break;
                case SubtitleSortCriteria.Duration:
                    Paragraphs = Paragraphs.OrderBy(p => p.DurationTotalMilliseconds).ThenBy(p => p.Number).ToList();
                    break;
                case SubtitleSortCriteria.Gap:
                    var lookupDictionary = new Dictionary<string, double>();
                    for (var index = 0; index < Paragraphs.Count; index++)
                    {
                        var paragraph = Paragraphs[index];
                        var next = GetParagraphOrDefault(index + 1);
                        if (next == null)
                        {
                            lookupDictionary.Add(paragraph.Id, 100_000);
                        }
                        else
                        {
                            var gapMilliseconds = next.StartTime.TotalMilliseconds - paragraph.EndTime.TotalMilliseconds;
                            lookupDictionary.Add(paragraph.Id, gapMilliseconds);
                        }
                    }

                    Paragraphs = Paragraphs.OrderBy(p => lookupDictionary[p.Id]).ThenBy(p => p.Number).ToList();
                    break;
                case SubtitleSortCriteria.Text:
                    Paragraphs = Paragraphs.OrderBy(p => p.Text, StringComparer.Ordinal).ThenBy(p => p.Number).ToList();
                    break;
                case SubtitleSortCriteria.TextMaxLineLength:
                    Paragraphs = Paragraphs.OrderBy(p => Utilities.GetMaxLineLength(p.Text)).ThenBy(p => p.Number).ToList();
                    break;
                case SubtitleSortCriteria.TextTotalLength:
                    Paragraphs = Paragraphs.OrderBy(p => p.Text.Length).ThenBy(p => p.Number).ToList();
                    break;
                case SubtitleSortCriteria.TextNumberOfLines:
                    Paragraphs = Paragraphs.OrderBy(p => p.NumberOfLines).ThenBy(p => p.Number).ToList();
                    break;
                case SubtitleSortCriteria.TextCharactersPerSeconds:
                    Paragraphs = Paragraphs.OrderBy(p => p.GetCharactersPerSecond()).ThenBy(p => p.Number).ToList();
                    break;
                case SubtitleSortCriteria.WordsPerMinute:
                    Paragraphs = Paragraphs.OrderBy(p => p.WordsPerMinute).ThenBy(p => p.Number).ToList();
                    break;
                case SubtitleSortCriteria.Style:
                    Paragraphs = Paragraphs.OrderBy(p => p.Extra, StringComparer.Ordinal).ThenBy(p => p.Number).ToList();
                    break;
                case SubtitleSortCriteria.Actor:
                    Paragraphs = Paragraphs.OrderBy(p => p.Actor, StringComparer.Ordinal).ThenBy(p => p.Number).ToList();
                    break;
            }
        }

        public int InsertParagraphInCorrectTimeOrder(Paragraph newParagraph)
        {
            for (int i = 0; i < Paragraphs.Count; i++)
            {
                var p = Paragraphs[i];
                if (newParagraph.StartTime.TotalMilliseconds < p.StartTime.TotalMilliseconds)
                {
                    Paragraphs.Insert(i, newParagraph);
                    return i;
                }
            }
            Paragraphs.Add(newParagraph);
            return Paragraphs.Count - 1;
        }

        public Paragraph GetFirstParagraphOrDefaultByTime(double milliseconds)
        {
            foreach (var p in Paragraphs)
            {
                if (p.StartTime.TotalMilliseconds < milliseconds && milliseconds < p.EndTime.TotalMilliseconds)
                {
                    return p;
                }
            }

            return null;
        }

        /// <summary>
        /// Fast hash code for subtitle - includes pre (encoding atm) + header + number + start + end + text + style + actor + extra.
        /// </summary>
        /// <returns>Hash value that can be used for quick compare</returns>
        public int GetFastHashCode(string pre)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                if (pre != null)
                {
                    hash = hash * 23 + pre.GetHashCode();
                }
                if (Header != null)
                {
                    hash = hash * 23 + Header.Trim().GetHashCode();
                }
                if (Footer != null)
                {
                    hash = hash * 23 + Footer.Trim().GetHashCode();
                }
                var max = Paragraphs.Count;
                for (int i = 0; i < max; i++)
                {
                    var p = Paragraphs[i];
                    hash = hash * 23 + p.Number.GetHashCode();
                    hash = hash * 23 + p.StartTime.TotalMilliseconds.GetHashCode();
                    hash = hash * 23 + p.EndTime.TotalMilliseconds.GetHashCode();
                    hash = hash * 23 + p.Text.GetHashCode();
                    if (p.Style != null)
                    {
                        hash = hash * 23 + p.Style.GetHashCode();
                    }
                    if (p.Extra != null)
                    {
                        hash = hash * 23 + p.Extra.GetHashCode();
                    }
                    if (p.Actor != null)
                    {
                        hash = hash * 23 + p.Actor.GetHashCode();
                    }
                    hash = hash * 23 + p.Layer.GetHashCode();
                }

                return hash;
            }
        }

        /// <summary>
        /// Fast hash code for subtitle text.
        /// </summary>
        /// <returns>Hash value that can be used for quick text compare</returns>
        public int GetFastHashCodeTextOnly()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = 17;
                var max = Paragraphs.Count;
                for (var i = 0; i < max; i++)
                {
                    var p = Paragraphs[i];
                    hash = hash * 23 + p.Text.GetHashCode();
                }

                return hash;
            }
        }

        /// <summary>
        /// Concatenates all Paragraphs Text property, using the default NewLine string between each Text.
        /// </summary>
        /// <returns>Concatenated Text property of all Paragraphs.</returns>
        public string GetAllTexts()
        {
            var max = Paragraphs.Count;
            const int averageLength = 40;
            var sb = new StringBuilder(max * averageLength);
            for (var index = 0; index < max; index++)
            {
                sb.AppendLine(Paragraphs[index].Text);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Concatenates all Paragraphs Text property, using the default NewLine string between each Text.
        /// </summary>
        /// <returns>Concatenated Text property of all Paragraphs.</returns>
        public string GetAllTexts(int stopAfterBytes)
        {
            var max = Paragraphs.Count;
            const int averageLength = 40;
            var sb = new StringBuilder(max * averageLength);
            for (var index = 0; index < max; index++)
            {
                sb.AppendLine(Paragraphs[index].Text);
                if (sb.Length > stopAfterBytes)
                {
                    return sb.ToString();
                }
            }

            return sb.ToString();
        }
    }
}
