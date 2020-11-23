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
                var sb = new StringBuilder();
                int max = text.Length;
                int i = 0;
                var isNewLine = false;
                while (i < max)
                {
                    var ch = text[i];
                    if (ch == '\r')
                    {
                        //continue
                    }
                    else if (ch == '\n')
                    {
                        if (!isNewLine)
                        {
                            sb.Append(Environment.NewLine);
                        }

                        isNewLine = !isNewLine;
                    }
                    else
                    {
                        sb.Append(ch);
                        isNewLine = false;
                    }

                    i++;
                }

                var result = sb.ToString();
                return result;
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
                    //Document.InvokeScript("setText", new object[] { value });
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
        }

        public void ClientClick()
        {
            var mp = PointToClient(MousePosition);
            MouseClick?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 1, mp.X, mp.Y, 1));
            TextChanged?.Invoke(this, new KeyEventArgs(0));
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

        private string _lastHtml = null;

        public void UpdateSyntaxColor(string text)
        {
            var html = HighLightHtml(text);
            if (html.Contains("\n"))
            {
                html = html.Replace("\r\n\r\n", "<br />");
                html = html.Replace("\r\n", "<br />");
                html = html.Replace("\n", "<br />");
            }

            if (html != _lastHtml && Document != null)
            {
                Document.InvokeScript("setHtml", new object[] { html });
                _lastHtml = html;
            }
        }

        public string HighLightHtml(string text)
        {
            if (text == null)
            {
                return string.Empty;
            }

            bool htmlTagOn = false;
            bool htmlTagFontOn = false;
            int htmlTagStart = -1;
            bool assaTagOn = false;
            bool assaPrimaryColorTagOn = false;
            bool assaSecondaryColorTagOn = false;
            bool assaBorderColorTagOn = false;
            bool assaShadowColorTagOn = false;
            var assaTagStart = -1;
            int tagOn = -1;
            var textLength = text.Length;
            int i = 0;
            var sb = new StringBuilder();

            while (i < textLength)
            {
                var ch = text[i];
                if (assaTagOn)
                {
                    if (ch == '}' && tagOn >= 0)
                    {
                        assaTagOn = false;
                        sb.Append($"<font color=\"{ColorTranslator.ToHtml(Configuration.Settings.General.SubtitleTextBoxAssColor)}\">");
                        var inner = text.Substring(assaTagStart, i - assaTagStart + 1);
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
                        sb.Append($"<font color=\"{ColorTranslator.ToHtml(Configuration.Settings.General.SubtitleTextBoxHtmlColor)}\">");
                        var inner = text.Substring(htmlTagStart, i - htmlTagStart + 1);
                        sb.Append(WebUtility.HtmlEncode(inner));
                        sb.Append("</font>");
                        htmlTagStart = -1;
                    }
                }
                else if (ch == '{' && i < textLength - 1 && text[i + 1] == '\\' && text.IndexOf('}', i) > 0)
                {
                    var s = text.Substring(i);
                    assaTagOn = true;
                    tagOn = i;
                    assaTagStart = i;
                }
                else if (ch == '<')
                {
                    var s = text.Substring(i);
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
                         text.IndexOf("</font>", i, StringComparison.OrdinalIgnoreCase) > 0))
                    {
                        htmlTagOn = true;
                        htmlTagStart = i;
                        htmlTagFontOn = s.StartsWith("<font ", StringComparison.OrdinalIgnoreCase);
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

            return sb.ToString();
        }

    }
}