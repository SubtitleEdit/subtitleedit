﻿using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.MpcHC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Settings;

namespace Nikse.SubtitleEdit.Logic
{
    internal static class UiUtil
    {
        public static readonly Lazy<string> SubtitleExtensionFilter = new Lazy<string>(GetOpenDialogFilter);

        public static VideoInfo GetVideoInfo(string fileName)
        {
            var info = FileUtil.TryReadVideoInfoViaAviHeader(fileName);
            if (info.Success)
            {
                return info;
            }

            info = FileUtil.TryReadVideoInfoViaMatroskaHeader(fileName);
            if (info.Success)
            {
                return info;
            }

            info = FileUtil.TryReadVideoInfoViaMp4(fileName);
            if (info.Success)
            {
                return info;
            }

            info = TryReadVideoInfoViaDirectShow(fileName);
            if (info.Success)
            {
                return info;
            }

            info = TryReadVideoInfoViaLibMpv(fileName);
            if (info.Success)
            {
                return info;
            }

            return new VideoInfo { VideoCodec = "Unknown" };
        }

        private static VideoInfo TryReadVideoInfoViaDirectShow(string fileName)
        {
            return QuartsPlayer.GetVideoInfo(fileName);
        }

        private static VideoInfo TryReadVideoInfoViaLibMpv(string fileName)
        {
            return LibMpvDynamic.GetVideoInfo(fileName);
        }

        private static long _lastShowSubTicks = DateTime.UtcNow.Ticks;
        private static int _lastShowSubHash;

        public static int ShowSubtitle(Subtitle subtitle, VideoPlayerContainer videoPlayerContainer, SubtitleFormat format)
        {
            if (videoPlayerContainer.VideoPlayer == null)
            {
                return -1;
            }

            double positionInMilliseconds = videoPlayerContainer.CurrentPosition * TimeCode.BaseUnit;
            var max = subtitle.Paragraphs.Count;
            for (int i = 0; i < max; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (p.StartTime.TotalMilliseconds <= positionInMilliseconds + 0.01 && p.EndTime.TotalMilliseconds >= positionInMilliseconds - 0.01)
                {
                    string text = p.Text.Replace("|", Environment.NewLine);
                    bool isInfo = p == subtitle.Paragraphs[0] && (Math.Abs(p.StartTime.TotalMilliseconds) < 0.01 && Math.Abs(p.DurationTotalMilliseconds) < 0.01 || Math.Abs(p.StartTime.TotalMilliseconds - Pac.PacNullTime.TotalMilliseconds) < 0.01);
                    if (!isInfo)
                    {
                        if (videoPlayerContainer.LastParagraph != p)
                        {
                            videoPlayerContainer.SetSubtitleText(text, p, subtitle, format);
                        }
                        else if (videoPlayerContainer.SubtitleText != text)
                        {
                            videoPlayerContainer.SetSubtitleText(text, p, subtitle, format);
                        }
                        TimeOutRefresh(subtitle, videoPlayerContainer, format, p);
                        return i;
                    }
                }
            }

            if (!string.IsNullOrEmpty(videoPlayerContainer.SubtitleText))
            {
                videoPlayerContainer.SetSubtitleText(string.Empty, null, subtitle, format);
            }
            else
            {
                TimeOutRefresh(subtitle, videoPlayerContainer, format);
            }
            return -1;
        }

        private static void TimeOutRefresh(Subtitle subtitle, VideoPlayerContainer videoPlayerContainer, SubtitleFormat format, Paragraph p = null)
        {
            if (DateTime.UtcNow.Ticks - _lastShowSubTicks > 10000 * 1000) // more than 1+ seconds ago
            {
                var newHash = subtitle.GetFastHashCode(string.Empty);
                if (newHash != _lastShowSubHash)
                {
                    videoPlayerContainer.SetSubtitleText(p == null ? string.Empty : p.Text, p, subtitle, format);
                    _lastShowSubHash = newHash;
                }

                _lastShowSubTicks = DateTime.UtcNow.Ticks;
            }
        }

        public static int ShowSubtitle(Subtitle subtitle, Subtitle original, VideoPlayerContainer videoPlayerContainer, SubtitleFormat format)
        {
            if (videoPlayerContainer.VideoPlayer == null)
            {
                return -1;
            }

            double positionInMilliseconds = (videoPlayerContainer.VideoPlayer.CurrentPosition * TimeCode.BaseUnit) + 15;
            var max = subtitle.Paragraphs.Count;
            for (int i = 0; i < max; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (p.StartTime.TotalMilliseconds <= positionInMilliseconds && p.EndTime.TotalMilliseconds > positionInMilliseconds)
                {
                    var op = Utilities.GetOriginalParagraph(0, p, original.Paragraphs);
                    string text = p.Text.Replace("|", Environment.NewLine);
                    if (op != null)
                    {
                        text = text + Environment.NewLine + Environment.NewLine + op.Text.Replace("|", Environment.NewLine);
                    }

                    bool isInfo = p == subtitle.Paragraphs[0] && Math.Abs(p.StartTime.TotalMilliseconds) < 0.01 && positionInMilliseconds > 3000;
                    if (!isInfo)
                    {
                        if (videoPlayerContainer.LastParagraph != p || videoPlayerContainer.SubtitleText != text)
                        {
                            videoPlayerContainer.SetSubtitleText(text, p, subtitle, format);
                        }
                        return i;
                    }
                }
            }
            if (!string.IsNullOrEmpty(videoPlayerContainer.SubtitleText))
            {
                videoPlayerContainer.SetSubtitleText(string.Empty, null, subtitle, format);
            }
            return -1;
        }

        public static bool IsQuartsDllInstalled
        {
            get
            {
                if (Utilities.IsRunningOnMono())
                {
                    return false;
                }

                string quartzFileName = Environment.GetFolderPath(Environment.SpecialFolder.System).TrimEnd('\\') + @"\quartz.dll";
                return File.Exists(quartzFileName);
            }
        }

        public static bool IsMpcInstalled
        {
            get
            {
                if (Utilities.IsRunningOnMono())
                {
                    return false;
                }

                try
                {
                    return MpcHc.GetMpcFileName() != null;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
            }
        }

        public static VideoPlayer GetVideoPlayer()
        {
            var gs = Configuration.Settings.General;

            if (Configuration.IsRunningOnLinux)
            {
                if (gs.VideoPlayer == "VLC" && LibVlcDynamic.IsInstalled)
                {
                    return new LibVlcDynamic();
                }
                if (LibMpvDynamic.IsInstalled)
                {
                    return new LibMpvDynamic();
                }
                throw new NotSupportedException("You need 'libmpv-dev' or 'libvlc-dev' (on Ubuntu) and X11 to use the video player on Linux!");
            }
            // Mono on OS X is 32 bit and thus requires 32 bit VLC. Place VLC in the same
            // folder as Subtitle Edit and add this to the app.config inside the
            // "configuration" element:
            // <dllmap dll="libvlc" target="VLC.app/Contents/MacOS/lib/libvlc.dylib" />
            if (Configuration.IsRunningOnMac)
            {
                return new LibVlcMono();
            }

            if (gs.VideoPlayer == "VLC" && LibVlcDynamic.IsInstalled)
            {
                return new LibVlcDynamic();
            }

            if (gs.VideoPlayer == "MPV" && LibMpvDynamic.IsInstalled)
            {
                return new LibMpvDynamic();
            }

            if (gs.VideoPlayer == "MPC-HC" && MpcHc.IsInstalled)
            {
                return new MpcHc();
            }

            if (IsQuartsDllInstalled)
            {
                return new QuartsPlayer();
            }

            throw new NotSupportedException("You need DirectX, or mpv media player, or VLC media player installed as well as Subtitle Edit dll files in order to use the video player!");
        }

        public static bool InitializeVideoPlayerAndContainer(string fileName, VideoInfo videoInfo, VideoPlayerContainer videoPlayerContainer, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            try
            {
                DoInitializeVideoPlayer(fileName, videoInfo, videoPlayerContainer, onVideoLoaded, onVideoEnded);
                return true;
            }
            catch (Exception exception)
            {
                videoPlayerContainer.VideoPlayer = null;
                var videoError = new VideoError();
                videoError.Initialize(fileName);
                videoError.ShowDialog();
                SeLogger.Error(exception, "InitializeVideoPlayerAndContainer failed to load video player");

                if (videoError.VideoPlayerInstalled)
                {
                    try
                    {
                        DoInitializeVideoPlayer(fileName, videoInfo, videoPlayerContainer, onVideoLoaded, onVideoEnded);
                        return true;
                    }
                    catch
                    {
                        // ignore second error
                    }
                }

                return false;
            }
        }

        private static void DoInitializeVideoPlayer(string fileName, VideoInfo videoInfo, VideoPlayerContainer videoPlayerContainer, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            videoPlayerContainer.VideoPlayer = GetVideoPlayer();
            videoPlayerContainer.VideoPlayer.Initialize(videoPlayerContainer.PanelPlayer, fileName, onVideoLoaded, onVideoEnded);
            videoPlayerContainer.ShowStopButton = Configuration.Settings.General.VideoPlayerShowStopButton;
            videoPlayerContainer.ShowFullscreenButton = false;
            videoPlayerContainer.ShowMuteButton = Configuration.Settings.General.VideoPlayerShowMuteButton;
            videoPlayerContainer.Volume = Configuration.Settings.General.VideoPlayerDefaultVolume;
            videoPlayerContainer.EnableMouseWheelStep();
            if (fileName != null && (fileName.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || fileName.StartsWith("http://", StringComparison.OrdinalIgnoreCase)))
            {
                // we don't have videoInfo for streams...
            }
            else
            {
                videoPlayerContainer.VideoWidth = videoInfo.Width;
                videoPlayerContainer.VideoHeight = videoInfo.Height;
                videoPlayerContainer.VideoPlayer?.Resize(videoPlayerContainer.PanelPlayer.Width, videoPlayerContainer.PanelPlayer.Height);
            }
        }

        public static void CheckAutoWrap(TextBox textBox, KeyEventArgs e, int numberOfNewLines)
        {
            // Do not auto-break lines more than 1 line.
            if (numberOfNewLines != 1 || !Configuration.Settings.General.AutoWrapLineWhileTyping)
            {
                return;
            }

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
                    {
                        newText = textBox.Text.Remove(lastSpace, 1).Insert(lastSpace, Environment.NewLine);
                    }
                    else
                    {
                        newText = textBox.Text;
                    }
                }

                int autoBreakIndex = newText.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                if (autoBreakIndex > 0)
                {
                    int selectionStart = textBox.SelectionStart;
                    textBox.Text = newText;
                    if (selectionStart > autoBreakIndex)
                    {
                        selectionStart += Environment.NewLine.Length - 1;
                    }
                    if (selectionStart >= 0)
                    {
                        textBox.SelectionStart = selectionStart;
                    }
                }
            }
        }

        public static void CheckAutoWrap(SETextBox textBox, KeyEventArgs e, int numberOfNewLines)
        {
            // Do not auto-break lines more than 1 line.
            if (numberOfNewLines != 1 || !Configuration.Settings.General.AutoWrapLineWhileTyping)
            {
                return;
            }

            var length = textBox.Text.CountCharacters(false);
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
                    {
                        newText = textBox.Text.Remove(lastSpace, 1).Insert(lastSpace, Environment.NewLine);
                    }
                    else
                    {
                        newText = textBox.Text;
                    }
                }

                int autoBreakIndex = newText.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                if (autoBreakIndex > 0)
                {
                    int selectionStart = textBox.SelectionStart;
                    textBox.Text = newText;
                    if (selectionStart > autoBreakIndex)
                    {
                        selectionStart += Environment.NewLine.Length - 1;
                    }
                    if (selectionStart >= 0)
                    {
                        textBox.SelectionStart = selectionStart;
                    }
                }
            }
        }

        private static readonly Dictionary<string, Keys> AllKeys = new Dictionary<string, Keys>();
        private static Keys _helpKeys;

        public static Keys GetKeys(string keysInString)
        {
            if (string.IsNullOrEmpty(keysInString))
            {
                return Keys.None;
            }

            if (AllKeys.Count == 0)
            {
                foreach (Keys val in Enum.GetValues(typeof(Keys)))
                {
                    var k = val.ToString().ToLowerInvariant();
                    if (!AllKeys.ContainsKey(k))
                    {
                        AllKeys.Add(k, val);
                    }
                }
                if (!AllKeys.ContainsKey("pagedown"))
                {
                    AllKeys.Add("pagedown", Keys.RButton | Keys.Space);
                }

                if (!AllKeys.ContainsKey("home"))
                {
                    AllKeys.Add("home", Keys.MButton | Keys.Space);
                }

                if (!AllKeys.ContainsKey("capslock"))
                {
                    AllKeys.Add("capslock", Keys.CapsLock);
                }
            }

            var parts = keysInString.ToLowerInvariant().Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            var resultKeys = Keys.None;
            foreach (var k in parts)
            {
                if (AllKeys.ContainsKey(k))
                {
                    resultKeys |= AllKeys[k];
                }
            }

            return resultKeys;
        }

        public static Keys HelpKeys
        {
            get
            {
                if (_helpKeys == Keys.None)
                {
                    _helpKeys = GetKeys(Configuration.Settings.Shortcuts.GeneralHelp);
                }
                return _helpKeys;
            }
            set => _helpKeys = value;
        }

        public static void SetButtonHeight(Control control, int newHeight, int level)
        {
            if (level > 6)
            {
                return;
            }

            if (control.HasChildren)
            {
                foreach (Control subControl in control.Controls)
                {
                    if (subControl.HasChildren)
                    {
                        SetButtonHeight(subControl, newHeight, level + 1);
                    }
                    else if (subControl is Button)
                    {
                        subControl.Height = newHeight;
                    }
                }
            }
            else if (control is Button)
            {
                control.Height = newHeight;
            }
        }

        public static void InitializeSubtitleFont(Control control)
        {
            var gs = Configuration.Settings.General;

            if (string.IsNullOrEmpty(gs.SubtitleFontName))
            {
                gs.SubtitleFontName = DefaultSystemFont.Name;
            }

            try
            {
                if (control is ListView)
                {
                    if (gs.SubtitleListViewFontBold)
                    {
                        control.Font = new Font(gs.SubtitleFontName, gs.SubtitleListViewFontSize, FontStyle.Bold);
                    }
                    else
                    {
                        control.Font = new Font(gs.SubtitleFontName, gs.SubtitleListViewFontSize);
                    }
                }
                else if (control is SETextBox seTextBox)
                {
                    seTextBox.UpdateFontAndColors(seTextBox);
                }
                else if (control is TextBox)
                {
                    if (gs.SubtitleTextBoxFontBold)
                    {
                        control.Font = new Font(gs.SubtitleFontName, gs.SubtitleTextBoxFontSize, FontStyle.Bold);
                    }
                    else
                    {
                        control.Font = new Font(gs.SubtitleFontName, gs.SubtitleTextBoxFontSize);
                    }
                }

                control.BackColor = gs.SubtitleBackgroundColor;
                control.ForeColor = gs.SubtitleFontColor;
            }
            catch
            {
                // ignored
            }
        }

        private static Font _defaultSystemFont;
        private static Font DefaultSystemFont
        {
            get
            {
                if (_defaultSystemFont != null)
                {
                    return _defaultSystemFont;
                }

                var font = SystemFonts.MessageBoxFont;
                if (Configuration.IsRunningOnLinux && IsFontPresent(Configuration.DefaultLinuxFontName))
                {
                    font = new Font(Configuration.DefaultLinuxFontName, 8F);
                }

                _defaultSystemFont = IsFontPresent(font.Name) ? font : SystemFonts.DefaultFont;
                return _defaultSystemFont;
            }
        }

        private static bool IsFontPresent(string fontName)
        {
            var fontStyles = new[] { FontStyle.Bold, FontStyle.Italic, FontStyle.Regular };
            Font font = null;

            foreach (var style in fontStyles)
            {
                try
                {
                    font = new Font(fontName, 9, style);
                }
                catch
                {
                    return false;
                }
                finally
                {
                    font?.Dispose();
                }
            }

            return true;
        }

        public static Font GetDefaultFont()
        {
            var gs = Configuration.Settings.General;
            if (string.IsNullOrEmpty(gs.SystemSubtitleFontNameOverride) || gs.SystemSubtitleFontSizeOverride < 5)
            {
                return DefaultSystemFont;
            }

            try
            {
                return new Font(gs.SystemSubtitleFontNameOverride, gs.SystemSubtitleFontSizeOverride);
            }
            catch
            {
                return DefaultSystemFont;
            }
        }

        internal static void PreInitialize(Form form)
        {
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.Font = GetDefaultFont();
        }

        public static void FixFonts(Control form, int iterations = 5)
        {
            if (form == null)
            {
                return;
            }

            FixFontsInner(form, iterations);
            if (Configuration.Settings.General.UseDarkTheme)
            {
                DarkTheme.SetDarkTheme(form, 1500);
            }
        }

        internal static void FixFonts(ToolStripItem item)
        {
            item.Font = GetDefaultFont();
            if (Configuration.Settings.General.UseDarkTheme)
            {
                DarkTheme.SetDarkTheme(item);
            }
        }

        private static void FixFontsInner(Control form, int iterations = 5)
        {
            if (iterations < 1 || form is SETextBox)
            {
                return;
            }

            if (form is ContextMenuStrip cms)
            {
                foreach (var item in cms.Items)
                {
                    if (item is ToolStripMenuItem tsmi)
                    {
                        tsmi.Font = GetDefaultFont();
                        if (tsmi.HasDropDownItems)
                        {
                            foreach (var innerItem in tsmi.DropDownItems)
                            {
                                if (innerItem is ToolStripMenuItem innerTsmi)
                                {
                                    innerTsmi.Font = GetDefaultFont();
                                }
                            }
                        }
                    }
                }
            }

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
            if (mainCtrl == null || ctrl == null)
            {
                return;
            }

            using (var graphics = mainCtrl.CreateGraphics())
            {
                var textSize = graphics.MeasureString(ctrl.Text, ctrl.Font);
                if (textSize.Height > ctrl.Height - 4)
                {
                    SetButtonHeight(mainCtrl, (int)Math.Round(textSize.Height + 7.5), 1);
                }
            }
        }

        public static void SetSaveDialogFilter(SaveFileDialog saveFileDialog, SubtitleFormat currentFormat)
        {
            var sb = new StringBuilder();
            var index = 0;
            foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
            {
                sb.Append(format.Name + "|*" + format.Extension + "|");
                if (currentFormat.Name == format.Name)
                {
                    saveFileDialog.FilterIndex = index + 1;
                }
                index++;
            }
            saveFileDialog.Filter = sb.ToString().TrimEnd('|');
        }

        public static void GetLineLengths(Label label, string text)
        {
            label.ForeColor = ForeColor;
            var lines = text.SplitToLines();
            const int max = 3;
            var sb = new StringBuilder();
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (i > 0)
                {
                    sb.Append('/');
                }

                if (i > max)
                {
                    sb.Append("...");
                    label.Text = sb.ToString();
                    return;
                }

                var count = line.CountCharacters(false);
                sb.Append(count);
                if (count > Configuration.Settings.General.SubtitleLineMaximumLength || i >= Configuration.Settings.General.MaxNumberOfLines)
                {
                    label.ForeColor = Color.Red;
                }
            }
            label.Text = sb.ToString();
        }

        public static void GetLinePixelWidths(Label label, string text)
        {
            label.ForeColor = ForeColor;
            var lines = text.SplitToLines();
            const int max = 3;
            var sb = new StringBuilder();
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (i > 0)
                {
                    sb.Append('/');
                }

                if (i > max)
                {
                    sb.Append("...");
                    label.Text = sb.ToString();
                    return;
                }

                var lineWidth = TextWidth.CalcPixelWidth(line);
                sb.Append(lineWidth);
                if (lineWidth > Configuration.Settings.General.SubtitleLineMaximumPixelWidth)
                {
                    label.ForeColor = Color.Red;
                }
            }

            label.Text = sb.ToString();
        }

        public static void InitializeSubtitleFormatComboBox(NikseComboBox comboBox, SubtitleFormat format)
        {
            InitializeSubtitleFormatComboBox(comboBox, new List<string> { format.FriendlyName }, format.FriendlyName);
        }

        public static void InitializeSubtitleFormatComboBox(NikseComboBox comboBox, string selectedName)
        {
            var formatNames = SubtitleFormat.AllSubtitleFormats.Where(format => !format.IsVobSubIndexFile).Select(format => format.FriendlyName);
            InitializeSubtitleFormatComboBox(comboBox, formatNames.ToList(), selectedName);
        }

        public static void InitializeSubtitleFormatComboBox(NikseComboBox comboBox, List<string> formatNames, string selectedName)
        {
            var selectedIndex = 0;
            using (var graphics = comboBox.CreateGraphics())
            {
                var maxWidth = (float)comboBox.DropDownWidth;
                var max = formatNames.Count;
                for (var index = 0; index < max; index++)
                {
                    var name = formatNames[index];
                    if (name.Equals(selectedName, StringComparison.OrdinalIgnoreCase))
                    {
                        selectedIndex = index;
                    }
                    if (name.Length > 30)
                    {
                        var width = graphics.MeasureString(name, comboBox.Font).Width;
                        if (width > maxWidth)
                        {
                            maxWidth = width;
                        }
                    }
                }

                comboBox.DropDownWidth = (int)Math.Round(maxWidth + 17.5);
            }

            comboBox.BeginUpdate();
            comboBox.Items.Clear();
            comboBox.Items.AddRange(formatNames.ToArray<object>());
            comboBox.SelectedIndex = selectedIndex;
            comboBox.EndUpdate();
        }

        public static void InitializeTextEncodingComboBox(NikseComboBox comboBox)
        {
            var defaultEncoding = Configuration.Settings.General.DefaultEncoding;
            var selectedItem = (TextEncoding)null;
            comboBox.BeginUpdate();
            comboBox.Items.Clear();
            var encList = new List<TextEncoding>();
            using (var graphics = comboBox.CreateGraphics())
            {
                var maxWidth = 0.0F;
                foreach (var encoding in Configuration.AvailableEncodings)
                {
                    if (encoding.CodePage >= 874 && !encoding.IsEbcdic())
                    {
                        var item = new TextEncoding(encoding, null);
                        if (selectedItem == null && item.Equals(defaultEncoding))
                        {
                            selectedItem = item;
                        }
                        var width = graphics.MeasureString(item.DisplayName, comboBox.Font).Width;
                        if (width > maxWidth)
                        {
                            maxWidth = width;
                        }
                        if (encoding.CodePage.Equals(Encoding.UTF8.CodePage))
                        {
                            item = new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithBom);
                            encList.Insert(TextEncoding.Utf8WithBomIndex, item);
                            if (item.Equals(defaultEncoding))
                            {
                                selectedItem = item;
                            }

                            item = new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithoutBom);
                            encList.Insert(TextEncoding.Utf8WithoutBomIndex, item);
                            if (item.Equals(defaultEncoding))
                            {
                                selectedItem = item;
                            }
                        }
                        else
                        {
                            encList.Add(item);
                        }
                    }
                }
                comboBox.DropDownWidth = (int)Math.Round(maxWidth + 7.5);
            }
            comboBox.Items.AddRange(encList.ToArray<object>());
            if (selectedItem == null)
            {
                comboBox.SelectedIndex = TextEncoding.Utf8WithBomIndex; // UTF-8 if DefaultEncoding is not found
            }
            else if (selectedItem.DisplayName == TextEncoding.Utf8WithoutBom)
            {
                comboBox.SelectedIndex = TextEncoding.Utf8WithoutBomIndex;
            }
            else
            {
                comboBox.SelectedItem = selectedItem;
            }
            comboBox.EndUpdate();
            if (comboBox.SelectedItem is TextEncoding textEncodingListItem)
            {
                Configuration.Settings.General.DefaultEncoding = textEncodingListItem.DisplayName;
            }
        }

        public static TextEncoding GetTextEncodingComboBoxCurrentEncoding(NikseComboBox comboBox)
        {
            if (comboBox.SelectedIndex > 0 && comboBox.SelectedItem is TextEncoding textEncodingListItem)
            {
                return textEncodingListItem;
            }

            return new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithBom);
        }

        public static void SetTextEncoding(NikseComboBox comboBoxEncoding, string encodingName)
        {
            if (encodingName == TextEncoding.Utf8WithBom)
            {
                comboBoxEncoding.SelectedIndex = TextEncoding.Utf8WithBomIndex;
                return;
            }

            if (encodingName == TextEncoding.Utf8WithoutBom)
            {
                comboBoxEncoding.SelectedIndex = TextEncoding.Utf8WithoutBomIndex;
                return;
            }

            foreach (TextEncoding item in comboBoxEncoding.Items)
            {
                if (item.Equals(encodingName))
                {
                    comboBoxEncoding.SelectedItem = item;
                    return;
                }
            }

            comboBoxEncoding.SelectedIndex = TextEncoding.Utf8WithBomIndex; // UTF-8 with BOM
        }

        public static void BeginRichTextBoxUpdate(this RichTextBox richTextBox)
        {
            NativeMethods.SendMessage(richTextBox.Handle, NativeMethods.WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
        }

        public static void EndRichTextBoxUpdate(this RichTextBox richTextBox)
        {
            NativeMethods.SendMessage(richTextBox.Handle, NativeMethods.WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            richTextBox.Invalidate();
        }

        private const string BreakChars = "\",:;.¡!¿?()[]{}<>♪♫-–—/#*|؟،";

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
                        {
                            break;
                        }
                    }
                    if (index > 0) // HTML tag?
                    {
                        if (text[textIndex] == '>')
                        {
                            var openingBracketIndex = text.LastIndexOf('<', textIndex - 1);
                            if (openingBracketIndex >= 0 && text.IndexOf('>', openingBracketIndex + 1) == textIndex)
                            {
                                deleteFrom = openingBracketIndex; // delete whole tag
                            }
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
                        {
                            deleteFrom = -2;
                        }

                        while (index > 0)
                        {
                            index--;
                            textIndex = textElementIndices[index];
                            if (IsSpaceCategory(CharUnicodeInfo.GetUnicodeCategory(text, textIndex)))
                            {
                                if (deleteFrom > -2)
                                {
                                    if (deleteFrom < 0)
                                    {
                                        deleteFrom = textElementIndices[index + 1];
                                    }
                                    break;
                                }
                                deleteFrom = textElementIndices[index + 1];
                                if (!":!?".Contains(text[deleteFrom]))
                                {
                                    break;
                                }
                            }
                            else if (BreakChars.Contains(text[textIndex]))
                            {
                                if (deleteFrom > -2)
                                {
                                    if (deleteFrom < 0)
                                    {
                                        deleteFrom = textElementIndices[index + 1];
                                    }
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
                        {
                            deleteFrom = 0;
                        }
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

        public static void AddExtension(StringBuilder sb, string extension)
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
            sb.Append(LanguageSettings.Current.General.SubtitleFiles + "|");
            foreach (var s in SubtitleFormat.AllSubtitleFormats.Concat(SubtitleFormat.GetTextOtherFormats()))
            {
                AddExtension(sb, s.Extension);
                foreach (var ext in s.AlternateExtensions)
                {
                    AddExtension(sb, ext);
                }
            }
            AddExtension(sb, new Pac().Extension);
            AddExtension(sb, new Cavena890().Extension);
            AddExtension(sb, new Spt().Extension);
            AddExtension(sb, new Sptx().Extension);
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
            AddExtension(sb, new FinalDraftTemplate2().Extension);
            AddExtension(sb, new Ayato().Extension);
            AddExtension(sb, new PacUnicode().Extension);
            AddExtension(sb, new WinCaps32().Extension);
            AddExtension(sb, new IsmtDfxp().Extension);
            AddExtension(sb, new PlayCaptionsFreeEditor().Extension);
            AddExtension(sb, ".cdg"); // karaoke
            AddExtension(sb, ".pns"); // karaoke

            if (!string.IsNullOrEmpty(Configuration.Settings.General.OpenSubtitleExtraExtensions))
            {
                var extraExtensions = Configuration.Settings.General.OpenSubtitleExtraExtensions.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string ext in extraExtensions)
                {
                    if (ext.StartsWith("*.", StringComparison.Ordinal) && !sb.ToString().Contains(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        AddExtension(sb, ext);
                    }
                }
            }
            AddExtension(sb, ".son");
            AddExtension(sb, ".mts");
            AddExtension(sb, ".m2ts");
            AddExtension(sb, ".m4s");
            AddExtension(sb, ".se-job");

            sb.Append('|');
            sb.Append(LanguageSettings.Current.General.AllFiles);
            sb.Append("|*.*");
            return sb.ToString();
        }

        public static string GetListViewTextFromString(string s) =>
            s.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString);

        public static string GetStringFromListViewText(string lviText) =>
            lviText.Replace(Configuration.Settings.General.ListViewLineSeparatorString, Environment.NewLine);

        public static void AutoSizeLastColumn(this ListView listView) =>
            listView.Columns[listView.Columns.Count - 1].Width = -2;

        public static void CheckAll(this ListView lv)
        {
            lv.BeginUpdate();
            foreach (ListViewItem item in lv.Items)
            {
                item.Checked = true;
            }
            lv.EndUpdate();
        }
        
        public static void InvertCheck(this ListView lv)
        {
            lv.BeginUpdate();
            foreach (ListViewItem item in lv.Items)
            {
                item.Checked = !item.Checked;
            }
            lv.EndUpdate();
        }
        
        public static void UncheckAll(this ListView lv)
        {
            lv.BeginUpdate();
            foreach (ListViewItem item in lv.Items)
            {
                item.Checked = false;
            }
            lv.EndUpdate();
        }
        
        public static void SelectAll(this ListView lv)
        {
            lv.BeginUpdate();
            foreach (ListViewItem item in lv.Items)
            {
                item.Selected = true;
            }
            lv.EndUpdate();
        }

        public static void SelectFirstSelectedItemOnly(this ListView lv)
        {
            if (lv.SelectedIndices.Count > 1)
            {
                var first = lv.SelectedIndices[0];
                lv.BeginUpdate();
                lv.SelectedIndices.Clear();
                lv.EnsureVisible(first);
                lv.FocusedItem = lv.Items[first];
                lv.Items[first].Selected = true;
                lv.EndUpdate();
            }
        }

        public static void InverseSelection(this ListView lv)
        {
            lv.BeginUpdate();
            foreach (ListViewItem item in lv.Items)
            {
                item.Selected = !item.Selected;
            }
            lv.EndUpdate();
        }

        public static void SelectAll(this ListBox listbox)
        {
            listbox.BeginUpdate();
            for (int i = 0; i < listbox.Items.Count; i++)
            {
                listbox.SetSelected(i, true);
            }
            listbox.EndUpdate();
        }

        public static void InverseSelection(this ListBox listbox)
        {
            listbox.BeginUpdate();
            for (int i = 0; i < listbox.Items.Count; i++)
            {
                listbox.SetSelected(i, !listbox.GetSelected(i));
            }
            listbox.EndUpdate();
        }

        internal static void CleanUpMenuItemPlugin(ToolStripMenuItem tsmi)
        {
            if (tsmi == null)
            {
                return;
            }
            for (int k = tsmi.DropDownItems.Count - 1; k > 0; k--)
            {
                ToolStripItem x = tsmi.DropDownItems[k];
                var fileName = (string)x.Tag;
                if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    tsmi.DropDownItems.Remove(x);
                }
            }
        }

        public static Color BackColor => Configuration.Settings.General.UseDarkTheme ? Configuration.Settings.General.DarkThemeBackColor : Control.DefaultBackColor;

        public static Color ForeColor => Configuration.Settings.General.UseDarkTheme ? Configuration.Settings.General.DarkThemeForeColor : Control.DefaultForeColor;

        public static Color WarningColor => Configuration.Settings.General.UseDarkTheme ? Color.Yellow : Color.DarkGoldenrod;

        public static void OpenFolderFromFileName(string fileName)
        {
            string folderName = Path.GetDirectoryName(fileName);
            if (Configuration.IsRunningOnWindows)
            {
                var argument = @"/select, " + fileName;
                Process.Start("explorer.exe", argument);
            }
            else
            {
                OpenFolder(folderName);
            }
        }

        public static void OpenFolder(string folder)
        {
            OpenItem(folder, "folder");
        }

        public static void OpenUrl(string url)
        {
            OpenItem(url, "url");
        }

        public static void OpenFile(string file)
        {
            OpenItem(file, "file");
        }

        public static void OpenItem(string item, string type)
        {
            try
            {
                if (Configuration.IsRunningOnWindows || Configuration.IsRunningOnMac)
                {
                    var startInfo = new ProcessStartInfo(item)
                    {
                        UseShellExecute = true
                    };

                    Process.Start(startInfo);
                }
                else if (Configuration.IsRunningOnLinux)
                {
                    var process = new Process
                    {
                        EnableRaisingEvents = false,
                        StartInfo = { FileName = "xdg-open", Arguments = item }
                    };
                    process.Start();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Cannot open {type}: {item}{Environment.NewLine}{Environment.NewLine}{exception.Source}: {exception.Message}", "Error opening URL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static string GetVideoFileFilter(bool includeAudioFiles)
        {
            var sb = new StringBuilder();
            sb.Append(LanguageSettings.Current.General.VideoFiles + "|*");
            sb.Append(string.Join(";*", Utilities.VideoFileExtensions));

            if (includeAudioFiles)
            {
                sb.Append("|" + LanguageSettings.Current.General.AudioFiles + "|*");
                sb.Append(string.Join(";*", Utilities.AudioFileExtensions));
            }

            sb.Append("|" + LanguageSettings.Current.General.AllFiles + "|*.*");
            return sb.ToString();
        }

        public static void ShowHelp(string parameter)
        {
            var helpFile = LanguageSettings.Current.General.HelpFile;
            if (string.IsNullOrEmpty(helpFile))
            {
                helpFile = "https://www.nikse.dk/SubtitleEdit/Help";
            }

            try
            {
                if (Configuration.IsRunningOnWindows || Configuration.IsRunningOnMac)
                {
                    Process.Start(helpFile + parameter);
                }
                else if (Configuration.IsRunningOnLinux)
                {
                    var process = new Process
                    {
                        EnableRaisingEvents = false,
                        StartInfo = { FileName = "xdg-open", Arguments = helpFile + parameter }
                    };
                    process.Start();
                }
            }
            catch
            {
                //Don't do anything
            }
        }

        public static string GetContinuationStyleName(ContinuationStyle continuationStyle)
        {
            switch (continuationStyle)
            {
                case ContinuationStyle.NoneTrailingDots:
                    return LanguageSettings.Current.Settings.ContinuationStyleNoneTrailingDots;
                case ContinuationStyle.NoneLeadingTrailingDots:
                    return LanguageSettings.Current.Settings.ContinuationStyleNoneLeadingTrailingDots;
                case ContinuationStyle.NoneTrailingEllipsis:
                    return LanguageSettings.Current.Settings.ContinuationStyleNoneTrailingEllipsis;
                case ContinuationStyle.NoneLeadingTrailingEllipsis:
                    return LanguageSettings.Current.Settings.ContinuationStyleNoneLeadingTrailingEllipsis;
                case ContinuationStyle.OnlyTrailingDots:
                    return LanguageSettings.Current.Settings.ContinuationStyleOnlyTrailingDots;
                case ContinuationStyle.LeadingTrailingDots:
                    return LanguageSettings.Current.Settings.ContinuationStyleLeadingTrailingDots;
                case ContinuationStyle.OnlyTrailingEllipsis:
                    return LanguageSettings.Current.Settings.ContinuationStyleOnlyTrailingEllipsis;
                case ContinuationStyle.LeadingTrailingEllipsis:
                    return LanguageSettings.Current.Settings.ContinuationStyleLeadingTrailingEllipsis;
                case ContinuationStyle.LeadingTrailingDash:
                    return LanguageSettings.Current.Settings.ContinuationStyleLeadingTrailingDash;
                case ContinuationStyle.LeadingTrailingDashDots:
                    return LanguageSettings.Current.Settings.ContinuationStyleLeadingTrailingDashDots;
                case ContinuationStyle.Custom:
                    return LanguageSettings.Current.Settings.ContinuationStyleCustom;
                default:
                    return LanguageSettings.Current.Settings.ContinuationStyleNone;
            }
        }

        public static string GetBeautifyTimeCodesProfilePresetName(BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.Preset preset)
        {
            switch (preset)
            {
                case BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.Preset.Default:
                    return LanguageSettings.Current.BeautifyTimeCodesProfile.PresetDefault;
                case BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.Preset.Netflix:
                    return LanguageSettings.Current.BeautifyTimeCodesProfile.PresetNetflix;
                case BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.Preset.SDI:
                    return LanguageSettings.Current.BeautifyTimeCodesProfile.PresetSDI;
                default:
                    return preset.ToString();
            }
        }

        public static string DecimalSeparator => CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        public static Color GreenBackgroundColor => Configuration.Settings.General.UseDarkTheme ? DarkTheme.GreenBackColor : ColorTranslator.FromHtml("#6ebe6e");
        public static Color GreenBackgroundColorAlternate => Configuration.Settings.General.UseDarkTheme ? DarkTheme.GreenBackColorAlternate : ColorTranslator.FromHtml("#6ecf5e");
        public static Control FindFocusedControl(Control control)
        {
            var container = control as ContainerControl;
            while (container != null)
            {
                control = container.ActiveControl;
                container = control as ContainerControl;
            }

            return control;
        }

        public static void SetNumericUpDownValue(NikseUpDown numericUpDown, int value)
        {
            if (value < numericUpDown.Minimum)
            {
                numericUpDown.Value = numericUpDown.Minimum;
            }
            else if (value > numericUpDown.Maximum)
            {
                numericUpDown.Value = numericUpDown.Maximum;
            }
            else
            {
                numericUpDown.Value = value;
            }
        }
    }
}
