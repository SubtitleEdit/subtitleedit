using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    public enum ActionType
    {
        None,
        File,
        Tool,
        Sync,
        Translate,
        Spellcheck,
    }

    public class PluginInfo
    {
        public decimal Version { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
    }

    public class PluginInfoOnline : PluginInfo
    {
        public string Url { get; set; }
        public string Date { get; set; }
    }

    public class PluginInfoLocal : PluginInfo
    {
        public string Text { get; set; }
        public string File { get; set; }
        public string Shortcut { get; set; }
        public ActionType ActioType { get; set; }

        public Func<Form, string, double, string, string, string, string, string> DoAction;

        public override string ToString() => Name;

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Text))
            {
                return false;
            }
            if (DoAction == null || ActioType == ActionType.None)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(File))
            {
                return false;
            }
            return true;
        }

    }
}
