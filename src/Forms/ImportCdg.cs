using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.CDG;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ImportCdg : Form, IBinaryParagraphList
    {
        private readonly CdgGraphics _cdgGraphics;
        private readonly Subtitle _subtitle;
        private List<NikseBitmap> _imageList;
        public string FileName { get; }

        public ImportCdg(string fileName)
        {
            InitializeComponent();
            _cdgGraphics = CdgGraphicsFile.Load(fileName);
            _subtitle = new Subtitle();
            FileName = fileName;
            labelProgress.Text = string.Empty;
            labelProgress2.Text = string.Empty;
            labelFileName.Text = string.Format("File name: {0}", Path.GetFileName(fileName));
            labelDuration.Text = string.Format("Duration: {0}", TimeCode.FromSeconds(_cdgGraphics.DurationInMilliseconds / 1000.0).ToShortDisplayString());
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            radioButtonSrt.Enabled = false;
            radioButtonSrtOnlyText.Enabled = false;
            radioButtonBluRaySup.Enabled = false;
            radioButtonASS.Enabled = false;

            NikseBitmap lastNBmp = null;
            int count = 0;
            int total = (int)Math.Round(_cdgGraphics.DurationInMilliseconds / CdgGraphics.TimeMsFactor);
            _imageList = new List<NikseBitmap>();
            _subtitle.Paragraphs.Clear();
            Paragraph p = null;
            int packetNumber = 0;
            var max = _cdgGraphics.NumberOfPackets;
            while (packetNumber < max)
            {
                using (var bmp = _cdgGraphics.ToBitmap(packetNumber))
                {
                    var nBmp = new NikseBitmap(bmp);
                    var timeMs = packetNumber * CdgGraphics.TimeMsFactor;
                    if (lastNBmp == null)
                    {
                        if (nBmp.Width > 0)
                        {
                            lastNBmp = nBmp;
                            p = new Paragraph(string.Empty, timeMs, timeMs);
                        }
                    }
                    else
                    {
                        if (!IsImagesTheSame(nBmp, lastNBmp))
                        {
                            if (lastNBmp.Width > 0)
                            {
                                p.EndTime.TotalMilliseconds = timeMs;
                                _subtitle.Paragraphs.Add(p);
                                _imageList.Add(lastNBmp);
                            }

                            if (nBmp.Width > 0)
                            {
                                p = new Paragraph(string.Empty, timeMs, timeMs);
                            }
                        }
                        lastNBmp = nBmp;
                    }
                }
                count++;
                packetNumber += 20;
                labelProgress.Text = $"Frame {count:#,###,##0} of {total:#,###,##0}";
                labelProgress.Refresh();
                labelProgress2.Text = $"Unique images {_imageList.Count:#,###,##0}";
                labelProgress2.Refresh();
            }

            _subtitle.Renumber();
            new FixOverlappingDisplayTimes().Fix(_subtitle, new EmptyFixCallback());

            // fix small gaps
            for (int i = 0; i < _subtitle.Paragraphs.Count - 1; i++)
            {
                var current = _subtitle.Paragraphs[i];
                var next = _subtitle.Paragraphs[i + 1];
                if (!next.StartTime.IsMaxTime && !current.EndTime.IsMaxTime)
                {
                    double gap = next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds;
                    if (gap < 999)
                    {
                        current.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                    }
                }
            }

            using (var exportBdnXmlPng = new ExportPngXml())
            {
                exportBdnXmlPng.InitializeFromVobSubOcr(_subtitle, new SubRip(), ExportPngXml.ExportFormats.BluraySup, FileName, this, "Test123");
                //        exportBdnXmlPng.SetResolution(new Point(bmp.Width, bmp.Height));
                exportBdnXmlPng.ShowDialog(this);
            }
        }

        private bool IsImagesTheSame(NikseBitmap a, NikseBitmap b)
        {
            return a.IsEqualTo(b);
        }

        public Bitmap GetSubtitleBitmap(int index, bool crop = true)
        {
            return _imageList[index].GetBitmap();
        }

        public bool GetIsForced(int index)
        {
            return false;
        }
    }
}
