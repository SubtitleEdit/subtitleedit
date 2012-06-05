using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{


    public partial class AlignmentPicker : Form
    {
        Timer t1 = new Timer();
        public ContentAlignment Alignment { get; set; }

        public AlignmentPicker()
        {
            InitializeComponent();
            t1.Interval = 100;
            t1.Tick += new EventHandler(t1_Tick);
            t1.Start();
            Text = Configuration.Settings.Language.SubStationAlphaStyles.Alignment;

            button1.Text = Configuration.Settings.Language.SubStationAlphaStyles.TopLeft;
            button2.Text = Configuration.Settings.Language.SubStationAlphaStyles.TopCenter;
            button3.Text = Configuration.Settings.Language.SubStationAlphaStyles.TopRight;

            button4.Text = Configuration.Settings.Language.SubStationAlphaStyles.MiddleLeft;
            button5.Text = Configuration.Settings.Language.SubStationAlphaStyles.MiddleCenter;
            button6.Text = Configuration.Settings.Language.SubStationAlphaStyles.MiddleRight;

            button7.Text = Configuration.Settings.Language.SubStationAlphaStyles.BottomLeft;
            button8.Text = Configuration.Settings.Language.SubStationAlphaStyles.BottomCenter;
            button9.Text = Configuration.Settings.Language.SubStationAlphaStyles.BottomRight;
        }

        public void Done()
        {
            DialogResult = DialogResult.OK;
        }

        void t1_Tick(object sender, EventArgs e)
        {
            var currentPos = System.Windows.Forms.Cursor.Position;
            if (currentPos.X < this.Left || currentPos.X > this.Left + this.Width)
                Close();
            if (currentPos.Y < this.Top || currentPos.Y > this.Top + this.Height)
                Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.TopLeft;
            Done();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.TopCenter;
            Done();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.TopRight;
            Done();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.MiddleLeft;
            Done();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.MiddleCenter;
            Done();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.MiddleRight;
            Done();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.BottomLeft;
            Done();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.BottomCenter;
            Done();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.BottomRight;
            Done();
        }

        private void AlignmentPicker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void AlignmentPicker_Shown(object sender, EventArgs e)
        {
            button8.Focus();
        }

    }
}
