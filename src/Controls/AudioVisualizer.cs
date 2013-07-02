using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Controls
{
    public partial class AudioVisualizer : UserControl
    {
        private enum MouseDownParagraphType
        {
            None,
            Start,
            Whole,
            End
        }

        private const int ClosenessForBorderSelection = 14;
        private const int MininumSelectionMilliseconds = 100;

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
        private double _currentVideoPositionSeconds = -1;
        private WavePeakGenerator _wavePeaks;
        private Subtitle _subtitle;
        private ListView.SelectedIndexCollection _selectedIndices;
        private bool _noClear;

        private List<Bitmap> _spectrogramBitmaps = new List<Bitmap>();
        private string _spectrogramDirectory;
        private double _sampleDuration;
        private double _totalDuration = 0;
        private const int SpectrogramBitmapWidth = 1024;

        public delegate void ParagraphChangedHandler(Paragraph paragraph);
        public event ParagraphChangedHandler OnNewSelectionRightClicked;
        public event PositionChangedEventHandler OnParagraphRightClicked;
        public event PositionChangedEventHandler OnNonParagraphRightClicked;

        public delegate void PositionChangedEventHandler(double seconds, Paragraph paragraph);
        public event PositionChangedEventHandler OnPositionSelected;

        public delegate void TimeChangedEventHandler(double seconds, Paragraph paragraph);
        public delegate void TimeChangedWithHistoryEventHandler(double seconds, Paragraph paragraph, Paragraph beforeParagraph);
        public event TimeChangedWithHistoryEventHandler OnTimeChanged;
        public event TimeChangedEventHandler OnTimeChangedAndOffsetRest;

        public event TimeChangedEventHandler OnSingleClick;
        public event TimeChangedEventHandler OnDoubleClickNonParagraph;
        public event EventHandler OnPause;
        public event EventHandler OnZoomedChanged;

        public bool MouseWheelScrollUpIsForward = true;
        public const double ZoomMininum = 0.1;
        public const double ZoomMaxinum = 2.5;
        private double _zoomFactor = 1.0; // 1.0=no zoom
        public double ZoomFactor
        {
            get
            {
                return _zoomFactor;
            }
            set
            {
                if (value < ZoomMininum)
                    _zoomFactor = ZoomMininum;
                else if (value > ZoomMaxinum)
                    _zoomFactor = ZoomMaxinum;
                else
                    _zoomFactor = value;
                Invalidate();
            }
        }

        public bool IsSpectrogramAvailable
        {
            get
            {
                return _spectrogramBitmaps != null && _spectrogramBitmaps.Count > 0;
            }
        }

        public bool ShowSpectrogram { get; set; }
        private bool _tempShowSpectrogram;

        public bool ShowWaveform { get; set; }

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
        public Paragraph RightClickedParagraph { get; private set; }
        public double RightClickedSeconds { get; private set; }

        public string WaveFormNotLoadedText { get; set; }
        public Color BackgroundColor { get; set; }
        public Color Color { get; set; }
        public Color SelectedColor { get; set; }
        public Color TextColor { get; set; }
        public Color GridColor { get; set; }
        public bool DrawGridLines { get; set; }
        public bool AllowNewSelection { get; set; }

        public bool Locked { get; set; }

        public double VerticalZoomPercent { get; set; }

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
                _subtitle = null;
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
            WaveFormNotLoadedText = "Click to add waveform/spectrogram";
            MouseWheel += WaveFormMouseWheel;

            BackgroundColor = Color.Black;
            Color = Color.GreenYellow;
            SelectedColor = Color.Red;
            TextColor = Color.Gray;
            GridColor = Color.FromArgb(255, 20, 20, 18);
            DrawGridLines = true;
            AllowNewSelection = true;
            ShowSpectrogram = true;
            ShowWaveform = true;
            VerticalZoomPercent = 1.0;
        }

        public void NearestSubtitles(Subtitle subtitle, double currentVideoPositionSeconds, int subtitleIndex)
        {
            _previousAndNextParagraphs.Clear();
            _currentParagraph = null;
            int positionMilliseconds = (int)Math.Round(currentVideoPositionSeconds * 1000.0);
            if (_selectedParagraph != null && _selectedParagraph.StartTime.TotalMilliseconds < positionMilliseconds && _selectedParagraph.EndTime.TotalMilliseconds > positionMilliseconds)
            {
                _currentParagraph = _selectedParagraph;
                for (int j = 1; j < 12; j++)
                {
                    Paragraph nextParagraph = subtitle.GetParagraphOrDefault(subtitleIndex - j);
                    _previousAndNextParagraphs.Add(nextParagraph);
                }
                for (int j=1; j < 10; j++)
                {
                    Paragraph nextParagraph = subtitle.GetParagraphOrDefault(subtitleIndex + j);
                    _previousAndNextParagraphs.Add(nextParagraph);
                }
            }
            else
            {
                for (int i = 0; i < subtitle.Paragraphs.Count; i++)
                {
                    Paragraph p = subtitle.Paragraphs[i];
                    if (p.EndTime.TotalMilliseconds > positionMilliseconds)
                    {
                        _currentParagraph = p;
                        for (int j = 1; j < 10; j++)
                        {
                            Paragraph nextParagraph = subtitle.GetParagraphOrDefault(i - j);
                            _previousAndNextParagraphs.Add(nextParagraph);
                        }
                        for (int j = 1; j < 10; j++)
                        {
                            Paragraph nextParagraph = subtitle.GetParagraphOrDefault(i + j);
                            _previousAndNextParagraphs.Add(nextParagraph);
                        }

                        break;
                    }
                }
                if (_previousAndNextParagraphs.Count == 0)
                {
                    for (int i = 0; i < subtitle.Paragraphs.Count; i++)
                    {
                        Paragraph p = subtitle.Paragraphs[i];
                        if (p.EndTime.TotalMilliseconds > StartPositionSeconds * 1000)
                        {
                            _currentParagraph = p;
                            for (int j = 1; j < 10; j++)
                            {
                                Paragraph nextParagraph = subtitle.GetParagraphOrDefault(i - j);
                                _previousAndNextParagraphs.Add(nextParagraph);
                            }
                            for (int j = 1; j < 10; j++)
                            {
                                Paragraph nextParagraph = subtitle.GetParagraphOrDefault(i + j);
                                _previousAndNextParagraphs.Add(nextParagraph);
                            }

                            break;
                        }
                    }
                    if (_previousAndNextParagraphs.Count == 0 && _subtitle.Paragraphs.Count > 0)
                    {
                        int i = _subtitle.Paragraphs.Count;
                        for (int j = 1; j < 10; j++)
                        {
                            Paragraph nextParagraph = subtitle.GetParagraphOrDefault(i - j);
                            _previousAndNextParagraphs.Add(nextParagraph);
                        }
                    }
                }
            }
        }

        public void SetPosition(double startPositionSeconds, Subtitle subtitle, double currentVideoPositionSeconds, int subtitleIndex, ListView.SelectedIndexCollection selectedIndices)
        {
            StartPositionSeconds = startPositionSeconds;
            _selectedIndices = selectedIndices;
            _subtitle = subtitle;
            _currentVideoPositionSeconds = currentVideoPositionSeconds;
            _selectedParagraph = _subtitle.GetParagraphOrDefault(subtitleIndex);
            NearestSubtitles(subtitle, currentVideoPositionSeconds, subtitleIndex);
            Invalidate();
        }

        private static int CalculateHeight(double value, int imageHeight, int maxHeight)
        {
            double percentage = value / maxHeight;
            int result = (int)Math.Round((percentage * imageHeight) + (imageHeight / 2.0));
            return imageHeight - result;
        }

        private bool IsSelectedIndex(int pos, ref int lastCurrentEnd, List<Paragraph> selectedParagraphs)
        {
            if (pos < lastCurrentEnd)
                return true;

            if (_selectedIndices == null || _subtitle == null)
                return false;

            foreach (Paragraph p in selectedParagraphs)
            {
                if (pos >= p.StartFrame && pos <= p.EndFrame) // not really frames...
                {
                    lastCurrentEnd = p.EndFrame;
                    return true;
                }
            }
            return false;
        }

        internal void WaveFormPaint(object sender, PaintEventArgs e)
        {
            if (_wavePeaks != null && _wavePeaks.AllSamples != null)
            {
                List<Paragraph> selectedParagraphs = new List<Paragraph>();
                if (_selectedIndices != null)
                {
                    var n = _wavePeaks.Header.SampleRate * _zoomFactor;
                    try
                    {
                        foreach (int index in _selectedIndices)
                        {
                            Paragraph p = _subtitle.GetParagraphOrDefault(index);
                            if (p != null)
                            {
                                p = new Paragraph(p);
                                // not really frames... just using them as position markers for better performance
                                p.StartFrame = (int)(p.StartTime.TotalSeconds * n);
                                p.EndFrame = (int)(p.EndTime.TotalSeconds * n);
                                selectedParagraphs.Add(p);
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                if (StartPositionSeconds < 0)
                    StartPositionSeconds = 0;

                if (XPositionToSeconds(Width) > _wavePeaks.Header.LengthInSeconds)
                    StartPositionSeconds = _wavePeaks.Header.LengthInSeconds - ((Width / (double)_wavePeaks.Header.SampleRate) / _zoomFactor);

                Graphics graphics = e.Graphics;
                int begin = SecondsToXPosition(StartPositionSeconds);
                int beginNoZoomFactor = (int)Math.Round(StartPositionSeconds * _wavePeaks.Header.SampleRate); // do not use zoom factor here!

                int start = -1;
                int end = -1;
                if (_selectedParagraph != null)
                {
                    start = SecondsToXPosition(_selectedParagraph.StartTime.TotalSeconds);
                    end = SecondsToXPosition(_selectedParagraph.EndTime.TotalSeconds);
                }
                int imageHeight = Height;
                int maxHeight = (int)(Math.Max(Math.Abs(_wavePeaks.DataMinValue), Math.Abs(_wavePeaks.DataMaxValue)) + 0.5);
                maxHeight = (int)(maxHeight * VerticalZoomPercent);
                if (maxHeight < 0)
                    maxHeight = 1000;
                var pen = new Pen(Color);

                DrawBackground(graphics);
                int x = 0;
                int y = Height / 2;

                if (IsSpectrogramAvailable && ShowSpectrogram)
                {
                    DrawSpectrogramBitmap(StartPositionSeconds, graphics);
                    if (ShowWaveform)
                        imageHeight -= 128;
                }

                var penNormal = new Pen(Color);
                var penSelected = new Pen(SelectedColor); // selected paragraph
                int lastCurrentEnd = -1;

                if (ShowWaveform)
                {
                    if (_zoomFactor > 0.9999 && ZoomFactor < 1.00001)
                    {
                        for (int i = 0; i < _wavePeaks.AllSamples.Count && i < Width; i++)
                        {
                            int n = begin + i;
                            if (n < _wavePeaks.AllSamples.Count)
                            {
                                int newY = CalculateHeight(_wavePeaks.AllSamples[n], imageHeight, maxHeight);
                                graphics.DrawLine(pen, x, y, i, newY);
                                //graphics.FillRectangle(new SolidBrush(Color), x, y, 1, 1); // draw pixel instead of line

                                x = i;
                                y = newY;
                                if (n <= end && n >= start)
                                    pen = penSelected;
                                else if (IsSelectedIndex(n, ref lastCurrentEnd, selectedParagraphs))
                                    pen = penSelected;
                                else
                                    pen = penNormal;
                            }
                        }
                    }
                    else
                    {
                        // calculate lines with zoom factor
                        float x2 = 0;
                        float x3 = 0;
                        for (int i = 0; i < _wavePeaks.AllSamples.Count && ((int)Math.Round(x3)) < Width; i++)
                        {
                            if (beginNoZoomFactor + i < _wavePeaks.AllSamples.Count)
                            {
                                int newY = CalculateHeight(_wavePeaks.AllSamples[beginNoZoomFactor + i], imageHeight, maxHeight);
                                x3 = (float)(_zoomFactor * i);
                                graphics.DrawLine(pen, x2, y, x3, newY);
                                x2 = x3;
                                y = newY;
                                int n = (int)(begin + x3);
                                if (n <= end && n >= start)
                                    pen = penSelected;
                                else if (IsSelectedIndex(n, ref lastCurrentEnd, selectedParagraphs))
                                    pen = penSelected;
                                else
                                    pen = penNormal;
                            }
                        }
                    }
                }
                DrawTimeLine(StartPositionSeconds, e, imageHeight);

                // current video position
                if (_currentVideoPositionSeconds > 0)
                {
                    int videoPosition = SecondsToXPosition(_currentVideoPositionSeconds);
                    videoPosition -= begin;
                    if (videoPosition > 0 && videoPosition < Width)
                    {
                        using (var p = new Pen(Color.Turquoise))
                        {
                            graphics.DrawLine(p, videoPosition, 0, videoPosition, Height);
                        }
                    }
                }

                // mark paragraphs
                var textBrush = new SolidBrush(TextColor);
                DrawParagraph(_currentParagraph, e, begin, textBrush);
                foreach (Paragraph p in _previousAndNextParagraphs)
                    DrawParagraph(p, e, begin, textBrush);
                textBrush.Dispose();

                // current selection
                if (NewSelectionParagraph != null)
                {
                    int currentRegionLeft = SecondsToXPosition(NewSelectionParagraph.StartTime.TotalSeconds - StartPositionSeconds);
                    int currentRegionRight = SecondsToXPosition(NewSelectionParagraph.EndTime.TotalSeconds - StartPositionSeconds);

                    int currentRegionWidth = currentRegionRight - currentRegionLeft;
                    SolidBrush brush = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
                    if (currentRegionLeft >= 0 && currentRegionLeft <= Width)
                    {
                        graphics.FillRectangle(brush, currentRegionLeft, 0, currentRegionWidth, graphics.VisibleClipBounds.Height);

                        if (currentRegionWidth > 40)
                        {
                            SolidBrush tBrush = new SolidBrush(Color.Turquoise);
                            graphics.DrawString(string.Format("{0:0.###} {1}",((double)currentRegionWidth / _wavePeaks.Header.SampleRate / _zoomFactor),
                                                                Configuration.Settings.Language.WaveForm.Seconds),
                                                  Font, tBrush, new PointF(currentRegionLeft + 3, Height - 32));
                            tBrush.Dispose();
                        }
                    }
                    brush.Dispose();
                }
                pen.Dispose();
                penNormal.Dispose();
                penSelected.Dispose();
            }
            else
            {
                DrawBackground(e.Graphics);

                SolidBrush textBrush = new SolidBrush(TextColor);
                Font textFont = new Font(Font.FontFamily, 8);

                if (Width > 90)
                {
                    e.Graphics.DrawString(WaveFormNotLoadedText, textFont, textBrush, new PointF(Width / 2 - 65, Height / 2 - 10));
                }
                else
                {
                    StringFormat stringFormat = new StringFormat();
                    stringFormat.FormatFlags = StringFormatFlags.DirectionVertical;

                    e.Graphics.DrawString(WaveFormNotLoadedText, textFont, textBrush, new PointF(1, 10), stringFormat);
                }
                textBrush.Dispose();
                textFont.Dispose();
            }
            if (Focused)
            {
                using (Pen p = new Pen(SelectedColor))
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
                using (Pen pen = new Pen(new SolidBrush(GridColor)))
                {
                    for (int i = 0; i < Width; i += 10)
                    {
                        graphics.DrawLine(pen, i, 0, i, Height);
                        graphics.DrawLine(pen, 0, i, Width, i);
                    }
                }
            }
        }

        private void DrawTimeLine(double startPositionSeconds, PaintEventArgs e, int imageHeight)
        {
            int start = (int)Math.Round(startPositionSeconds + 0.5);
            double seconds = start - StartPositionSeconds;
            float position = SecondsToXPosition(seconds);
            Pen pen = new Pen(TextColor);
            SolidBrush textBrush = new SolidBrush(TextColor);
            Font textFont = new Font(Font.FontFamily, 7);
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
            pen.Dispose();
            textBrush.Dispose();
            textBrush.Dispose();
        }

        private static string GetDisplayTime(double seconds)
        {
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            if (ts.Minutes == 0 && ts.Hours == 0)
                return ts.Seconds.ToString();
            if (ts.Hours == 0)
                return string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
            return string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
        }

        private void DrawParagraph(Paragraph paragraph, PaintEventArgs e, int begin, SolidBrush textBrush)
        {
            if (paragraph != null)
            {
                int currentRegionLeft = SecondsToXPosition(paragraph.StartTime.TotalSeconds) - begin;
                int currentRegionRight = SecondsToXPosition(paragraph.EndTime.TotalSeconds) - begin;
                int currentRegionWidth = currentRegionRight - currentRegionLeft;
                SolidBrush brush = new SolidBrush(Color.FromArgb(32, 255, 255, 255));
                e.Graphics.FillRectangle(brush, currentRegionLeft, 0, currentRegionWidth, e.Graphics.VisibleClipBounds.Height);

                Pen pen = new Pen(new SolidBrush(Color.FromArgb(128, 150, 150, 150)));
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                e.Graphics.DrawLine(pen, currentRegionLeft, 0, currentRegionLeft, e.Graphics.VisibleClipBounds.Height);
                pen.Dispose();
                pen = new Pen(new SolidBrush(Color.FromArgb(135, 0, 100, 0)));
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                e.Graphics.DrawLine(pen, currentRegionRight, 0, currentRegionRight, e.Graphics.VisibleClipBounds.Height);
               
                var n = _zoomFactor * _wavePeaks.Header.SampleRate;
                if (Configuration.Settings != null && Configuration.Settings.General.UseTimeFormatHHMMSSFF)
                {
                    if (n > 80)
                    {
                        e.Graphics.DrawString(paragraph.Text.Replace(Environment.NewLine, "  "), Font, textBrush, new PointF(currentRegionLeft + 3, 10));
                        e.Graphics.DrawString("#" + paragraph.Number + "  " + paragraph.StartTime.ToShortStringHHMMSSFF() + " --> " + paragraph.EndTime.ToShortStringHHMMSSFF(), Font, textBrush, new PointF(currentRegionLeft + 3, Height - 32));
                    }
                    else if (n > 51)
                        e.Graphics.DrawString("#" + paragraph.Number + "  " + paragraph.StartTime.ToShortStringHHMMSSFF(), Font, textBrush, new PointF(currentRegionLeft + 3, Height - 32));
                    else if (n > 25)
                        e.Graphics.DrawString("#" + paragraph.Number, Font, textBrush, new PointF(currentRegionLeft + 3, Height - 32));
                }
                else
                {
                    if (n > 80)
                    {
                        e.Graphics.DrawString(paragraph.Text.Replace(Environment.NewLine, "  "), Font, textBrush, new PointF(currentRegionLeft + 3, 10));
                        e.Graphics.DrawString("#" + paragraph.Number + "  " + paragraph.StartTime.ToShortString() + " --> " + paragraph.EndTime.ToShortString(), Font, textBrush, new PointF(currentRegionLeft + 3, Height - 32));
                    }
                    else if (n > 51)
                        e.Graphics.DrawString("#" + paragraph.Number + "  " + paragraph.StartTime.ToShortString(), Font, textBrush, new PointF(currentRegionLeft + 3, Height - 32));
                    else if (n > 25)
                        e.Graphics.DrawString("#" + paragraph.Number, Font, textBrush, new PointF(currentRegionLeft + 3, Height - 32));
                }
                brush.Dispose();
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

        private void WaveFormMouseDown(object sender, MouseEventArgs e)
        {
            if (_wavePeaks == null)
                return;

            _mouseDownParagraphType = MouseDownParagraphType.None;
            if (e.Button == MouseButtons.Left)
            {
                _buttonDownTimeTicks = DateTime.Now.Ticks;

                Cursor = Cursors.VSplit;
                double seconds = XPositionToSeconds(e.X);
                int milliseconds = (int)(seconds * 1000.0);

                if (SetParagrapBorderHit(milliseconds, NewSelectionParagraph))
                {
                    if (_mouseDownParagraphType == MouseDownParagraphType.Start)
                    {
                        _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                        NewSelectionParagraph.StartTime.TotalMilliseconds = milliseconds;
                        _mouseMoveStartX = e.X;
                        _mouseMoveEndX = SecondsToXPosition(NewSelectionParagraph.EndTime.TotalSeconds);
                    }
                    else
                    {
                        _mouseDownParagraph.EndTime.TotalMilliseconds = milliseconds;
                        NewSelectionParagraph.EndTime.TotalMilliseconds = milliseconds;
                        _mouseMoveStartX = SecondsToXPosition(NewSelectionParagraph.StartTime.TotalSeconds);
                        _mouseMoveEndX = e.X;

                    }
                }
                else if (SetParagrapBorderHit(milliseconds, _selectedParagraph) ||
                    SetParagrapBorderHit(milliseconds, _currentParagraph) ||
                    SetParagrapBorderHit(milliseconds, _previousAndNextParagraphs))
                {
                    NewSelectionParagraph = null;
                    if (_mouseDownParagraphType == MouseDownParagraphType.Start)
                        _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                    else
                        _mouseDownParagraph.EndTime.TotalMilliseconds = milliseconds;
                }
                else
                {
                    Paragraph p = GetParagraphAtMilliseconds(milliseconds);
                    if (p != null)
                    {
                        _oldParagraph = new Paragraph(p);
                        _mouseDownParagraph = p;
                        _mouseDownParagraphType = MouseDownParagraphType.Whole;
                        _moveWholeStartDifferenceMilliseconds = (XPositionToSeconds(e.X) * 1000.0) - p.StartTime.TotalMilliseconds;
                        Cursor = Cursors.Hand;
                    }
                    else if (!AllowNewSelection)
                    {
                        Cursor = Cursors.Default;
                    }
                    NewSelectionParagraph = null;
                    _mouseMoveStartX = e.X;
                    _mouseMoveEndX = e.X;
                }

                _mouseDown = true;
            }
            else
            {
                if (e.Button == MouseButtons.Right)
                {
                    double seconds = XPositionToSeconds(e.X);
                    int milliseconds = (int)(seconds * 1000.0);

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
                                Paragraph paragraph = new Paragraph();
                                paragraph.StartTime = new TimeCode(TimeSpan.FromSeconds(currentRegionLeft));
                                paragraph.EndTime = new TimeCode(TimeSpan.FromSeconds(currentRegionRight));
                                OnNewSelectionRightClicked.Invoke(paragraph);
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
                                OnParagraphRightClicked.Invoke(seconds, p);
                            }
                        }
                        else
                        {
                            if (OnNonParagraphRightClicked != null)
                            {
                                OnNonParagraphRightClicked.Invoke(seconds, null);
                            }
                        }
                    }
                }
                Cursor = Cursors.Default;
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

            if (Math.Abs(milliseconds - paragraph.StartTime.TotalMilliseconds) <= ClosenessForBorderSelection)
            {
                _oldParagraph = new Paragraph(paragraph);
                _mouseDownParagraph = paragraph;
                _mouseDownParagraphType = MouseDownParagraphType.Start;
                return true;
            }
            if (Math.Abs(milliseconds - paragraph.EndTime.TotalMilliseconds) <= ClosenessForBorderSelection)
            {
                _oldParagraph = new Paragraph(paragraph);
                _mouseDownParagraph = paragraph;
                _mouseDownParagraphType = MouseDownParagraphType.End;
                return true;
            }
            return false;
        }

        private void WaveFormMouseMove(object sender, MouseEventArgs e)
        {
            if (_wavePeaks == null)
                return;

            if (e.X < 0 && StartPositionSeconds > 0.1 && _mouseDown)
            {
                if (e.X < _mouseMoveLastX)
                {
                    StartPositionSeconds -= 0.1;
                    if (_mouseDownParagraph == null)
                    {
                        _mouseMoveEndX = 0;
                        _mouseMoveStartX += (int)(_wavePeaks.Header.SampleRate * 0.1);
                        OnPositionSelected.Invoke(StartPositionSeconds, null);
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
                        OnPositionSelected.Invoke(StartPositionSeconds, null);
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
                int milliseconds = (int)(seconds * 1000.0);

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
                if (_mouseDown)
                {
                    if (_mouseDownParagraph != null)
                    {
                        double seconds = XPositionToSeconds(e.X);
                        int milliseconds = (int)(seconds * 1000.0);

                        if (_mouseDownParagraphType == MouseDownParagraphType.Start)
                        {
                            if (_mouseDownParagraph.EndTime.TotalMilliseconds - milliseconds > MininumSelectionMilliseconds)
                            {
                                _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                                if (NewSelectionParagraph != null)
                                {
                                    NewSelectionParagraph.StartTime.TotalMilliseconds = milliseconds;
                                    _mouseMoveStartX = e.X;
                                }
                                else
                                {
                                    if (OnTimeChanged != null)
                                        OnTimeChanged.Invoke(seconds, _mouseDownParagraph, _oldParagraph);
                                }
                            }
                        }
                        else if (_mouseDownParagraphType == MouseDownParagraphType.End)
                        {
                            if (milliseconds - _mouseDownParagraph.StartTime.TotalMilliseconds > MininumSelectionMilliseconds)
                            {
                                _mouseDownParagraph.EndTime.TotalMilliseconds = milliseconds;
                                if (NewSelectionParagraph != null)
                                {
                                    NewSelectionParagraph.EndTime.TotalMilliseconds = milliseconds;
                                    _mouseMoveEndX = e.X;
                                }
                                else
                                {
                                    if (OnTimeChanged != null)
                                        OnTimeChanged.Invoke(seconds, _mouseDownParagraph, _oldParagraph);
                                }
                            }
                        }
                        else if (_mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            double durationMilliseconds = _mouseDownParagraph.Duration.TotalMilliseconds;
                            _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds - _moveWholeStartDifferenceMilliseconds;
                            _mouseDownParagraph.EndTime.TotalMilliseconds = _mouseDownParagraph.StartTime.TotalMilliseconds + durationMilliseconds;
                            if (OnTimeChanged != null)
                                OnTimeChanged.Invoke(seconds, _mouseDownParagraph, _oldParagraph);
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
                            NewSelectionParagraph.StartTime.TotalSeconds = XPositionToSeconds(start);
                            NewSelectionParagraph.EndTime.TotalSeconds = XPositionToSeconds(end);
                        }
                    }
                    Invalidate();
                }
            }
        }

        private static bool IsParagrapBorderHit(int milliseconds, List<Paragraph> paragraphs)
        {
            foreach (Paragraph p in paragraphs)
            {
                bool hit = IsParagrapBorderHit(milliseconds, p);
                if (hit)
                    return true;
            }
            return false;
        }

        private static bool IsParagrapBorderHit(int milliseconds, Paragraph paragraph)
        {
            if (paragraph == null)
                return false;

            return Math.Abs(milliseconds - paragraph.StartTime.TotalMilliseconds) <= ClosenessForBorderSelection ||
                   Math.Abs(milliseconds - paragraph.EndTime.TotalMilliseconds) <= ClosenessForBorderSelection;
        }

        private void WaveFormMouseUp(object sender, MouseEventArgs e)
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

        private void WaveFormMouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
            _mouseDown = false;
            Invalidate();
        }

        private void WaveFormMouseEnter(object sender, EventArgs e)
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

        private void WaveFormMouseDoubleClick(object sender, MouseEventArgs e)
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
                int milliseconds = (int)(seconds * 1000.0);

                Paragraph p = null;
                if (IsParagrapHit(milliseconds, _selectedParagraph))
                    p = _selectedParagraph;
                else if (IsParagrapHit(milliseconds, _currentParagraph))
                    p = _currentParagraph;

                if (p == null)
                {
                    foreach (Paragraph p2 in _previousAndNextParagraphs)
                    {
                        if (IsParagrapHit(milliseconds, p2))
                        {
                            p = p2;
                            break;
                        }
                    }
                }

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
                    OnDoubleClickNonParagraph.Invoke(seconds, p);
            }
        }

        private bool IsParagrapHit(int milliseconds, Paragraph paragraph)
        {
            if (paragraph == null)
                return false;

            return milliseconds >= paragraph.StartTime.TotalMilliseconds && milliseconds <= paragraph.EndTime.TotalMilliseconds;
        }

        private void WaveFormMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && OnSingleClick != null)
            {
                int diff = Math.Abs(_mouseMoveStartX - e.X);
                if (_mouseMoveStartX == -1 || _mouseMoveEndX == -1 || diff < 10 && TimeSpan.FromTicks(DateTime.Now.Ticks - _buttonDownTimeTicks).TotalSeconds < 0.25)
                {

                    if (ModifierKeys == Keys.Shift && _selectedParagraph != null)
                    {
                        double seconds = XPositionToSeconds(e.X);
                        int milliseconds = (int)(seconds * 1000.0);
                        if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            if (seconds < _selectedParagraph.EndTime.TotalSeconds)
                            {
                                _oldParagraph = new Paragraph(_selectedParagraph);
                                _mouseDownParagraph = _selectedParagraph;
                                _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                                if (OnTimeChanged != null)
                                    OnTimeChanged.Invoke(seconds, _mouseDownParagraph, _oldParagraph);
                            }
                        }
                        return;
                    }
                    if (ModifierKeys == Keys.Control && _selectedParagraph != null)
                    {
                        double seconds = XPositionToSeconds(e.X);
                        int milliseconds = (int)(seconds * 1000.0);
                        if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            if (seconds > _selectedParagraph.StartTime.TotalSeconds)
                            {
                                _oldParagraph = new Paragraph(_selectedParagraph);
                                _mouseDownParagraph = _selectedParagraph;
                                _mouseDownParagraph.EndTime.TotalMilliseconds = milliseconds;
                                if (OnTimeChanged != null)
                                    OnTimeChanged.Invoke(seconds, _mouseDownParagraph, _oldParagraph);
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
                                OnTimeChangedAndOffsetRest.Invoke(seconds, _mouseDownParagraph);
                        }
                        return;
                    }
                    if (ModifierKeys == Keys.Alt && _selectedParagraph != null)
                    {
                        double seconds = XPositionToSeconds(e.X);
                        int milliseconds = (int)(seconds * 1000.0);
                        if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            _oldParagraph = new Paragraph(_selectedParagraph);
                            _mouseDownParagraph = _selectedParagraph;
                            double durationMilliseconds = _mouseDownParagraph.Duration.TotalMilliseconds;
                            _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                            _mouseDownParagraph.EndTime.TotalMilliseconds = _mouseDownParagraph.StartTime.TotalMilliseconds + durationMilliseconds;
                            if (OnTimeChanged != null)
                                OnTimeChanged.Invoke(seconds, _mouseDownParagraph, _oldParagraph);
                        }
                        return;
                    }

                    if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        OnSingleClick.Invoke(XPositionToSeconds(e.X), null);
                }
            }
        }

        private void WaveFormKeyDown(object sender, KeyEventArgs e)
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
                    OnPositionSelected.Invoke(StartPositionSeconds, null);
                    Invalidate();
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.X)
            {
                if (StartPositionSeconds + 0.1 < _wavePeaks.Header.LengthInSeconds)
                {
                    StartPositionSeconds += 0.1;
                    OnPositionSelected.Invoke(StartPositionSeconds, null);
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
        }

        public double FindDataBelowThresshold(int thresshold, double durationInSeconds)
        {
            int begin = SecondsToXPosition(_currentVideoPositionSeconds+1);
            int length = SecondsToXPosition(durationInSeconds);

            int hitCount = 0;
            for (int i = begin; i < _wavePeaks.AllSamples.Count; i++)
            {
                if (i > 0 && i < _wavePeaks.AllSamples.Count && Math.Abs(_wavePeaks.AllSamples[i]) <= thresshold)
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
                        OnSingleClick.Invoke(seconds, null);
                        Invalidate();
                    }
                    return seconds;
                }
            }
            return -1;
        }

        public double FindDataBelowThressholdBack(int thresshold, double durationInSeconds)
        {
            int begin = SecondsToXPosition(_currentVideoPositionSeconds-1);
            int length = SecondsToXPosition(durationInSeconds);

            int hitCount = 0;
            for (int i = begin; i > 0; i--)
            {
                if (i > 0 && i < _wavePeaks.AllSamples.Count && Math.Abs(_wavePeaks.AllSamples[i]) <= thresshold)
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
                        OnSingleClick.Invoke(seconds, null);
                        Invalidate();
                    }
                    return seconds;
                }
            }
            return -1;
        }

        public void ZoomIn()
        {
            ZoomFactor = ZoomFactor + 0.1;
            if (OnZoomedChanged != null)
                OnZoomedChanged.Invoke(null, null);
        }

        public void ZoomOut()
        {
            ZoomFactor = ZoomFactor - 0.1;
            if (OnZoomedChanged != null)
                OnZoomedChanged.Invoke(null, null);
        }

        void WaveFormMouseWheel(object sender, MouseEventArgs e)
        {
            int delta = e.Delta;
            if (!MouseWheelScrollUpIsForward)
                delta = delta * -1;
            if (Locked)
            {
                OnPositionSelected.Invoke(_currentVideoPositionSeconds + (delta / 256.0), null);
            }
            else
            {
                StartPositionSeconds += delta / 256.0;
                if (_currentVideoPositionSeconds < StartPositionSeconds || _currentVideoPositionSeconds >= EndPositionSeconds)
                    OnPositionSelected.Invoke(StartPositionSeconds, null);
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
                var bw = new System.ComponentModel.BackgroundWorker();
                bw.DoWork += LoadSpectrogramBitmapsAsync;
                bw.RunWorkerCompleted += LoadSpectrogramBitmapsCompleted;
                bw.RunWorkerAsync();
            }
        }

        void LoadSpectrogramBitmapsCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            var doc = new XmlDocument();
            doc.Load(Path.Combine(_spectrogramDirectory, "Info.xml"));
            _sampleDuration = Convert.ToDouble(doc.DocumentElement.SelectSingleNode("SampleDuration").InnerText, System.Globalization.CultureInfo.InvariantCulture);
            _totalDuration = Convert.ToDouble(doc.DocumentElement.SelectSingleNode("TotalDuration").InnerText, System.Globalization.CultureInfo.InvariantCulture);
            ShowSpectrogram = _tempShowSpectrogram;
        }

        void LoadSpectrogramBitmapsAsync(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            int count = 0;
            string fileName = Path.Combine(_spectrogramDirectory, count + ".gif");
            while (File.Exists(Path.Combine(_spectrogramDirectory, count + ".gif")))
            {
                using (var ms = new MemoryStream(File.ReadAllBytes(fileName)))
                {
                    _spectrogramBitmaps.Add((Bitmap)Bitmap.FromStream(ms));
                }
                count++;
                fileName = Path.Combine(_spectrogramDirectory, count + ".gif");
            }
        }

        public void InitializeSpectrogram(List<Bitmap> spectrogramBitmaps, string spectrogramDirectory)
        {
            _spectrogramBitmaps = spectrogramBitmaps;

            var doc = new XmlDocument();
            string xmlInfoFileName = Path.Combine(spectrogramDirectory, "Info.xml");
            if (File.Exists(xmlInfoFileName))
            {
                doc.Load(xmlInfoFileName);
                _sampleDuration = Convert.ToDouble(doc.DocumentElement.SelectSingleNode("SampleDuration").InnerText, System.Globalization.CultureInfo.InvariantCulture);
                _totalDuration = Convert.ToDouble(doc.DocumentElement.SelectSingleNode("TotalDuration").InnerText, System.Globalization.CultureInfo.InvariantCulture);
                ShowSpectrogram = true;
            }
            else
            {
                ShowSpectrogram = false;
            }
        }

        private void DrawSpectrogramBitmap(double seconds, Graphics graphics)
        {
            double duration = EndPositionSeconds - StartPositionSeconds;
            int width = (int) (duration / _sampleDuration);

            Bitmap bmpDestination = new Bitmap(width, 128); //calculate width
            Graphics gfx = Graphics.FromImage(bmpDestination);

            double startRow = seconds / (_sampleDuration);
            int bitmapIndex = (int)(startRow / SpectrogramBitmapWidth);
            int subtractValue = (int)startRow % SpectrogramBitmapWidth;

            int i = 0;
            while (i * SpectrogramBitmapWidth < width && i + bitmapIndex < _spectrogramBitmaps.Count)
            {
                var bmp = _spectrogramBitmaps[i + bitmapIndex];
                gfx.DrawImage(bmp, new Point(bmp.Width * i - subtractValue, 0));
                i++;
            }
            if (i + bitmapIndex < _spectrogramBitmaps.Count && subtractValue > 0)
            {
                var bmp = _spectrogramBitmaps[i + bitmapIndex];
                gfx.DrawImage(bmp, new Point(bmp.Width * i - subtractValue, 0));
                i++;
            }
            gfx.Dispose();

            if (ShowWaveform)
                graphics.DrawImage(bmpDestination, new Rectangle(0, Height - bmpDestination.Height, Width, bmpDestination.Height));
            else
                graphics.DrawImage(bmpDestination, new Rectangle(0, 0, Width, Height));
            bmpDestination.Dispose();
        }


        private double GetAverageVolumeForNextMilliseconds(int sampleIndex, int milliseconds)
        {
            milliseconds = 150;
            int length = SecondsToXPosition(milliseconds / 1000.0); // min length
            if (length < 9)
                length = 9;
            double v = 0;
            int count = 0;
            for (int i = sampleIndex; i < sampleIndex+length; i++)
            {
                if (i > 0 && i < _wavePeaks.AllSamples.Count)
                {
                    v += Math.Abs(_wavePeaks.AllSamples[i]);
                    count++;
                }
            }
            return v / count;
        }

        internal void GenerateTimeCodes(Subtitle subtitle, double startFromSeconds, int blockSizeMilliseconds, int mininumVolumePercent, int maximumVolumePercent, int defaultMilliseconds)
        {
            int begin = SecondsToXPosition(startFromSeconds);

            double average = 0;
            for (int k=begin; k<_wavePeaks.AllSamples.Count; k++)
                average += Math.Abs(_wavePeaks.AllSamples[k]);
            average = average / (_wavePeaks.AllSamples.Count - begin);

            int maxThresshold = (int)(_wavePeaks.DataMaxValue * (maximumVolumePercent / 100.0));
            int silenceThresshold = 390;
            silenceThresshold = (int)(average * (mininumVolumePercent / 100.0));

            int length = SecondsToXPosition(0.1); // min length
            int length50Ms = SecondsToXPosition(0.050);
            double secondsPerParagraph = defaultMilliseconds / 1000.0;
            int searchExtra = SecondsToXPosition(1.5); // search extra too see if subtitle ends.
            int minBetween = SecondsToXPosition(Configuration.Settings.General.MininumMillisecondsBetweenLines / 1000.0);
            bool subtitleOn = false;
            int i = begin;
            while (i < _wavePeaks.AllSamples.Count)
            {
                if (subtitleOn)
                {
                    var currentLengthInSeconds = XPositionToSeconds(i - begin) - StartPositionSeconds;
                    if (currentLengthInSeconds > 1.0)
                    {
                        subtitleOn = EndParagraphDueToLowVolume(silenceThresshold, begin, subtitleOn, i, subtitle);
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
                            subtitleOn = EndParagraphDueToLowVolume(silenceThresshold, begin, subtitleOn, i + (j * length50Ms), subtitle);
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
                            var p = new Paragraph(string.Empty, (XPositionToSeconds(begin) - StartPositionSeconds) * 1000.0, (XPositionToSeconds(i) - StartPositionSeconds) * 1000.0);
                            _subtitle.Paragraphs.Add(p);
                            begin = i + minBetween;
                            i = begin;
                        }
                    }
                }
                else
                {
                    double avgVol = GetAverageVolumeForNextMilliseconds(i, 100);
                    if (avgVol > silenceThresshold)
                    {
                        if (avgVol < maxThresshold)
                        {
                            subtitleOn = true;
                            begin = i;
                        }
                        else
                        {
//                            MessageBox.Show("Too much");
                        }
                    }
                }
                i++;
            }
        }

        private bool EndParagraphDueToLowVolume(double silenceThresshold, int begin, bool subtitleOn, int i, Subtitle subtitle)
        {
            double avgVol = GetAverageVolumeForNextMilliseconds(i, 100);
            if (avgVol < silenceThresshold)
            {
                var p = new Paragraph(string.Empty, (XPositionToSeconds(begin) - StartPositionSeconds) * 1000.0, (XPositionToSeconds(i) - StartPositionSeconds) * 1000.0);
                _subtitle.Paragraphs.Add(p);
                subtitleOn = false;
            }
            return subtitleOn;
        }


    }
}
