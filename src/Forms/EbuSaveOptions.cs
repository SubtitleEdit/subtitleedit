using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class EbuSaveOptions : Form
    {
        public EbuSaveOptions()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Ebu stl files (*.stl)|*.stl";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            { 
            }
        }
    }
}
