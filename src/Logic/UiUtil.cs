using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    internal static class UiUtil
    {
        public static readonly Lazy<string> SubtitleExtensionFilter = new Lazy<string>(GetOpenDialogFilter);

        public static VideoInfo GetVideoInfo(string fileName)
        {
            VideoInfo info = FileUtil.TryReadVideoInfoViaAviHeader(fileName);
            if (info.Success)
                return info;

            info = FileUtil.TryReadVideoInfoViaMatroskaHeader(fileName);
            if (info.Success)
                return info;

            info = FileUtil.TryReadVideoInfoViaMp4(fileName);
            if (info.Success)
                return info;

            info = TryReadVideoInfoViaDirectShow(fileName);
            if (info.Success)
                return info;

            return new VideoInfo { VideoCodec = "Unknown" };
        }

        private static VideoInfo TryReadVideoInfoViaDirectShow(string fileName)
        {
            return QuartsPlayer.GetVideoInfo(fileName);
        }

        public static int ShowSubtitle(Subtitle subtitle, VideoPlayerContainer videoPlayerContainer)
        {
            if (videoPlayerContainer.VideoPlayer != null)
            {
                double positionInMilliseconds = (videoPlayerContainer.CurrentPosition * TimeCode.BaseUnit) + 5;
                for (int i = 0; i < subtitle.Paragraphs.Count; i++)
                {
                    var p = subtitle.Paragraphs[i];
                    if (p.StartTime.TotalMilliseconds <= positionInMilliseconds &&
                        p.EndTime.TotalMilliseconds > positionInMilliseconds)
                    {
                        string text = p.Text.Replace("|", Environment.NewLine);
                        bool isInfo = p == subtitle.Paragraphs[0] && (Math.Abs(p.StartTime.TotalMilliseconds) < 0.01 && Math.Abs(p.Duration.TotalMilliseconds) < 0.01 || Math.Abs(p.StartTime.TotalMilliseconds - Pac.PacNullTime.TotalMilliseconds) < 0.01);
                        if (!isInfo)
                        {
                            if (videoPlayerContainer.LastParagraph != p)
                                videoPlayerContainer.SetSubtitleText(text, p, subtitle);
                            else if (videoPlayerContainer.SubtitleText != text)
                                videoPlayerContainer.SetSubtitleText(text, p, subtitle);
                            return i;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(videoPlayerContainer.SubtitleText))
                    videoPlayerContainer.SetSubtitleText(string.Empty, null, subtitle);
            }
            return -1;
        }

        public static int ShowSubtitle(Subtitle subtitle, Subtitle original, VideoPlayerContainer videoPlayerContainer)
        {
            if (videoPlayerContainer.VideoPlayer != null)
            {
                double positionInMilliseconds = (videoPlayerContainer.VideoPlayer.CurrentPosition * TimeCode.BaseUnit) + 15;
                for (int i = 0; i < subtitle.Paragraphs.Count; i++)
                {
                    var p = subtitle.Paragraphs[i];
                    if (p.StartTime.TotalMilliseconds <= positionInMilliseconds &&
                        p.EndTime.TotalMilliseconds > positionInMilliseconds)
                    {
                        var op = Utilities.GetOriginalParagraph(0, p, original.Paragraphs);

                        string text = p.Text.Replace("|", Environment.NewLine);
                        if (op != null)
                            text = text + Environment.NewLine + Environment.NewLine + op.Text.Replace("|", Environment.NewLine);

                        bool isInfo = p == subtitle.Paragraphs[0] && Math.Abs(p.StartTime.TotalMilliseconds) < 0.01 && positionInMilliseconds > 3000;
                        if (!isInfo)
                        {
                            if (videoPlayerContainer.LastParagraph != p)
                                videoPlayerContainer.SetSubtitleText(text, p, subtitle);
                            else if (videoPlayerContainer.SubtitleText != text)
                                videoPlayerContainer.SetSubtitleText(text, p, subtitle);
                            return i;
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(videoPlayerContainer.SubtitleText))
                videoPlayerContainer.SetSubtitleText(string.Empty, null, subtitle);
            return -1;
        }

        public static bool IsQuartsDllInstalled
        {
            get
            {
                if (Utilities.IsRunningOnMono())
                    return false;

                string quartzFileName = Environment.GetFolderPath(Environment.SpecialFolder.System).TrimEnd('\\') + @"\quartz.dll";
                return File.Exists(quartzFileName);
            }
        }

        public static bool IsMPlayerAvailable
        {
            get
            {
                if (Configuration.IsRunningOnLinux() || Utilities.IsRunningOnMono())
                    return File.Exists(Path.Combine(Configuration.BaseDirectory, "mplayer"));

                return MPlayer.GetMPlayerFileName != null;
            }
        }

        public static bool IsMpcHcInstalled
        {
            get
            {
                if (Utilities.IsRunningOnMono())
                    return false;

                try
                {
                    return VideoPlayers.MpcHC.MpcHc.GetMpcHcFileName() != null;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
            }
        }

        public static VideoPlayer GetVideoPlayer()
        {
            int RTLD_NOW = 0x0001;
            int RTLD_GLOBAL = 0x0100;
            GeneralSettings gs = Configuration.Settings.General;

            if (Configuration.IsRunningOnLinux())
            {
                var handle = NativeMethods.dlopen("libmpv.so", RTLD_NOW | RTLD_GLOBAL);
                if (handle != IntPtr.Zero)
                {
                    NativeMethods.dlclose(handle);
                    return new LibMpvMono();
                }
                return new MPlayer();
            }
            // Mono on OS X is 32 bit and thus requires 32 bit VLC. Place VLC in the same
            // folder as Subtitle Edit and add this to the app.config inside the
            // "configuration" element:
            // <dllmap dll="libvlc" target="VLC.app/Contents/MacOS/lib/libvlc.dylib" />
            if (Configuration.IsRunningOnMac())
                return new LibVlcMono();

            if (gs.VideoPlayer == "VLC" && LibVlcDynamic.IsInstalled)
                return new LibVlcDynamic();

            if (gs.VideoPlayer == "MPV" && LibMpvDynamic.IsInstalled)
                return new LibMpvDynamic();

            //if (gs.VideoPlayer == "WindowsMediaPlayer" && IsWmpAvailable)
            //    return new WmpPlayer();

            //            if (gs.VideoPlayer == "MPlayer" && MPlayer.IsInstalled)
            //                return new MPlayer();

            if (gs.VideoPlayer == "MPC-HC" && VideoPlayers.MpcHC.MpcHc.IsInstalled)
                return new VideoPlayers.MpcHC.MpcHc();

            if (IsQuartsDllInstalled)
                return new QuartsPlayer();
            //if (IsWmpAvailable)
            //    return new WmpPlayer();

            throw new NotSupportedException("You need DirectX, VLC media player 1.1.x, or MPlayer2 installed as well as Subtitle Edit dll files in order to use the video player!");
        }

        public static void InitializeVideoPlayerAndContainer(string fileName, VideoInfo videoInfo, VideoPlayerContainer videoPlayerContainer, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            try
            {
                videoPlayerContainer.VideoPlayer = GetVideoPlayer();
                videoPlayerContainer.VideoPlayer.Initialize(videoPlayerContainer.PanelPlayer, fileName, onVideoLoaded, onVideoEnded);
                videoPlayerContainer.ShowStopButton = Configuration.Settings.General.VideoPlayerShowStopButton;
                videoPlayerContainer.ShowFullscreenButton = false;
                videoPlayerContainer.ShowMuteButton = Configuration.Settings.General.VideoPlayerShowMuteButton;
                videoPlayerContainer.Volume = Configuration.Settings.General.VideoPlayerDefaultVolume;
                videoPlayerContainer.EnableMouseWheelStep();
                if (fileName != null && (fileName.StartsWith("https://") || fileName.StartsWith("http://")))
                {
                    // we don't have videoInfo for streams...
                }
                else
                {
                    videoPlayerContainer.VideoWidth = videoInfo.Width;
                    videoPlayerContainer.VideoHeight = videoInfo.Height;
                    videoPlayerContainer.VideoPlayer.Resize(videoPlayerContainer.PanelPlayer.Width, videoPlayerContainer.PanelPlayer.Height);
                }

            }
            catch (Exception exception)
            {
                videoPlayerContainer.VideoPlayer = null;
                var videoError = new VideoError();
                videoError.Initialize(fileName, videoInfo, exception);
                videoError.ShowDialog();
            }
        }

        public static void CheckAutoWrap(TextBox textBox, KeyEventArgs e, int numberOfNewLines)
        {
            // Do not autobreak lines more than 1 line.
            if (numberOfNewLines != 1 || !Configuration.Settings.General.AutoWrapLineWhileTyping)
                return;

            int length = HtmlUtil.RemoveHtmlTags(textBox.Text, true).Length;
            if (e.Modifiers == Keys.None && e.KeyCode != Keys.Enter && length > Configuration.Settings.General.SubtitleLineMaximumLength)
            {
                string newText;
                if (length > Configuration.Settings.General.SubtitleLineMaximumLength + 30)
                {
                    newText = Utilities.AutoBreakLine(textBox.Text);
                }
                else
                {
                    int lastSpace = textBox.Text.LastIndexOf(' ');
                    if (lastSpace > 0)
                        newText = textBox.Text.Remove(lastSpace, 1).Insert(lastSpace, Environment.NewLine);
                    else
                        newText = textBox.Text;
                }

                int autobreakIndex = newText.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                if (autobreakIndex > 0)
                {
                    int selectionStart = textBox.SelectionStart;
                    textBox.Text = newText;
                    if (selectionStart > autobreakIndex)
                        selectionStart += Environment.NewLine.Length - 1;
                    if (selectionStart >= 0)
                        textBox.SelectionStart = selectionStart;
                }
            }
        }

        private static readonly Dictionary<string, Keys> AllKeys = new Dictionary<string, Keys>();
        private static Keys _helpKeys;

        public static Keys GetKeys(string keysInString)
        {
            if (string.IsNullOrEmpty(keysInString))
                return Keys.None;

            if (AllKeys.Count == 0)
            {
                foreach (Keys val in Enum.GetValues(typeof(Keys)))
                {
                    string k = val.ToString().ToLower();
                    if (!AllKeys.ContainsKey(k))
                        AllKeys.Add(k, val);
                }
                if (!AllKeys.ContainsKey("pagedown"))
                    AllKeys.Add("pagedown", Keys.RButton | Keys.Space);
                if (!AllKeys.ContainsKey("home"))
                    AllKeys.Add("home", Keys.MButton | Keys.Space);
                if (!AllKeys.ContainsKey("capslock"))
                    AllKeys.Add("capslock", Keys.CapsLock);
            }

            string[] parts = keysInString.ToLower().Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            var resultKeys = Keys.None;
            foreach (string k in parts)
            {
                if (AllKeys.ContainsKey(k))
                    resultKeys = resultKeys | AllKeys[k];
            }
            return resultKeys;
        }

        public static Keys HelpKeys
        {
            get
            {
                if (_helpKeys == Keys.None)
                    _helpKeys = GetKeys(Configuration.Settings.Shortcuts.GeneralHelp);
                return _helpKeys;
            }
            set { _helpKeys = value; }
        }

        public static void SetButtonHeight(Control control, int newHeight, int level)
        {
            if (level > 6)
                return;

            if (control.HasChildren)
            {
                foreach (Control subControl in control.Controls)
                {
                    if (subControl.HasChildren)
                        SetButtonHeight(subControl, newHeight, level + 1);
                    else if (subControl is Button)
                        subControl.Height = newHeight;
                }
            }
            else if (control is Button)
                control.Height = newHeight;
        }

        public static void InitializeSubtitleFont(Control control)
        {
            var gs = Configuration.Settings.General;

            if (string.IsNullOrEmpty(gs.SubtitleFontName))
                gs.SubtitleFontName = SystemFonts.MessageBoxFont.Name;

            try
            {
                if (control is ListView)
                {
                    if (gs.SubtitleListViewFontBold)
                        control.Font = new Font(gs.SubtitleFontName, gs.SubtitleListViewFontSize, FontStyle.Bold);
                    else
                        control.Font = new Font(gs.SubtitleFontName, gs.SubtitleListViewFontSize);
                }
                else
                {
                    if (gs.SubtitleFontBold)
                        control.Font = new Font(gs.SubtitleFontName, gs.SubtitleFontSize, FontStyle.Bold);
                    else
                        control.Font = new Font(gs.SubtitleFontName, gs.SubtitleFontSize);
                }

                control.BackColor = gs.SubtitleBackgroundColor;
                control.ForeColor = gs.SubtitleFontColor;
            }
            catch
            {
                // ignored
            }
        }

        private static Font GetDefaultFont()
        {
            var gs = Configuration.Settings.General;
            if (string.IsNullOrEmpty(gs.SystemSubtitleFontNameOverride) || gs.SystemSubtitleFontSizeOverride < 5)
            {
                return SystemFonts.MessageBoxFont;
            }

            try
            {
                return new Font(gs.SystemSubtitleFontNameOverride, gs.SystemSubtitleFontSizeOverride);
            }
            catch
            {
                return SystemFonts.MessageBoxFont;
            }
        }

        internal static void PreInitialize(Form form)
        {
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.Font = GetDefaultFont();
        }

        public static void FixFonts(Control form, int iterations = 5)
        {
            FixFontsInner(form, iterations);
            if (Configuration.Settings.General.UseDarkTheme)
                DarkTheme.SetDarkTheme(form, 1500);
        }

        public static void FixFonts(ToolStripMenuItem item)
        {
            item.Font = GetDefaultFont();
            if (Configuration.Settings.General.UseDarkTheme)
                DarkTheme.SetDarkTheme(item);
        }

        public static void FixFonts(ToolStripSeparator item)
        {
            item.Font = GetDefaultFont();
            if (Configuration.Settings.General.UseDarkTheme)
                DarkTheme.SetDarkTheme(item);
        }

        internal static void FixFonts(ToolStripItem item)
        {
            item.Font = GetDefaultFont();
            if (Configuration.Settings.General.UseDarkTheme)
                DarkTheme.SetDarkTheme(item);
        }

        private static void FixFontsInner(Control form, int iterations = 5)
        {
            if (iterations < 1)
                return;

            foreach (Control c in form.Controls)
            {
                if (!c.Font.Name.Equals("Tahoma", StringComparison.Ordinal))
                {
                    c.Font = GetDefaultFont();
                }
                foreach (Control inner in c.Controls)
                {
                    FixFontsInner(inner, iterations - 1);
                }
            }
        }

        public static void FixLargeFonts(Control mainCtrl, Control ctrl)
        {
            using (Graphics graphics = mainCtrl.CreateGraphics())
            {
                SizeF textSize = graphics.MeasureString(ctrl.Text, ctrl.Font);
                if (textSize.Height > ctrl.Height - 4)
                {
                    SetButtonHeight(mainCtrl, (int)Math.Round(textSize.Height + 7.5), 1);
                }
            }
        }

        public static void SetSaveDialogFilter(SaveFileDialog saveFileDialog, SubtitleFormat currentFormat)
        {
            var sb = new StringBuilder();
            int index = 0;
            foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
            {
                sb.Append(format.Name + "|*" + format.Extension + "|");
                if (currentFormat.Name == format.Name)
                    saveFileDialog.FilterIndex = index + 1;
                index++;
            }
            saveFileDialog.Filter = sb.ToString().TrimEnd('|');
        }

        public static void GetLineLengths(Label label, string text)
        {
            label.ForeColor = Color.Black;
            var lines = text.SplitToLines();

            const int max = 3;

            var sb = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                if (i > 0)
                {
                    sb.Append('/');
                }

                if (i > max)
                {
                    label.ForeColor = Color.Red;
                    sb.Append("...");
                    label.Text = sb.ToString();
                    return;
                }

                sb.Append(line.Length);
                if (line.Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                    label.ForeColor = Color.Red;
            }
            label.Text = sb.ToString();
        }

        public static void InitializeSubtitleFormatComboBox(ToolStripComboBox comboBox, SubtitleFormat format)
        {
            InitializeSubtitleFormatComboBox(comboBox.ComboBox, format);
            comboBox.DropDownWidth += 5; // .Net quirk?
        }

        public static void InitializeSubtitleFormatComboBox(ComboBox comboBox, SubtitleFormat format)
        {
            InitializeSubtitleFormatComboBox(comboBox, new[] { format.FriendlyName }, format.FriendlyName);
        }

        public static void InitializeSubtitleFormatComboBox(ToolStripComboBox comboBox, string selectedName)
        {
            InitializeSubtitleFormatComboBox(comboBox.ComboBox, selectedName);
            comboBox.DropDownWidth += 5; // .Net quirk?
        }

        public static void InitializeSubtitleFormatComboBox(ComboBox comboBox, string selectedName)
        {
            var formatNames = SubtitleFormat.AllSubtitleFormats.Where(format => !format.IsVobSubIndexFile).Select(format => format.FriendlyName);
            InitializeSubtitleFormatComboBox(comboBox, formatNames, selectedName);
        }

        public static void InitializeSubtitleFormatComboBox(ComboBox comboBox, IEnumerable<string> formatNames, string selectedName)
        {
            var selectedIndex = 0;
            comboBox.BeginUpdate();
            comboBox.Items.Clear();
            using (var graphics = comboBox.CreateGraphics())
            {
                var maxWidth = 0.0F;
                foreach (var name in formatNames)
                {
                    var index = comboBox.Items.Add(name);
                    if (name.Equals(selectedName, StringComparison.OrdinalIgnoreCase))
                        selectedIndex = index;
                    var width = graphics.MeasureString(name, comboBox.Font).Width;
                    if (width > maxWidth)
                        maxWidth = width;
                }
                comboBox.DropDownWidth = (int)Math.Round(maxWidth + 7.5);
            }
            comboBox.SelectedIndex = selectedIndex;
            comboBox.EndUpdate();
        }

        public static void InitializeTextEncodingComboBox(ComboBox comboBox)
        {
            var defaultEncoding = Configuration.Settings.General.DefaultEncoding;
            var selectedItem = (TextEncodingListItem)null;
            comboBox.BeginUpdate();
            comboBox.Items.Clear();
            using (var graphics = comboBox.CreateGraphics())
            {
                var maxWidth = 0.0F;
                foreach (var encoding in Configuration.AvailableEncodings)
                {
                    if (encoding.CodePage >= 949 && !encoding.IsEbcdic())
                    {
                        var item = new TextEncodingListItem(encoding);
                        if (selectedItem == null && item.Equals(defaultEncoding))
                            selectedItem = item;
                        var width = graphics.MeasureString(item.DisplayName, comboBox.Font).Width;
                        if (width > maxWidth)
                            maxWidth = width;
                        if (encoding.CodePage.Equals(Encoding.UTF8.CodePage))
                            comboBox.Items.Insert(0, item);
                        else
                            comboBox.Items.Add(item);
                    }
                }
                comboBox.DropDownWidth = (int)Math.Round(maxWidth + 7.5);
            }
            if (selectedItem == null)
                comboBox.SelectedIndex = 0; // UTF-8 if DefaultEncoding is not found
            else
                comboBox.SelectedItem = selectedItem;
            comboBox.EndUpdate();
            Configuration.Settings.General.DefaultEncoding = (comboBox.SelectedItem as TextEncodingListItem).Encoding.WebName;
            comboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBox.AutoCompleteMode = AutoCompleteMode.Append;
        }

        public static Encoding GetTextEncodingComboBoxCurrentEncoding(ComboBox comboBox)
        {
            if (comboBox.SelectedIndex > 0)
            {
                return (comboBox.SelectedItem as TextEncodingListItem).Encoding;
            }
            return Encoding.UTF8;
        }

        private const string BreakChars = "\",:;.¡!¿?()[]{}<>♪♫-–—/#*|";

        public static void ApplyControlBackspace(TextBox textBox)
        {
            if (textBox.SelectionLength == 0)
            {
                var text = textBox.Text;
                var deleteUpTo = textBox.SelectionStart;
                if (deleteUpTo > 0 && deleteUpTo <= text.Length)
                {
                    text = text.Substring(0, deleteUpTo);
                    var textElementIndices = StringInfo.ParseCombiningCharacters(text);
                    var index = textElementIndices.Length;
                    var textIndex = deleteUpTo;
                    var deleteFrom = -1;
                    while (index > 0)
                    {
                        index--;
                        textIndex = textElementIndices[index];
                        if (!IsSpaceCategory(CharUnicodeInfo.GetUnicodeCategory(text, textIndex)))
                            break;
                    }
                    if (index > 0) // HTML tag?
                    {
                        if (text[textIndex] == '>')
                        {
                            var openingBracketIndex = text.LastIndexOf('<', textIndex - 1);
                            if (openingBracketIndex >= 0 && text.IndexOf('>', openingBracketIndex + 1) == textIndex)
                                deleteFrom = openingBracketIndex; // delete whole tag
                        }
                        else if (text[textIndex] == '}')
                        {
                            var startIdx = text.LastIndexOf(@"{\", textIndex - 1, StringComparison.Ordinal);
                            if (startIdx >= 0 && text.IndexOf('}', startIdx + 1) == textIndex)
                            {
                                deleteFrom = startIdx;
                            }
                        }
                    }
                    if (deleteFrom < 0)
                    {
                        if (BreakChars.Contains(text[textIndex]))
                            deleteFrom = -2;
                        while (index > 0)
                        {
                            index--;
                            textIndex = textElementIndices[index];
                            if (IsSpaceCategory(CharUnicodeInfo.GetUnicodeCategory(text, textIndex)))
                            {
                                if (deleteFrom > -2)
                                {
                                    if (deleteFrom < 0)
                                        deleteFrom = textElementIndices[index + 1];
                                    break;
                                }
                                deleteFrom = textElementIndices[index + 1];
                                if (!":!?".Contains(text[deleteFrom]))
                                    break;
                            }
                            else if (BreakChars.Contains(text[textIndex]))
                            {
                                if (deleteFrom > -2)
                                {
                                    if (deleteFrom < 0)
                                        deleteFrom = textElementIndices[index + 1];
                                    break;
                                }
                            }
                            else
                            {
                                deleteFrom = -1;
                            }
                        }
                    }
                    if (deleteFrom < deleteUpTo)
                    {
                        if (deleteFrom < 0)
                            deleteFrom = 0;
                        textBox.Select(deleteFrom, deleteUpTo - deleteFrom);
                        textBox.Paste(string.Empty);
                    }
                }
            }
        }

        public static void SelectWordAtCaret(TextBox textBox)
        {
            var text = textBox.Text;
            var endIndex = textBox.SelectionStart;
            var startIndex = endIndex;

            while (startIndex > 0 && !IsSpaceCategory(CharUnicodeInfo.GetUnicodeCategory(text[startIndex - 1])) && !BreakChars.Contains(text[startIndex - 1]))
            {
                startIndex--;
            }
            textBox.SelectionStart = startIndex;

            while (endIndex < text.Length && !IsSpaceCategory(CharUnicodeInfo.GetUnicodeCategory(text[endIndex])) && !BreakChars.Contains(text[endIndex]))
            {
                endIndex++;
            }
            textBox.SelectionLength = endIndex - startIndex;
        }

        private static bool IsSpaceCategory(UnicodeCategory c)
        {
            return c == UnicodeCategory.SpaceSeparator || c == UnicodeCategory.Control || c == UnicodeCategory.LineSeparator || c == UnicodeCategory.ParagraphSeparator;
        }

        private static void AddExtension(StringBuilder sb, string extension)
        {
            if (!sb.ToString().Contains("*" + extension + ";", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append('*');
                sb.Append(extension.TrimStart('*'));
                sb.Append(';');
            }
        }

        private static string GetOpenDialogFilter()
        {
            var sb = new StringBuilder();
            sb.Append(Configuration.Settings.Language.General.SubtitleFiles + "|");
            foreach (SubtitleFormat s in SubtitleFormat.AllSubtitleFormats)
            {
                AddExtension(sb, s.Extension);
                foreach (string ext in s.AlternateExtensions)
                    AddExtension(sb, ext);
            }
            AddExtension(sb, new Pac().Extension);
            AddExtension(sb, new Cavena890().Extension);
            AddExtension(sb, new Spt().Extension);
            AddExtension(sb, new Wsb().Extension);
            AddExtension(sb, new CheetahCaption().Extension);
            AddExtension(sb, ".chk");
            AddExtension(sb, new CaptionsInc().Extension);
            AddExtension(sb, new Ultech130().Extension);
            AddExtension(sb, new ELRStudioClosedCaption().Extension);
            AddExtension(sb, ".uld"); // Ultech drop frame
            AddExtension(sb, new SonicScenaristBitmaps().Extension);
            AddExtension(sb, ".mks");
            AddExtension(sb, ".mxf");
            AddExtension(sb, ".sup");
            AddExtension(sb, ".dost");
            AddExtension(sb, new FinalDraftTemplate2().Extension);
            AddExtension(sb, new Ayato().Extension);
            AddExtension(sb, new PacUnicode().Extension);
            AddExtension(sb, new WinCaps32().Extension);
            AddExtension(sb, new IsmtDfxp().Extension);

            if (!string.IsNullOrEmpty(Configuration.Settings.General.OpenSubtitleExtraExtensions))
            {
                var extraExtensions = Configuration.Settings.General.OpenSubtitleExtraExtensions.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string ext in extraExtensions)
                {
                    if (ext.StartsWith("*.", StringComparison.Ordinal) && !sb.ToString().Contains(ext, StringComparison.OrdinalIgnoreCase))
                        AddExtension(sb, ext);
                }
            }
            AddExtension(sb, ".son");

            sb.Append('|');
            sb.Append(Configuration.Settings.Language.General.AllFiles);
            sb.Append("|*.*");
            return sb.ToString();
        }

        public static string GetListViewTextFromString(string s) => s.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString);

        public static string GetStringFromListViewText(string lviText) => lviText.Replace(Configuration.Settings.General.ListViewLineSeparatorString, Environment.NewLine);

        public static void SelectAll(this ListView lv)
        {
            lv.BeginUpdate();
            foreach (ListViewItem item in lv.Items)
                item.Selected = true;
            lv.EndUpdate();
        }

        public static void SelectFirstSelectedItemOnly(this ListView lv)
        {
            int itemsCount = lv.SelectedItems.Count - 1;
            if (itemsCount > 0)
            {
                lv.BeginUpdate();
                do
                {
                    lv.SelectedItems[itemsCount--].Selected = false;
                }
                while (itemsCount > 0);
                if (lv.SelectedItems.Count > 0)
                {
                    lv.EnsureVisible(lv.SelectedItems[0].Index);
                    lv.FocusedItem = lv.SelectedItems[0];
                }
                else if (lv.Items.Count > 0)
                {
                    lv.EnsureVisible(0);
                    lv.FocusedItem = lv.Items[0];
                }
                lv.EndUpdate();
            }
        }

        public static void InverseSelection(this ListView lv)
        {
            lv.BeginUpdate();
            foreach (ListViewItem item in lv.Items)
                item.Selected = !item.Selected;
            lv.EndUpdate();
        }

    }
}
