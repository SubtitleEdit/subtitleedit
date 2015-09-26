﻿using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Controls
{
    public partial class AudioVisualizer : UserControl
    {
        public enum MouseDownParagraphType
        {
            None,
            Start,
            Whole,
            End
        }

        public class ParagraphEventArgs : EventArgs
        {
            public Paragraph Paragraph { get; private set; }
            public double Seconds { get; private set; }
            public Paragraph BeforeParagraph { get; set; }
            public MouseDownParagraphType MouseDownParagraphType { get; set; }
            public bool MovePreviousOrNext { get; set; }
            public ParagraphEventArgs(Paragraph p)
            {
                Paragraph = p;
            }
            public ParagraphEventArgs(double seconds, Paragraph p)
            {
                Seconds = seconds;
                Paragraph = p;
            }
            public ParagraphEventArgs(double seconds, Paragraph p, Paragraph b)
            {
                Seconds = seconds;
                Paragraph = p;
                BeforeParagraph = b;
            }
            public ParagraphEventArgs(double seconds, Paragraph p, Paragraph b, MouseDownParagraphType mouseDownParagraphType)
            {
                Seconds = seconds;
                Paragraph = p;
                BeforeParagraph = b;
                MouseDownParagraphType = mouseDownParagraphType;
            }
            public ParagraphEventArgs(double seconds, Paragraph p, Paragraph b, MouseDownParagraphType mouseDownParagraphType, bool movePreviousOrNext)
            {
                Seconds = seconds;
                Paragraph = p;
                BeforeParagraph = b;
                MouseDownParagraphType = mouseDownParagraphType;
                MovePreviousOrNext = movePreviousOrNext;
            }
        }

        public int ClosenessForBorderSelection = 15;
        private const int MinimumSelectionMilliseconds = 100;

        private long _buttonDownTimeTicks;
        private int _mouseMoveLastX = -1;
        private int _mouseMoveStartX = -1;
        private double _moveWholeStartDifferenceMilliseconds = -1;
        private int _mouseMoveEndX = -1;
        private bool _mouseDown;
        private Paragraph _oldParagraph;
        private Paragraph _mouseDownParagraph;
        private MouseDownParagraphType _mouseDownParagraphType = MouseDownParagraphType.Start;
        private Paragraph _selectedParagraph;
        private Paragraph _currentParagraph;
        private List<Paragraph> _previousAndNextParagraphs = new List<Paragraph>();
        private Paragraph _prevParagraph;
        private Paragraph _nextParagraph;
        private bool _firstMove = true;
        private double _currentVideoPositionSeconds = -1;
        private WavePeakGenerator _wavePeaks;
        private Subtitle _subtitle;
        private string _subtitleHash;
        private IEnumerable<int> _selectedIndices = Enumerable.Empty<int>();
        private bool _noClear;
        private double _gapAtStart = -1;

        private List<Bitmap> _spectrogramBitmaps = new List<Bitmap>();
        private string _spectrogramDirectory;
        private const int SpectrogramDisplayHeight = 128;
        private double _sampleDuration;
        private double _imageWidth;
        private int _nfft;

        public delegate void ParagraphEventHandler(object sender, ParagraphEventArgs e);
        public event ParagraphEventHandler OnNewSelectionRightClicked;
        public event ParagraphEventHandler OnParagraphRightClicked;
        public event ParagraphEventHandler OnNonParagraphRightClicked;
        public event ParagraphEventHandler OnPositionSelected;

        public event ParagraphEventHandler OnTimeChanged;
        public event ParagraphEventHandler OnTimeChangedAndOffsetRest;

        public event ParagraphEventHandler OnSingleClick;
        public event ParagraphEventHandler OnDoubleClickNonParagraph;
        public event EventHandler OnPause;
        public event EventHandler OnZoomedChanged;
        public event EventHandler InsertAtVideoPosition;

        private double _wholeParagraphMinMilliseconds;
        private double _wholeParagraphMaxMilliseconds = double.MaxValue;
        private System.ComponentModel.BackgroundWorker _spectrogramBackgroundWorker;
        public Keys InsertAtVideoPositionShortcut = Keys.None;
        public bool MouseWheelScrollUpIsForward = true;

        public const double ZoomMinimum = 0.1;
        public const double ZoomMaximum = 2.5;
        private double _zoomFactor = 1.0; // 1.0=no zoom
        public double ZoomFactor
        {
            get
            {
                return _zoomFactor;
            }
            set
            {
                if (value < ZoomMinimum)
                    value = ZoomMinimum;
                if (value > ZoomMaximum)
                    value = ZoomMaximum;
                _zoomFactor = Math.Round(value, 2); // round to prevent accumulated rounding errors
                Invalidate();
            }
        }

        public const double VerticalZoomMinimum = 1.0;
        public const double VerticalZoomMaximum = 40.0;
        private double _verticalZoomFactor = 2.0; // 1.0=no zoom
        public double VerticalZoomFactor
        {
            get
            {
                return _verticalZoomFactor;
            }
            set
            {
                if (value < VerticalZoomMinimum)
                    value = VerticalZoomMinimum;
                if (value > VerticalZoomMaximum)
                    value = VerticalZoomMaximum;
                _verticalZoomFactor = Math.Round(value, 2); // round to prevent accumulated rounding errors
                Invalidate();
            }
        }

        private List<double> _sceneChanges = new List<double>();

        /// <summary>
        /// Scene changes (seconds)
        /// </summary>
        public List<double> SceneChanges
        {
            get
            {
                return _sceneChanges;
            }
            set
            {
                _sceneChanges = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Video offset in seconds
        /// </summary>
        public double Offset { get; set; }

        public bool IsSpectrogramAvailable
        {
            get
            {
                return _spectrogramBitmaps != null && _spectrogramBitmaps.Count > 0;
            }
        }

        private bool _showSpectrogram;
        public bool ShowSpectrogram
        {
            get
            {
                return _showSpectrogram;
            }
            set
            {
                _showSpectrogram = value;
                Invalidate();
            }
        }

        public bool AllowOverlap { get; set; }
        private bool _tempShowSpectrogram;

        private bool _showWaveform;
        public bool ShowWaveform
        {
            get
            {
                return _showWaveform;
            }
            set
            {
                _showWaveform = value;
                Invalidate();
            }
        }

        private double _startPositionSeconds;
        public double StartPositionSeconds
        {
            get
            {
                return _startPositionSeconds;
            }
            set
            {
                if (value < 0)
                    _startPositionSeconds = 0;
                else
                    _startPositionSeconds = value;
            }
        }

        public Paragraph NewSelectionParagraph { get; set; }
        public Paragraph SelectedParagraph { get { return _selectedParagraph; } }
        public Paragraph RightClickedParagraph { get; private set; }
        public double RightClickedSeconds { get; private set; }

        public string WaveformNotLoadedText { get; set; }
        public Color BackgroundColor { get; set; }
        public Color Color { get; set; }
        public Color SelectedColor { get; set; }
        public Color ParagraphColor { get; set; }
        public Color TextColor { get; set; }
        public float TextSize { get; set; }
        public bool TextBold { get; set; }
        public Color GridColor { get; set; }
        public bool DrawGridLines { get; set; }
        public bool AllowNewSelection { get; set; }

        public bool Locked { get; set; }

        public double EndPositionSeconds
        {
            get
            {
                return XPositionToSeconds(Width);
            }
        }

        public WavePeakGenerator WavePeaks
        {
            get
            {
                return _wavePeaks;
            }
            set
            {
                _zoomFactor = 1.0;
                _currentParagraph = null;
                _selectedParagraph = null;
                _buttonDownTimeTicks = 0;
                _mouseMoveLastX = -1;
                _mouseMoveStartX = -1;
                _moveWholeStartDifferenceMilliseconds = -1;
                _mouseMoveEndX = -1;
                _mouseDown = false;
                _mouseDownParagraph = null;
                _mouseDownParagraphType = MouseDownParagraphType.Start;
                _previousAndNextParagraphs = new List<Paragraph>();
                _currentVideoPositionSeconds = -1;
                _subtitle = new Subtitle();
                _noClear = false;
                _wavePeaks = value;
            }
        }

        public void ResetSpectrogram()
        {
            if (_spectrogramBitmaps != null)
            {
                for (int i = 0; i < _spectrogramBitmaps.Count; i++)
                {
                    try
                    {
                        Bitmap bmp = _spectrogramBitmaps[i];
                        bmp.Dispose();
                    }
                    catch
                    {
                    }
                }
            }
            _spectrogramBitmaps = new List<Bitmap>();
        }

        public void ClearSelection()
        {
            _mouseDown = false;
            _mouseDownParagraph = null;
            _mouseMoveStartX = -1;
            _mouseMoveEndX = -1;
            Invalidate();
        }

        public AudioVisualizer()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
            WaveformNotLoadedText = "Click to add waveform/spectrogram";
            MouseWheel += WaveformMouseWheel;

            BackgroundColor = Color.Black;
            Color = Color.GreenYellow;
            SelectedColor = Color.Red;
            ParagraphColor = Color.LimeGreen;
            TextColor = Color.Gray;
            TextSize = 9;
            TextBold = true;
            GridColor = Color.FromArgb(255, 20, 20, 18);
            DrawGridLines = true;
            AllowNewSelection = true;
            ShowSpectrogram = true;
            ShowWaveform = true;
            InsertAtVideoPositionShortcut = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainWaveformInsertAtCurrentPosition);
        }

        private void NearestSubtitles(Subtitle subtitle, double currentVideoPositionSeconds, int subtitleIndex)
        {
            _currentParagraph = null;
            _previousAndNextParagraphs.Clear();

            if (subtitle.Paragraphs.Count == 0)
                return;

            // Locate the current paragraph, which is the paragraph ending soonest after the
            // current video position, or if none such exist, the last paragraph. There's a
            // good chance this will end up being the selected paragraph, so we'll check it
            // first as an optimization.
            bool isPositionInsideSelection = _selectedParagraph != null &&
                _selectedParagraph.StartTime.TotalSeconds <= currentVideoPositionSeconds &&
                _selectedParagraph.EndTime.TotalSeconds > currentVideoPositionSeconds;
            int currentParagraphIndex = isPositionInsideSelection ? subtitleIndex :
                subtitle.Paragraphs.FindIndex(p => p.EndTime.TotalSeconds > currentVideoPositionSeconds);
            if (currentParagraphIndex == -1)
                currentParagraphIndex = subtitle.Paragraphs.Count - 1;

            _currentParagraph = subtitle.Paragraphs[currentParagraphIndex];

            // Locate the other paragraphs inside the visible time range, as well as one extra
            // on either side (helps when scrolling).
            for (int i = currentParagraphIndex + 1; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                _previousAndNextParagraphs.Add(p);
                if (p.StartTime.TotalSeconds >= EndPositionSeconds)
                    break;
            }
            for (int i = currentParagraphIndex - 1; i >= 0; i--)
            {
                Paragraph p = subtitle.Paragraphs[i];
                _previousAndNextParagraphs.Add(p);
                if (p.EndTime.TotalSeconds <= StartPositionSeconds)
                    break;
            }
        }

        public void SetPosition(double startPositionSeconds, Subtitle subtitle, double currentVideoPositionSeconds, int subtitleIndex, ListView.SelectedIndexCollection selectedIndexes)
        {
            string subtitleHash = subtitle.GetFastHashCode();
            int[] selectedIndexArray = selectedIndexes.Cast<int>().ToArray();
            bool changed =
                currentVideoPositionSeconds != _currentVideoPositionSeconds ||
                !selectedIndexArray.SequenceEqual(_selectedIndices) ||
                subtitleHash != _subtitleHash;
            StartPositionSeconds = startPositionSeconds;
            _selectedIndices = selectedIndexArray;
            _subtitle.Paragraphs.Clear();
            foreach (var p in subtitle.Paragraphs)
            {
                if (!p.StartTime.IsMaxTime)
                    _subtitle.Paragraphs.Add(p);
            }
            _subtitleHash = subtitleHash;
            _currentVideoPositionSeconds = currentVideoPositionSeconds;
            _selectedParagraph = _subtitle.GetParagraphOrDefault(subtitleIndex);
            NearestSubtitles(subtitle, currentVideoPositionSeconds, subtitleIndex);
            if (changed)
                Invalidate();
        }

        private static int CalculateHeight(double value, int imageHeight, int maxHeight)
        {
            double percentage = value / maxHeight;
            var result = (int)Math.Round((percentage / 2.0 + 0.5) * imageHeight);
            return imageHeight - result;
        }

        private class IsSelectedHelper
        {
            private readonly List<SelectionRange> _ranges = new List<SelectionRange>();
            private int _lastPosition = int.MaxValue;
            private SelectionRange _nextSelection;

            public IsSelectedHelper(IList<Paragraph> paragraphs, IEnumerable<int> selectedIndices, Paragraph additionalSelectedParagraph, Func<double, int> secondsToPosition)
            {
                // I'm not sure why there's a try/catch, I copied it from the original code. It
                // seems unnecessary but perhaps there's a thread safety issue.
                try
                {
                    foreach (int index in selectedIndices)
                        AddParagraph(paragraphs[index], secondsToPosition);
                }
                catch
                {
                }

                AddParagraph(additionalSelectedParagraph, secondsToPosition);
            }

            public bool IsSelected(int position)
            {
                if (position < _lastPosition || position > _nextSelection.End)
                    FindNextSelection(position);

                _lastPosition = position;

                return position >= _nextSelection.Start && position <= _nextSelection.End;
            }

            private void FindNextSelection(int position)
            {
                _nextSelection = new SelectionRange(int.MaxValue, int.MaxValue);
                foreach (SelectionRange range in _ranges)
                {
                    if (range.End >= position && (range.Start < _nextSelection.Start || (range.Start == _nextSelection.Start && range.End > _nextSelection.End)))
                        _nextSelection = range;
                }
            }

            private void AddParagraph(Paragraph p, Func<double, int> secondsToPosition)
            {
                if (p == null)
                    return;

                _ranges.Add(new SelectionRange(secondsToPosition(p.StartTime.TotalSeconds), secondsToPosition(p.EndTime.TotalSeconds)));
            }

            private struct SelectionRange
            {
                public readonly int Start;
                public readonly int End;

                public SelectionRange(int start, int end)
                {
                    Start = start;
                    End = end;
                }
            }
        }

        internal void WaveformPaint(object sender, PaintEventArgs e)
        {
            if (_wavePeaks != null && _wavePeaks.AllSamples != null)
            {
                if (StartPositionSeconds < 0)
                    StartPositionSeconds = 0;

                if (XPositionToSeconds(Width) > _wavePeaks.Header.LengthInSeconds)
                    StartPositionSeconds = _wavePeaks.Header.LengthInSeconds - ((Width / (double)_wavePeaks.Header.SampleRate) / _zoomFactor);

                Graphics graphics = e.Graphics;
                int imageHeight = Height;

                DrawBackground(graphics);

                if (IsSpectrogramAvailable && ShowSpectrogram)
                {
                    DrawSpectrogramBitmap(StartPositionSeconds, graphics);
                    if (ShowWaveform)
                        imageHeight -= SpectrogramDisplayHeight;
                }

                // waveform
                if (ShowWaveform)
                {
                    using (var penNormal = new Pen(Color))
                    using (var penSelected = new Pen(SelectedColor)) // selected paragraph
                    {
                        var pen = penNormal;
                        var isSelectedHelper = new IsSelectedHelper(_subtitle.Paragraphs, _selectedIndices, _selectedParagraph, SecondsToXPositionNoZoom);
                        int maxHeight = (int)(Math.Max(Math.Abs(_wavePeaks.DataMinValue), Math.Abs(_wavePeaks.DataMaxValue)) / VerticalZoomFactor);
                        int start = SecondsToXPositionNoZoom(StartPositionSeconds);
                        float xPrev = 0;
                        int yPrev = Height / 2;
                        float x = 0;
                        int y;
                        for (int i = 0; i < _wavePeaks.AllSamples.Count - start && x < Width; i++)
                        {
                            int n = start + i;
                            x = (float)(_zoomFactor * i);
                            y = CalculateHeight(_wavePeaks.AllSamples[n], imageHeight, maxHeight);
                            graphics.DrawLine(pen, xPrev, yPrev, x, y);
                            xPrev = x;
                            yPrev = y;
                            pen = isSelectedHelper.IsSelected(n) ? penSelected : penNormal;
                        }
                    }
                }

                // time line
                DrawTimeLine(StartPositionSeconds, e, imageHeight);

                // scene changes
                if (_sceneChanges != null)
                {
                    foreach (double time in _sceneChanges)
                    {
                        int pos = SecondsToXPosition(time - StartPositionSeconds);
                        if (pos > 0 && pos < Width)
                        {
                            using (var p = new Pen(Color.AntiqueWhite))
                            {
                                graphics.DrawLine(p, pos, 0, pos, Height);
                            }
                        }
                    }
                }

                // current video position
                if (_currentVideoPositionSeconds > 0)
                {
                    int pos = SecondsToXPosition(_currentVideoPositionSeconds - StartPositionSeconds);
                    if (pos > 0 && pos < Width)
                    {
                        using (var p = new Pen(Color.Turquoise))
                        {
                            graphics.DrawLine(p, pos, 0, pos, Height);
                        }
                    }
                }

                // paragraphs
                using (var textBrush = new SolidBrush(TextColor))
                {
                    DrawParagraph(_currentParagraph, e, textBrush);
                    foreach (Paragraph p in _previousAndNextParagraphs)
                        DrawParagraph(p, e, textBrush);
                }

                // current selection
                if (NewSelectionParagraph != null)
                {
                    int currentRegionLeft = SecondsToXPosition(NewSelectionParagraph.StartTime.TotalSeconds - StartPositionSeconds);
                    int currentRegionRight = SecondsToXPosition(NewSelectionParagraph.EndTime.TotalSeconds - StartPositionSeconds);
                    int currentRegionWidth = currentRegionRight - currentRegionLeft;
                    using (var brush = new SolidBrush(Color.FromArgb(128, 255, 255, 255)))
                    {
                        if (currentRegionLeft >= 0 && currentRegionLeft <= Width)
                        {
                            graphics.FillRectangle(brush, currentRegionLeft, 0, currentRegionWidth, graphics.VisibleClipBounds.Height);

                            if (currentRegionWidth > 40)
                            {
                                using (var tBrush = new SolidBrush(Color.Turquoise))
                                {
                                    graphics.DrawString(string.Format("{0:0.###} {1}", ((double)currentRegionWidth / _wavePeaks.Header.SampleRate / _zoomFactor), Configuration.Settings.Language.Waveform.Seconds), Font, tBrush, new PointF(currentRegionLeft + 3, Height - 32));
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                DrawBackground(e.Graphics);

                using (var textBrush = new SolidBrush(TextColor))
                {
                    using (var textFont = new Font(Font.FontFamily, 8))
                    {
                        if (Width > 90)
                        {
                            e.Graphics.DrawString(WaveformNotLoadedText, textFont, textBrush, new PointF(Width / 2 - 65, Height / 2 - 10));
                        }
                        else
                        {
                            using (var stringFormat = new StringFormat())
                            {
                                stringFormat.FormatFlags = StringFormatFlags.DirectionVertical;
                                e.Graphics.DrawString(WaveformNotLoadedText, textFont, textBrush, new PointF(1, 10), stringFormat);
                            }
                        }
                    }
                }
            }
            if (Focused)
            {
                using (var p = new Pen(SelectedColor))
                {
                    e.Graphics.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));
                }
            }
        }

        private void DrawBackground(Graphics graphics)
        {
            graphics.Clear(BackgroundColor);

            if (DrawGridLines)
            {
                if (_wavePeaks == null)
                {
                    using (var pen = new Pen(new SolidBrush(GridColor)))
                    {
                        for (int i = 0; i < Width; i += 10)
                        {
                            graphics.DrawLine(pen, i, 0, i, Height);
                            graphics.DrawLine(pen, 0, i, Width, i);
                        }
                    }
                }
                else
                {
                    double interval = 0.1 * _wavePeaks.Header.SampleRate * _zoomFactor; // pixels that is 0.1 second
                    if (ZoomFactor < 0.4)
                        interval = 1.0 * _wavePeaks.Header.SampleRate * _zoomFactor; // pixels that is 1 second
                    int start = SecondsToXPosition(StartPositionSeconds) % ((int)Math.Round(interval));
                    using (var pen = new Pen(new SolidBrush(GridColor)))
                    {
                        for (double i = start; i < Width; i += interval)
                        {
                            var j = (int)Math.Round(i);
                            graphics.DrawLine(pen, j, 0, j, Height);
                            graphics.DrawLine(pen, 0, j, Width, j);
                        }
                    }
                }
            }
        }

        private void DrawTimeLine(double startPositionSeconds, PaintEventArgs e, int imageHeight)
        {
            var start = (int)Math.Round(startPositionSeconds + 0.5);
            double seconds = start - StartPositionSeconds;
            float position = SecondsToXPosition(seconds);
            using (var pen = new Pen(TextColor))
            {
                using (var textBrush = new SolidBrush(TextColor))
                {
                    using (var textFont = new Font(Font.FontFamily, 7))
                    {
                        while (position < Width)
                        {
                            var n = _zoomFactor * _wavePeaks.Header.SampleRate;
                            if (n > 38 || (int)Math.Round(StartPositionSeconds + seconds) % 5 == 0)
                            {
                                e.Graphics.DrawLine(pen, position, imageHeight, position, imageHeight - 10);
                                e.Graphics.DrawString(GetDisplayTime(StartPositionSeconds + seconds), textFont, textBrush, new PointF(position + 2, imageHeight - 13));
                            }

                            seconds += 0.5;
                            position = SecondsToXPosition(seconds);

                            if (n > 64)
                                e.Graphics.DrawLine(pen, position, imageHeight, position, imageHeight - 5);

                            seconds += 0.5;
                            position = SecondsToXPosition(seconds);
                        }
                    }
                }
            }
        }

        private static string GetDisplayTime(double seconds)
        {
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            if (ts.Minutes == 0 && ts.Hours == 0)
                return ts.Seconds.ToString(CultureInfo.InvariantCulture);
            if (ts.Hours == 0)
                return string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
            return string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
        }

        private void DrawParagraph(Paragraph paragraph, PaintEventArgs e, SolidBrush textBrush)
        {
            if (paragraph == null)
            {
                return;
            }

            int currentRegionLeft = SecondsToXPosition(paragraph.StartTime.TotalSeconds - StartPositionSeconds);
            int currentRegionRight = SecondsToXPosition(paragraph.EndTime.TotalSeconds - StartPositionSeconds);
            int currentRegionWidth = currentRegionRight - currentRegionLeft;
            var drawingStyle = TextBold ? FontStyle.Bold : FontStyle.Regular;
            using (var brush = new SolidBrush(Color.FromArgb(42, 255, 255, 255))) // back color for paragraphs
            {
                e.Graphics.FillRectangle(brush, currentRegionLeft, 0, currentRegionWidth, e.Graphics.VisibleClipBounds.Height);

                var pen = new Pen(new SolidBrush(Color.FromArgb(175, 0, 100, 0))) { DashStyle = System.Drawing.Drawing2D.DashStyle.Solid, Width = 2 };
                e.Graphics.DrawLine(pen, currentRegionLeft, 0, currentRegionLeft, e.Graphics.VisibleClipBounds.Height);
                pen.Dispose();
                pen = new Pen(new SolidBrush(Color.FromArgb(175, 110, 10, 10))) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash, Width = 2 };
                e.Graphics.DrawLine(pen, currentRegionRight - 1, 0, currentRegionRight - 1, e.Graphics.VisibleClipBounds.Height);

                string durationStr = paragraph.Duration.ToShortDisplayString();
                var n = _zoomFactor * _wavePeaks.Header.SampleRate;
                if (n > 80)
                {
                    using (var font = new Font(Configuration.Settings.General.SubtitleFontName, TextSize, drawingStyle))
                    using (var blackBrush = new SolidBrush(Color.Black))
                    {
                        var text = HtmlUtil.RemoveHtmlTags(paragraph.Text, true);
                        text = text.Replace(Environment.NewLine, "  ");

                        int actualWidth = (int)e.Graphics.MeasureString(text, font).Width;
                        bool shortned = false;
                        while (actualWidth > currentRegionWidth - 12 && text.Length > 1)
                        {
                            text = text.Remove(text.Length - 1);
                            actualWidth = (int)e.Graphics.MeasureString(text, font).Width;
                            shortned = true;
                        }
                        if (shortned)
                        {
                            text = text.TrimEnd() + "…";
                        }

                        // poor mans outline + text
                        e.Graphics.DrawString(text, font, blackBrush, new PointF(currentRegionLeft + 3, 11 - 7));
                        e.Graphics.DrawString(text, font, blackBrush, new PointF(currentRegionLeft + 3, 9 - 7));
                        e.Graphics.DrawString(text, font, blackBrush, new PointF(currentRegionLeft + 2, 10 - 7));
                        e.Graphics.DrawString(text, font, blackBrush, new PointF(currentRegionLeft + 4, 10 - 7));
                        e.Graphics.DrawString(text, font, textBrush, new PointF(currentRegionLeft + 3, 10 - 7));

                        text = "#" + paragraph.Number + "  " + durationStr;
                        actualWidth = (int)e.Graphics.MeasureString(text, font).Width;
                        if (actualWidth >= currentRegionWidth)
                            text = durationStr;
                        int top = Height - 14 - (int)e.Graphics.MeasureString("#", font).Height;
                        // poor mans outline + text
                        e.Graphics.DrawString(text, font, blackBrush, new PointF(currentRegionLeft + 3, top + 1));
                        e.Graphics.DrawString(text, font, blackBrush, new PointF(currentRegionLeft + 3, top - 1));
                        e.Graphics.DrawString(text, font, blackBrush, new PointF(currentRegionLeft + 2, top));
                        e.Graphics.DrawString(text, font, blackBrush, new PointF(currentRegionLeft + 4, top));
                        e.Graphics.DrawString(text, font, textBrush, new PointF(currentRegionLeft + 3, top));
                    }
                }
                else if (n > 51)
                    e.Graphics.DrawString("#" + paragraph.Number + "  " + durationStr, Font, textBrush, new PointF(currentRegionLeft + 3, Height - 32));
                else if (n > 25)
                    e.Graphics.DrawString("#" + paragraph.Number, Font, textBrush, new PointF(currentRegionLeft + 3, Height - 32));
                pen.Dispose();
            }
        }

        private double XPositionToSeconds(double x)
        {
            return StartPositionSeconds + (x / _wavePeaks.Header.SampleRate) / _zoomFactor;
        }

        private int SecondsToXPosition(double seconds)
        {
            return (int)Math.Round(seconds * _wavePeaks.Header.SampleRate * _zoomFactor);
        }

        private int SecondsToXPositionNoZoom(double seconds)
        {
            return (int)Math.Round(seconds * _wavePeaks.Header.SampleRate);
        }

        private void WaveformMouseDown(object sender, MouseEventArgs e)
        {
            if (_wavePeaks == null)
                return;

            Paragraph oldMouseDownParagraph = null;
            _mouseDownParagraphType = MouseDownParagraphType.None;
            _gapAtStart = -1;
            _firstMove = true;
            if (e.Button == MouseButtons.Left)
            {
                _buttonDownTimeTicks = DateTime.Now.Ticks;

                Cursor = Cursors.VSplit;
                double seconds = XPositionToSeconds(e.X);
                var milliseconds = (int)(seconds * TimeCode.BaseUnit);

                if (SetParagrapBorderHit(milliseconds, NewSelectionParagraph))
                {
                    if (_mouseDownParagraph != null)
                        oldMouseDownParagraph = new Paragraph(_mouseDownParagraph);
                    if (_mouseDownParagraphType == MouseDownParagraphType.Start)
                    {
                        _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                        NewSelectionParagraph.StartTime.TotalMilliseconds = milliseconds;
                        _mouseMoveStartX = e.X;
                        _mouseMoveEndX = SecondsToXPosition(NewSelectionParagraph.EndTime.TotalSeconds - StartPositionSeconds);
                    }
                    else
                    {
                        _mouseDownParagraph.EndTime.TotalMilliseconds = milliseconds;
                        NewSelectionParagraph.EndTime.TotalMilliseconds = milliseconds;
                        _mouseMoveStartX = SecondsToXPosition(NewSelectionParagraph.StartTime.TotalSeconds - StartPositionSeconds);
                        _mouseMoveEndX = e.X;
                    }
                    SetMinMaxViaSeconds(seconds);
                }
                else if (SetParagrapBorderHit(milliseconds, _selectedParagraph) ||
                    SetParagrapBorderHit(milliseconds, _currentParagraph) ||
                    SetParagrapBorderHit(milliseconds, _previousAndNextParagraphs))
                {
                    if (_mouseDownParagraph != null)
                        oldMouseDownParagraph = new Paragraph(_mouseDownParagraph);
                    NewSelectionParagraph = null;

                    int curIdx = _subtitle.Paragraphs.IndexOf(_mouseDownParagraph);
                    if (_mouseDownParagraphType == MouseDownParagraphType.Start)
                    {
                        if (curIdx > 0)
                        {
                            var prev = _subtitle.Paragraphs[curIdx - 1];
                            if (prev.EndTime.TotalMilliseconds + Configuration.Settings.General.MinimumMillisecondsBetweenLines < milliseconds)
                                _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                        }
                        else
                        {
                            _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                        }
                    }
                    else
                    {
                        if (curIdx < _subtitle.Paragraphs.Count - 1)
                        {
                            var next = _subtitle.Paragraphs[curIdx + 1];
                            if (milliseconds + Configuration.Settings.General.MinimumMillisecondsBetweenLines < next.StartTime.TotalMilliseconds)
                                _mouseDownParagraph.EndTime.TotalMilliseconds = milliseconds;
                        }
                        else
                        {
                            _mouseDownParagraph.EndTime.TotalMilliseconds = milliseconds;
                        }
                    }
                    SetMinAndMax();
                }
                else
                {
                    Paragraph p = GetParagraphAtMilliseconds(milliseconds);
                    if (p != null)
                    {
                        _oldParagraph = new Paragraph(p);
                        _mouseDownParagraph = p;
                        oldMouseDownParagraph = new Paragraph(_mouseDownParagraph);
                        _mouseDownParagraphType = MouseDownParagraphType.Whole;
                        _moveWholeStartDifferenceMilliseconds = (XPositionToSeconds(e.X) * TimeCode.BaseUnit) - p.StartTime.TotalMilliseconds;
                        Cursor = Cursors.Hand;
                        SetMinAndMax();
                    }
                    else if (!AllowNewSelection)
                    {
                        Cursor = Cursors.Default;
                    }
                    if (p == null)
                        SetMinMaxViaSeconds(seconds);
                    NewSelectionParagraph = null;
                    _mouseMoveStartX = e.X;
                    _mouseMoveEndX = e.X;
                }
                if (_mouseDownParagraphType == MouseDownParagraphType.Start)
                {
                    if (_subtitle != null && _mouseDownParagraph != null)
                    {
                        int curIdx = _subtitle.Paragraphs.IndexOf(_mouseDownParagraph);
                        //if (curIdx > 0)
                        //    _gapAtStart = _subtitle.Paragraphs[curIdx].StartTime.TotalMilliseconds - _subtitle.Paragraphs[curIdx - 1].EndTime.TotalMilliseconds;
                        if (curIdx > 0)
                            _gapAtStart = oldMouseDownParagraph.StartTime.TotalMilliseconds - _subtitle.Paragraphs[curIdx - 1].EndTime.TotalMilliseconds;
                    }
                }
                else if (_mouseDownParagraphType == MouseDownParagraphType.End)
                {
                    if (_subtitle != null && _mouseDownParagraph != null)
                    {
                        int curIdx = _subtitle.Paragraphs.IndexOf(_mouseDownParagraph);
                        //if (curIdx >= 0 && curIdx < _subtitle.Paragraphs.Count - 1)
                        //    _gapAtStart = _subtitle.Paragraphs[curIdx + 1].StartTime.TotalMilliseconds - _subtitle.Paragraphs[curIdx].EndTime.TotalMilliseconds;
                        if (curIdx >= 0 && curIdx < _subtitle.Paragraphs.Count - 1)
                            _gapAtStart = _subtitle.Paragraphs[curIdx + 1].StartTime.TotalMilliseconds - oldMouseDownParagraph.EndTime.TotalMilliseconds;
                    }
                }
                _mouseDown = true;
            }
            else
            {
                if (e.Button == MouseButtons.Right)
                {
                    double seconds = XPositionToSeconds(e.X);
                    var milliseconds = (int)(seconds * TimeCode.BaseUnit);

                    double currentRegionLeft = Math.Min(_mouseMoveStartX, _mouseMoveEndX);
                    double currentRegionRight = Math.Max(_mouseMoveStartX, _mouseMoveEndX);
                    currentRegionLeft = XPositionToSeconds(currentRegionLeft);
                    currentRegionRight = XPositionToSeconds(currentRegionRight);

                    if (OnNewSelectionRightClicked != null && seconds > currentRegionLeft && seconds < currentRegionRight)
                    {
                        if (_mouseMoveStartX >= 0 && _mouseMoveEndX >= 0)
                        {
                            if (currentRegionRight - currentRegionLeft > 0.1) // not too small subtitles
                            {
                                var paragraph = new Paragraph { StartTime = TimeCode.FromSeconds(currentRegionLeft), EndTime = TimeCode.FromSeconds(currentRegionRight) };
                                if (PreventOverlap)
                                {
                                    if (paragraph.StartTime.TotalMilliseconds <= _wholeParagraphMinMilliseconds)
                                        paragraph.StartTime.TotalMilliseconds = _wholeParagraphMinMilliseconds + 1;
                                    if (paragraph.EndTime.TotalMilliseconds >= _wholeParagraphMaxMilliseconds)
                                        paragraph.EndTime.TotalMilliseconds = _wholeParagraphMaxMilliseconds - 1;
                                }
                                OnNewSelectionRightClicked.Invoke(this, new ParagraphEventArgs(paragraph));
                                NewSelectionParagraph = paragraph;
                                RightClickedParagraph = null;
                                _noClear = true;
                            }
                        }
                    }
                    else
                    {
                        Paragraph p = GetParagraphAtMilliseconds(milliseconds);
                        RightClickedParagraph = p;
                        RightClickedSeconds = seconds;
                        if (p != null)
                        {
                            if (OnParagraphRightClicked != null)
                            {
                                NewSelectionParagraph = null;
                                OnParagraphRightClicked.Invoke(this, new ParagraphEventArgs(seconds, p));
                            }
                        }
                        else
                        {
                            if (OnNonParagraphRightClicked != null)
                            {
                                OnNonParagraphRightClicked.Invoke(this, new ParagraphEventArgs(seconds, null));
                            }
                        }
                    }
                }
                Cursor = Cursors.Default;
            }
        }

        private void SetMinMaxViaSeconds(double seconds)
        {
            _wholeParagraphMinMilliseconds = 0;
            _wholeParagraphMaxMilliseconds = double.MaxValue;
            if (_subtitle != null)
            {
                Paragraph prev = null;
                Paragraph next = null;
                for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
                {
                    Paragraph p2 = _subtitle.Paragraphs[i];
                    if (p2.StartTime.TotalSeconds < seconds)
                    {
                        prev = p2;
                    }
                    else if (p2.EndTime.TotalSeconds > seconds)
                    {
                        next = p2;
                        break;
                    }
                }
                if (prev != null)
                    _wholeParagraphMinMilliseconds = prev.EndTime.TotalMilliseconds + Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                if (next != null)
                    _wholeParagraphMaxMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
            }
        }

        private void SetMinAndMax()
        {
            _wholeParagraphMinMilliseconds = 0;
            _wholeParagraphMaxMilliseconds = double.MaxValue;
            if (_subtitle != null && _mouseDownParagraph != null)
            {
                int curIdx = _subtitle.Paragraphs.IndexOf(_mouseDownParagraph);
                if (curIdx >= 0)
                {
                    if (curIdx > 0)
                    {
                        _wholeParagraphMinMilliseconds = _subtitle.Paragraphs[curIdx - 1].EndTime.TotalMilliseconds + Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }
                    if (curIdx < _subtitle.Paragraphs.Count - 1)
                    {
                        _wholeParagraphMaxMilliseconds = _subtitle.Paragraphs[curIdx + 1].StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }
                }
            }
        }

        private void SetMinAndMaxMoveStart()
        {
            _wholeParagraphMinMilliseconds = 0;
            _wholeParagraphMaxMilliseconds = double.MaxValue;
            if (_subtitle != null && _mouseDownParagraph != null)
            {
                int curIdx = _subtitle.Paragraphs.IndexOf(_mouseDownParagraph);
                if (curIdx >= 0)
                {
                    var gap = Math.Abs(_subtitle.Paragraphs[curIdx - 1].EndTime.TotalMilliseconds - _subtitle.Paragraphs[curIdx].StartTime.TotalMilliseconds);
                    _wholeParagraphMinMilliseconds = _subtitle.Paragraphs[curIdx - 1].StartTime.TotalMilliseconds + gap + 200;
                }
            }
        }

        private void SetMinAndMaxMoveEnd()
        {
            _wholeParagraphMinMilliseconds = 0;
            _wholeParagraphMaxMilliseconds = double.MaxValue;
            if (_subtitle != null && _mouseDownParagraph != null)
            {
                int curIdx = _subtitle.Paragraphs.IndexOf(_mouseDownParagraph);
                if (curIdx >= 0)
                {
                    if (curIdx < _subtitle.Paragraphs.Count - 1)
                    {
                        var gap = Math.Abs(_subtitle.Paragraphs[curIdx].EndTime.TotalMilliseconds - _subtitle.Paragraphs[curIdx + 1].StartTime.TotalMilliseconds);
                        _wholeParagraphMaxMilliseconds = _subtitle.Paragraphs[curIdx + 1].EndTime.TotalMilliseconds - gap - 200;
                    }
                }
            }
        }

        private bool SetParagrapBorderHit(int milliseconds, List<Paragraph> paragraphs)
        {
            foreach (Paragraph p in paragraphs)
            {
                bool hit = SetParagrapBorderHit(milliseconds, p);
                if (hit)
                    return true;
            }
            return false;
        }

        private Paragraph GetParagraphAtMilliseconds(int milliseconds)
        {
            Paragraph p = null;
            if (IsParagrapHit(milliseconds, _selectedParagraph))
                p = _selectedParagraph;
            else if (IsParagrapHit(milliseconds, _currentParagraph))
                p = _currentParagraph;

            if (p == null)
            {
                foreach (Paragraph pNext in _previousAndNextParagraphs)
                {
                    if (IsParagrapHit(milliseconds, pNext))
                    {
                        p = pNext;
                        break;
                    }
                }
            }

            return p;
        }

        private bool SetParagrapBorderHit(int milliseconds, Paragraph paragraph)
        {
            if (paragraph == null)
                return false;

            if (IsParagrapBorderStartHit(milliseconds, paragraph.StartTime.TotalMilliseconds))
            {
                _oldParagraph = new Paragraph(paragraph);
                _mouseDownParagraph = paragraph;
                _mouseDownParagraphType = MouseDownParagraphType.Start;
                return true;
            }
            if (IsParagrapBorderEndHit(milliseconds, paragraph.EndTime.TotalMilliseconds))
            {
                _oldParagraph = new Paragraph(paragraph);
                _mouseDownParagraph = paragraph;
                _mouseDownParagraphType = MouseDownParagraphType.End;
                return true;
            }
            return false;
        }

        private bool PreventOverlap
        {
            get
            {
                if (ModifierKeys == Keys.Shift)
                    return AllowOverlap;
                return !AllowOverlap;
            }
        }

        private bool AllowMovePrevOrNext
        {
            get
            {
                return _gapAtStart >= 0 && _gapAtStart < 500 && ModifierKeys == Keys.Alt;
            }
        }

        private void WaveformMouseMove(object sender, MouseEventArgs e)
        {
            if (_wavePeaks == null)
                return;

            int oldMouseMoveLastX = _mouseMoveLastX;
            if (e.X < 0 && StartPositionSeconds > 0.1 && _mouseDown)
            {
                if (e.X < _mouseMoveLastX)
                {
                    StartPositionSeconds -= 0.1;
                    if (_mouseDownParagraph == null)
                    {
                        _mouseMoveEndX = 0;
                        _mouseMoveStartX += (int)(_wavePeaks.Header.SampleRate * 0.1);
                        OnPositionSelected.Invoke(this, new ParagraphEventArgs(StartPositionSeconds, null));
                    }
                }
                _mouseMoveLastX = e.X;
                Invalidate();
                return;
            }
            if (e.X > Width && StartPositionSeconds + 0.1 < _wavePeaks.Header.LengthInSeconds && _mouseDown)
            {
                //if (e.X > _mouseMoveLastX) // not much room for moving mouse cursor, so just scroll right
                {
                    StartPositionSeconds += 0.1;
                    if (_mouseDownParagraph == null)
                    {
                        _mouseMoveEndX = Width;
                        _mouseMoveStartX -= (int)(_wavePeaks.Header.SampleRate * 0.1);
                        OnPositionSelected.Invoke(this, new ParagraphEventArgs(StartPositionSeconds, null));
                    }
                }
                _mouseMoveLastX = e.X;
                Invalidate();
                return;
            }
            _mouseMoveLastX = e.X;

            if (e.X < 0 || e.X > Width)
                return;

            if (e.Button == MouseButtons.None)
            {
                double seconds = XPositionToSeconds(e.X);
                var milliseconds = (int)(seconds * TimeCode.BaseUnit);

                if (IsParagrapBorderHit(milliseconds, NewSelectionParagraph))
                    Cursor = Cursors.VSplit;
                else if (IsParagrapBorderHit(milliseconds, _selectedParagraph) ||
                         IsParagrapBorderHit(milliseconds, _currentParagraph) ||
                         IsParagrapBorderHit(milliseconds, _previousAndNextParagraphs))
                {
                    Cursor = Cursors.VSplit;
                }
                else
                {
                    Cursor = Cursors.Default;
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (oldMouseMoveLastX == e.X)
                    return; // no horizontal movement

                if (_mouseDown)
                {
                    if (_mouseDownParagraph != null)
                    {
                        double seconds = XPositionToSeconds(e.X);
                        var milliseconds = (int)(seconds * TimeCode.BaseUnit);
                        var subtitleIndex = _subtitle.GetIndex(_mouseDownParagraph);
                        _prevParagraph = _subtitle.GetParagraphOrDefault(subtitleIndex - 1);
                        _nextParagraph = _subtitle.GetParagraphOrDefault(subtitleIndex + 1);

                        if (_firstMove && Math.Abs(oldMouseMoveLastX - e.X) < Configuration.Settings.General.MinimumMillisecondsBetweenLines && GetParagraphAtMilliseconds(milliseconds) == null)
                        {
                            if (_mouseDownParagraphType == MouseDownParagraphType.Start && _prevParagraph != null && Math.Abs(_mouseDownParagraph.StartTime.TotalMilliseconds - _prevParagraph.EndTime.TotalMilliseconds) <= ClosenessForBorderSelection + 15)
                                return; // do not decide which paragraph to move yet
                            if (_mouseDownParagraphType == MouseDownParagraphType.End && _nextParagraph != null && Math.Abs(_mouseDownParagraph.EndTime.TotalMilliseconds - _nextParagraph.StartTime.TotalMilliseconds) <= ClosenessForBorderSelection + 15)
                                return; // do not decide which paragraph to move yet
                        }

                        if (ModifierKeys != Keys.Alt)
                        {
                            // decide which paragraph to move
                            if (_firstMove && e.X > oldMouseMoveLastX && _nextParagraph != null && _mouseDownParagraphType == MouseDownParagraphType.End)
                            {
                                if (milliseconds >= _nextParagraph.StartTime.TotalMilliseconds && milliseconds < _nextParagraph.EndTime.TotalMilliseconds)
                                {
                                    _mouseDownParagraph = _nextParagraph;
                                    _mouseDownParagraphType = MouseDownParagraphType.Start;
                                }
                            }
                            else if (_firstMove && e.X < oldMouseMoveLastX && _prevParagraph != null && _mouseDownParagraphType == MouseDownParagraphType.Start)
                            {
                                if (milliseconds <= _prevParagraph.EndTime.TotalMilliseconds && milliseconds > _prevParagraph.StartTime.TotalMilliseconds)
                                {
                                    _mouseDownParagraph = _prevParagraph;
                                    _mouseDownParagraphType = MouseDownParagraphType.End;
                                }
                            }
                        }
                        _firstMove = false;

                        if (_mouseDownParagraphType == MouseDownParagraphType.Start)
                        {
                            if (_mouseDownParagraph.EndTime.TotalMilliseconds - milliseconds > MinimumSelectionMilliseconds)
                            {
                                if (AllowMovePrevOrNext)
                                    SetMinAndMaxMoveStart();
                                else
                                    SetMinAndMax();
                                _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                                if (PreventOverlap && _mouseDownParagraph.StartTime.TotalMilliseconds <= _wholeParagraphMinMilliseconds)
                                {
                                    _mouseDownParagraph.StartTime.TotalMilliseconds = _wholeParagraphMinMilliseconds + 1;
                                }

                                if (NewSelectionParagraph != null)
                                {
                                    NewSelectionParagraph.StartTime.TotalMilliseconds = milliseconds;
                                    if (PreventOverlap && NewSelectionParagraph.StartTime.TotalMilliseconds <= _wholeParagraphMinMilliseconds)
                                    {
                                        NewSelectionParagraph.StartTime.TotalMilliseconds = _wholeParagraphMinMilliseconds + 1;
                                    }
                                    _mouseMoveStartX = e.X;
                                }
                                else
                                {
                                    if (OnTimeChanged != null)
                                        OnTimeChanged.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph, _oldParagraph, _mouseDownParagraphType, AllowMovePrevOrNext));
                                    Refresh();
                                    return;
                                }
                            }
                        }
                        else if (_mouseDownParagraphType == MouseDownParagraphType.End)
                        {
                            if (milliseconds - _mouseDownParagraph.StartTime.TotalMilliseconds > MinimumSelectionMilliseconds)
                            {
                                if (AllowMovePrevOrNext)
                                    SetMinAndMaxMoveEnd();
                                else
                                    SetMinAndMax();
                                _mouseDownParagraph.EndTime.TotalMilliseconds = milliseconds;
                                if (PreventOverlap && _mouseDownParagraph.EndTime.TotalMilliseconds >= _wholeParagraphMaxMilliseconds)
                                {
                                    _mouseDownParagraph.EndTime.TotalMilliseconds = _wholeParagraphMaxMilliseconds - 1;
                                }

                                if (NewSelectionParagraph != null)
                                {
                                    NewSelectionParagraph.EndTime.TotalMilliseconds = milliseconds;
                                    if (PreventOverlap && NewSelectionParagraph.EndTime.TotalMilliseconds >= _wholeParagraphMaxMilliseconds)
                                    {
                                        NewSelectionParagraph.EndTime.TotalMilliseconds = _wholeParagraphMaxMilliseconds - 1;
                                    }
                                    _mouseMoveEndX = e.X;
                                }
                                else
                                {
                                    //SHOW DEBUG MSG                     SolidBrush tBrush = new SolidBrush(Color.Turquoise);
                                    //var g = this.CreateGraphics();
                                    //g.DrawString("AllowMovePrevOrNext: " + AllowMovePrevOrNext.ToString() + ", GapStart: " + _gapAtStart.ToString(), Font, tBrush, new PointF(100, 100));
                                    //tBrush.Dispose();
                                    //g.Dispose();

                                    if (OnTimeChanged != null)
                                        OnTimeChanged.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph, _oldParagraph, _mouseDownParagraphType, AllowMovePrevOrNext));
                                    Refresh();
                                    return;
                                }
                            }
                        }
                        else if (_mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            double durationMilliseconds = _mouseDownParagraph.Duration.TotalMilliseconds;

                            _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds - _moveWholeStartDifferenceMilliseconds;
                            _mouseDownParagraph.EndTime.TotalMilliseconds = _mouseDownParagraph.StartTime.TotalMilliseconds + durationMilliseconds;

                            if (PreventOverlap && _mouseDownParagraph.EndTime.TotalMilliseconds >= _wholeParagraphMaxMilliseconds)
                            {
                                _mouseDownParagraph.EndTime.TotalMilliseconds = _wholeParagraphMaxMilliseconds - 1;
                                _mouseDownParagraph.StartTime.TotalMilliseconds = _mouseDownParagraph.EndTime.TotalMilliseconds - durationMilliseconds;
                            }
                            else if (PreventOverlap && _mouseDownParagraph.StartTime.TotalMilliseconds <= _wholeParagraphMinMilliseconds)
                            {
                                _mouseDownParagraph.StartTime.TotalMilliseconds = _wholeParagraphMinMilliseconds + 1;
                                _mouseDownParagraph.EndTime.TotalMilliseconds = _mouseDownParagraph.StartTime.TotalMilliseconds + durationMilliseconds;
                            }

                            if (OnTimeChanged != null)
                                OnTimeChanged.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph, _oldParagraph));
                        }
                    }
                    else
                    {
                        _mouseMoveEndX = e.X;
                        if (NewSelectionParagraph == null && Math.Abs(_mouseMoveEndX - _mouseMoveStartX) > 2)
                        {
                            if (AllowNewSelection)
                                NewSelectionParagraph = new Paragraph();
                        }

                        if (NewSelectionParagraph != null)
                        {
                            int start = Math.Min(_mouseMoveStartX, _mouseMoveEndX);
                            int end = Math.Max(_mouseMoveStartX, _mouseMoveEndX);

                            var startTotalSeconds = XPositionToSeconds(start);
                            var endTotalSeconds = XPositionToSeconds(end);

                            if (PreventOverlap && endTotalSeconds * TimeCode.BaseUnit >= _wholeParagraphMaxMilliseconds)
                            {
                                NewSelectionParagraph.EndTime.TotalMilliseconds = _wholeParagraphMaxMilliseconds - 1;
                                Invalidate();
                                return;
                            }
                            if (PreventOverlap && startTotalSeconds * TimeCode.BaseUnit <= _wholeParagraphMinMilliseconds)
                            {
                                NewSelectionParagraph.StartTime.TotalMilliseconds = _wholeParagraphMinMilliseconds + 1;
                                Invalidate();
                                return;
                            }
                            NewSelectionParagraph.StartTime.TotalSeconds = startTotalSeconds;
                            NewSelectionParagraph.EndTime.TotalSeconds = endTotalSeconds;
                        }
                    }
                    Invalidate();
                }
            }
        }

        private bool IsParagrapBorderHit(int milliseconds, List<Paragraph> paragraphs)
        {
            foreach (Paragraph p in paragraphs)
            {
                bool hit = IsParagrapBorderHit(milliseconds, p);
                if (hit)
                    return true;
            }
            return false;
        }

        private bool IsParagrapBorderHit(int milliseconds, Paragraph paragraph)
        {
            if (paragraph == null)
                return false;

            return IsParagrapBorderStartHit(milliseconds, paragraph.StartTime.TotalMilliseconds) ||
                   IsParagrapBorderEndHit(milliseconds, paragraph.EndTime.TotalMilliseconds);
        }

        private bool IsParagrapBorderStartHit(double milliseconds, double startMs)
        {
            return Math.Abs(milliseconds - (startMs - 5)) - 10 <= ClosenessForBorderSelection;
        }

        private bool IsParagrapBorderEndHit(double milliseconds, double endMs)
        {
            return Math.Abs(milliseconds - (endMs - 22)) - 7 <= ClosenessForBorderSelection;
        }

        private void WaveformMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_mouseDown)
                {
                    if (_mouseDownParagraph != null)
                    {
                        _mouseDownParagraph = null;
                    }
                    else
                    {
                        _mouseMoveEndX = e.X;
                    }
                    _mouseDown = false;
                }
            }
            Cursor = Cursors.Default;
        }

        private void WaveformMouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
            _mouseDown = false;
            Invalidate();
        }

        private void WaveformMouseEnter(object sender, EventArgs e)
        {
            if (_wavePeaks == null || _wavePeaks.Header == null)
                return;

            if (_noClear)
            {
                _noClear = false;
            }
            else
            {
                Cursor = Cursors.Default;
                _mouseDown = false;
                _mouseDownParagraph = null;
                _mouseMoveStartX = -1;
                _mouseMoveEndX = -1;
                Invalidate();
            }

            if (NewSelectionParagraph != null)
            {
                _mouseMoveStartX = SecondsToXPosition(NewSelectionParagraph.StartTime.TotalSeconds - StartPositionSeconds);
                _mouseMoveEndX = SecondsToXPosition(NewSelectionParagraph.EndTime.TotalSeconds - StartPositionSeconds);
            }
        }

        private void WaveformMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_wavePeaks == null)
                return;

            _mouseDown = false;
            _mouseDownParagraph = null;
            _mouseMoveStartX = -1;
            _mouseMoveEndX = -1;

            if (e.Button == MouseButtons.Left)
            {
                if (OnPause != null)
                    OnPause.Invoke(sender, null);
                double seconds = XPositionToSeconds(e.X);
                var milliseconds = (int)(seconds * TimeCode.BaseUnit);

                Paragraph p = GetParagraphAtMilliseconds(milliseconds);
                if (p != null)
                {
                    seconds = p.StartTime.TotalSeconds;
                    double endSeconds = p.EndTime.TotalSeconds;
                    if (seconds < StartPositionSeconds)
                    {
                        StartPositionSeconds = (p.StartTime.TotalSeconds) + 0.1; // move earlier - show whole selected paragraph
                    }
                    else if (endSeconds > EndPositionSeconds)
                    {
                        double newStartPos = StartPositionSeconds + (endSeconds - EndPositionSeconds); // move later, so whole selected paragraph is visible
                        if (newStartPos < seconds) // but only if visibile screen is wide enough
                            StartPositionSeconds = newStartPos;
                    }
                }

                if (OnDoubleClickNonParagraph != null)
                    OnDoubleClickNonParagraph.Invoke(this, new ParagraphEventArgs(seconds, p));
            }
        }

        private static bool IsParagrapHit(int milliseconds, Paragraph paragraph)
        {
            if (paragraph == null)
                return false;

            return milliseconds >= paragraph.StartTime.TotalMilliseconds && milliseconds <= paragraph.EndTime.TotalMilliseconds;
        }

        private void WaveformMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && OnSingleClick != null)
            {
                int diff = Math.Abs(_mouseMoveStartX - e.X);
                if (_mouseMoveStartX == -1 || _mouseMoveEndX == -1 || diff < 10 && TimeSpan.FromTicks(DateTime.Now.Ticks - _buttonDownTimeTicks).TotalSeconds < 0.25)
                {
                    if (ModifierKeys == Keys.Shift && _selectedParagraph != null)
                    {
                        double seconds = XPositionToSeconds(e.X);
                        var milliseconds = (int)(seconds * TimeCode.BaseUnit);
                        if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            if (seconds < _selectedParagraph.EndTime.TotalSeconds)
                            {
                                _oldParagraph = new Paragraph(_selectedParagraph);
                                _mouseDownParagraph = _selectedParagraph;
                                _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                                if (OnTimeChanged != null)
                                    OnTimeChanged.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph, _oldParagraph));
                            }
                        }
                        return;
                    }
                    if (ModifierKeys == Keys.Control && _selectedParagraph != null)
                    {
                        double seconds = XPositionToSeconds(e.X);
                        var milliseconds = (int)(seconds * TimeCode.BaseUnit);
                        if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            if (seconds > _selectedParagraph.StartTime.TotalSeconds)
                            {
                                _oldParagraph = new Paragraph(_selectedParagraph);
                                _mouseDownParagraph = _selectedParagraph;
                                _mouseDownParagraph.EndTime.TotalMilliseconds = milliseconds;
                                if (OnTimeChanged != null)
                                    OnTimeChanged.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph, _oldParagraph));
                            }
                        }
                        return;
                    }
                    if (ModifierKeys == (Keys.Control | Keys.Shift) && _selectedParagraph != null)
                    {
                        double seconds = XPositionToSeconds(e.X);
                        if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            _oldParagraph = new Paragraph(_selectedParagraph);
                            _mouseDownParagraph = _selectedParagraph;
                            if (OnTimeChangedAndOffsetRest != null)
                                OnTimeChangedAndOffsetRest.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph));
                        }
                        return;
                    }
                    if (ModifierKeys == Keys.Alt && _selectedParagraph != null)
                    {
                        double seconds = XPositionToSeconds(e.X);
                        var milliseconds = (int)(seconds * TimeCode.BaseUnit);
                        if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            _oldParagraph = new Paragraph(_selectedParagraph);
                            _mouseDownParagraph = _selectedParagraph;
                            double durationMilliseconds = _mouseDownParagraph.Duration.TotalMilliseconds;
                            _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                            _mouseDownParagraph.EndTime.TotalMilliseconds = _mouseDownParagraph.StartTime.TotalMilliseconds + durationMilliseconds;
                            if (OnTimeChanged != null)
                                OnTimeChanged.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph, _oldParagraph));
                        }
                        return;
                    }

                    if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        OnSingleClick.Invoke(this, new ParagraphEventArgs(XPositionToSeconds(e.X), null));
                }
            }
        }

        private void WaveformKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.None && e.KeyCode == Keys.Add)
            {
                ZoomIn();
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Subtract)
            {
                ZoomOut();
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Z)
            {
                if (StartPositionSeconds > 0.1)
                {
                    StartPositionSeconds -= 0.1;
                    OnPositionSelected.Invoke(this, new ParagraphEventArgs(StartPositionSeconds, null));
                    Invalidate();
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.X)
            {
                if (StartPositionSeconds + 0.1 < _wavePeaks.Header.LengthInSeconds)
                {
                    StartPositionSeconds += 0.1;
                    OnPositionSelected.Invoke(this, new ParagraphEventArgs(StartPositionSeconds, null));
                    Invalidate();
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.C)
            {
                Locked = !Locked;
                Invalidate();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == InsertAtVideoPositionShortcut)
            {
                if (InsertAtVideoPosition != null)
                {
                    InsertAtVideoPosition.Invoke(this, null);
                    e.SuppressKeyPress = true;
                }
            }
        }

        public double FindDataBelowThreshold(int threshold, double durationInSeconds)
        {
            int begin = SecondsToXPosition(_currentVideoPositionSeconds + 1);
            int length = SecondsToXPosition(durationInSeconds);

            int hitCount = 0;
            for (int i = begin; i < _wavePeaks.AllSamples.Count; i++)
            {
                if (i > 0 && i < _wavePeaks.AllSamples.Count && Math.Abs(_wavePeaks.AllSamples[i]) <= threshold)
                    hitCount++;
                else
                    hitCount = 0;
                if (hitCount > length)
                {
                    double seconds = ((i - (length / 2)) / (double)_wavePeaks.Header.SampleRate) / _zoomFactor;
                    if (seconds >= 0)
                    {
                        StartPositionSeconds = seconds;
                        if (StartPositionSeconds > 1)
                            StartPositionSeconds -= 1;
                        OnSingleClick.Invoke(this, new ParagraphEventArgs(seconds, null));
                        Invalidate();
                    }
                    return seconds;
                }
            }
            return -1;
        }

        public double FindDataBelowThresholdBack(int threshold, double durationInSeconds)
        {
            int begin = SecondsToXPosition(_currentVideoPositionSeconds - 1);
            int length = SecondsToXPosition(durationInSeconds);

            int hitCount = 0;
            for (int i = begin; i > 0; i--)
            {
                if (i > 0 && i < _wavePeaks.AllSamples.Count && Math.Abs(_wavePeaks.AllSamples[i]) <= threshold)
                    hitCount++;
                else
                    hitCount = 0;
                if (hitCount > length)
                {
                    double seconds = (i + (length / 2)) / (double)_wavePeaks.Header.SampleRate / _zoomFactor;
                    if (seconds >= 0)
                    {
                        StartPositionSeconds = seconds;
                        if (StartPositionSeconds > 1)
                            StartPositionSeconds -= 1;
                        else
                            StartPositionSeconds = 0;
                        OnSingleClick.Invoke(this, new ParagraphEventArgs(seconds, null));
                        Invalidate();
                    }
                    return seconds;
                }
            }
            return -1;
        }

        public void ZoomIn()
        {
            ZoomFactor += 0.1;
            if (OnZoomedChanged != null)
                OnZoomedChanged.Invoke(this, null);
        }

        public void ZoomOut()
        {
            ZoomFactor -= 0.1;
            if (OnZoomedChanged != null)
                OnZoomedChanged.Invoke(this, null);
        }

        private void VerticalZoomIn()
        {
            VerticalZoomFactor *= 1.1;
        }

        private void VerticalZoomOut()
        {
            VerticalZoomFactor /= 1.1;
        }

        private void WaveformMouseWheel(object sender, MouseEventArgs e)
        {
            // The scroll wheel could work in theory without the waveform loaded (it would be
            // just like dragging the slider, which does work without the waveform), but the
            // code below doesn't support it, so bail out until someone feels like fixing it.
            if (_wavePeaks == null)
                return;

            if (ModifierKeys == Keys.Control)
            {
                if (e.Delta > 0)
                    ZoomIn();
                else
                    ZoomOut();

                return;
            }

            if (ModifierKeys == (Keys.Control | Keys.Shift))
            {
                if (e.Delta > 0)
                    VerticalZoomIn();
                else
                    VerticalZoomOut();

                return;
            }

            int delta = e.Delta;
            if (!MouseWheelScrollUpIsForward)
                delta = delta * -1;
            if (Locked)
            {
                OnPositionSelected.Invoke(this, new ParagraphEventArgs(_currentVideoPositionSeconds + (delta / 256.0), null));
            }
            else
            {
                StartPositionSeconds += delta / 256.0;
                if (_currentVideoPositionSeconds < StartPositionSeconds || _currentVideoPositionSeconds >= EndPositionSeconds)
                    OnPositionSelected.Invoke(this, new ParagraphEventArgs(StartPositionSeconds, null));
            }
            Invalidate();
        }

        /////////////////////////////////////////////////

        public void InitializeSpectrogram(string spectrogramDirectory)
        {
            _spectrogramBitmaps = new List<Bitmap>();
            _tempShowSpectrogram = ShowSpectrogram;
            ShowSpectrogram = false;
            if (Directory.Exists(spectrogramDirectory))
            {
                _spectrogramDirectory = spectrogramDirectory;
                _spectrogramBackgroundWorker = new System.ComponentModel.BackgroundWorker();
                _spectrogramBackgroundWorker.DoWork += LoadSpectrogramBitmapsAsync;
                _spectrogramBackgroundWorker.RunWorkerCompleted += LoadSpectrogramBitmapsCompleted;
                _spectrogramBackgroundWorker.RunWorkerAsync();
            }
        }

        private void LoadSpectrogramBitmapsCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            LoadSpectrogramInfo(_spectrogramDirectory);
            ShowSpectrogram = _tempShowSpectrogram;
            if (_spectrogramBackgroundWorker != null)
                _spectrogramBackgroundWorker.Dispose();
        }

        private void LoadSpectrogramBitmapsAsync(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                for (var count = 0; ; count++)
                {
                    var fileName = Path.Combine(_spectrogramDirectory, count + ".gif");

                    // important that this does not lock file (do NOT use Image.FromFile(fileName) or alike!!!)
                    using (var ms = new MemoryStream(File.ReadAllBytes(fileName)))
                    {
                        _spectrogramBitmaps.Add((Bitmap)Image.FromStream(ms));
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // no more files
            }
        }

        public void InitializeSpectrogram(List<Bitmap> spectrogramBitmaps, string spectrogramDirectory)
        {
            _spectrogramBitmaps = spectrogramBitmaps;
            LoadSpectrogramInfo(spectrogramDirectory);
        }

        private void LoadSpectrogramInfo(string spectrogramDirectory)
        {
            try
            {
                var doc = new XmlDocument();
                string xmlInfoFileName = Path.Combine(spectrogramDirectory, "Info.xml");
                if (File.Exists(xmlInfoFileName))
                {
                    doc.Load(xmlInfoFileName);
                    _sampleDuration = Convert.ToDouble(doc.DocumentElement.SelectSingleNode("SampleDuration").InnerText, CultureInfo.InvariantCulture);
                    _nfft = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("NFFT").InnerText, CultureInfo.InvariantCulture);
                    _imageWidth = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("ImageWidth").InnerText, CultureInfo.InvariantCulture);
                    ShowSpectrogram = true;
                }
                else
                {
                    ShowSpectrogram = false;
                }
            }
            catch
            {
                ShowSpectrogram = false;
            }
        }

        private void DrawSpectrogramBitmap(double seconds, Graphics graphics)
        {
            double duration = EndPositionSeconds - StartPositionSeconds;
            var width = (int)(duration / _sampleDuration);

            using (var bmpDestination = new Bitmap(width, _nfft / 2)) //calculate width
            {
                using (var gfx = Graphics.FromImage(bmpDestination))
                {
                    double startRow = seconds / (_sampleDuration * _imageWidth);
                    var bitmapIndex = (int)startRow;
                    var subtractValue = (int)Math.Round((startRow - bitmapIndex) * _imageWidth);

                    int i = 0;
                    while (i * _imageWidth < width && i + bitmapIndex < _spectrogramBitmaps.Count)
                    {
                        var bmp = _spectrogramBitmaps[i + bitmapIndex];
                        gfx.DrawImageUnscaled(bmp, new Point(bmp.Width * i - subtractValue, 0));
                        i++;
                    }
                    if (i + bitmapIndex < _spectrogramBitmaps.Count && subtractValue > 0)
                    {
                        var bmp = _spectrogramBitmaps[i + bitmapIndex];
                        gfx.DrawImageUnscaled(bmp, new Point(bmp.Width * i - subtractValue, 0));
                    }
                }
                if (ShowWaveform)
                    graphics.DrawImage(bmpDestination, new Rectangle(0, Height - SpectrogramDisplayHeight, Width, SpectrogramDisplayHeight));
                else
                    graphics.DrawImage(bmpDestination, new Rectangle(0, 0, Width, Height));
            }
        }

        private double GetAverageVolumeForNextMilliseconds(int sampleIndex, int milliseconds)
        {
            int length = SecondsToXPosition(milliseconds / TimeCode.BaseUnit);
            if (length < 9)
                length = 9;
            double v = 0;
            int count = 0;
            for (int i = sampleIndex; i < sampleIndex + length; i++)
            {
                if (i > 0 && i < _wavePeaks.AllSamples.Count)
                {
                    v += Math.Abs(_wavePeaks.AllSamples[i]);
                    count++;
                }
            }
            if (count == 0)
                return 0;
            return v / count;
        }

        internal void GenerateTimeCodes(double startFromSeconds, int minimumVolumePercent, int maximumVolumePercent, int defaultMilliseconds)
        {
            int begin = SecondsToXPosition(startFromSeconds);

            double average = 0;
            for (int k = begin; k < _wavePeaks.AllSamples.Count; k++)
                average += Math.Abs(_wavePeaks.AllSamples[k]);
            average = average / (_wavePeaks.AllSamples.Count - begin);

            var maxThreshold = (int)(_wavePeaks.DataMaxValue * (maximumVolumePercent / 100.0));
            var silenceThreshold = (int)(average * (minimumVolumePercent / 100.0));

            int length50Ms = SecondsToXPosition(0.050);
            double secondsPerParagraph = defaultMilliseconds / TimeCode.BaseUnit;
            int minBetween = SecondsToXPosition(Configuration.Settings.General.MinimumMillisecondsBetweenLines / TimeCode.BaseUnit);
            bool subtitleOn = false;
            int i = begin;
            while (i < _wavePeaks.AllSamples.Count)
            {
                if (subtitleOn)
                {
                    var currentLengthInSeconds = XPositionToSeconds(i - begin) - StartPositionSeconds;
                    if (currentLengthInSeconds > 1.0)
                    {
                        subtitleOn = EndParagraphDueToLowVolume(silenceThreshold, begin, true, i);
                        if (!subtitleOn)
                        {
                            begin = i + minBetween;
                            i = begin;
                        }
                    }
                    if (subtitleOn && currentLengthInSeconds >= secondsPerParagraph)
                    {
                        for (int j = 0; j < 20; j++)
                        {
                            subtitleOn = EndParagraphDueToLowVolume(silenceThreshold, begin, true, i + (j * length50Ms));
                            if (!subtitleOn)
                            {
                                i += (j * length50Ms);
                                begin = i + minBetween;
                                i = begin;
                                break;
                            }
                        }

                        if (subtitleOn) // force break
                        {
                            var p = new Paragraph(string.Empty, (XPositionToSeconds(begin) - StartPositionSeconds) * TimeCode.BaseUnit, (XPositionToSeconds(i) - StartPositionSeconds) * TimeCode.BaseUnit);
                            _subtitle.Paragraphs.Add(p);
                            begin = i + minBetween;
                            i = begin;
                        }
                    }
                }
                else
                {
                    double avgVol = GetAverageVolumeForNextMilliseconds(i, 100);
                    if (avgVol > silenceThreshold)
                    {
                        if (avgVol < maxThreshold)
                        {
                            subtitleOn = true;
                            begin = i;
                        }
                    }
                }
                i++;
            }
        }

        private bool EndParagraphDueToLowVolume(double silenceThreshold, int begin, bool subtitleOn, int i)
        {
            double avgVol = GetAverageVolumeForNextMilliseconds(i, 100);
            if (avgVol < silenceThreshold)
            {
                var p = new Paragraph(string.Empty, (XPositionToSeconds(begin) - StartPositionSeconds) * TimeCode.BaseUnit, (XPositionToSeconds(i) - StartPositionSeconds) * TimeCode.BaseUnit);
                _subtitle.Paragraphs.Add(p);
                subtitleOn = false;
            }
            return subtitleOn;
        }

    }
}