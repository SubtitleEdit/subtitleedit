using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public class Subtitle
    {
        public const int MaxFileSize = 1024 * 1024 * 20; // 20 MB

        public List<Paragraph> Paragraphs { get; private set; }

        public string Header { get; set; } = string.Empty;
        public string Footer { get; set; } = string.Empty;

        public string FileName { get; set; }

        public static int MaximumHistoryItems => 100;

        public SubtitleFormat OriginalFormat { get; private set; }

        public List<HistoryItem> HistoryItems { get; }
        public bool CanUndo => HistoryItems.Count > 0;

        public Subtitle()
        {
            Paragraphs = new List<Paragraph>();
            HistoryItems = new List<HistoryItem>();
            FileName = "Untitled";
        }

        public Subtitle(List<HistoryItem> historyItems)
            : this()
        {
            HistoryItems = historyItems;
        }

        public Subtitle(List<Paragraph> paragraphs, List<HistoryItem> historyItems)
            : this()
        {
            HistoryItems = historyItems;
            Paragraphs = paragraphs;
        }

        /// <summary>
        /// Copy constructor (only paragraphs)
        /// </summary>
        /// <param name="subtitle">Subtitle to copy</param>
        /// <param name="generateNewId">Generate new ID (guid) for paragraphs</param>
        public Subtitle(Subtitle subtitle, bool generateNewId = true)
            : this()
        {
            if (subtitle == null)
            {
                return;
            }

            foreach (var p in subtitle.Paragraphs)
            {
                Paragraphs.Add(new Paragraph(p, generateNewId));
            }
            Header = subtitle.Header;
            Footer = subtitle.Footer;
            FileName = subtitle.FileName;
        }

        public Subtitle(List<Paragraph> paragraphs) : this()
        {
            Paragraphs = paragraphs;
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
            return Paragraphs.FirstOrDefault(p => p.Id == id);
        }

        public SubtitleFormat ReloadLoadSubtitle(List<string> lines, string fileName, SubtitleFormat format)
        {
            Paragraphs.Clear();
            if (format != null && format.IsMine(lines, fileName))
            {
                format.LoadSubtitle(this, lines, fileName);
                OriginalFormat = format;
                return format;
            }
            foreach (var subtitleFormat in SubtitleFormat.AllSubtitleFormats)
            {
                if (subtitleFormat.IsMine(lines, fileName))
                {
                    subtitleFormat.LoadSubtitle(this, lines, fileName);
                    OriginalFormat = subtitleFormat;
                    return subtitleFormat;
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
            StreamReader sr;
            if (useThisEncoding != null)
            {
                try
                {
                    sr = new StreamReader(fileName, useThisEncoding);
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                    encoding = Encoding.UTF8;
                    return null;
                }
            }
            else
            {
                try
                {
                    sr = new StreamReader(fileName, LanguageAutoDetect.GetEncodingFromFile(fileName), true);
                }
                catch
                {
                    try
                    {
                        var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        sr = new StreamReader(fs);
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                        encoding = Encoding.UTF8;
                        return null;
                    }
                }
            }

            encoding = sr.CurrentEncoding;
            var lines = sr.ReadToEnd().SplitToLines();
            sr.Close();

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

        public void MakeHistoryForUndo(string description, SubtitleFormat subtitleFormat, DateTime fileModified, Subtitle original, string originalSubtitleFileName, int lineNumber, int linePosition, int linePositionAlternate)
        {
            // don't fill memory with history - use a max rollback points
            if (HistoryItems.Count > MaximumHistoryItems)
            {
                HistoryItems.RemoveAt(0);
            }

            HistoryItems.Add(new HistoryItem(HistoryItems.Count, this, description, FileName, fileModified, subtitleFormat.FriendlyName, original, originalSubtitleFileName, lineNumber, linePosition, linePositionAlternate));
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

        public void AdjustDisplayTimeUsingPercent(double percent, List<int> selectedIndexes)
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
                    if (newEndMilliseconds > nextStartMilliseconds)
                    {
                        newEndMilliseconds = nextStartMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }

                    if (percent > 100 && newEndMilliseconds > Paragraphs[i].EndTime.TotalMilliseconds || percent < 100)
                    {
                        Paragraphs[i].EndTime.TotalMilliseconds = newEndMilliseconds;
                    }
                }
            }
        }

        public void AdjustDisplayTimeUsingSeconds(double seconds, List<int> selectedIndexes)
        {
            if (Math.Abs(seconds) < 0.01)
            {
                return;
            }

            var adjustMs = seconds * TimeCode.BaseUnit;
            if (selectedIndexes != null)
            {
                foreach (var idx in selectedIndexes)
                {
                    AdjustDisplayTimeUsingMilliseconds(idx, adjustMs);
                }
            }
            else
            {
                for (int idx = 0; idx < Paragraphs.Count; idx++)
                {
                    AdjustDisplayTimeUsingMilliseconds(idx, adjustMs);
                }
            }
        }

        private void AdjustDisplayTimeUsingMilliseconds(int idx, double ms)
        {
            var p = Paragraphs[idx];
            var nextStartTimeInMs = double.MaxValue;
            if (idx + 1 < Paragraphs.Count)
            {
                nextStartTimeInMs = Paragraphs[idx + 1].StartTime.TotalMilliseconds;
            }
            var newEndTimeInMs = p.EndTime.TotalMilliseconds + ms;

            // handle overlap with next
            if (newEndTimeInMs > nextStartTimeInMs)
            {
                newEndTimeInMs = nextStartTimeInMs - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
            }

            // fix too short duration
            var minDur = Math.Max(Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds, 100);
            if (p.StartTime.TotalMilliseconds + minDur > newEndTimeInMs)
            {
                newEndTimeInMs = p.StartTime.TotalMilliseconds + minDur;
            }

            if (ms > 0 && newEndTimeInMs < p.EndTime.TotalMilliseconds || ms < 0 && newEndTimeInMs > p.EndTime.TotalMilliseconds)
            {
                return; // do not adjust wrong way
            }

            p.EndTime.TotalMilliseconds = newEndTimeInMs;
        }

        public void RecalculateDisplayTimes(double maxCharPerSec, List<int> selectedIndexes, double optimalCharPerSec, bool extendOnly = false)
        {
            if (selectedIndexes != null)
            {
                foreach (var index in selectedIndexes)
                {
                    RecalculateDisplayTime(maxCharPerSec, index, optimalCharPerSec, extendOnly);
                }
            }
            else
            {
                for (int i = 0; i < Paragraphs.Count; i++)
                {
                    RecalculateDisplayTime(maxCharPerSec, i, optimalCharPerSec, extendOnly);
                }
            }
        }

        public void RecalculateDisplayTime(double maxCharactersPerSecond, int index, double optimalCharactersPerSeconds, bool extendOnly = false)
        {
            var p = GetParagraphOrDefault(index);
            if (p == null)
            {
                return;
            }

            var originalEndTime = p.EndTime.TotalMilliseconds;

            var duration = Utilities.GetOptimalDisplayMilliseconds(p.Text, optimalCharactersPerSeconds);
            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + duration;
            var numberOfCharacters = p.Text.CountCharacters(Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace);
            while (Utilities.GetCharactersPerSecond(p, numberOfCharacters) > maxCharactersPerSecond)
            {
                duration++;
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + duration;
            }

            if (extendOnly && p.EndTime.TotalMilliseconds < originalEndTime)
            {
                p.EndTime.TotalMilliseconds = originalEndTime;
            }

            var next = GetParagraphOrDefault(index + 1);
            if (next != null && p.StartTime.TotalMilliseconds + duration + Configuration.Settings.General.MinimumMillisecondsBetweenLines > next.StartTime.TotalMilliseconds)
            {
                p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                if (p.Duration.TotalMilliseconds <= 0)
                {
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 1;
                }
            }
        }

        public void SetFixedDuration(List<int> selectedIndexes, double fixedDurationMilliseconds)
        {
            for (int i = 0; i < Paragraphs.Count; i++)
            {
                if (selectedIndexes == null || selectedIndexes.Contains(i))
                {
                    var p = GetParagraphOrDefault(i);
                    if (p == null)
                    {
                        continue;
                    }

                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + fixedDurationMilliseconds;

                    var next = GetParagraphOrDefault(i + 1);
                    if (next != null && p.StartTime.TotalMilliseconds + fixedDurationMilliseconds + Configuration.Settings.General.MinimumMillisecondsBetweenLines > next.StartTime.TotalMilliseconds)
                    {
                        p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                        if (p.Duration.TotalMilliseconds <= 0)
                        {
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 1;
                        }
                    }
                }
            }
        }

        public void Renumber(int startNumber = 1)
        {
            var number = startNumber;
            int l = Paragraphs.Count + number;
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

            int index = Paragraphs.IndexOf(p);
            if (index >= 0)
            {
                return index;
            }

            for (int i = 0; i < Paragraphs.Count; i++)
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
            for (int i = 0; i < Paragraphs.Count; i++)
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

        public int RemoveEmptyLines()
        {
            int count = Paragraphs.Count;
            if (count <= 0)
            {
                return 0;
            }

            int firstNumber = Paragraphs[0].Number;
            for (int i = Paragraphs.Count - 1; i >= 0; i--)
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
            int count = 0;
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
            int beforeCount = Paragraphs.Count;
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
                    Paragraphs = Paragraphs.OrderBy(p => p.Duration.TotalMilliseconds).ThenBy(p => p.Number).ToList();
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
                    Paragraphs = Paragraphs.OrderBy(Utilities.GetCharactersPerSecond).ThenBy(p => p.Number).ToList();
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
        /// Fast hash code for subtitle - includes pre (encoding atm) + header + number + start + end + text.
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
                var max = Paragraphs.Count;
                for (int i = 0; i < max; i++)
                {
                    var p = Paragraphs[i];
                    hash = hash * 23 + p.Number.GetHashCode();
                    hash = hash * 23 + p.StartTime.TotalMilliseconds.GetHashCode();
                    hash = hash * 23 + p.EndTime.TotalMilliseconds.GetHashCode();
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
            int max = Paragraphs.Count;
            const int averageLength = 40;
            var sb = new StringBuilder(max * averageLength);
            for (var index = 0; index < max; index++)
            {
                sb.AppendLine(Paragraphs[index].Text);
            }
            return sb.ToString();
        }
    }
}
