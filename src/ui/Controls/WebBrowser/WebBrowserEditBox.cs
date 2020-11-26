using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls.WebBrowser
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class WebBrowserEditBox : System.Windows.Forms.WebBrowser
    {
        private bool _rightToLeft;
        private bool _center;
        private long _lastKeyOrClick = -1;
        private bool _lastKeyOrClickActivity;
        private readonly System.Windows.Forms.Timer _timerSyntaxColor;

        public new event EventHandler TextChanged;
        public new event KeyEventHandler KeyDown;
        public new event MouseEventHandler MouseClick;
        public new event EventHandler Enter;
        public new event KeyEventHandler KeyUp;
        public new event EventHandler Leave;
        public new event MouseEventHandler MouseMove;

        public WebBrowserEditBox()
        {
            DocumentCompleted += (sender, args) =>
            {
                Application.DoEvents();
                Thread.Sleep(5);
                Application.DoEvents();
                Thread.Sleep(5);
                Application.DoEvents();
                Document?.InvokeScript("initEvents");
            };
            _timerSyntaxColor = new System.Windows.Forms.Timer { Interval = 100 };
            _timerSyntaxColor.Tick += (sender, args) => { UpdateSyntaxColorFromJs(); };
            _timerSyntaxColor.Start();
        }

        public void Initialize()
        {
            var assembly = Assembly.GetExecutingAssembly();
            BringToFront();

            LoadHtml(assembly);

            IsWebBrowserContextMenuEnabled = false;
            AllowWebBrowserDrop = false;
            ObjectForScripting = this;
        }

        private void LoadHtml(Assembly assembly)
        {
            using (var stream = assembly.GetManifestResourceStream("Nikse.SubtitleEdit.Controls.WebBrowser.WebBrowserEditBox.html"))
            using (var reader = new StreamReader(stream))
            {
                var s = reader.ReadToEnd();
                s = s.Replace("color: brown;", "color: " + ColorTranslator.ToHtml(Configuration.Settings.General.SubtitleFontColor) + ";");
                s = s.Replace("background-color: lightblue;", "background-color: " + ColorTranslator.ToHtml(Configuration.Settings.General.SubtitleBackgroundColor) + ";");
                
                DocumentText = s;
            }

            Application.DoEvents();
            Thread.Sleep(5);
            Application.DoEvents();
            for (int i = 0; i < 200 && ReadyState != WebBrowserReadyState.Complete; i++)
            {
                Application.DoEvents();
                Thread.Sleep(5);
            }
        }

        public override string Text
        {
            get
            {
                if (Document == null)
                {
                    return string.Empty;
                }

                var text = (Document.InvokeScript("getText") ?? string.Empty).ToString();
                return text;
            }
            set
            {
                if (Document != null)
                {
                    if (Configuration.Settings.General.RightToLeftMode != _rightToLeft ||
                        Configuration.Settings.General.CenterSubtitleInTextBox != _center)
                    {
                        Application.DoEvents();
                        Thread.Sleep(5);
                        Application.DoEvents();

                        _center = Configuration.Settings.General.CenterSubtitleInTextBox;
                        _rightToLeft = Configuration.Settings.General.RightToLeftMode;

                        var align = _center ? "text-align:center" : string.Empty;
                        var dir = _rightToLeft ? "rtl" : string.Empty;

                        Document.InvokeScript("setTextDirection", new object[] { align, dir });
                    }

                    UpdateSyntaxColor(value);
                }
            }
        }

        public new bool Enabled
        {
            get
            {
                if (Document == null)
                {
                    return false;
                }

                var result = Document.InvokeScript("getEnabled");
                return Convert.ToBoolean(result);
            }
            set
            {
                if (Document == null)
                {
                    return;
                }

                Document.InvokeScript("setEnabled", new object[] { value });
            }
        }

        public string SelectedText
        {
            get
            {
                if (Document == null)
                {
                    return string.Empty;
                }

                return (Document.InvokeScript("getSelectionText") ?? string.Empty).ToString();
            }
            set
            {
                //TODO:
            }
        }

        public int SelectionStart
        {
            get
            {
                if (Document == null)
                {
                    return 0;
                }

                var position = (Document.InvokeScript("getCursorPosition") ?? "0").ToString();
                if (int.TryParse(position, out var result))
                {
                    var text = Text;
                    if (text.Length < result)
                    {
                        return text.Length;
                    }
                    return result;
                }

                return 0;
            }
            set
            {
                //TODO:
            }
        }

        public int SelectionLength
        {
            get => SelectedText.Length;
            set
            {
                //TODO: fix
            }
        }

        public bool HideSelection { get; set; }

        public void SelectAll()
        {
            if (Document == null)
            {
                return;
            }

            Document.ExecCommand("SelectAll", false, null);
        }

        public void Copy()
        {
            if (Document == null)
            {
                return;
            }

            Document.ExecCommand("copy", false, null);
        }

        public void Cut()
        {
            if (Document == null)
            {
                return;
            }

            Document.ExecCommand("cut", false, null);
        }

        public void Paste()
        {
            if (Document == null)
            {
                return;
            }

            Document.ExecCommand("paste", false, null);
        }

        public void ClientKeyDown(int keyCode, bool ctrlKey, bool shiftKey, bool altKey)
        {
            var keyData = (Keys)keyCode;
            if (ctrlKey)
            {
                keyData = (keyData | Keys.Control);
            }
            if (shiftKey)
            {
                keyData = (keyData | Keys.Shift);
            }
            if (altKey)
            {
                keyData = (keyData | Keys.Alt);
            }

            KeyDown?.Invoke(this, new KeyEventArgs(keyData));
        }

        public void ClientFocus()
        {
            Enter?.Invoke(this, new KeyEventArgs(0));
        }

        public void ClientBlur()
        {
            Leave?.Invoke(this, new KeyEventArgs(0));
        }

        public void ClientKeyUp(int keyCode, bool ctrlKey, bool shiftKey, bool altKey)
        {
            var keyData = (Keys)keyCode;
            if (ctrlKey)
            {
                keyData = (keyData | Keys.Control);
            }
            if (shiftKey)
            {
                keyData = (keyData | Keys.Shift);
            }
            if (altKey)
            {
                keyData = (keyData | Keys.Alt);
            }

            KeyUp?.Invoke(this, new KeyEventArgs(keyData));
            TextChanged?.Invoke(this, new KeyEventArgs(0));
            _lastKeyOrClick = DateTime.UtcNow.Ticks;
            _lastKeyOrClickActivity = true;
        }

        public void ClientClick()
        {
            var mp = PointToClient(MousePosition);
            MouseClick?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 1, mp.X, mp.Y, 1));
            TextChanged?.Invoke(this, new KeyEventArgs(0));
            _lastKeyOrClick = DateTime.UtcNow.Ticks;
            _lastKeyOrClickActivity = true;
        }

        public void ClientMouseMove()
        {
            var mp = PointToClient(MousePosition);
            MouseMove?.Invoke(this, new MouseEventArgs(MouseButtons.None, 0, mp.X, mp.Y, 0));
        }

        internal int GetCharIndexFromPosition(Point pt)
        {
            return 0;
        }

        public void Clear()
        {
            Text = string.Empty;
        }

        public void Undo()
        {
            if (Document == null)
            {
                return;
            }

            Document.ExecCommand("undo", false, null);
        }

        public void ClearUndo()
        {
        }

        private string _lastHtml;

        public void UpdateSyntaxColor(string text)
        {
            _lastKeyOrClick = DateTime.UtcNow.Ticks;
            _lastKeyOrClickActivity = false;
            var html = HighLightHtml(text);
            if (html != _lastHtml && Document != null)
            {
                Document.InvokeScript("setHtml", new object[] { html });
                _lastHtml = html;
            }
        }

        public void UpdateSyntaxColorFromJs()
        {
            if (!_lastKeyOrClickActivity)
            {
                return; // exit if not key or click activity
            }

            var diff = DateTime.UtcNow.Ticks - _lastKeyOrClick;
            if (diff < 10_000 * 750)
            {
                return; // exit if activity within the last 500 ms
            }

            _lastKeyOrClick = DateTime.UtcNow.Ticks;

            var text = Text;
            var html = HighLightHtml(text);
            if (html != _lastHtml && Document != null)
            {
                //var oldHtml = (Document.InvokeScript("getHtml") ?? "0").ToString();
                Document.InvokeScript("setHtml", new object[] { html });
                _lastHtml = html;
            }
        }

        public string HighLightHtml(string text)
        {
            bool htmlTagOn = false;
            int htmlTagStart = -1;
            bool assaTagOn = false;
            var assaTagStart = -1;
            int tagOn = -1;
            var sb = new StringBuilder();
            var lines = text.SplitToLines();
            var multiLine = lines.Count > 1;
            foreach (var line in lines)
            {
                int i = 0;
                var textLength = line.Length;
                if (multiLine)
                {
                    sb.Append("<p>");
                }

                while (i < textLength)
                {
                    var ch = line[i];
                    if (assaTagOn)
                    {
                        if (ch == '}' && tagOn >= 0)
                        {
                            assaTagOn = false;
                            sb.Append("<font color=\"#4444dd\">");
                            var inner = line.Substring(assaTagStart, i - assaTagStart + 1);
                            sb.Append(WebUtility.HtmlEncode(inner));
                            sb.Append("</font>");
                            assaTagStart = -1;
                        }
                    }
                    else if (htmlTagOn)
                    {
                        if (ch == '>' && tagOn >= 0)
                        {
                            htmlTagOn = false;
                            sb.Append("<font color=\"#44dd44\">");
                            var inner = line.Substring(htmlTagStart, i - htmlTagStart + 1);
                            sb.Append(WebUtility.HtmlEncode(inner));
                            sb.Append("</font>");
                            htmlTagStart = -1;
                        }
                    }
                    else if (ch == '{' && i < textLength - 1 && line[i + 1] == '\\' && line.IndexOf('}', i) > 0)
                    {
                        assaTagOn = true;
                        tagOn = i;
                        assaTagStart = i;
                    }
                    else if (ch == '<')
                    {
                        var s = line.Substring(i);
                        if (s.StartsWith("<i>", StringComparison.OrdinalIgnoreCase) ||
                            s.StartsWith("<b>", StringComparison.OrdinalIgnoreCase) ||
                            s.StartsWith("<u>", StringComparison.OrdinalIgnoreCase) ||
                            s.StartsWith("</i>", StringComparison.OrdinalIgnoreCase) ||
                            s.StartsWith("</b>", StringComparison.OrdinalIgnoreCase) ||
                            s.StartsWith("</u>", StringComparison.OrdinalIgnoreCase) ||
                            s.StartsWith("<box>", StringComparison.OrdinalIgnoreCase) ||
                            s.StartsWith("</box>", StringComparison.OrdinalIgnoreCase) ||
                            s.StartsWith("</font>", StringComparison.OrdinalIgnoreCase) ||
                            (s.StartsWith("<font ", StringComparison.OrdinalIgnoreCase) &&
                             line.IndexOf("</font>", i, StringComparison.OrdinalIgnoreCase) > 0))
                        {
                            htmlTagOn = true;
                            htmlTagStart = i;
                            tagOn = i;
                        }
                        else
                        {
                            sb.Append(WebUtility.HtmlEncode(ch.ToString()));
                        }
                    }
                    else
                    {

                        sb.Append(WebUtility.HtmlEncode(ch.ToString()));
                    }

                    i++;
                }
                if (line == string.Empty && multiLine)
                {
                    sb.Append("<br>");
                }
                if (multiLine)
                {
                    sb.Append("</p>");
                }
            }

            return sb.ToString();
        }

    }
}