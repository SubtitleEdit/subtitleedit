using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Represents a subtitle with associated paragraphs, header, footer, and metadata.
    /// </summary>
    public class Subtitle
    {
        /// <summary>
        /// The maximum file size allowed for processing subtitles.
        /// Value is set to 20 MB (20 * 1024 * 1024 bytes).
        /// </summary>
        public const int MaxFileSize = 1024 * 1024 * 20; // 20 MB

        /// <summary>
        /// Represents a collection of subtitle paragraphs.
        /// This property holds the list of paragraphs for a subtitle,
        /// where each paragraph contains a piece of subtitle text and its associated timing information.
        /// </summary>
        public List<Paragraph> Paragraphs { get; private set; }

        /// <summary>
        /// The header information of the subtitle.
        /// This property might be used to store metadata or configuration details
        /// that are specific to the subtitle format in use.
        /// </summary>
        public string Header { get; set; } = string.Empty;

        /// <summary>
        /// Represents the footer content of a subtitle file.
        /// This field typically contains metadata or additional information
        /// appended at the end of the subtitle.
        /// </summary>
        public string Footer { get; set; } = string.Empty;

        /// <summary>
        /// The name of the file associated with the subtitle.
        /// If not specified, defaults to "Untitled".
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Specifies the maximum number of history items that can be stored for undo operations.
        /// Value is set to 100.
        /// </summary>
        public static int MaximumHistoryItems => 100;

        /// <summary>
        /// Gets or sets the original format of the subtitle.
        /// This property is typically used to preserve the format from which the subtitle was initially loaded.
        /// </summary>
        public SubtitleFormat OriginalFormat { get; set; }

        /// <summary>
        /// Represents the original text encoding of the subtitle file.
        /// This property is assigned when the subtitle is loaded from a file.
        /// </summary>
        public Encoding OriginalEncoding { get; private set; }

        /// <summary>
        /// A collection that maintains the history of modifications made to the subtitle.
        /// This list can be used for undo and redo functionality to revert or reapply changes.
        /// </summary>
        public List<HistoryItem> HistoryItems { get; }

        /// <summary>
        /// Indicates whether there are any actions in the history that can be undone.
        /// Returns true if there are items in the history; otherwise, false.
        /// </summary>
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
        /// Gets the paragraph at the specified index or returns null if the index is out of bounds.
        /// </summary>
        /// <param name="index">Index of the desired paragraph.</param>
        /// <returns>The paragraph at the specified index, or null if the index is out of bounds.</returns>
        public Paragraph GetParagraphOrDefault(int index)
        {
            if (Paragraphs == null || Paragraphs.Count <= index || index < 0)
            {
                return null;
            }

            return Paragraphs[index];
        }

        /// <summary>
        /// Retrieves a paragraph from the list by its ID, or returns null if no matching paragraph is found.
        /// </summary>
        /// <param name="id">The ID of the paragraph to retrieve.</param>
        /// <returns>The paragraph with the specified ID, or null if not found.</returns>
        public Paragraph GetParagraphOrDefaultById(string id)
        {
            return Paragraphs.Find(p => p.Id == id);
        }

        /// <summary>
        /// Reloads and loads a subtitle based on a list of lines and a file name,
        /// using specified subtitle formats to determine the correct format.
        /// </summary>
        /// <param name="lines">List of lines representing the subtitle content.</param>
        /// <param name="fileName">Name of the file from which the subtitle is being loaded.</param>
        /// <param name="format">Primary subtitle format to be checked and used.</param>
        /// <param name="format2">Secondary subtitle format to be checked and used if the primary format does not match.</param>
        /// <param name="format3">Tertiary subtitle format to be checked and used if neither the primary nor secondary formats match.</param>
        /// <returns>The subtitle format that was used to load the subtitle, or null if no suitable format was found.</returns>
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

        /// <summary>
        /// Loads a subtitle from a file.
        /// </summary>
        /// <param name="fileName">Path to the file containing the subtitle</param>
        /// <param name="encoding">Output parameter for the detected or used encoding</param>
        /// <param name="useThisEncoding">Optional encoding to use for reading the file</param>
        /// <param name="batchMode">Indicates if the loading should be done in batch mode</param>
        /// <param name="sourceFrameRate">Optional frame rate, used by formats that require it</param>
        /// <param name="loadSubtitle">Indicates if the subtitle should be loaded</param>
        /// <returns>The subtitle format used to load the subtitle</returns>
        public SubtitleFormat LoadSubtitle(string fileName, out Encoding encoding, Encoding useThisEncoding)
        {
            return LoadSubtitle(fileName, out encoding, useThisEncoding, false);
        }

        /// <summary>
        /// Loads a subtitle file and returns its format.
        /// </summary>
        /// <param name="fileName">The name of the subtitle file to load.</param>
        /// <param name="encoding">The encoding of the subtitle file.</param>
        /// <param name="useThisEncoding">The encoding to use if the file encoding is not detected.</param>
        /// <param name="batchMode">Flag indicating whether the loading is done in batch mode.</param>
        /// <param name="sourceFrameRate">The frame rate of the source, if available.</param>
        /// <param name="loadSubtitle">Flag indicating whether to fully load the subtitle.</param>
        /// <returns>The format of the loaded subtitle file.</returns>
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

        /// <summary>
        /// Reads lines from a file, attempting to automatically determine the file's encoding if none is provided.
        /// </summary>
        /// <param name="fileName">The path to the file to read from.</param>
        /// <param name="useThisEncoding">The encoding to use when reading the file. If null, the encoding is automatically detected.</param>
        /// <param name="encoding">Outputs the detected or used encoding for reading the file.</param>
        /// <returns>A list of strings representing the lines read from the file.</returns>
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

        /// <summary>
        /// Finalizes the subtitle format by setting various properties and loading the subtitle if needed.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="batchMode">Indicates if batch mode is enabled.</param>
        /// <param name="sourceFrameRate">The source frame rate of the subtitle.</param>
        /// <param name="lines">The lines of the subtitle file.</param>
        /// <param name="subtitleFormat">The format of the subtitle.</param>
        /// <param name="loadSubtitle">Specifies whether the subtitle should be loaded.</param>
        /// <returns>The finalized subtitle format.</returns>
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

        /// <summary>
        /// Records the current state of the subtitle for undo functionality.
        /// Limits the number of stored history items to prevent excessive memory usage.
        /// </summary>
        /// <param name="description">Description of the change being made</param>
        /// <param name="subtitleFormatFriendlyName">Friendly name of the subtitle format</param>
        /// <param name="fileModified">Date and time when the file was last modified</param>
        /// <param name="original">Original subtitle before the change</param>
        /// <param name="originalSubtitleFileName">File name of the original subtitle</param>
        /// <param name="lineNumber">Index of the line being modified</param>
        /// <param name="linePosition">Position in the line where the modification starts</param>
        /// <param name="linePositionOriginal">Original position in the line before modification</param>
        public void MakeHistoryForUndo(string description, string subtitleFormatFriendlyName, DateTime fileModified, Subtitle original, string originalSubtitleFileName, int lineNumber, int linePosition, int linePositionOriginal)
        {
            // don't fill memory with history - use a max rollback points
            if (HistoryItems.Count > MaximumHistoryItems)
            {
                HistoryItems.RemoveAt(0);
            }

            HistoryItems.Add(new HistoryItem(HistoryItems.Count, this, description, FileName, fileModified, subtitleFormatFriendlyName, original, originalSubtitleFileName, lineNumber, linePosition, linePositionOriginal));
        }

        /// <summary>
        /// Restores the subtitle to a previous state from the history at the specified index.
        /// </summary>
        /// <param name="index">Index of the history item to undo to.</param>
        /// <param name="subtitleFormatFriendlyName">Outputs the friendly name of the subtitle format.</param>
        /// <param name="fileModified">Outputs the date and time the file was last modified.</param>
        /// <param name="originalSubtitle">Outputs the original subtitle before modifications.</param>
        /// <param name="originalSubtitleFileName">Outputs the file name of the original subtitle before modifications.</param>
        /// <returns>Returns the file name after the undo operation.</returns>
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

        /// <summary>
        /// Adds the specified time to the start and end times of all paragraphs in the subtitle.
        /// </summary>
        /// <param name="time">The time to add to each paragraph's start and end times</param>
        public void AddTimeToAllParagraphs(TimeSpan time)
        {
            double milliseconds = time.TotalMilliseconds;
            foreach (var p in Paragraphs)
            {
                p.StartTime.TotalMilliseconds += milliseconds;
                p.EndTime.TotalMilliseconds += milliseconds;
            }
        }

        /// <summary>
        /// Changes the frame rate of all paragraphs in the subtitle.
        /// </summary>
        /// <param name="oldFrameRate">The original frame rate of the subtitle</param>
        /// <param name="newFrameRate">The new frame rate to be set for the subtitle</param>
        public void ChangeFrameRate(double oldFrameRate, double newFrameRate)
        {
            var factor = SubtitleFormat.GetFrameForCalculation(oldFrameRate) / SubtitleFormat.GetFrameForCalculation(newFrameRate);
            foreach (var p in Paragraphs)
            {
                p.StartTime.TotalMilliseconds *= factor;
                p.EndTime.TotalMilliseconds *= factor;
            }
        }

        /// <summary>
        /// Adjusts the display time of subtitle paragraphs by a specified percentage.
        /// </summary>
        /// <param name="percent">The percentage by which to adjust the display time.</param>
        /// <param name="selectedIndexes">The list of indexes of the paragraphs to be adjusted. If null, all paragraphs are adjusted.</param>
        /// <param name="shotChanges">Optional list of shot change times. If provided, shot changes are taken into account during adjustment.</param>
        /// <param name="enforceDurationLimits">If true, enforces minimum and maximum duration limits for subtitle display times.</param>
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

        /// <summary>
        /// Adjusts the display time of the subtitle paragraphs by a specified number of seconds.
        /// </summary>
        /// <param name="seconds">The amount of time in seconds to adjust the display time by.</param>
        /// <param name="selectedIndexes">The list of indexes for the paragraphs to adjust. If null, all paragraphs will be adjusted.</param>
        /// <param name="shotChanges">Optional list of shot change times, used to enforce shot change rules if provided.</param>
        /// <param name="enforceDurationLimits">Specifies whether to enforce duration limits on the adjusted display time.</param>
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

        /// <summary>
        /// Adjusts the display time of the paragraph at the specified index by a given number of milliseconds.
        /// </summary>
        /// <param name="idx">Index of the paragraph to adjust.</param>
        /// <param name="ms">Milliseconds to adjust by. Positive values increase duration, negative values decrease.</param>
        /// <param name="shotChanges">List of shot change times in milliseconds, if any. Used to ensure paragraph does not overlap with shot changes.</param>
        /// <param name="enforceDurationLimits">Indicates whether to enforce minimum and maximum duration limits.</param>
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

        /// <summary>
        /// Recalculates the display times of each paragraph in the subtitle based on character per second settings.
        /// </summary>
        /// <param name="maxCharPerSec">The maximum number of characters per second.</param>
        /// <param name="selectedIndexes">The list of selected paragraph indexes to recalculate. If null, all paragraphs are recalculated.</param>
        /// <param name="optimalCharPerSec">The optimal number of characters per second.</param>
        /// <param name="extendOnly">If true, only extend the display time of paragraphs.</param>
        /// <param name="shotChanges">List of shot change times used to adjust display times for better synchronization. Can be null.</param>
        /// <param name="enforceDurationLimits">If true, enforce minimum and maximum duration limits for the display times.</
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

        /// <summary>
        /// Recalculates the display time of a subtitle paragraph given its index.
        /// Adjusts the end time based on maximum and optimal characters per second,
        /// extending only if specified, and enforces duration limits.
        /// </summary>
        /// <param name="maxCharactersPerSecond">The maximum allowed characters per second for the paragraph.</param>
        /// <param name="index">The index of the subtitle paragraph to recalculate.</param>
        /// <param name="optimalCharactersPerSeconds">The optimal characters per second value for calculation.</param>
        /// <param name="extendOnly">Flag indicating if the recalculation should only extend the duration.</param>
        /// <param name="onlyOptimal">Flag indicating if only optimal display time should be calculated.</param>
        /// <param name="shotChanges">List of shot changes to consider for end time adjustments.</param>
        /// <param name="enforceDurationLimits">Flag indicating if duration limits should be enforced.</param>
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

        /// <summary>
        /// Sets a fixed duration for the selected paragraphs in the subtitle.
        /// </summary>
        /// <param name="selectedIndexes">Indexes of the paragraphs to adjust. If null, all paragraphs are adjusted.</param>
        /// <param name="fixedDurationMilliseconds">Desired fixed duration in milliseconds for each paragraph.</param>
        /// <param name="shotChanges">Optional list of shot change timestamps to consider for adjusting paragraph end times.</param>
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

        /// <summary>
        /// Renumbers the paragraphs in the subtitle starting from a specified number.
        /// </summary>
        /// <param name="startNumber">The starting number for the first paragraph.</param>
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

        /// <summary>
        /// Returns the index of the specified paragraph in the subtitle.
        /// </summary>
        /// <param name="p">The paragraph to locate.</param>
        /// <returns>The index of the paragraph if found; otherwise, -1.</returns>
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

        /// <summary>
        /// Finds the first paragraph in the list that is alike the given paragraph.
        /// </summary>
        /// <param name="p">The paragraph to compare.</param>
        /// <returns>The first alike paragraph if found; otherwise, null.</returns>
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

        /// <summary>
        /// Finds the nearest similar Paragraph to the given Paragraph based on start time, end time, and text content.
        /// </summary>
        /// <param name="p">The Paragraph to compare with others in the Subtitle</param>
        /// <returns>A Paragraph that is nearest in similarity to the given Paragraph</returns>
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

        /// <summary>
        /// Removes empty lines from the subtitle.
        /// </summary>
        /// <returns>The number of removed empty lines.</returns>
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

        /// <summary>
        /// Inserts a new paragraph into the list of paragraphs, maintaining the correct chronological order based on start times.
        /// </summary>
        /// <param name="newParagraph">The new Paragraph to insert.</param>
        /// <returns>The index at which the new paragraph was inserted.</returns>
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

        /// <summary>
        /// Gets the first paragraph that overlaps with the specified time, or null if no such paragraph exists.
        /// </summary>
        /// <param name="milliseconds">The time in milliseconds to check against paragraph start and end times.</param>
        /// <return>The first paragraph overlapping the specified time, or null if there is no match.</return>
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
