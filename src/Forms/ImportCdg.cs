using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.CDG;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

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
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _cdgGraphics = CdgGraphicsFile.Load(fileName);
            _subtitle = new Subtitle();
            FileName = fileName;
            labelProgress.Text = string.Empty;
            labelProgress2.Text = string.Empty;
            labelFileName.Text = string.Format("File name: {0}", Path.GetFileName(fileName));
            labelDuration.Text = string.Format("Duration: {0}", TimeCode.FromSeconds(_cdgGraphics.DurationInMilliseconds / 1000.0).ToDisplayString());
            buttonCancel.Text = Configuration.Settings.Language.General.Ok;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            radioButtonBluRaySup.Enabled = false;
            buttonStart.Enabled = false;

            var cdgToImageList = new CdgToImageList();
            int total = (int)Math.Round(_cdgGraphics.DurationInMilliseconds / CdgGraphics.TimeMsFactor);

            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.DoWork += (o, args) =>
            {
                _imageList = cdgToImageList.MakeImageList(_cdgGraphics, _subtitle, (number, unique) =>
                {
                    bw.ReportProgress(number, unique);
                });
            };
            bw.RunWorkerCompleted += (o, args) =>
            {
                using (var exportBdnXmlPng = new ExportPngXml())
                {
                    var old = Configuration.Settings.Tools.ExportBluRayRemoveSmallGaps;
                    Configuration.Settings.Tools.ExportBluRayRemoveSmallGaps = false; //true;
                    exportBdnXmlPng.InitializeFromVobSubOcr(_subtitle, new SubRip(), ExportPngXml.ExportFormats.BluraySup, FileName, this, "Test123");
                    exportBdnXmlPng.ShowDialog(this);
                    Configuration.Settings.Tools.ExportBluRayRemoveSmallGaps = old;
                    DialogResult = DialogResult.OK;
                }
            };
            bw.ProgressChanged += (o, args) =>
            {
                labelProgress.Text = $"Frame {args.ProgressPercentage:#,###,##0} of {total:#,###,##0}";
                labelProgress.Refresh();
                labelProgress2.Text = $"Unique images {(int)args.UserState:#,###,##0}";
                labelProgress2.Refresh();
            };
            bw.RunWorkerAsync();
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
