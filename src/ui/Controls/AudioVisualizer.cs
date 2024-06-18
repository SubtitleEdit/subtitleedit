using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    public sealed partial class AudioVisualizer : UserControl
    {
        public enum MouseDownParagraphType
        {
            None,
            Start,
            Whole,
            End,
            StartOrEnd,
        }

        public class MinMax
        {
            public double Min { get; set; }
            public double Max { get; set; }
            public double Avg { get; set; }
        }

        public class ParagraphEventArgs : EventArgs
        {
            public Paragraph Paragraph { get; }
            public double Seconds { get; }
            public Paragraph BeforeParagraph { get; set; }
            public MouseDownParagraphType MouseDownParagraphType { get; set; }
            public bool MovePreviousOrNext { get; set; }
            public double AdjustMs { get; set; }
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

        public int ClosenessForBorderSelection { get; set; } = 15;
        private const int MinimumSelectionMilliseconds = 100;

        private long _buttonDownTimeTicks;
        private long _lastMouseWheelScroll = -1;
        private int _mouseMoveLastX = -1;
        private int _mouseMoveStartX = -1;
        private double _moveWholeStartDifferenceMilliseconds = -1;
        private int _mouseMoveEndX = -1;
        private bool _mouseDown;
        private Paragraph _oldParagraph;
        private Paragraph _mouseDownParagraph;
        private Paragraph[] _mouseDownParagraphs;
        private MouseDownParagraphType _mouseDownParagraphType = MouseDownParagraphType.Start;
        private readonly List<Paragraph> _displayableParagraphs;
        private readonly List<Paragraph> _allSelectedParagraphs;
        private Paragraph _prevParagraph;
        private Paragraph _nextParagraph;
        private bool _firstMove = true;
        private double _currentVideoPositionSeconds = -1;
        private WavePeakData _wavePeaks;
        private Subtitle _subtitle;
        private bool _noClear;
        private double _gapAtStart = -1;

        private SpectrogramData _spectrogram;
        private const int SpectrogramDisplayHeight = 128;

        public delegate void ParagraphEventHandler(object sender, ParagraphEventArgs e);
        public event ParagraphEventHandler OnNewSelectionRightClicked;
        public event ParagraphEventHandler OnParagraphRightClicked;
        public event ParagraphEventHandler OnNonParagraphRightClicked;
        public event ParagraphEventHandler OnPositionSelected;

        public event ParagraphEventHandler OnTimeChanged;
        public event ParagraphEventHandler OnStartTimeChanged;
        public event ParagraphEventHandler OnTimeChangedAndOffsetRest;

        public event ParagraphEventHandler OnSingleClick;
        public event ParagraphEventHandler OnDoubleClickNonParagraph;
        public event EventHandler OnPause;
        public event EventHandler OnZoomedChanged;
        public event EventHandler OnInsertAtVideoPosition;
        public event EventHandler OnPasteAtVideoPosition;
        public event EventHandler OnSelectAll;

        private double _wholeParagraphMinMilliseconds;
        private double _wholeParagraphMaxMilliseconds = double.MaxValue;
        public Keys InsertAtVideoPositionShortcut { get; set; }
        public Keys Move100MsLeft { get; set; }
        public Keys Move100MsRight { get; set; }
        public Keys MoveOneSecondLeft { get; set; }
        public Keys MoveOneSecondRight { get; set; }
        public bool MouseWheelScrollUpIsForward { get; set; } = true;

        public const double ZoomMinimum = 0.1;
        public const double ZoomMaximum = 2.5;
        private double _zoomFactor = 1.0; // 1.0=no zoom

        public int ShotChangeSnapPixels = 8;

        public double ZoomFactor
        {
            get => _zoomFactor;
            set
            {
                if (value < ZoomMinimum)
                {
                    value = ZoomMinimum;
                }

                if (value > ZoomMaximum)
                {
                    value = ZoomMaximum;
                }

                value = Math.Round(value, 2); // round to prevent accumulated rounding errors
                if (Math.Abs(_zoomFactor - value) > 0.01)
                {
                    _zoomFactor = value;
                    UpdateSnappingDistance();
                    Invalidate();
                }
            }
        }

        public const double VerticalZoomMinimum = 0.1;
        public const double VerticalZoomMaximum = 20.0;
        private double _verticalZoomFactor = 1.0; // 1.0=no zoom

        public double VerticalZoomFactor
        {
            get => _verticalZoomFactor;
            set
            {
                if (value < VerticalZoomMinimum)
                {
                    value = VerticalZoomMinimum;
                }

                if (value > VerticalZoomMaximum)
                {
                    value = VerticalZoomMaximum;
                }

                value = Math.Round(value, 2); // round to prevent accumulated rounding errors
                if (Math.Abs(_verticalZoomFactor - value) > 0.01)
                {
                    _verticalZoomFactor = value;
                    Invalidate();
                }
            }
        }

        private List<double> _shotChanges = new List<double>();

        /// <summary>
        /// Shot changes (seconds)
        /// </summary>
        public List<double> ShotChanges
        {
            get => _shotChanges;
            set
            {
                _shotChanges = value;
                Invalidate();
            }
        }

        private MatroskaChapter[] _chapters = Array.Empty<MatroskaChapter>();

        public MatroskaChapter[] Chapters
        {
            get => _chapters;
            set
            {
                _chapters = value;
                Invalidate();
            }
        }

        public bool IsSpectrogramAvailable => _spectrogram != null && _spectrogram.Images.Count > 0;

        private bool _showSpectrogram;

        public bool ShowSpectrogram
        {
            get => _showSpectrogram;
            set
            {
                if (_showSpectrogram != value)
                {
                    _showSpectrogram = value;
                    Invalidate();
                }
            }
        }

        public bool AllowOverlap { get; set; }

        private bool _showWaveform;

        public bool ShowWaveform
        {
            get => _showWaveform;
            set
            {
                if (_showWaveform != value)
                {
                    _showWaveform = value;
                    Invalidate();
                }
            }
        }

        private double _startPositionSeconds;

        public double StartPositionSeconds
        {
            get => _startPositionSeconds;
            set
            {
                if (_wavePeaks != null)
                {
                    var endPositionSeconds = value + ((double)Width / _wavePeaks.SampleRate) / _zoomFactor;
                    if (endPositionSeconds > _wavePeaks.LengthInSeconds)
                    {
                        value -= endPositionSeconds - _wavePeaks.LengthInSeconds;
                    }
                }
                if (value < 0)
                {
                    value = 0;
                }

                if (Math.Abs(_startPositionSeconds - value) > 0.01)
                {
                    _startPositionSeconds = value;
                    Invalidate();
                }
            }
        }

        public Paragraph NewSelectionParagraph { get; set; }
        public Paragraph SelectedParagraph { get; private set; }
        public Paragraph RightClickedParagraph { get; private set; }
        public double RightClickedSeconds { get; private set; }

        public string WaveformNotLoadedText { get; set; }
        public Color BackgroundColor { get; set; }
        public Color Color { get; set; }
        public Color SelectedColor { get; set; }
        public Color ParagraphColor { get; set; }
        public Color TextColor { get; set; }
        public Color CursorColor { get; set; }
        public Color ChaptersColor { get; set; }
        public float TextSize { get; set; }
        public bool TextBold { get; set; }
        public Color GridColor { get; set; }
        public bool ShowGridLines { get; set; }
        public bool AllowNewSelection { get; set; }

        public bool Locked { get; set; }

        public double EndPositionSeconds
        {
            get
            {
                if (_wavePeaks == null)
                {
                    return 0;
                }

                return RelativeXPositionToSeconds(Width);
            }
        }

        public WavePeakData WavePeaks
        {
            get => _wavePeaks;
            set
            {
                _zoomFactor = 1.0;
                SelectedParagraph = null;
                _buttonDownTimeTicks = 0;
                _mouseMoveLastX = -1;
                _mouseMoveStartX = -1;
                _moveWholeStartDifferenceMilliseconds = -1;
                _mouseMoveEndX = -1;
                _mouseDown = false;
                _mouseDownParagraph = null;
                _mouseDownParagraphType = MouseDownParagraphType.Start;
                _currentVideoPositionSeconds = -1;
                _subtitle = new Subtitle();
                _noClear = false;
                _wavePeaks = value;

                UpdateSnappingDistance();
            }
        }

        public void UseSmpteDropFrameTime()
        {
            if (_wavePeaks != null)
            {
                var list = new List<WavePeak>();
                for (var i = 0; i < _wavePeaks.Peaks.Count; i++)
                {
                    if (i % 1001 != 0)
                    {
                        list.Add(_wavePeaks.Peaks[i]);
                    }
                }

                _wavePeaks = new WavePeakData(_wavePeaks.SampleRate, list);

                if (_shotChanges?.Count > 0)
                {
                    _shotChanges = _shotChanges.Select(sc => Math.Round(sc /= 1.001, 3, MidpointRounding.AwayFromZero)).ToList();
                }
            }
        }

        public void SetSpectrogram(SpectrogramData spectrogramData)
        {
            InitializeSpectrogram(spectrogramData);
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
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = UiUtil.GetDefaultFont();
            InitializeComponent();
            UiUtil.FixFonts(this);

            _displayableParagraphs = new List<Paragraph>();
            _allSelectedParagraphs = new List<Paragraph>();

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
            ShowGridLines = true;
            AllowNewSelection = true;
            ShowSpectrogram = true;
            ShowWaveform = true;
            InsertAtVideoPositionShortcut = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainWaveformInsertAtCurrentPosition);

            UpdateSnappingDistance();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            var key = keyData & Keys.KeyCode;

            switch (key)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Right:
                case Keys.Left:
                    return true;

                default:
                    return base.IsInputKey(keyData);
            }
        }

        private void LoadParagraphs(Subtitle subtitle, int primarySelectedIndex, ListView.SelectedIndexCollection selectedIndexes)
        {
            _subtitle.Paragraphs.Clear();
            _displayableParagraphs.Clear();
            SelectedParagraph = null;
            _allSelectedParagraphs.Clear();

            if (_wavePeaks == null)
            {
                return;
            }

            const double additionalSeconds = 15.0; // Helps when scrolling
            var startThresholdMilliseconds = (_startPositionSeconds - additionalSeconds) * TimeCode.BaseUnit;
            var endThresholdMilliseconds = (EndPositionSeconds + additionalSeconds) * TimeCode.BaseUnit;
            var displayableParagraphs = new List<Paragraph>();
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];

                if (p.StartTime.IsMaxTime)
                {
                    continue;
                }

                _subtitle.Paragraphs.Add(p);
                if (p.EndTime.TotalMilliseconds >= startThresholdMilliseconds && p.StartTime.TotalMilliseconds <= endThresholdMilliseconds)
                {
                    displayableParagraphs.Add(p);
                    if (displayableParagraphs.Count > 99)
                    {
                        break;
                    }
                }
            }

            displayableParagraphs = displayableParagraphs.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
            var lastStartTime = -1d;
            foreach (var p in displayableParagraphs)
            {
                if (displayableParagraphs.Count > 30 &&
                    (p.DurationTotalMilliseconds < 0.01 || p.StartTime.TotalMilliseconds - lastStartTime < 90))
                {
                    continue;
                }

                _displayableParagraphs.Add(p);
                lastStartTime = p.StartTime.TotalMilliseconds;
            }

            var primaryParagraph = subtitle.GetParagraphOrDefault(primarySelectedIndex);
            if (primaryParagraph != null && !primaryParagraph.StartTime.IsMaxTime)
            {
                SelectedParagraph = primaryParagraph;
                _allSelectedParagraphs.Add(primaryParagraph);
            }

            foreach (int index in selectedIndexes)
            {
                var p = subtitle.GetParagraphOrDefault(index);
                if (p != null && !p.StartTime.IsMaxTime)
                {
                    _allSelectedParagraphs.Add(p);
                }
            }
        }

        public void SetPosition(double startPositionSeconds, Subtitle subtitle, double currentVideoPositionSeconds, int subtitleIndex, ListView.SelectedIndexCollection selectedIndexes)
        {
            if (TimeSpan.FromTicks(DateTime.UtcNow.Ticks - _lastMouseWheelScroll).TotalSeconds > 0.25)
            { // don't set start position when scrolling with mouse wheel as it will make a bad (jumping back) forward scrolling
                StartPositionSeconds = startPositionSeconds;
            }
            _currentVideoPositionSeconds = currentVideoPositionSeconds;
            LoadParagraphs(subtitle, subtitleIndex, selectedIndexes);
            Invalidate();
        }

        private class IsSelectedHelper
        {
            private readonly SelectionRange[] _ranges;
            private int _lastPosition = int.MaxValue;
            private SelectionRange _nextSelection;

            public IsSelectedHelper(List<Paragraph> paragraphs, int sampleRate)
            {
                var count = paragraphs.Count;
                _ranges = new SelectionRange[count];
                for (var index = 0; index < count; index++)
                {
                    var p = paragraphs[index];
                    var start = (int)Math.Round(p.StartTime.TotalSeconds * sampleRate);
                    var end = (int)Math.Round(p.EndTime.TotalSeconds * sampleRate);
                    _ranges[index] = new SelectionRange(start, end);
                }
            }

            public bool IsSelected(int position)
            {
                if (position < _lastPosition || position > _nextSelection.End)
                {
                    FindNextSelection(position);
                }

                _lastPosition = position;

                return position >= _nextSelection.Start && position <= _nextSelection.End;
            }

            private void FindNextSelection(int position)
            {
                _nextSelection = new SelectionRange(int.MaxValue, int.MaxValue);
                for (var index = 0; index < _ranges.Length; index++)
                {
                    var range = _ranges[index];
                    if (range.End >= position && (range.Start < _nextSelection.Start || range.Start == _nextSelection.Start && range.End > _nextSelection.End))
                    {
                        _nextSelection = range;
                    }
                }
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

        //private Stopwatch _sw;
        //private readonly List<long> _ticks = new List<long>();
        internal void WaveformPaint(object sender, PaintEventArgs e)
        {
            //            _sw = Stopwatch.StartNew();
            var graphics = e.Graphics;
            if (_wavePeaks != null)
            {
                var showSpectrogram = _showSpectrogram && IsSpectrogramAvailable;
                var showSpectrogramOnly = showSpectrogram && !_showWaveform;
                var waveformHeight = Height - (showSpectrogram ? SpectrogramDisplayHeight : 0);

                // background
                graphics.Clear(BackgroundColor);

                // grid lines
                if (ShowGridLines && !showSpectrogramOnly)
                {
                    DrawGridLines(graphics, waveformHeight);
                }

                // spectrogram
                if (showSpectrogram)
                {
                    DrawSpectrogram(graphics);
                }

                // waveform
                if (_showWaveform)
                {
                    using (var penNormal = new Pen(Color))
                    using (var penSelected = new Pen(SelectedColor)) // selected paragraph
                    {
                        var isSelectedHelper = new IsSelectedHelper(_allSelectedParagraphs, _wavePeaks.SampleRate);
                        var baseHeight = (int)(_wavePeaks.HighestPeak / _verticalZoomFactor);
                        var halfWaveformHeight = waveformHeight / 2;
                        Func<double, float> calculateY = value =>
                        {
                            var offset = (value / baseHeight) * halfWaveformHeight;
                            if (offset > halfWaveformHeight)
                            {
                                offset = halfWaveformHeight;
                            }

                            if (offset < -halfWaveformHeight)
                            {
                                offset = -halfWaveformHeight;
                            }

                            return (float)(halfWaveformHeight - offset);
                        };
                        var div = _wavePeaks.SampleRate * _zoomFactor;
                        for (var x = 0; x < Width; x++)
                        {
                            var pos = (_startPositionSeconds + x / div) * _wavePeaks.SampleRate;
                            var pos0 = (int)pos;
                            var pos1 = pos0;
                            pos1++;
                            if (pos1 >= _wavePeaks.Peaks.Count)
                            {
                                break;
                            }

                            var pos1Weight = pos - pos0;
                            var pos0Weight = 1F - pos1Weight;
                            var peak0 = _wavePeaks.Peaks[pos0];
                            var peak1 = _wavePeaks.Peaks[pos1];
                            var max = peak0.Max * pos0Weight + peak1.Max * pos1Weight;
                            var min = peak0.Min * pos0Weight + peak1.Min * pos1Weight;
                            var yMax = calculateY(max);
                            var yMin = Math.Max(calculateY(min), yMax + 0.1F);
                            var pen = isSelectedHelper.IsSelected(pos0) ? penSelected : penNormal;
                            graphics.DrawLine(pen, x, yMax, x, yMin);
                        }
                    }
                }

                // time line
                if (!showSpectrogramOnly)
                {
                    DrawTimeLine(graphics, waveformHeight);
                }

                var currentPositionPos = SecondsToXPosition(_currentVideoPositionSeconds - _startPositionSeconds);
                var currentPosDone = false;

                // paragraphs
                var startPositionMilliseconds = _startPositionSeconds * 1000.0;
                var endPositionMilliseconds = RelativeXPositionToSeconds(Width) * 1000.0;
                var paragraphStartList = new List<int>();
                var paragraphEndList = new List<int>();
                foreach (var p in _displayableParagraphs)
                {
                    if (p.EndTime.TotalMilliseconds >= startPositionMilliseconds && p.StartTime.TotalMilliseconds <= endPositionMilliseconds)
                    {
                        paragraphStartList.Add(SecondsToXPosition(p.StartTime.TotalSeconds - _startPositionSeconds));
                        paragraphEndList.Add(SecondsToXPosition(p.EndTime.TotalSeconds - _startPositionSeconds));
                        DrawParagraph(p, graphics);
                    }
                }

                // shot changes
                if (_shotChanges != null)
                {
                    try
                    {
                        var index = 0;
                        while (index < _shotChanges.Count)
                        {
                            int pos;
                            try
                            {
                                var time = _shotChanges[index++];
                                pos = SecondsToXPosition(time - _startPositionSeconds);
                            }
                            catch
                            {
                                pos = -1;
                            }

                            if (pos > 0 && pos < Width)
                            {
                                if (currentPositionPos == pos)
                                { // shot change and current pos are the same - draw 2 pixels + current pos dotted
                                    currentPosDone = true;
                                    using (var p = new Pen(Color.AntiqueWhite, 2))
                                    {
                                        graphics.DrawLine(p, pos, 0, pos, Height);
                                    }

                                    using (var p = new Pen(CursorColor, 2) { DashStyle = DashStyle.Dash })
                                    {
                                        graphics.DrawLine(p, pos, 0, pos, Height);
                                    }
                                }
                                else if (paragraphStartList.Contains(pos))
                                { // shot change and start pos are the same - draw 2 pixels + current pos dotted
                                    using (var p = new Pen(Color.AntiqueWhite, 2))
                                    {
                                        graphics.DrawLine(p, pos, 0, pos, Height);
                                    }

                                    using (var p = new Pen(Color.FromArgb(175, 0, 100, 0), 2) { DashStyle = DashStyle.Dash })
                                    {
                                        graphics.DrawLine(p, pos, 0, pos, Height);
                                    }
                                }
                                else if (paragraphEndList.Contains(pos))
                                { // shot change and end pos are the same - draw 2 pixels + current pos dotted
                                    using (var p = new Pen(Color.AntiqueWhite, 2))
                                    {
                                        graphics.DrawLine(p, pos, 0, pos, Height);
                                    }

                                    using (var p = new Pen(Color.FromArgb(175, 110, 10, 10), 2) { DashStyle = DashStyle.Dash })
                                    {
                                        graphics.DrawLine(p, pos, 0, pos, Height);
                                    }
                                }
                                else
                                {
                                    using (var p = new Pen(Color.AntiqueWhite))
                                    {
                                        graphics.DrawLine(p, pos, 0, pos, Height);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                // chapters
                if (_chapters != null)
                {
                    try
                    {
                        var index = 0;
                        while (index < _chapters.Length)
                        {
                            int pos;
                            try
                            {
                                var time = _chapters[index].StartTime;
                                pos = SecondsToXPosition(time - _startPositionSeconds);
                            }
                            catch
                            {
                                pos = -1;
                            }
                            if (pos >= 0 && pos < Width)
                            {
                                // draw chapter text
                                using (var font = new Font(Configuration.Settings.General.SubtitleFontName, TextSize, TextBold ? FontStyle.Bold : FontStyle.Regular))
                                using (var brush = new SolidBrush(Color.White))
                                {
                                    string name;
                                    var x = pos + 3;
                                    var y = index + 1 < _chapters.Length && Math.Abs(_chapters[index].StartTime - _chapters[index + 1].StartTime) < 0.01 ? Height / 2 - font.Height - 12 : Height / 2 - 12;
                                    using (var chapterTextBackBrush = new SolidBrush(ChaptersColor))
                                    {
                                        name = _chapters[index].Nested ? "+ " + _chapters[index].Name : _chapters[index].Name;
                                        var textSize = graphics.MeasureString(name, font);
                                        graphics.FillRectangle(chapterTextBackBrush, x, y, textSize.Width + 2, textSize.Height);
                                    }

                                    x += 2;
                                    graphics.DrawString(name, font, brush, new PointF(x, y));
                                }

                                // draw chapter line
                                if (currentPositionPos == pos)
                                { // chapter and current pos are the same - draw 2 pixels + current pos dotted
                                    currentPosDone = true;
                                    using (var p = new Pen(ChaptersColor, 2))
                                    {
                                        graphics.DrawLine(p, pos, 0, pos, Height);
                                    }

                                    using (var p = new Pen(CursorColor, 2) { DashStyle = DashStyle.Dash })
                                    {
                                        graphics.DrawLine(p, pos, 0, pos, Height);
                                    }
                                }
                                else if (paragraphStartList.Contains(pos))
                                { // chapter and start pos are the same - draw 2 pixels + current pos dotted
                                    using (var p = new Pen(ChaptersColor, 2))
                                    {
                                        graphics.DrawLine(p, pos, 0, pos, Height);
                                    }

                                    using (var p = new Pen(Color.FromArgb(175, 0, 100, 0), 2) { DashStyle = DashStyle.Dash })
                                    {
                                        graphics.DrawLine(p, pos, 0, pos, Height);
                                    }
                                }
                                else if (paragraphEndList.Contains(pos))
                                { // chapter and end pos are the same - draw 2 pixels + current pos dotted
                                    using (var p = new Pen(ChaptersColor, 2))
                                    {
                                        graphics.DrawLine(p, pos, 0, pos, Height);
                                    }

                                    using (var p = new Pen(Color.FromArgb(175, 110, 10, 10), 2) { DashStyle = DashStyle.Dash })
                                    {
                                        graphics.DrawLine(p, pos, 0, pos, Height);
                                    }
                                }
                                else
                                {
                                    using (var p = new Pen(ChaptersColor))
                                    {
                                        graphics.DrawLine(p, pos, 0, pos, Height);
                                    }
                                }
                            }

                            index++;
                        }
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }

                // current video position
                if (_currentVideoPositionSeconds > 0 && !currentPosDone && currentPositionPos > 0 && currentPositionPos < Width)
                {
                    using (var p = new Pen(CursorColor))
                    {
                        graphics.DrawLine(p, currentPositionPos, 0, currentPositionPos, Height);
                    }
                }

                // current selection
                if (NewSelectionParagraph != null)
                {
                    var currentRegionLeft = SecondsToXPosition(NewSelectionParagraph.StartTime.TotalSeconds - _startPositionSeconds);
                    var currentRegionRight = SecondsToXPosition(NewSelectionParagraph.EndTime.TotalSeconds - _startPositionSeconds);
                    var currentRegionWidth = currentRegionRight - currentRegionLeft;
                    if (currentRegionRight >= 0 && currentRegionLeft <= Width)
                    {
                        using (var brush = new SolidBrush(Color.FromArgb(128, 255, 255, 255)))
                        {
                            graphics.FillRectangle(brush, currentRegionLeft, 0, currentRegionWidth, graphics.VisibleClipBounds.Height);
                        }

                        if (currentRegionWidth > 40)
                        {
                            using (var brush = new SolidBrush(CursorColor))
                            {
                                graphics.DrawString($"{(double)currentRegionWidth / _wavePeaks.SampleRate / _zoomFactor:0.###} {LanguageSettings.Current.Waveform.Seconds}", Font, brush, new PointF(currentRegionLeft + 3, Height - 32));
                            }
                        }
                    }
                }
            }
            else
            {
                graphics.Clear(BackgroundColor);

                if (ShowGridLines)
                {
                    DrawGridLines(graphics, Height);
                }

                using (var textBrush = new SolidBrush(TextColor))
                using (var textFont = new Font(Font.FontFamily, 8))
                {
                    if (Width > 90)
                    {
                        graphics.DrawString(WaveformNotLoadedText, textFont, textBrush, new PointF(Width / 2.0f - 65, Height / 2.0f - 10));
                    }
                    else
                    {
                        using (var stringFormat = new StringFormat(StringFormatFlags.DirectionVertical))
                        {
                            graphics.DrawString(WaveformNotLoadedText, textFont, textBrush, new PointF(1, 10), stringFormat);
                        }
                    }
                }
            }

            if (Focused)
            {
                using (var p = new Pen(SelectedColor))
                {
                    graphics.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));
                }
            }
            //_sw.Stop();
            //_ticks.Add(_sw.ElapsedMilliseconds);
            //e.Graphics.DrawString("X = " + _ticks.Average().ToString(), Font, new SolidBrush(Color.Cyan), 100, 130);
        }

        private void DrawGridLines(Graphics graphics, int imageHeight)
        {
            if (_wavePeaks == null)
            {
                using (var pen = new Pen(new SolidBrush(GridColor)))
                {
                    for (var i = 0; i < Width; i += 10)
                    {
                        graphics.DrawLine(pen, i, 0, i, imageHeight);
                        graphics.DrawLine(pen, 0, i, Width, i);
                    }
                }
            }
            else
            {
                var seconds = Math.Ceiling(_startPositionSeconds) - _startPositionSeconds - 1;
                var xPosition = SecondsToXPosition(seconds);
                var yPosition = 0;
                var yCounter = 0d;
                var interval = _zoomFactor >= 0.4d ?
                    0.1d : // a pixel is 0.1 second
                    1.0d;  // a pixel is 1.0 second
                using (var pen = new Pen(GridColor))
                {
                    while (xPosition < Width)
                    {
                        graphics.DrawLine(pen, xPosition, 0, xPosition, imageHeight);

                        seconds += interval;
                        xPosition = SecondsToXPosition(seconds);
                    }

                    while (yPosition < Height)
                    {
                        graphics.DrawLine(pen, 0, yPosition, Width, yPosition);

                        yCounter += interval;
                        yPosition = Convert.ToInt32(yCounter * _wavePeaks.SampleRate * _zoomFactor);
                    }
                }
            }
        }

        private void DrawTimeLine(Graphics graphics, int imageHeight)
        {
            var seconds = Math.Ceiling(_startPositionSeconds) - _startPositionSeconds;
            var position = SecondsToXPosition(seconds);
            using (var pen = new Pen(TextColor))
            using (var textBrush = new SolidBrush(TextColor))
            using (var textFont = new Font(Font.FontFamily, 7))
            {
                while (position < Width)
                {
                    var n = _zoomFactor * _wavePeaks.SampleRate;
                    if (n > 38 || (int)Math.Round(_startPositionSeconds + seconds) % 5 == 0)
                    {
                        graphics.DrawLine(pen, position, imageHeight, position, imageHeight - 10);
                        graphics.DrawString(GetDisplayTime(_startPositionSeconds + seconds), textFont, textBrush, new PointF(position + 2, imageHeight - 13));
                    }

                    seconds += 0.5;
                    position = SecondsToXPosition(seconds);

                    if (n > 64)
                    {
                        graphics.DrawLine(pen, position, imageHeight, position, imageHeight - 5);
                    }

                    seconds += 0.5;
                    position = SecondsToXPosition(seconds);
                }
            }
        }

        private static string GetDisplayTime(double seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds + Configuration.Settings.General.CurrentVideoOffsetInMs / TimeCode.BaseUnit);
            if (ts.Minutes == 0 && ts.Hours == 0)
            {
                return ts.Seconds.ToString(CultureInfo.InvariantCulture);
            }

            if (ts.Hours == 0)
            {
                return $"{ts.Minutes:00}:{ts.Seconds:00}";
            }

            return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
        }

        private void DrawParagraph(Paragraph paragraph, Graphics graphics)
        {
            var currentRegionLeft = SecondsToXPosition(paragraph.StartTime.TotalSeconds - _startPositionSeconds);
            var currentRegionRight = SecondsToXPosition(paragraph.EndTime.TotalSeconds - _startPositionSeconds);
            var currentRegionWidth = currentRegionRight - currentRegionLeft;

            // background
            using (var brush = new SolidBrush(Color.FromArgb(42, 255, 255, 255)))
            {
                graphics.FillRectangle(brush, currentRegionLeft, 0, currentRegionWidth, graphics.VisibleClipBounds.Height);
            }

            // left edge
            using (var pen = new Pen(new SolidBrush(Color.FromArgb(175, 0, 100, 0))) { DashStyle = DashStyle.Solid, Width = 2 })
            {
                graphics.DrawLine(pen, currentRegionLeft, 0, currentRegionLeft, graphics.VisibleClipBounds.Height);
            }

            // right edge
            using (var pen = new Pen(new SolidBrush(Color.FromArgb(175, 110, 10, 10))) { DashStyle = DashStyle.Dash, Width = 2 })
            {
                graphics.DrawLine(pen, currentRegionRight - 1, 0, currentRegionRight - 1, graphics.VisibleClipBounds.Height);
            }

            using (var font = new Font(Configuration.Settings.General.SubtitleFontName, TextSize, TextBold ? FontStyle.Bold : FontStyle.Regular))
            using (var textBrush = new SolidBrush(TextColor))
            using (var outlineBrush = new SolidBrush(Color.Black))
            {
                Action<string, int, int> drawStringOutlined = (text, x, y) =>
                {
                    // poor mans outline + text
                    graphics.DrawString(text, font, outlineBrush, new PointF(x, y - 1));
                    graphics.DrawString(text, font, outlineBrush, new PointF(x, y + 1));
                    graphics.DrawString(text, font, outlineBrush, new PointF(x - 1, y));
                    graphics.DrawString(text, font, outlineBrush, new PointF(x + 1, y));
                    graphics.DrawString(text, font, textBrush, new PointF(x, y));
                };

                const int padding = 3;
                var n = _zoomFactor * _wavePeaks.SampleRate;

                // bookmark text
                if (paragraph.Bookmark != null)
                {
                    var x = currentRegionLeft + padding;
                    var y = Height / 2 + (int)graphics.MeasureString("xx", font).Height / 2 + 2;

                    using (var bookmarkBackBrush = new SolidBrush(Color.FromArgb(255, 250, 205)))
                    {
                        var textSize = graphics.MeasureString(paragraph.Bookmark, font);
                        if (textSize.Width < 1)
                        {
                            textSize = new SizeF(-2, 18); // empty bookmark text
                        }

                        graphics.FillRectangle(bookmarkBackBrush, x, y, textSize.Width + 20, textSize.Height + 10);
                    }

                    x += 2;
                    graphics.FillPolygon(textBrush, new[]
                    {
                        new Point(x, y + 2),
                        new Point(x + 14, y + 2),
                        new Point(x + 14, y + 22 + 2),
                        new Point(x + 7, y + 14 + 2),
                        new Point(x, y + 22 + 2),
                        new Point(x, y + 2),
                    });
                    x += 16;
                    graphics.DrawString(paragraph.Bookmark, font, textBrush, new PointF(x, y));
                }

                // paragraph text
                if (n > 20)
                {
                    var text = HtmlUtil.RemoveHtmlTags(paragraph.Text, true);
                    if (Configuration.Settings.VideoControls.WaveformUnwrapText)
                    {
                        text = text.Replace(Environment.NewLine, "  ");
                    }

                    DrawParagraphText(graphics, text, font, currentRegionWidth, padding, drawStringOutlined, currentRegionLeft);
                }

                // paragraph number
                if (n > 15)
                {
                    var text = "#" + paragraph.Number + "  " + paragraph.Duration.ToShortDisplayString();
                    if (n <= 51 || graphics.MeasureString(text, font).Width >= currentRegionWidth - padding - 1)
                    {
                        text = "#" + paragraph.Number;
                    }
                    else if (n > 99)
                    {
                        if (Configuration.Settings.VideoControls.WaveformHideWpmCpsLabels)
                        {
                            if (Configuration.Settings.VideoControls.WaveformDrawWpm)
                            {
                                text = $"{paragraph.WordsPerMinute:0.00}" + Environment.NewLine + text;
                            }

                            if (Configuration.Settings.VideoControls.WaveformDrawCps)
                            {
                                text = $"{Utilities.GetCharactersPerSecond(paragraph):0.00}" + Environment.NewLine + text;
                            }
                        }
                        else
                        {
                            if (Configuration.Settings.VideoControls.WaveformDrawWpm)
                            {
                                text = string.Format(LanguageSettings.Current.Waveform.WordsMinX, paragraph.WordsPerMinute) + Environment.NewLine + text;
                            }

                            if (Configuration.Settings.VideoControls.WaveformDrawCps)
                            {
                                text = string.Format(LanguageSettings.Current.Waveform.CharsSecX, Utilities.GetCharactersPerSecond(paragraph)) + Environment.NewLine + text;
                            }
                        }
                    }
                    drawStringOutlined(text, currentRegionLeft + padding, Height - 14 - (int)graphics.MeasureString(text, font).Height);
                }
            }
        }

        private void DrawParagraphText(Graphics graphics, string text, Font font, int currentRegionWidth, int padding, Action<string, int, int> drawStringOutlined, int currentRegionLeft)
        {
            if (Configuration.Settings.General.RightToLeftMode && LanguageAutoDetect.CouldBeRightToLeftLanguage(new Subtitle(_displayableParagraphs)))
            {
                text = Utilities.ReverseStartAndEndingForRightToLeft(text);
            }

            if (text.Length > 255)
            {
                text = text.Substring(0, 255); // don't now allow very long texts as they can make SE unresponsive - see https://github.com/SubtitleEdit/subtitleedit/issues/2536
            }

            var y = padding;
            var max = currentRegionWidth - padding - 1;
            foreach (var line in text.SplitToLines())
            {
                text = line;
                var removeLength = 1;
                SizeF measureResult;
                try
                {
                    measureResult = graphics.MeasureString(text, font);
                }
                catch
                {
                    // This try/catch is due to some characters crashing MeasureString, see https://github.com/SubtitleEdit/subtitleedit/issues/6108
                    text = "?";
                    measureResult = new SizeF(5, 5);
                }

                while (text.Length > removeLength && graphics.MeasureString(text, font).Width > max)
                {
                    text = text.Remove(text.Length - removeLength).TrimEnd() + "…";
                    if (text.Length > 200)
                    {
                        removeLength = 21;
                    }
                    else if (text.Length > 100)
                    {
                        removeLength = 11;
                    }
                    else
                    {
                        removeLength = 2;
                    }

                    measureResult = graphics.MeasureString(text, font);
                }
                drawStringOutlined(text, currentRegionLeft + padding, y);
                y += (int)Math.Round(measureResult.Height, MidpointRounding.AwayFromZero);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double RelativeXPositionToSeconds(int x)
        {
            return _startPositionSeconds + (double)x / _wavePeaks.SampleRate / _zoomFactor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int SecondsToXPosition(double seconds)
        {
            return (int)Math.Round(seconds * _wavePeaks.SampleRate * _zoomFactor, MidpointRounding.AwayFromZero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int SecondsToSampleIndex(double seconds)
        {
            return (int)Math.Round(seconds * _wavePeaks.SampleRate, MidpointRounding.AwayFromZero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double SampleIndexToSeconds(int index)
        {
            return (double)index / _wavePeaks.SampleRate;
        }

        private void WaveformMouseDown(object sender, MouseEventArgs e)
        {
            if (_wavePeaks == null)
            {
                return;
            }

            Paragraph oldMouseDownParagraph = null;
            _mouseDownParagraphType = MouseDownParagraphType.None;
            _gapAtStart = -1;
            _firstMove = true;
            if (e.Button == MouseButtons.Left)
            {
                _buttonDownTimeTicks = DateTime.UtcNow.Ticks;

                Cursor = Cursors.VSplit;
                double seconds = RelativeXPositionToSeconds(e.X);
                var milliseconds = (int)(seconds * TimeCode.BaseUnit);

                if (SetParagraphBorderHit(milliseconds, NewSelectionParagraph))
                {
                    if (_mouseDownParagraph != null)
                    {
                        oldMouseDownParagraph = new Paragraph(_mouseDownParagraph);
                    }

                    if (_mouseDownParagraphType == MouseDownParagraphType.Start)
                    {
                        if (_mouseDownParagraph != null)
                        {
                            _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                            OnTimeChanged?.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph, _oldParagraph, _mouseDownParagraphType, AllowMovePrevOrNext));
                        }

                        NewSelectionParagraph.StartTime.TotalMilliseconds = milliseconds;
                        _mouseMoveStartX = e.X;
                        _mouseMoveEndX = SecondsToXPosition(NewSelectionParagraph.EndTime.TotalSeconds - _startPositionSeconds);
                    }
                    else
                    {
                        if (_mouseDownParagraph != null)
                        {
                            _mouseDownParagraph.EndTime.TotalMilliseconds = milliseconds;
                            OnTimeChanged?.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph, _oldParagraph, _mouseDownParagraphType, AllowMovePrevOrNext));
                        }

                        NewSelectionParagraph.EndTime.TotalMilliseconds = milliseconds;
                        _mouseMoveStartX = SecondsToXPosition(NewSelectionParagraph.StartTime.TotalSeconds - _startPositionSeconds);
                        _mouseMoveEndX = e.X;
                    }
                    SetMinMaxViaSeconds(seconds);
                }
                else if (SetParagraphBorderHit(milliseconds, SelectedParagraph) || SetParagraphBorderHit(milliseconds, _displayableParagraphs))
                {
                    NewSelectionParagraph = null;
                    if (_mouseDownParagraph != null)
                    {
                        oldMouseDownParagraph = new Paragraph(_mouseDownParagraph);
                        var curIdx = _subtitle.Paragraphs.IndexOf(_mouseDownParagraph);
                        if (_mouseDownParagraphType == MouseDownParagraphType.Start && ModifierKeys != Keys.Alt)
                        {
                            if (curIdx > 0)
                            {
                                var prev = _subtitle.Paragraphs[curIdx - 1];
                                if (prev.EndTime.TotalMilliseconds + Configuration.Settings.General.MinimumMillisecondsBetweenLines < milliseconds)
                                {
                                    _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                                    OnTimeChanged?.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph, _oldParagraph, _mouseDownParagraphType, AllowMovePrevOrNext));
                                }
                            }
                            else
                            {
                                _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                                OnTimeChanged?.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph, _oldParagraph, _mouseDownParagraphType, AllowMovePrevOrNext));
                            }
                        }
                    }
                    SetMinAndMax();
                }
                else
                {
                    var p = GetParagraphAtMilliseconds(milliseconds);
                    if (p != null)
                    {
                        _oldParagraph = new Paragraph(p);
                        _mouseDownParagraph = p;
                        oldMouseDownParagraph = new Paragraph(_mouseDownParagraph);
                        _mouseDownParagraphType = MouseDownParagraphType.Whole;
                        _moveWholeStartDifferenceMilliseconds = (RelativeXPositionToSeconds(e.X) * TimeCode.BaseUnit) - p.StartTime.TotalMilliseconds;
                        Cursor = Cursors.Hand;
                        SetMinAndMax();
                    }
                    else if (!AllowNewSelection)
                    {
                        Cursor = Cursors.Default;
                    }
                    if (p == null)
                    {
                        SetMinMaxViaSeconds(seconds);
                    }

                    NewSelectionParagraph = null;
                    _mouseMoveStartX = e.X;
                    _mouseMoveEndX = e.X;
                }
                if (_mouseDownParagraphType == MouseDownParagraphType.Start)
                {
                    if (_subtitle != null && _mouseDownParagraph != null)
                    {
                        var curIdx = _subtitle.Paragraphs.IndexOf(_mouseDownParagraph);
                        if (curIdx > 0 && oldMouseDownParagraph != null)
                        {
                            _gapAtStart = oldMouseDownParagraph.StartTime.TotalMilliseconds - _subtitle.Paragraphs[curIdx - 1].EndTime.TotalMilliseconds;
                        }
                    }
                }
                else if (_mouseDownParagraphType == MouseDownParagraphType.End)
                {
                    if (_subtitle != null && _mouseDownParagraph != null)
                    {
                        var curIdx = _subtitle.Paragraphs.IndexOf(_mouseDownParagraph);
                        if (curIdx >= 0 && curIdx < _subtitle.Paragraphs.Count - 1 && oldMouseDownParagraph != null)
                        {
                            _gapAtStart = _subtitle.Paragraphs[curIdx + 1].StartTime.TotalMilliseconds - oldMouseDownParagraph.EndTime.TotalMilliseconds;
                        }
                    }
                }
                _mouseDown = true;
            }
            else
            {
                if (e.Button == MouseButtons.Right)
                {
                    var seconds = RelativeXPositionToSeconds(e.X);
                    var milliseconds = (int)(seconds * TimeCode.BaseUnit);

                    if (OnNewSelectionRightClicked != null && NewSelectionParagraph != null)
                    {
                        OnNewSelectionRightClicked.Invoke(this, new ParagraphEventArgs(NewSelectionParagraph));
                        RightClickedParagraph = null;
                        _noClear = true;
                    }
                    else
                    {
                        var p = GetParagraphAtMilliseconds(milliseconds);
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
                            OnNonParagraphRightClicked?.Invoke(this, new ParagraphEventArgs(seconds, null));
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
                var paragraphs = _subtitle.Paragraphs.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
                for (var i = 0; i < paragraphs.Count; i++)
                {
                    var p2 = paragraphs[i];
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
                {
                    _wholeParagraphMinMilliseconds = prev.EndTime.TotalMilliseconds + Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                }

                if (next != null)
                {
                    _wholeParagraphMaxMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                }
            }
        }

        private void SetMinAndMax()
        {
            _wholeParagraphMinMilliseconds = 0;
            _wholeParagraphMaxMilliseconds = double.MaxValue;
            if (_subtitle != null && _mouseDownParagraph != null)
            {
                var paragraphs = _subtitle.Paragraphs.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
                var curIdx = paragraphs.IndexOf(_mouseDownParagraph);
                if (curIdx >= 0)
                {
                    if (curIdx > 0)
                    {
                        _wholeParagraphMinMilliseconds = paragraphs[curIdx - 1].EndTime.TotalMilliseconds + Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }

                    if (curIdx < _subtitle.Paragraphs.Count - 1)
                    {
                        _wholeParagraphMaxMilliseconds = paragraphs[curIdx + 1].StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }
                }
            }
        }

        /// <summary>
        /// Set limits for move paragraph(s).
        /// </summary>
        public void SetMinAndMax(double minMoveMs, double maxMoveMs)
        {
            _wholeParagraphMinMilliseconds = minMoveMs;
            _wholeParagraphMaxMilliseconds = maxMoveMs;
        }

        private void SetMinAndMaxMoveStart()
        {
            _wholeParagraphMinMilliseconds = 0;
            _wholeParagraphMaxMilliseconds = double.MaxValue;
            if (_subtitle != null && _mouseDownParagraph != null)
            {
                var paragraphs = _subtitle.Paragraphs.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
                var curIdx = paragraphs.IndexOf(_mouseDownParagraph);
                if (curIdx >= 0)
                {
                    var gap = Math.Abs(paragraphs[curIdx - 1].EndTime.TotalMilliseconds - paragraphs[curIdx].StartTime.TotalMilliseconds);
                    _wholeParagraphMinMilliseconds = paragraphs[curIdx - 1].StartTime.TotalMilliseconds + gap + 200;
                }
            }
        }

        private void SetMinAndMaxMoveEnd()
        {
            _wholeParagraphMinMilliseconds = 0;
            _wholeParagraphMaxMilliseconds = double.MaxValue;
            if (_subtitle != null && _mouseDownParagraph != null)
            {
                var paragraphs = _subtitle.Paragraphs.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
                var curIdx = paragraphs.IndexOf(_mouseDownParagraph);
                if (curIdx >= 0)
                {
                    if (curIdx < _subtitle.Paragraphs.Count - 1)
                    {
                        var gap = Math.Abs(paragraphs[curIdx].EndTime.TotalMilliseconds - paragraphs[curIdx + 1].StartTime.TotalMilliseconds);
                        _wholeParagraphMaxMilliseconds = paragraphs[curIdx + 1].EndTime.TotalMilliseconds - gap - 200;
                    }
                }
            }
        }

        private bool SetParagraphBorderHit(int milliseconds, List<Paragraph> paragraphs)
        {
            foreach (var p in paragraphs)
            {
                var hit = SetParagraphBorderHit(milliseconds, p);
                if (hit)
                {
                    return true;
                }
            }
            return false;
        }

        private Paragraph GetParagraphAtMilliseconds(int milliseconds)
        {
            Paragraph p = null;
            if (IsParagraphHit(milliseconds, SelectedParagraph))
            {
                p = SelectedParagraph;
            }

            if (p == null)
            {
                foreach (var pNext in _displayableParagraphs)
                {
                    if (IsParagraphHit(milliseconds, pNext))
                    {
                        p = pNext;
                        break;
                    }
                }
            }

            return p;
        }

        private bool SetParagraphBorderHit(int milliseconds, Paragraph paragraph)
        {
            if (paragraph == null)
            {
                return false;
            }

            if (IsParagraphBorderStartHit(milliseconds, paragraph.StartTime.TotalMilliseconds))
            {
                var idx = _displayableParagraphs.IndexOf(paragraph);
                if (idx > 0)
                {
                    var prev = _displayableParagraphs[idx - 1];
                    if (IsParagraphBorderStartHit(milliseconds, prev.EndTime.TotalMilliseconds) && ModifierKeys != Keys.Alt)
                    {
                        _mouseDownParagraph = null;
                        _mouseDownParagraphs = new List<Paragraph> { prev, paragraph }.ToArray();
                        _mouseDownParagraphType = MouseDownParagraphType.StartOrEnd;
                        return true;
                    }
                }

                _oldParagraph = new Paragraph(paragraph);
                _mouseDownParagraph = paragraph;
                _mouseDownParagraphs = null;
                _mouseDownParagraphType = MouseDownParagraphType.Start;
                return true;
            }

            if (IsParagraphBorderEndHit(milliseconds, paragraph.EndTime.TotalMilliseconds))
            {
                var idx = _displayableParagraphs.IndexOf(paragraph);
                if (idx < _displayableParagraphs.Count - 2 && ModifierKeys != Keys.Alt)
                {
                    var next = _displayableParagraphs[idx + 1];
                    if (IsParagraphBorderStartHit(milliseconds, next.StartTime.TotalMilliseconds))
                    {
                        _mouseDownParagraph = null;
                        _mouseDownParagraphs = new List<Paragraph> { paragraph, next }.ToArray();
                        _mouseDownParagraphType = MouseDownParagraphType.StartOrEnd;
                        return true;
                    }
                }

                _oldParagraph = new Paragraph(paragraph);
                _mouseDownParagraph = paragraph;
                _mouseDownParagraphs = null;
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
                {
                    return AllowOverlap;
                }

                return !AllowOverlap;
            }
        }

        private bool AllowMovePrevOrNext => _gapAtStart >= 0 && _gapAtStart < 500 && ModifierKeys == Keys.Alt;

        private void WaveformMouseMove(object sender, MouseEventArgs e)
        {
            if (_wavePeaks == null)
            {
                return;
            }

            var oldMouseMoveLastX = _mouseMoveLastX;
            if (e.X < 0 && _startPositionSeconds > 0.1 && _mouseDown)
            {
                if (e.X < _mouseMoveLastX)
                {
                    StartPositionSeconds -= 0.1;
                    if (_mouseDownParagraph == null && _mouseDownParagraphs == null)
                    {
                        _mouseMoveEndX = 0;
                        _mouseMoveStartX += (int)(_wavePeaks.SampleRate * 0.1);
                        OnPositionSelected?.Invoke(this, new ParagraphEventArgs(_startPositionSeconds, null));
                    }
                }
                _mouseMoveLastX = e.X;
                Invalidate();
                return;
            }
            if (e.X > Width && _startPositionSeconds + 0.1 < _wavePeaks.LengthInSeconds && _mouseDown)
            {
                StartPositionSeconds += 0.1;
                if (_mouseDownParagraph == null && _mouseDownParagraphs == null)
                {
                    _mouseMoveEndX = Width;
                    _mouseMoveStartX -= (int)(_wavePeaks.SampleRate * 0.1);
                    OnPositionSelected?.Invoke(this, new ParagraphEventArgs(_startPositionSeconds, null));
                }
                _mouseMoveLastX = e.X;
                Invalidate();
                return;
            }
            _mouseMoveLastX = e.X;

            if (e.X < 0 || e.X > Width)
            {
                return;
            }

            if (e.Button == MouseButtons.None)
            {
                var seconds = RelativeXPositionToSeconds(e.X);
                var milliseconds = (int)(seconds * TimeCode.BaseUnit);

                if (IsParagraphBorderHit(milliseconds, NewSelectionParagraph))
                {
                    Cursor = Cursors.VSplit;
                }
                else if (IsParagraphBorderHit(milliseconds, SelectedParagraph) ||
                         IsParagraphBorderHit(milliseconds, _displayableParagraphs))
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
                {
                    return; // no horizontal movement
                }

                if (_mouseDown)
                {
                    if (_mouseDownParagraphType == MouseDownParagraphType.StartOrEnd && _firstMove && ModifierKeys != Keys.Alt)
                    {
                        var seconds = RelativeXPositionToSeconds(e.X);
                        var milliseconds = (int)(seconds * TimeCode.BaseUnit);

                        if (_firstMove && Math.Abs(oldMouseMoveLastX - e.X) < Configuration.Settings.General.MinimumMillisecondsBetweenLines && GetParagraphAtMilliseconds(milliseconds) == null)
                        {
                            if (_mouseDownParagraphType == MouseDownParagraphType.StartOrEnd && _mouseDownParagraphs?.Length == 2 && Math.Abs(_mouseDownParagraphs[0].StartTime.TotalMilliseconds - _mouseDownParagraphs[0].EndTime.TotalMilliseconds) <= ClosenessForBorderSelection + 15)
                            {
                                return; // do not decide which paragraph to move yet
                            }

                            if (_mouseDownParagraphType == MouseDownParagraphType.StartOrEnd && _mouseDownParagraphs?.Length == 2 && Math.Abs(_mouseDownParagraphs[1].EndTime.TotalMilliseconds - _mouseDownParagraphs[1].StartTime.TotalMilliseconds) <= ClosenessForBorderSelection + 15)
                            {
                                return; // do not decide which paragraph to move yet
                            }
                        }

                        if (_mouseDownParagraphs?.Length == 2)
                        {
                            // decide which paragraph to move
                            if (_firstMove && e.X > oldMouseMoveLastX)
                            {
                                if (milliseconds >= _mouseDownParagraphs[1].StartTime.TotalMilliseconds && milliseconds < _mouseDownParagraphs[1].EndTime.TotalMilliseconds)
                                {
                                    _mouseDownParagraph = _mouseDownParagraphs[1];
                                    _mouseDownParagraphType = MouseDownParagraphType.Start;
                                    _mouseDownParagraphs = null;
                                    _oldParagraph = new Paragraph(_mouseDownParagraph);
                                    _firstMove = false;
                                }
                            }
                            else if (_firstMove && e.X < oldMouseMoveLastX)
                            {
                                if (milliseconds <= _mouseDownParagraphs[0].EndTime.TotalMilliseconds && milliseconds > _mouseDownParagraphs[0].StartTime.TotalMilliseconds)
                                {
                                    _mouseDownParagraph = _mouseDownParagraphs[0];
                                    _mouseDownParagraphType = MouseDownParagraphType.End;
                                    _mouseDownParagraphs = null;
                                    _oldParagraph = new Paragraph(_mouseDownParagraph);
                                    _firstMove = false;
                                }
                            }
                            return;
                        }
                    }

                    if (_mouseDownParagraph != null)
                    {
                        var seconds = RelativeXPositionToSeconds(e.X);
                        var milliseconds = (int)(seconds * TimeCode.BaseUnit);
                        var subtitleIndex = _subtitle.Paragraphs.IndexOf(_mouseDownParagraph);
                        _prevParagraph = _subtitle.GetParagraphOrDefault(subtitleIndex - 1);
                        _nextParagraph = _subtitle.GetParagraphOrDefault(subtitleIndex + 1);

                        if (_firstMove && Math.Abs(oldMouseMoveLastX - e.X) < Configuration.Settings.General.MinimumMillisecondsBetweenLines && GetParagraphAtMilliseconds(milliseconds) == null)
                        {
                            if (_mouseDownParagraphType == MouseDownParagraphType.StartOrEnd && _prevParagraph != null && Math.Abs(_mouseDownParagraph.StartTime.TotalMilliseconds - _prevParagraph.EndTime.TotalMilliseconds) <= ClosenessForBorderSelection + 15)
                            {
                                return; // do not decide which paragraph to move yet
                            }

                            if (_mouseDownParagraphType == MouseDownParagraphType.StartOrEnd && _nextParagraph != null && Math.Abs(_mouseDownParagraph.EndTime.TotalMilliseconds - _nextParagraph.StartTime.TotalMilliseconds) <= ClosenessForBorderSelection + 15)
                            {
                                return; // do not decide which paragraph to move yet
                            }
                        }

                        if (_firstMove && ModifierKeys != Keys.Alt && ModifierKeys != Keys.Shift &&
                            !Configuration.Settings.VideoControls.WaveformAllowOverlap)
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
                                {
                                    SetMinAndMaxMoveStart();
                                }
                                else
                                {
                                    SetMinAndMax();
                                }

                                _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;

                                if (Configuration.Settings.VideoControls.WaveformSnapToShotChanges && ModifierKeys != Keys.Shift &&
                                    _shotChanges?.Count > 0)
                                {
                                    var nearestShotChange = ShotChangeHelper.GetClosestShotChange(_shotChanges, new TimeCode(milliseconds));
                                    if (nearestShotChange != null && Math.Abs(e.X - SecondsToXPosition(nearestShotChange.Value - _startPositionSeconds)) < ShotChangeSnapPixels)
                                    {
                                        _mouseDownParagraph.StartTime.TotalMilliseconds = (nearestShotChange.Value * 1000) + TimeCodesBeautifierUtils.GetInCuesGapMs();
                                    }
                                }

                                if (PreventOverlap && _mouseDownParagraph.StartTime.TotalMilliseconds <= _wholeParagraphMinMilliseconds)
                                {
                                    _mouseDownParagraph.StartTime.TotalMilliseconds = _wholeParagraphMinMilliseconds + 1;
                                }

                                if (NewSelectionParagraph != null)
                                {
                                    NewSelectionParagraph.StartTime.TotalMilliseconds = milliseconds;

                                    if (Configuration.Settings.VideoControls.WaveformSnapToShotChanges && ModifierKeys != Keys.Shift)
                                    {
                                        var nearestShotChange = ShotChangeHelper.GetClosestShotChange(_shotChanges, new TimeCode(milliseconds));
                                        if (nearestShotChange != null && Math.Abs(e.X - SecondsToXPosition(nearestShotChange.Value - _startPositionSeconds)) < ShotChangeSnapPixels)
                                        {
                                            NewSelectionParagraph.StartTime.TotalMilliseconds = (nearestShotChange.Value * 1000) + TimeCodesBeautifierUtils.GetInCuesGapMs();
                                        }
                                    }

                                    if (PreventOverlap && NewSelectionParagraph.StartTime.TotalMilliseconds <= _wholeParagraphMinMilliseconds)
                                    {
                                        NewSelectionParagraph.StartTime.TotalMilliseconds = _wholeParagraphMinMilliseconds + 1;
                                    }

                                    _mouseMoveStartX = e.X;
                                }
                                else
                                {
                                    OnTimeChanged?.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph, _oldParagraph, _mouseDownParagraphType, AllowMovePrevOrNext));
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
                                {
                                    SetMinAndMaxMoveEnd();
                                }
                                else
                                {
                                    SetMinAndMax();
                                }

                                _mouseDownParagraph.EndTime.TotalMilliseconds = milliseconds;

                                if (Configuration.Settings.VideoControls.WaveformSnapToShotChanges && ModifierKeys != Keys.Shift)
                                {
                                    var nearestShotChange = ShotChangeHelper.GetClosestShotChange(_shotChanges, new TimeCode(milliseconds));
                                    if (nearestShotChange != null && Math.Abs(e.X - SecondsToXPosition(nearestShotChange.Value - _startPositionSeconds)) < ShotChangeSnapPixels)
                                    {
                                        _mouseDownParagraph.EndTime.TotalMilliseconds = (nearestShotChange.Value * 1000) - TimeCodesBeautifierUtils.GetOutCuesGapMs();
                                    }
                                }

                                if (PreventOverlap && _mouseDownParagraph.EndTime.TotalMilliseconds >= _wholeParagraphMaxMilliseconds)
                                {
                                    _mouseDownParagraph.EndTime.TotalMilliseconds = _wholeParagraphMaxMilliseconds - 1;
                                }

                                if (NewSelectionParagraph != null)
                                {
                                    NewSelectionParagraph.EndTime.TotalMilliseconds = milliseconds;

                                    if (Configuration.Settings.VideoControls.WaveformSnapToShotChanges && ModifierKeys != Keys.Shift)
                                    {
                                        var nearestShotChange = ShotChangeHelper.GetClosestShotChange(_shotChanges, new TimeCode(milliseconds));
                                        if (nearestShotChange != null && Math.Abs(e.X - SecondsToXPosition(nearestShotChange.Value - _startPositionSeconds)) < ShotChangeSnapPixels)
                                        {
                                            NewSelectionParagraph.EndTime.TotalMilliseconds = (nearestShotChange.Value * 1000) - TimeCodesBeautifierUtils.GetOutCuesGapMs();
                                        }
                                    }

                                    if (PreventOverlap && NewSelectionParagraph.EndTime.TotalMilliseconds >= _wholeParagraphMaxMilliseconds)
                                    {
                                        NewSelectionParagraph.EndTime.TotalMilliseconds = _wholeParagraphMaxMilliseconds - 1;
                                    }

                                    _mouseMoveEndX = e.X;
                                }
                                else
                                {
                                    OnTimeChanged?.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph, _oldParagraph, _mouseDownParagraphType, AllowMovePrevOrNext));
                                    Refresh();
                                    return;
                                }
                            }
                        }
                        else if (_mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            var durationMilliseconds = _mouseDownParagraph.DurationTotalMilliseconds;
                            var oldStart = _mouseDownParagraph.StartTime.TotalMilliseconds;
                            _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds - _moveWholeStartDifferenceMilliseconds;
                            _mouseDownParagraph.EndTime.TotalMilliseconds = _mouseDownParagraph.StartTime.TotalMilliseconds + durationMilliseconds;

                            if (Configuration.Settings.VideoControls.WaveformSnapToShotChanges && ModifierKeys != Keys.Shift)
                            {
                                var nearestShotChangeInFront = ShotChangeHelper.GetClosestShotChange(_shotChanges, _mouseDownParagraph.StartTime);
                                var nearestShotChangeInBack = ShotChangeHelper.GetClosestShotChange(_shotChanges, _mouseDownParagraph.EndTime);

                                if (nearestShotChangeInFront != null && Math.Abs(SecondsToXPosition(_mouseDownParagraph.StartTime.TotalSeconds - _startPositionSeconds) - SecondsToXPosition(nearestShotChangeInFront.Value - _startPositionSeconds)) < ShotChangeSnapPixels)
                                {
                                    var nearestShotChangeInFrontMs = (nearestShotChangeInFront.Value * 1000) + TimeCodesBeautifierUtils.GetInCuesGapMs();
                                    _mouseDownParagraph.StartTime.TotalMilliseconds = nearestShotChangeInFrontMs;
                                    _mouseDownParagraph.EndTime.TotalMilliseconds = nearestShotChangeInFrontMs + durationMilliseconds;
                                }
                                else if (nearestShotChangeInBack != null && Math.Abs(SecondsToXPosition(_mouseDownParagraph.EndTime.TotalSeconds - _startPositionSeconds) - SecondsToXPosition(nearestShotChangeInBack.Value - _startPositionSeconds)) < ShotChangeSnapPixels)
                                {
                                    var nearestShotChangeInBackMs = (nearestShotChangeInBack.Value * 1000) - TimeCodesBeautifierUtils.GetOutCuesGapMs();
                                    _mouseDownParagraph.EndTime.TotalMilliseconds = nearestShotChangeInBackMs;
                                    _mouseDownParagraph.StartTime.TotalMilliseconds = nearestShotChangeInBackMs - durationMilliseconds;
                                }
                            }

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

                            if (PreventOverlap &&
                                (_mouseDownParagraph.StartTime.TotalMilliseconds <= _wholeParagraphMinMilliseconds ||
                                 _mouseDownParagraph.EndTime.TotalMilliseconds >= _wholeParagraphMaxMilliseconds))
                            {
                                _mouseDownParagraph.StartTime.TotalMilliseconds = oldStart;
                                _mouseDownParagraph.EndTime.TotalMilliseconds = oldStart + durationMilliseconds;
                                return;
                            }

                            OnTimeChanged?.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph, _oldParagraph, _mouseDownParagraphType) { AdjustMs = _mouseDownParagraph.StartTime.TotalMilliseconds - oldStart });
                        }
                    }
                    else
                    {
                        _mouseMoveEndX = e.X;
                        if (NewSelectionParagraph == null && Math.Abs(_mouseMoveEndX - _mouseMoveStartX) > 2)
                        {
                            if (AllowNewSelection)
                            {
                                NewSelectionParagraph = new Paragraph();
                            }
                        }

                        if (NewSelectionParagraph != null)
                        {
                            var start = Math.Min(_mouseMoveStartX, _mouseMoveEndX);
                            var end = Math.Max(_mouseMoveStartX, _mouseMoveEndX);

                            var startTotalSeconds = RelativeXPositionToSeconds(start);
                            var endTotalSeconds = RelativeXPositionToSeconds(end);

                            NewSelectionParagraph.StartTime.TotalSeconds = startTotalSeconds;
                            NewSelectionParagraph.EndTime.TotalSeconds = endTotalSeconds;

                            if (Configuration.Settings.VideoControls.WaveformSnapToShotChanges && ModifierKeys != Keys.Shift)
                            {
                                var nearestShotChangeInFront = ShotChangeHelper.GetClosestShotChange(_shotChanges, TimeCode.FromSeconds(startTotalSeconds));
                                var nearestShotChangeInBack = ShotChangeHelper.GetClosestShotChange(_shotChanges, TimeCode.FromSeconds(endTotalSeconds));

                                if (nearestShotChangeInFront != null && Math.Abs(SecondsToXPosition(NewSelectionParagraph.StartTime.TotalSeconds - _startPositionSeconds) - SecondsToXPosition(nearestShotChangeInFront.Value - _startPositionSeconds)) < ShotChangeSnapPixels)
                                {
                                    NewSelectionParagraph.StartTime.TotalMilliseconds = (nearestShotChangeInFront.Value * 1000) + TimeCodesBeautifierUtils.GetInCuesGapMs();
                                    Invalidate();
                                }
                                if (nearestShotChangeInBack != null && Math.Abs(SecondsToXPosition(NewSelectionParagraph.EndTime.TotalSeconds - _startPositionSeconds) - SecondsToXPosition(nearestShotChangeInBack.Value - _startPositionSeconds)) < ShotChangeSnapPixels)
                                {
                                    NewSelectionParagraph.EndTime.TotalMilliseconds = (nearestShotChangeInBack.Value * 1000) - TimeCodesBeautifierUtils.GetOutCuesGapMs();
                                    Invalidate();
                                }
                            }

                            if (PreventOverlap && endTotalSeconds * TimeCode.BaseUnit >= _wholeParagraphMaxMilliseconds)
                            {
                                NewSelectionParagraph.EndTime.TotalMilliseconds = _wholeParagraphMaxMilliseconds - 1;
                                Invalidate();
                            }
                            if (PreventOverlap && startTotalSeconds * TimeCode.BaseUnit <= _wholeParagraphMinMilliseconds)
                            {
                                NewSelectionParagraph.StartTime.TotalMilliseconds = _wholeParagraphMinMilliseconds + 1;
                                Invalidate();
                            }
                        }
                    }
                    Invalidate();
                }
            }
        }

        private bool IsParagraphBorderHit(int milliseconds, List<Paragraph> paragraphs)
        {
            foreach (var p in paragraphs)
            {
                var hit = IsParagraphBorderHit(milliseconds, p);
                if (hit)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsParagraphBorderHit(int milliseconds, Paragraph paragraph)
        {
            if (paragraph == null)
            {
                return false;
            }

            return IsParagraphBorderStartHit(milliseconds, paragraph.StartTime.TotalMilliseconds) ||
                   IsParagraphBorderEndHit(milliseconds, paragraph.EndTime.TotalMilliseconds);
        }

        private bool IsParagraphBorderStartHit(double milliseconds, double startMs)
        {
            return Math.Abs(milliseconds - (startMs - 5)) - 10 <= ClosenessForBorderSelection / ZoomFactor;
        }

        private bool IsParagraphBorderEndHit(double milliseconds, double endMs)
        {
            return Math.Abs(milliseconds - (endMs - 22)) - 7 <= ClosenessForBorderSelection / ZoomFactor;
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
            _mouseDownParagraph = null;
            _mouseDownParagraphs = null;
        }

        private void WaveformMouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
            _mouseDown = false;
            Invalidate();
        }

        private void WaveformMouseEnter(object sender, EventArgs e)
        {
            if (_wavePeaks == null)
            {
                return;
            }

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
                _mouseMoveStartX = SecondsToXPosition(NewSelectionParagraph.StartTime.TotalSeconds - _startPositionSeconds);
                _mouseMoveEndX = SecondsToXPosition(NewSelectionParagraph.EndTime.TotalSeconds - _startPositionSeconds);
            }
        }

        private void WaveformMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_wavePeaks == null)
            {
                return;
            }

            _mouseDown = false;
            _mouseDownParagraph = null;
            _mouseMoveStartX = -1;
            _mouseMoveEndX = -1;

            if (e.Button == MouseButtons.Left)
            {
                OnPause?.Invoke(sender, null);
                var seconds = RelativeXPositionToSeconds(e.X);
                var milliseconds = (int)(seconds * TimeCode.BaseUnit);

                var p = GetParagraphAtMilliseconds(milliseconds);
                if (p != null)
                {
                    seconds = p.StartTime.TotalSeconds;
                    var endSeconds = p.EndTime.TotalSeconds;
                    if (seconds < _startPositionSeconds)
                    {
                        _startPositionSeconds = (p.StartTime.TotalSeconds) + 0.1; // move earlier - show whole selected paragraph
                    }
                    else if (endSeconds > EndPositionSeconds)
                    {
                        var newStartPos = _startPositionSeconds + (endSeconds - EndPositionSeconds); // move later, so whole selected paragraph is visible
                        if (newStartPos < seconds) // but only if visible screen is wide enough
                        {
                            _startPositionSeconds = newStartPos;
                        }
                    }
                }

                OnDoubleClickNonParagraph?.Invoke(this, new ParagraphEventArgs(seconds, p));
            }
        }

        private static bool IsParagraphHit(int milliseconds, Paragraph paragraph)
        {
            if (paragraph == null)
            {
                return false;
            }

            return milliseconds >= paragraph.StartTime.TotalMilliseconds && milliseconds <= paragraph.EndTime.TotalMilliseconds;
        }

        private void WaveformMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && OnSingleClick != null)
            {
                var diff = Math.Abs(_mouseMoveStartX - e.X);
                if (_mouseMoveStartX == -1 || _mouseMoveEndX == -1 || diff < 10 && TimeSpan.FromTicks(DateTime.UtcNow.Ticks - _buttonDownTimeTicks).TotalSeconds < 0.25)
                {
                    if (ModifierKeys == Keys.Shift && SelectedParagraph != null)
                    {
                        var seconds = RelativeXPositionToSeconds(e.X);
                        var milliseconds = (int)(seconds * TimeCode.BaseUnit);
                        if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            if (seconds < SelectedParagraph.EndTime.TotalSeconds)
                            {
                                _oldParagraph = new Paragraph(SelectedParagraph);
                                _mouseDownParagraph = SelectedParagraph;
                                _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                                OnStartTimeChanged?.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph, _oldParagraph));
                            }
                        }
                        return;
                    }
                    if (ModifierKeys == Keys.Control && SelectedParagraph != null)
                    {
                        var seconds = RelativeXPositionToSeconds(e.X);
                        var milliseconds = (int)(seconds * TimeCode.BaseUnit);
                        if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            if (seconds > SelectedParagraph.StartTime.TotalSeconds)
                            {
                                _oldParagraph = new Paragraph(SelectedParagraph);
                                _mouseDownParagraph = SelectedParagraph;
                                _mouseDownParagraph.EndTime.TotalMilliseconds = milliseconds;
                                OnTimeChanged?.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph, _oldParagraph));
                            }
                        }
                        return;
                    }
                    if (ModifierKeys == (Keys.Control | Keys.Shift) && SelectedParagraph != null)
                    {
                        var seconds = RelativeXPositionToSeconds(e.X);
                        if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            _oldParagraph = new Paragraph(SelectedParagraph);
                            _mouseDownParagraph = SelectedParagraph;
                            OnTimeChangedAndOffsetRest?.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph));
                        }
                        return;
                    }
                    if (ModifierKeys == Keys.Alt && SelectedParagraph != null)
                    {
                        var seconds = RelativeXPositionToSeconds(e.X);
                        var milliseconds = (int)(seconds * TimeCode.BaseUnit);
                        if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            _oldParagraph = new Paragraph(SelectedParagraph);
                            _mouseDownParagraph = SelectedParagraph;
                            var durationMilliseconds = _mouseDownParagraph.DurationTotalMilliseconds;
                            _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                            _mouseDownParagraph.EndTime.TotalMilliseconds = _mouseDownParagraph.StartTime.TotalMilliseconds + durationMilliseconds;
                            OnTimeChanged?.Invoke(this, new ParagraphEventArgs(seconds, _mouseDownParagraph, _oldParagraph));
                        }
                        return;
                    }

                    if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                    {
                        var seconds = RelativeXPositionToSeconds(e.X);
                        var milliseconds = (int)(seconds * TimeCode.BaseUnit);
                        var p = GetParagraphAtMilliseconds(milliseconds);
                        OnSingleClick?.Invoke(this, new ParagraphEventArgs(RelativeXPositionToSeconds(e.X), p));
                    }
                }
            }
        }

        private void WaveformKeyDown(object sender, KeyEventArgs e)
        {
            if (_wavePeaks == null)
            {
                return;
            }

            if (e.Modifiers == Keys.None && e.KeyCode == Keys.Add)
            {
                ZoomIn();
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Subtract)
            {
                ZoomOut();
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.D0)
            {
                ZoomFactor = 1.0;
                OnZoomedChanged?.Invoke(this, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Z)
            {
                if (_startPositionSeconds > 0.1)
                {
                    StartPositionSeconds -= 0.1;
                    OnPositionSelected?.Invoke(this, new ParagraphEventArgs(_startPositionSeconds, null));
                    Invalidate();
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.X)
            {
                if (_startPositionSeconds + 0.1 < _wavePeaks.LengthInSeconds)
                {
                    StartPositionSeconds += 0.1;
                    OnPositionSelected?.Invoke(this, new ParagraphEventArgs(_startPositionSeconds, null));
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
                OnInsertAtVideoPosition?.Invoke(this, null);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control) //Ctrl+v = Paste from clipboard
            {
                OnPasteAtVideoPosition?.Invoke(this, null);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control) //Ctrl+a
            {
                OnSelectAll?.Invoke(this, null);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Move100MsLeft)
            {
                var pos = Math.Max(0, _currentVideoPositionSeconds - 0.1);
                OnPositionSelected?.Invoke(this, new ParagraphEventArgs(pos, null));
                Invalidate();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Move100MsRight)
            {
                var pos = _currentVideoPositionSeconds + 0.1;
                OnPositionSelected?.Invoke(this, new ParagraphEventArgs(pos, null));
                Invalidate();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == MoveOneSecondLeft)
            {
                var pos = Math.Max(0, _currentVideoPositionSeconds - 1);
                OnPositionSelected?.Invoke(this, new ParagraphEventArgs(pos, null));
                Invalidate();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == MoveOneSecondRight)
            {
                var pos = _currentVideoPositionSeconds + 1;
                OnPositionSelected?.Invoke(this, new ParagraphEventArgs(pos, null));
                Invalidate();
                e.SuppressKeyPress = true;
            }
        }

        public void ZoomIn()
        {
            ZoomFactor += 0.1;
            OnZoomedChanged?.Invoke(this, null);
        }

        public void ZoomOut()
        {
            ZoomFactor -= 0.1;
            OnZoomedChanged?.Invoke(this, null);
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
            {
                return;
            }

            if (ModifierKeys == Keys.Control)
            {
                if (e.Delta > 0)
                {
                    ZoomIn();
                }
                else
                {
                    ZoomOut();
                }

                return;
            }

            if (ModifierKeys == (Keys.Control | Keys.Shift))
            {
                if (e.Delta > 0)
                {
                    VerticalZoomIn();
                }
                else
                {
                    VerticalZoomOut();
                }

                return;
            }

            var delta = e.Delta;
            if (!MouseWheelScrollUpIsForward)
            {
                delta *= -1;
            }

            if (Locked)
            {
                _currentVideoPositionSeconds += delta / 256.0;
                OnPositionSelected?.Invoke(this, new ParagraphEventArgs(_currentVideoPositionSeconds, null));
            }
            else
            {
                StartPositionSeconds += delta / 256.0;
                _lastMouseWheelScroll = DateTime.UtcNow.Ticks;
                if (_currentVideoPositionSeconds < _startPositionSeconds || _currentVideoPositionSeconds >= EndPositionSeconds)
                {
                    OnPositionSelected?.Invoke(this, new ParagraphEventArgs(_startPositionSeconds, null));
                }
            }
        }

        /////////////////////////////////////////////////

        private void InitializeSpectrogram(SpectrogramData spectrogram)
        {
            if (_spectrogram != null)
            {
                _spectrogram.Dispose();
                _spectrogram = null;
                Invalidate();
            }

            if (spectrogram == null)
            {
                return;
            }

            if (spectrogram.IsLoaded)
            {
                InitializeSpectrogramInternal(spectrogram);
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    spectrogram.Load();
                    BeginInvoke((Action)(() =>
                    {
                        InitializeSpectrogramInternal(spectrogram);
                    }));
                });
            }
        }

        private void InitializeSpectrogramInternal(SpectrogramData spectrogram)
        {
            if (_spectrogram != null)
            {
                return;
            }

            _spectrogram = spectrogram;
            Invalidate();
        }

        private void DrawSpectrogram(Graphics graphics)
        {
            var width = (int)Math.Round((EndPositionSeconds - _startPositionSeconds) / _spectrogram.SampleDuration);
            using (var bmpCombined = new Bitmap(width, _spectrogram.FftSize / 2))
            using (var gfxCombined = Graphics.FromImage(bmpCombined))
            {
                var left = (int)Math.Round(_startPositionSeconds / _spectrogram.SampleDuration);
                var offset = 0;
                var imageIndex = left / _spectrogram.ImageWidth;
                while (offset < width && imageIndex < _spectrogram.Images.Count)
                {
                    var x = (left + offset) % _spectrogram.ImageWidth;
                    var w = Math.Min(_spectrogram.ImageWidth - x, width - offset);
                    gfxCombined.DrawImage(_spectrogram.Images[imageIndex], offset, 0, new Rectangle(x, 0, w, bmpCombined.Height), GraphicsUnit.Pixel);
                    offset += w;
                    imageIndex++;
                }

                var displayHeight = _showWaveform ? SpectrogramDisplayHeight : Height;
                graphics.DrawImage(bmpCombined, new Rectangle(0, Height - displayHeight, Width, displayHeight));
            }
        }

        private double GetAverageVolumeForNextMilliseconds(int sampleIndex, int milliseconds)
        {
            // length cannot be less than 9
            var length = Math.Max(SecondsToSampleIndex(milliseconds / TimeCode.BaseUnit), 9);
            var max = Math.Min(sampleIndex + length, _wavePeaks.Peaks.Count);
            var from = Math.Max(sampleIndex, 1);

            if (from >= max)
            {
                return 0;
            }

            double v = 0;
            for (var i = from; i < max; i++)
            {
                v += _wavePeaks.Peaks[i].Abs;
            }

            return v / (max - from);
        }

        internal void GenerateTimeCodes(Subtitle subtitle, double startFromSeconds, int blockSizeMilliseconds, int minimumVolumePercent, int maximumVolumePercent, int defaultMilliseconds)
        {
            var begin = SecondsToSampleIndex(startFromSeconds);

            double average = 0;
            for (int k = begin; k < _wavePeaks.Peaks.Count; k++)
            {
                average += _wavePeaks.Peaks[k].Abs;
            }

            average /= _wavePeaks.Peaks.Count - begin;

            var maxThreshold = (int)(_wavePeaks.HighestPeak * (maximumVolumePercent / 100.0));
            var silenceThreshold = (int)(average * (minimumVolumePercent / 100.0));

            int length50Ms = SecondsToSampleIndex(0.050);
            double secondsPerParagraph = defaultMilliseconds / TimeCode.BaseUnit;
            int minBetween = SecondsToSampleIndex(Configuration.Settings.General.MinimumMillisecondsBetweenLines / TimeCode.BaseUnit);
            bool subtitleOn = false;
            int i = begin;
            while (i < _wavePeaks.Peaks.Count)
            {
                if (subtitleOn)
                {
                    var currentLengthInSeconds = SampleIndexToSeconds(i - begin);
                    if (currentLengthInSeconds > 1.0)
                    {
                        subtitleOn = EndParagraphDueToLowVolume(subtitle, blockSizeMilliseconds, silenceThreshold, begin, true, i);
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
                            subtitleOn = EndParagraphDueToLowVolume(subtitle, blockSizeMilliseconds, silenceThreshold, begin, true, i + (j * length50Ms));
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
                            var p = new Paragraph(string.Empty, SampleIndexToSeconds(begin) * TimeCode.BaseUnit, SampleIndexToSeconds(i) * TimeCode.BaseUnit);
                            subtitle.Paragraphs.Add(p);
                            begin = i + minBetween;
                            i = begin;
                        }
                    }
                }
                else
                {
                    double avgVol = GetAverageVolumeForNextMilliseconds(i, blockSizeMilliseconds);
                    if (avgVol > silenceThreshold && avgVol < maxThreshold)
                    {
                        subtitleOn = true;
                        begin = i;
                    }
                }
                i++;
            }

            subtitle.Renumber();
        }

        private bool EndParagraphDueToLowVolume(Subtitle subtitle, int blockSizeMilliseconds, double silenceThreshold, int begin, bool subtitleOn, int i)
        {
            var avgVol = GetAverageVolumeForNextMilliseconds(i, blockSizeMilliseconds);
            if (avgVol < silenceThreshold)
            {
                var p = new Paragraph(string.Empty, SampleIndexToSeconds(begin) * TimeCode.BaseUnit, SampleIndexToSeconds(i) * TimeCode.BaseUnit);
                subtitle.Paragraphs.Add(p);
                subtitleOn = false;
            }

            return subtitleOn;
        }

        private MinMax GetMinAndMax(int startIndex, int endIndex)
        {
            var minPeak = int.MaxValue;
            var maxPeak = int.MinValue;
            double total = 0;
            for (var i = startIndex; i < endIndex; i++)
            {
                var v = _wavePeaks.Peaks[i].Abs;
                total += v;
                if (v < minPeak)
                {
                    minPeak = v;
                }
                if (v > maxPeak)
                {
                    maxPeak = v;
                }
            }

            return new MinMax { Min = minPeak, Max = maxPeak, Avg = total / (endIndex - startIndex) };
        }

        public double FindDataBelowThreshold(double thresholdPercent, double durationInSeconds)
        {
            var begin = SecondsToSampleIndex(_currentVideoPositionSeconds + 1);
            var length = SecondsToSampleIndex(durationInSeconds);
            var threshold = thresholdPercent / 100.0 * _wavePeaks.HighestPeak;
            var hitCount = 0;
            for (var i = Math.Max(0, begin); i < _wavePeaks.Peaks.Count; i++)
            {
                if (_wavePeaks.Peaks[i].Abs <= threshold)
                {
                    hitCount++;
                }
                else
                {
                    hitCount = 0;
                }

                if (hitCount > length)
                {
                    var seconds = SampleIndexToSeconds(i - (length / 2));
                    if (seconds >= 0)
                    {
                        StartPositionSeconds = seconds;
                        if (_startPositionSeconds > 1)
                        {
                            StartPositionSeconds -= 1;
                        }

                        OnSingleClick?.Invoke(this, new ParagraphEventArgs(seconds, null));
                        Invalidate();
                    }

                    return seconds;
                }
            }

            return -1;
        }

        /// <returns>video position in seconds, -1 if not found</returns>
        public double FindDataBelowThresholdBack(double thresholdPercent, double durationInSeconds)
        {
            var begin = SecondsToSampleIndex(_currentVideoPositionSeconds - 1);
            var length = SecondsToSampleIndex(durationInSeconds);
            var threshold = thresholdPercent / 100.0 * _wavePeaks.HighestPeak;
            var hitCount = 0;
            for (var i = begin; i > 0; i--)
            {
                if (i < _wavePeaks.Peaks.Count && _wavePeaks.Peaks[i].Abs <= threshold)
                {
                    hitCount++;
                    if (hitCount > length)
                    {
                        var seconds = SampleIndexToSeconds(i + length / 2);
                        if (seconds >= 0)
                        {
                            StartPositionSeconds = seconds;
                            if (_startPositionSeconds > 1)
                            {
                                StartPositionSeconds -= 1;
                            }
                            else
                            {
                                StartPositionSeconds = 0;
                            }

                            OnSingleClick?.Invoke(this, new ParagraphEventArgs(seconds, null));
                            Invalidate();
                        }

                        return seconds;
                    }
                }
                else
                {
                    hitCount = 0;
                }
            }

            return -1;
        }

        /// <summary>
        /// Seeks silence in volume
        /// </summary>
        /// <returns>video position in seconds, -1 if not found</returns>
        public double FindDataBelowThresholdBackForStart(double thresholdPercent, double durationInSeconds, double startSeconds)
        {
            var min = Math.Max(0, SecondsToSampleIndex(startSeconds - 1));
            var maxShort = Math.Min(_wavePeaks.Peaks.Count, SecondsToSampleIndex(startSeconds + durationInSeconds + 0.01));
            var max = Math.Min(_wavePeaks.Peaks.Count, SecondsToSampleIndex(startSeconds + durationInSeconds + 0.8));
            var length = SecondsToSampleIndex(durationInSeconds);
            var threshold = thresholdPercent / 100.0 * _wavePeaks.HighestPeak;

            var minMax = GetMinAndMax(min, max);
            const int lowPeakDifference = 4_000;
            if (minMax.Max - minMax.Min < lowPeakDifference)
            {
                return -1; // all audio about the same
            }

            // look for start silence in the beginning of subtitle
            min = SecondsToSampleIndex(startSeconds);
            var hitCount = 0;
            int index;
            for (index = min; index < max; index++)
            {
                if (index > 0 && index < _wavePeaks.Peaks.Count && _wavePeaks.Peaks[index].Abs <= threshold)
                {
                    hitCount++;
                }
                else
                {
                    minMax = GetMinAndMax(min, index);
                    var currentMinMax = GetMinAndMax(SecondsToSampleIndex(startSeconds), SecondsToSampleIndex(startSeconds + 0.8));
                    if (currentMinMax.Avg > minMax.Avg + 300 || currentMinMax.Avg < 1000 && minMax.Avg < 1000 && Math.Abs(currentMinMax.Avg - minMax.Avg) < 500)
                    {
                        break;
                    }
                    hitCount = length / 2;
                }
            }

            if (hitCount > length)
            {
                minMax = GetMinAndMax(min, index);
                var currentMinMax = GetMinAndMax(SecondsToSampleIndex(startSeconds), SecondsToSampleIndex(startSeconds + 0.8));
                if (currentMinMax.Avg > minMax.Avg + 300 || currentMinMax.Avg < 1000 && minMax.Avg < 1000 && Math.Abs(currentMinMax.Avg - minMax.Avg) < 500)
                {
                    return Math.Max(0, SampleIndexToSeconds(index - 1) - 0.01);
                }
            }

            // move start left?
            min = SecondsToSampleIndex(startSeconds - 1);
            hitCount = 0;
            for (index = maxShort; index > min; index--)
            {
                if (index > 0 && index < _wavePeaks.Peaks.Count && _wavePeaks.Peaks[index].Abs <= threshold)
                {
                    hitCount++;
                    if (hitCount > length)
                    {
                        return Math.Max(0, SampleIndexToSeconds(index + length) - 0.01);
                    }
                }
                else
                {
                    hitCount = 0;
                }
            }

            return -1;
        }

        public double FindLowPercentage(double startSeconds, double endSeconds)
        {
            var min = Math.Max(0, SecondsToSampleIndex(startSeconds));
            var max = Math.Min(_wavePeaks.Peaks.Count, SecondsToSampleIndex(endSeconds));
            var minMax = GetMinAndMax(min, max);
            var threshold = minMax.Min * 100.0 / _wavePeaks.HighestPeak;
            return threshold;
        }

        public double FindHighPercentage(double startSeconds, double endSeconds)
        {
            var min = Math.Max(0, SecondsToSampleIndex(startSeconds));
            var max = Math.Min(_wavePeaks.Peaks.Count, SecondsToSampleIndex(endSeconds));
            var minMax = GetMinAndMax(min, max);
            var threshold = minMax.Max * 100.0 / _wavePeaks.HighestPeak;
            return threshold;
        }

        public int GetShotChangeIndex(double seconds)
        {
            if (ShotChanges == null)
            {
                return -1;
            }

            try
            {
                for (var index = 0; index < ShotChanges.Count; index++)
                {
                    var shotChange = ShotChanges[index];
                    if (Math.Abs(shotChange - seconds) < 0.04)
                    {
                        return index;
                    }
                }
            }
            catch
            {
                // ignored
            }

            return -1;
        }

        private void UpdateSnappingDistance()
        {
            if (_wavePeaks != null)
            {
                var largestGapInFrames = Math.Max(Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap, Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap);
                var pixelsPerFrame = (_wavePeaks.SampleRate * _zoomFactor) / Configuration.Settings.General.CurrentFrameRate;
                var snappingDistance = (int)Math.Round(pixelsPerFrame * Math.Max(1, largestGapInFrames));

                ShotChangeSnapPixels = Math.Max(8, snappingDistance);
            }
            else
            {
                ShotChangeSnapPixels = 8;
            }
        }
    }
}
