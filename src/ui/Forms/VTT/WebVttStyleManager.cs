using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms.VTT
{
    public partial class WebVttStyleManager : Form
    {
        public string Header { get; set; }

        public WebVttStyleManager(Subtitle subtitle)
        {
            InitializeComponent();
        }
    }
}
