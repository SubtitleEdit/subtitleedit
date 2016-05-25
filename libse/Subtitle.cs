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
        private List<Paragraph> _paragraphs;
        private readonly List<HistoryItem> _history;
        private SubtitleFormat _format;
        private bool _wasLoadedWithFrameNumbers;
        public string Header { get; set; }
        public string Footer { get; set; }

        public string FileName { get; set; }

        public const int MaximumHistoryItems = 100;

        public SubtitleFormat OriginalFormat
        {
            get
            {
                return _format;
            }
        }

        public List<HistoryItem> HistoryItems
        {
            get { return _history; }
        }

        public Subtitle()
        {
            _paragraphs = new List<Paragraph>();
            _history = new List<HistoryItem>();
            FileName = "Untitled";
        }

        public Subtitle(List<HistoryItem> historyItems)
            : this()
        {
            _history = historyItems;
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
                return;

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                _paragraphs.Add(new Paragraph(p, generateNewId));
            }
            _wasLoadedWithFrameNumbers = subtitle.WasLoadedWithFrameNumbers;
            Header = subtitle.Header;
            Footer = subtitle.Footer;
            FileName = subtitle.FileName;
        }

        public List<Paragraph> Paragraphs
        {
            get
            {
                return _paragraphs;
            }
        }

        /// <summary>
        /// Get the paragraph of index, null if out of bounds
        /// </summary>
        /// <param name="index">Index of wanted paragraph</param>
        /// <returns>Paragraph, null if index is index is out of bounds</returns>
        public Paragraph GetParagraphOrDefault(int index)
        {
            if (_paragraphs == null || _paragraphs.Count <= index || index < 0)
                return null;

            return _paragraphs[index];
        }

        public Paragraph GetParagraphOrDefaultById(string id)
        {
            return _paragraphs.FirstOrDefault(p => p.ID == id);
        }

        public SubtitleFormat ReloadLoadSubtitle(List<string> lines, string fileName, SubtitleFormat format)
        {
            Paragraphs.Clear();
            if (format != null && format.IsMine(lines, fileName))
            {
                format.LoadSubtitle(this, lines, fileName);
                _format = format;
                return format;
            }
            foreach (SubtitleFormat subtitleFormat in SubtitleFormat.AllSubtitleFormats)
            {
                if (subtitleFormat.IsMine(lines, fileName))
                {
                    subtitleFormat.LoadSubtitle(this, lines, fileName);
                    _format = subtitleFormat;
                    return subtitleFormat;
                }
            }
            return null;
        }

        public SubtitleFormat LoadSubtitle(string fileName, out Encoding encoding, Encoding useThisEncoding)
        {
            return LoadSubtitle(fileName, out encoding, useThisEncoding, false);
        }

        public SubtitleFormat LoadSubtitle(string fileName, out Encoding encoding, Encoding useThisEncoding, bool batchMode)
        {
            FileName = fileName;

            _paragraphs = new List<Paragraph>();

            var lines = new List<string>();
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
                        Stream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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
            while (!sr.EndOfStream)
                lines.Add(sr.ReadLine());
            sr.Close();

            foreach (SubtitleFormat subtitleFormat in SubtitleFormat.AllSubtitleFormats)
            {
                if (subtitleFormat.IsMine(lines, fileName))
                {
                    Header = null;
                    subtitleFormat.BatchMode = batchMode;
                    subtitleFormat.LoadSubtitle(this, lines, fileName);
                    _format = subtitleFormat;
                    _wasLoadedWithFrameNumbers = _format.IsFrameBased;
                    if (_wasLoadedWithFrameNumbers)
                        CalculateTimeCodesFromFrameNumbers(Configuration.Settings.General.CurrentFrameRate);
                    return subtitleFormat;
                }
            }

            if (useThisEncoding == null)
                return LoadSubtitle(fileName, out encoding, Encoding.Unicode);

            return null;
        }

        public void MakeHistoryForUndo(string description, SubtitleFormat subtitleFormat, DateTime fileModified, Subtitle original, string originalSubtitleFileName, int lineNumber, int linePosition, int linePositionAlternate)
        {
            // don't fill memory with history - use a max rollback points
            if (_history.Count > MaximumHistoryItems)
                _history.RemoveAt(0);

            _history.Add(new HistoryItem(_history.Count, this, description, FileName, fileModified, subtitleFormat.FriendlyName, original, originalSubtitleFileName, lineNumber, linePosition, linePositionAlternate));
        }

        public bool CanUndo
        {
            get
            {
                return _history.Count > 0;
            }
        }

        public string UndoHistory(int index, out string subtitleFormatFriendlyName, out DateTime fileModified, out Subtitle originalSubtitle, out string originalSubtitleFileName)
        {
            _paragraphs.Clear();
            foreach (Paragraph p in _history[index].Subtitle.Paragraphs)
                _paragraphs.Add(new Paragraph(p));

            subtitleFormatFriendlyName = _history[index].SubtitleFormatFriendlyName;
            FileName = _history[index].FileName;
            fileModified = _history[index].FileModified;
            originalSubtitle = new Subtitle(_history[index].OriginalSubtitle);
            originalSubtitleFileName = _history[index].OriginalSubtitleFileName;

            return FileName;
        }

        /// <summary>
        /// Creates subtitle as text in it'snative format
        /// </summary>
        /// <param name="format">Format to output</param>
        /// <returns>Native format as text string</returns>
        public string ToText(SubtitleFormat format)
        {
            return format.ToText(this, Path.GetFileNameWithoutExtension(FileName));
        }

        public void AddTimeToAllParagraphs(TimeSpan time)
        {
            foreach (Paragraph p in Paragraphs)
            {
                p.StartTime.AddTime(time);
                p.EndTime.AddTime(time);
            }
        }

        /// <summary>
        /// Calculate the time codes from frame number/frame rate
        /// </summary>
        /// <param name="frameRate">Number of frames per second</param>
        /// <returns>True if times could be calculated</returns>
        public bool CalculateTimeCodesFromFrameNumbers(double frameRate)
        {
            if (_format == null || _format.IsTimeBased)
                return false;

            foreach (Paragraph p in Paragraphs)
            {
                p.CalculateTimeCodesFromFrameNumbers(frameRate);
            }
            return true;
        }

        /// <summary>
        /// Calculate the frame numbers from time codes/frame rate
        /// </summary>
        /// <param name="frameRate"></param>
        /// <returns></returns>
        public bool CalculateFrameNumbersFromTimeCodes(double frameRate)
        {
            if (_format == null || _format.IsFrameBased)
                return false;

            foreach (Paragraph p in Paragraphs)
            {
                p.CalculateFrameNumbersFromTimeCodes(frameRate);
            }

            FixEqualOrJustOverlappingFrameNumbers();

            return true;
        }

        public void CalculateFrameNumbersFromTimeCodesNoCheck(double frameRate)
        {
            foreach (Paragraph p in Paragraphs)
                p.CalculateFrameNumbersFromTimeCodes(frameRate);

            FixEqualOrJustOverlappingFrameNumbers();
        }

        private void FixEqualOrJustOverlappingFrameNumbers()
        {
            for (int i = 0; i < Paragraphs.Count - 1; i++)
            {
                Paragraph p = Paragraphs[i];
                Paragraph next = GetParagraphOrDefault(i + 1);
                if (next != null && (p.EndFrame == next.StartFrame || p.EndFrame == next.StartFrame + 1))
                    p.EndFrame = next.StartFrame - 1;
            }
        }

        public void ChangeFrameRate(double oldFrameRate, double newFrameRate)
        {
            foreach (Paragraph p in Paragraphs)
            {
                p.StartTime.TotalMilliseconds = (p.StartTime.TotalMilliseconds * oldFrameRate / newFrameRate);
                p.EndTime.TotalMilliseconds = (p.EndTime.TotalMilliseconds * oldFrameRate / newFrameRate);
                p.CalculateFrameNumbersFromTimeCodes(newFrameRate);
            }
        }

        public bool WasLoadedWithFrameNumbers
        {
            get
            {
                return _wasLoadedWithFrameNumbers;
            }
            set
            {
                _wasLoadedWithFrameNumbers = value;
            }
        }

        public void AdjustDisplayTimeUsingPercent(double percent, List<int> selectedIndexes)
        {
            for (int i = 0; i < _paragraphs.Count; i++)
            {
                if (selectedIndexes == null || selectedIndexes.Contains(i))
                {
                    double nextStartMilliseconds = double.MaxValue;
                    if (i + 1 < _paragraphs.Count)
                        nextStartMilliseconds = _paragraphs[i + 1].StartTime.TotalMilliseconds;

                    double newEndMilliseconds = _paragraphs[i].EndTime.TotalMilliseconds;
                    newEndMilliseconds = _paragraphs[i].StartTime.TotalMilliseconds + (((newEndMilliseconds - _paragraphs[i].StartTime.TotalMilliseconds) * percent) / 100);
                    if (newEndMilliseconds > nextStartMilliseconds)
                        newEndMilliseconds = nextStartMilliseconds - 1;
                    _paragraphs[i].EndTime.TotalMilliseconds = newEndMilliseconds;
                }
            }
        }

        public void AdjustDisplayTimeUsingSeconds(double seconds, List<int> selectedIndexes)
        {
            for (int i = 0; i < _paragraphs.Count; i++)
            {
                if (selectedIndexes == null || selectedIndexes.Contains(i))
                {
                    double nextStartMilliseconds = double.MaxValue;
                    if (i + 1 < _paragraphs.Count)
                        nextStartMilliseconds = _paragraphs[i + 1].StartTime.TotalMilliseconds;

                    double newEndMilliseconds = _paragraphs[i].EndTime.TotalMilliseconds + (seconds * TimeCode.BaseUnit);
                    if (newEndMilliseconds > nextStartMilliseconds)
                        newEndMilliseconds = nextStartMilliseconds - 1;

                    if (seconds < 0)
                    {
                        if (_paragraphs[i].StartTime.TotalMilliseconds + 100 > newEndMilliseconds)
                            _paragraphs[i].EndTime.TotalMilliseconds = _paragraphs[i].StartTime.TotalMilliseconds + 100;
                        else
                            _paragraphs[i].EndTime.TotalMilliseconds = newEndMilliseconds;
                    }
                    else
                    {
                        _paragraphs[i].EndTime.TotalMilliseconds = newEndMilliseconds;
                    }
                }
            }
        }

        public void RecalculateDisplayTimes(double maxCharactersPerSecond, List<int> selectedIndexes)
        {
            for (int i = 0; i < _paragraphs.Count; i++)
            {
                if (selectedIndexes == null || selectedIndexes.Contains(i))
                {
                    Paragraph p = _paragraphs[i];
                    double duration = Utilities.GetOptimalDisplayMilliseconds(p.Text);
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + duration;
                    while (Utilities.GetCharactersPerSecond(p) > maxCharactersPerSecond)
                    {
                        duration++;
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + duration;
                    }

                    Paragraph next = GetParagraphOrDefault(i + 1);
                    if (next != null && p.StartTime.TotalMilliseconds + duration + Configuration.Settings.General.MinimumMillisecondsBetweenLines > next.StartTime.TotalMilliseconds)
                    {
                        p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                        if (p.Duration.TotalMilliseconds <= 0)
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 1;
                    }
                }
            }
        }

        public void Renumber(int startNumber = 1)
        {
            foreach (Paragraph p in _paragraphs)
            {
                p.Number = startNumber++;
            }
        }

        public int GetIndex(Paragraph p)
        {
            if (p == null)
                return -1;

            int index = _paragraphs.IndexOf(p);
            if (index >= 0)
                return index;

            for (int i = 0; i < _paragraphs.Count; i++)
            {
                if (p.ID == _paragraphs[i].ID)
                    return i;
                if (i < _paragraphs.Count - 1 && p.ID == _paragraphs[i + 1].ID)
                    return i + 1;
                if (Math.Abs(p.StartTime.TotalMilliseconds - _paragraphs[i].StartTime.TotalMilliseconds) < 0.1 &&
                    Math.Abs(p.EndTime.TotalMilliseconds - _paragraphs[i].EndTime.TotalMilliseconds) < 0.1)
                    return i;
                if (p.Number == _paragraphs[i].Number && (Math.Abs(p.StartTime.TotalMilliseconds - _paragraphs[i].StartTime.TotalMilliseconds) < 0.1 ||
                    Math.Abs(p.EndTime.TotalMilliseconds - _paragraphs[i].EndTime.TotalMilliseconds) < 0.1))
                    return i;
                if (p.Text == _paragraphs[i].Text && (Math.Abs(p.StartTime.TotalMilliseconds - _paragraphs[i].StartTime.TotalMilliseconds) < 0.1 ||
                    Math.Abs(p.EndTime.TotalMilliseconds - _paragraphs[i].EndTime.TotalMilliseconds) < 0.1))
                    return i;
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
            foreach (Paragraph item in _paragraphs)
            {
                if (Math.Abs(p.StartTime.TotalMilliseconds - item.StartTime.TotalMilliseconds) < 0.1 &&
                    Math.Abs(p.EndTime.TotalMilliseconds - item.EndTime.TotalMilliseconds) < 0.1 &&
                    p.Text == item.Text)
                    return item;
            }
            return null;
        }

        public int RemoveEmptyLines()
        {
            int count = _paragraphs.Count;
            if (count > 0)
            {
                int firstNumber = _paragraphs[0].Number;
                for (int i = _paragraphs.Count - 1; i >= 0; i--)
                {
                    Paragraph p = _paragraphs[i];
                    if (string.IsNullOrWhiteSpace(p.Text))
                        _paragraphs.RemoveAt(i);
                }
                if (count != _paragraphs.Count)
                    Renumber(firstNumber);
            }
            return count - _paragraphs.Count;
        }

        /// <summary>
        /// Removes paragrahs by a list of indices
        /// </summary>
        /// <param name="indices">Indices of pargraphs/lines to delete</param>
        /// <returns>Number of lines deleted</returns>
        public int RemoveParagraphsByIndices(IEnumerable<int> indices)
        {
            int count = 0;
            foreach (var index in indices.OrderByDescending(p => p))
            {
                if (index >= 0 && index < _paragraphs.Count)
                {
                    _paragraphs.RemoveAt(index);
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Removes paragrahs by a list of IDs
        /// </summary>
        /// <param name="ids">IDs of pargraphs/lines to delete</param>
        /// <returns>Number of lines deleted</returns>
        public int RemoveParagraphsByIds(IEnumerable<string> ids)
        {
            int beforeCount = _paragraphs.Count;
            _paragraphs = _paragraphs.Where(p => !ids.Contains(p.ID)).ToList();
            return beforeCount - _paragraphs.Count;
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
                    _paragraphs.Sort((p1, p2) => p1.Number.CompareTo(p2.Number));
                    break;
                case SubtitleSortCriteria.StartTime:
                    _paragraphs.Sort((p1, p2) => p1.StartTime.TotalMilliseconds.CompareTo(p2.StartTime.TotalMilliseconds));
                    break;
                case SubtitleSortCriteria.EndTime:
                    _paragraphs.Sort((p1, p2) => p1.EndTime.TotalMilliseconds.CompareTo(p2.EndTime.TotalMilliseconds));
                    break;
                case SubtitleSortCriteria.Duration:
                    _paragraphs.Sort((p1, p2) => p1.Duration.TotalMilliseconds.CompareTo(p2.Duration.TotalMilliseconds));
                    break;
                case SubtitleSortCriteria.Text:
                    _paragraphs.Sort((p1, p2) => string.Compare(p1.Text, p2.Text, StringComparison.Ordinal));
                    break;
                case SubtitleSortCriteria.TextMaxLineLength:
                    _paragraphs.Sort((p1, p2) => Utilities.GetMaxLineLength(p1.Text).CompareTo(Utilities.GetMaxLineLength(p2.Text)));
                    break;
                case SubtitleSortCriteria.TextTotalLength:
                    _paragraphs.Sort((p1, p2) => p1.Text.Length.CompareTo(p2.Text.Length));
                    break;
                case SubtitleSortCriteria.TextNumberOfLines:
                    _paragraphs.Sort((p1, p2) => p1.NumberOfLines.CompareTo(p2.NumberOfLines));
                    break;
                case SubtitleSortCriteria.TextCharactersPerSeconds:
                    _paragraphs.Sort((p1, p2) => Utilities.GetCharactersPerSecond(p1).CompareTo(Utilities.GetCharactersPerSecond(p2)));
                    break;
                case SubtitleSortCriteria.WordsPerMinute:
                    _paragraphs.Sort((p1, p2) => p1.WordsPerMinute.CompareTo(p2.WordsPerMinute));
                    break;
                case SubtitleSortCriteria.Style:
                    _paragraphs.Sort((p1, p2) => string.Compare(p1.Extra, p2.Extra, StringComparison.Ordinal));
                    break;
            }
        }

        public void InsertParagraphInCorrectTimeOrder(Paragraph newParagraph)
        {
            for (int i = 0; i < Paragraphs.Count; i++)
            {
                Paragraph p = Paragraphs[i];
                if (newParagraph.StartTime.TotalMilliseconds < p.StartTime.TotalMilliseconds)
                {
                    Paragraphs.Insert(i, newParagraph);
                    return;
                }
            }
            Paragraphs.Add(newParagraph);
        }

        /// <summary>
        /// Fast hash code for subtitle (only includes start + end + text)
        /// </summary>
        /// <returns>Hash value that can be used for quick compare</returns>
        public string GetFastHashCode()
        {
            var sb = new StringBuilder(Paragraphs.Count * 50);
            for (int i = 0; i < Paragraphs.Count; i++)
            {
                var p = Paragraphs[i];
                sb.Append(p.StartTime.TotalMilliseconds.GetHashCode());
                sb.Append(p.EndTime.TotalMilliseconds.GetHashCode());
                sb.Append(p.Text);
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Concatenates all the Paragraph its Text property from Paragraphs, using the default line terminator between each Text.
        /// </summary>
        /// <returns>Contatenated Text property of all Paragraph present in Paragraphs property.</returns>
        public string GetAllTexts()
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in Paragraphs)
            {
                sb.AppendLine(p.Text);
            }
            return sb.ToString();
        }
    }
}
