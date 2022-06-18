using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Xceed.Document.NET;
using Xceed.Words.NET;

//TODO: formatting extract / merge
//TODO: fix italic for split
//TODO: remember check boxes settings

namespace Nikse.SubtitleEdit.Forms.Translate
{
    public partial class TranslateExportImport : Form
    {
        public Subtitle Subtitle { get; private set; }
        public Subtitle SubtitleOriginal { get; private set; }
        private Dictionary<int, int> _skipIndices;

        public TranslateExportImport(Subtitle subtitle)
        {
            InitializeComponent();
            SubtitleOriginal = subtitle;
            _skipIndices = new Dictionary<int, int>();
        }

        private void ButtonExportClick(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog
            {
                Title = LanguageSettings.Current.General.OpenSubtitle,
                FileName = "translate.docx",
                Filter = "Word docx files|*.docx",
            })
            {
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Subtitle = new Subtitle(SubtitleOriginal, false);
                _skipIndices = new Dictionary<int, int>();
                var texts = new List<string>();
                var mergeCount = 0;
                var autoMerge = checkBoxAutoMerge.Checked;
                var language = LanguageAutoDetect.AutoDetectGoogleLanguage(Subtitle);
                using (var document = DocX.Create(saveFileDialog.FileName))
                {
                    for (var index = 0; index < Subtitle.Paragraphs.Count; index++)
                    {
                        if (mergeCount > 0)
                        {
                            mergeCount--;
                            continue;
                        }

                        var subtitleParagraph = Subtitle.Paragraphs[index];
                        var text = subtitleParagraph.Text;
                        if (autoMerge)
                        {
                            if (MergeWithThreeNext(Subtitle, index, language))
                            {
                                mergeCount = 3;
                                _skipIndices.Add(index, mergeCount);
                                text = Utilities.RemoveLineBreaks(text + Environment.NewLine +
                                                                  Subtitle.Paragraphs[index + 1].Text + Environment.NewLine +
                                                                  Subtitle.Paragraphs[index + 2].Text);
                            }
                            else if (MergeWithTwoNext(Subtitle, index, language))
                            {
                                mergeCount = 2;
                                _skipIndices.Add(index, mergeCount);
                                text = Utilities.RemoveLineBreaks(text + Environment.NewLine +
                                                                  Subtitle.Paragraphs[index + 1].Text + Environment.NewLine +
                                                                  Subtitle.Paragraphs[index + 2].Text);
                            }
                            else if (MergeWithNext(Subtitle, index, language))
                            {
                                mergeCount = 1;
                                _skipIndices.Add(index, mergeCount);
                                text = Utilities.RemoveLineBreaks(text + Environment.NewLine + Subtitle.Paragraphs[index + 1].Text);
                            }
                        }

                        texts.Add(text);
                    }

                    var table = document.AddTable(texts.Count, 1);
                    table.AutoFit = AutoFit.Window;
                    for (var index = 0; index < texts.Count; index++)
                    {
                        var text = texts[index];
                        table.Rows[index].Cells[0].InsertParagraph(text);
                    }

                    var p = document.InsertParagraph(string.Empty);
                    p.Alignment = Alignment.both;
                    p.InsertTableAfterSelf(table);
                    document.Save();
                }
            }
        }

        private void ButtonImportClick(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenSubtitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = "Word docx files|*.docx";
                openFileDialog1.FileName = string.Empty;
                var result = openFileDialog1.ShowDialog(this);
                if (result != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var document = DocX.Load(openFileDialog1.FileName))
                    {
                        var table = document.Tables.FirstOrDefault();
                        if (table == null)
                        {
                            return;
                        }

                        var splitLines = table.Rows.Count + _skipIndices.Count == Subtitle.Paragraphs.Count;

                        if (!splitLines && table.Rows.Count != Subtitle.Paragraphs.Count)
                        {
                            var res = MessageBox.Show($"Table rows ({table.Rows.Count} + {_skipIndices.Count}) does not match subtitle count ({Subtitle.Paragraphs.Count})" + Environment.NewLine +
                                                     "Continue?",
                                            "Subtitel Edit", MessageBoxButtons.YesNoCancel);
                            if (res != DialogResult.Yes)
                            {
                                return;
                            }
                        }

                        var language = LanguageAutoDetect.AutoDetectGoogleLanguage(Subtitle);
                        var count = 0;
                        foreach (var tableRow in table.Rows)
                        {
                            var p = Subtitle.GetParagraphOrDefault(count);
                            if (p != null)
                            {
                                p.Text = string.Empty;
                                foreach (var paragraph in tableRow.Cells[0].Paragraphs)
                                {
                                    p.Text = (p.Text + Environment.NewLine + paragraph.Text).Trim();
                                }

                                var text = string.Join(Environment.NewLine, p.Text.SplitToLines());
                                if (_skipIndices.TryGetValue(count, out var splitCount))
                                {
                                    var lines = SplitResult(text, splitCount, language);
                                    while (lines.Count < splitCount)
                                    {
                                        lines.Add(string.Empty);
                                    }

                                    foreach (var line in lines)
                                    {
                                        p = Subtitle.GetParagraphOrDefault(count);
                                        p.Text = line;
                                        count++;
                                    }

                                    continue;
                                }

                                p.Text = text;

                            }

                            count++;
                        }
                    }

                    DialogResult = DialogResult.OK;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private static bool MergeWithNext(Subtitle subtitle, int i, string source)
        {
            if (i + 1 >= subtitle.Paragraphs.Count || source.ToLowerInvariant() == "zh" || source.ToLowerInvariant() == "ja")
            {
                return false;
            }

            var p = subtitle.Paragraphs[i];
            var text = HtmlUtil.RemoveHtmlTags(p.Text, true).TrimEnd('"');
            if (text.EndsWith(".", StringComparison.Ordinal) ||
                text.EndsWith("!", StringComparison.Ordinal) ||
                text.EndsWith("?", StringComparison.Ordinal) ||
                text.EndsWith(")", StringComparison.Ordinal) ||
                text.EndsWith("]", StringComparison.Ordinal) ||
                text.EndsWith(":", StringComparison.Ordinal) ||
                text.EndsWith("♪", StringComparison.Ordinal) ||
                text.EndsWith("♫", StringComparison.Ordinal))
            {
                return false;
            }

            var next = subtitle.Paragraphs[i + 1];
            return next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds < 500;
        }

        private static bool MergeWithTwoNext(Subtitle subtitle, int i, string source)
        {
            if (i + 2 >= subtitle.Paragraphs.Count || source.ToLowerInvariant() == "zh" || source.ToLowerInvariant() == "ja")
            {
                return false;
            }

            return MergeWithNext(subtitle, i, source) && MergeWithNext(subtitle, i + 1, source);
        }

        private static bool MergeWithThreeNext(Subtitle subtitle, int i, string source)
        {
            if (i + 3 >= subtitle.Paragraphs.Count || source.ToLowerInvariant() == "zh" || source.ToLowerInvariant() == "ja")
            {
                return false;
            }

            return MergeWithNext(subtitle, i, source) && MergeWithNext(subtitle, i + 1, source) && MergeWithNext(subtitle, i + 2, source);
        }

        private static List<string> SplitResult(string result, int mergeCount, string language)
        {
            if (mergeCount == 1)
            {
                var arr = Utilities.AutoBreakLine(result, 84, 1, language).SplitToLines();
                if (arr.Count == 1)
                {
                    arr = Utilities.AutoBreakLine(result, 42, 1, language).SplitToLines();
                }

                if (arr.Count == 1)
                {
                    arr = Utilities.AutoBreakLine(result, 22, 1, language).SplitToLines();
                }

                if (arr.Count == 2)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                    };
                }

                if (arr.Count == 1)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                    };
                }

                return new List<string> { result };
            }

            if (mergeCount == 2)
            {
                var arr = SplitHelper.SplitToXLines(3, result, 84).ToArray();

                if (arr.Length == 3)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[2], 42, language == "zh" ? 0 : 25, language),
                    };
                }

                if (arr.Length == 2)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                    };
                }

                if (arr.Length == 1)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                        string.Empty,
                    };
                }

                return new List<string> { result };
            }

            if (mergeCount == 3)
            {
                var arr = SplitHelper.SplitToXLines(4, result, 84).ToArray();

                if (arr.Length == 4)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[2], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[3], 42, language == "zh" ? 0 : 25, language),
                    };
                }

                if (arr.Length == 3)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[2], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                    };
                }

                if (arr.Length == 2)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                        string.Empty,
                    };
                }

                if (arr.Length == 1)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                        string.Empty,
                        string.Empty,
                    };
                }

                return new List<string> { result };
            }

            return new List<string> { result };
        }
    }
}
