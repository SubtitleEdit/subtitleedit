using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Forms;

namespace Nikse.SubtitleEdit
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (Nikse.SubtitleEdit.Logic.Utilities.AllLetters == "A") // hack to force load of settings
                return;
            Application.Run(new Main());
        }
    }
}
