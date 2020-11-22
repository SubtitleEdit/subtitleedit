using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
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

            using (var stream = assembly.GetManifestResourceStream("Nikse.SubtitleEdit.Controls.WebBrowser.WebBrowserEditBox.html"))
            using (var reader = new StreamReader(stream))
            {
                var s = reader.ReadToEnd();
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

            IsWebBrowserContextMenuEnabled = false;
            AllowWebBrowserDrop = false;
            ObjectForScripting = this;
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
                return string.Join(Environment.NewLine, text.SplitToLines());
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

                        _rightToLeft = Configuration.Settings.General.RightToLeftMode;
                        _center = Configuration.Settings.General.CenterSubtitleInTextBox;

                        var code = "left";
                        if (_rightToLeft)
                        {
                            code = "rtl";
                        }
                        else if (_center)
                        {
                            code = "center";
                        }

                        Document.InvokeScript("setTextDirection", new object[] { code });
                    }

                    var text = string.Join("\n", value.SplitToLines());
                    Document.InvokeScript("setText", new object[] { text });
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

        public int SelectionLength { get; set; }
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

        public void ClientChanged()
        {
            TextChanged?.Invoke(this, new KeyEventArgs(0));
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
        }

        public void ClientClick()
        {
            MouseClick?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 1, 1, 1, 1));
        }

        public void ClientMouseMove()
        {
            MouseMove?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 1, 1, 1, 1));
        }

        internal int GetCharIndexFromPosition(Point pt)
        {
            return 0;
        }

        public void Clear()
        {
        }

        public void Undo()
        {
        }

        public void ClearUndo()
        {
        }
    }
}