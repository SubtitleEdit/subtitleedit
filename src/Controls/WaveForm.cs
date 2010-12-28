using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Controls
{
    public partial class WaveForm : UserControl
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

        private long _buttonDownTimeTicks = 0;
        private int _mouseMoveLastX = -1;
        private int _mouseMoveStartX = -1;
        private double _moveWholeStartDifferenceMilliseconds = -1;
        private int _mouseMoveEndX = -1;
        private bool _mouseDown = false;
        private Paragraph _mouseDownParagraph = null;
        private MouseDownParagraphType _mouseDownParagraphType = MouseDownParagraphType.Start;
        private Paragraph _selectedParagraph = null;
        private Paragraph _currentParagraph = null;
        private List<Paragraph> _previousAndNextParagraphs = new List<Paragraph>();
        private double _currentVideoPositionSeconds = -1;
        private WavePeakGenerator _wavePeaks = null;
        private Subtitle _subtitle = null;
        private bool _noClear = false;

        public delegate void ParagraphChangedHandler(Paragraph paragraph);
        public event ParagraphChangedHandler OnNewSelectionRightClicked;
        public event PositionChangedEventHandler OnParagraphRightClicked;

        public delegate void PositionChangedEventHandler(double seconds, Paragraph paragraph);
        public event PositionChangedEventHandler OnPositionSelected;
        
        public delegate void TimeChangedEventHandler(double seconds, Paragraph paragraph);
        public event TimeChangedEventHandler OnTimeChanged;
        public event TimeChangedEventHandler OnTimeChangedAndOffsetRest;

        public event TimeChangedEventHandler OnSingleClick;
        public event TimeChangedEventHandler OnDoubleClickNonParagraph;
        public event EventHandler OnPause;
        public event EventHandler OnZoomedChanged;

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

        private double _startPositionSeconds = 0;
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
                _selectedParagraph = null;
                _currentParagraph = null;
                _previousAndNextParagraphs = new List<Paragraph>();
                _currentVideoPositionSeconds = -1;
                _subtitle = null;
                _noClear = false;
                _wavePeaks = value;
            }
        }

        public void ClearSelection()
        {
            _mouseDown = false;
            _mouseDownParagraph = null;
            _mouseMoveStartX = -1;
            _mouseMoveEndX = -1;
            Invalidate();
        }

        public WaveForm()
        {
            InitializeComponent();
            SetStyle(System.Windows.Forms.ControlStyles.UserPaint | System.Windows.Forms.ControlStyles.AllPaintingInWmPaint | System.Windows.Forms.ControlStyles.DoubleBuffer, true);
            WaveFormNotLoadedText = "Click to add wave form";
            this.MouseWheel += new MouseEventHandler(WaveForm_MouseWheel);

            BackgroundColor = Color.Black;
            Color = Color.GreenYellow;
            SelectedColor = Color.Red;
            TextColor = Color.Gray;
            GridColor = Color.FromArgb(255, 20, 20, 18);
            DrawGridLines = true;
            AllowNewSelection = true;
        }

        private void NearestSubtitles(Subtitle subtitle, double currentVideoPositionSeconds, int subtitleIndex)
        { 
            int positionMilliseconds = (int)Math.Round(currentVideoPositionSeconds * 1000.0);
            if (_selectedParagraph != null && _selectedParagraph.StartTime.TotalMilliseconds < positionMilliseconds && _selectedParagraph.EndTime.TotalMilliseconds > positionMilliseconds)
            {
                _currentParagraph = _selectedParagraph;
                _previousAndNextParagraphs.Clear();
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
                        _previousAndNextParagraphs.Clear();
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
            }
        }

        public void SetPosition(double startPositionSeconds, Subtitle subtitle, double currentVideoPositionSeconds, int subtitleIndex)
        {
            StartPositionSeconds = startPositionSeconds;
            _subtitle = subtitle;
            _currentVideoPositionSeconds = currentVideoPositionSeconds;
            _selectedParagraph = _subtitle.GetParagraphOrDefault(subtitleIndex);
            NearestSubtitles(subtitle, currentVideoPositionSeconds, subtitleIndex);           
            Invalidate();
        }

        private static int CalculateHeight(double value, int imageHeight, int maxHeight)
        {
            double percentage = value / maxHeight;
            int result = (int)Math.Round((percentage * imageHeight) + (imageHeight / 2));
            return imageHeight - result;
        }

        private void WaveForm_Paint(object sender, PaintEventArgs e)
        {
            if (_wavePeaks != null && _wavePeaks.AllSamples != null)
            {
                if (StartPositionSeconds < 0)
                    StartPositionSeconds = 0;

                if (XPositionToSeconds(Width) > _wavePeaks.Header.LengthInSeconds)
                    StartPositionSeconds = _wavePeaks.Header.LengthInSeconds - ((((double)Width) / (double)_wavePeaks.Header.SampleRate) / _zoomFactor);
 
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
                Pen pen = new System.Drawing.Pen(System.Drawing.Color.GreenYellow);

                DrawBackground(graphics);
                int x = 0;
                int y = Height / 2;

                if (_zoomFactor == 1.0)
                {
                    for (int i = 0; i < _wavePeaks.AllSamples.Count && i < Width; i++)
                    {
                        if (begin + i < _wavePeaks.AllSamples.Count)
                        {
                            int newY = CalculateHeight(_wavePeaks.AllSamples[begin + i], imageHeight, maxHeight);
                            graphics.DrawLine(pen, x, y, i, newY);
                            x = i;
                            y = newY;
                            if (begin + i > end || begin + i < start)
                                pen = new System.Drawing.Pen(Color);
                            else
                                pen = new System.Drawing.Pen(SelectedColor); // selected paragraph
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
                            if (begin + x2 > end || begin + x2 < start)
                                pen = new System.Drawing.Pen(Color);
                            else
                                pen = new System.Drawing.Pen(SelectedColor); // selected paragraph
                        }
                    }
                }
                DrawTimeLine(StartPositionSeconds, e);

                // current video position
                if (_currentVideoPositionSeconds > 0)
                {
                    int videoPosition = SecondsToXPosition(_currentVideoPositionSeconds);
                    videoPosition -= begin;
                    if (videoPosition > 0 && videoPosition < Width)
                    {
                        pen = new Pen(Color.Turquoise);
                        e.Graphics.DrawLine(pen, videoPosition, 0, videoPosition, Height);
                    }
                }

                // mark paragraphs
                DrawParagraph(_currentParagraph, e, begin);
                foreach (Paragraph p in _previousAndNextParagraphs)
                    DrawParagraph(p, e, begin);

                // current selection
                if (NewSelectionParagraph != null)
                {                    
                    int currentRegionLeft = SecondsToXPosition(NewSelectionParagraph.StartTime.TotalSeconds - StartPositionSeconds);
                    int currentRegionRight = SecondsToXPosition(NewSelectionParagraph.EndTime.TotalSeconds - StartPositionSeconds);

                    int currentRegionWidth = currentRegionRight - currentRegionLeft;
                    SolidBrush brush = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
                    if (currentRegionLeft >= 0 && currentRegionLeft <= Width)
                    {
                        e.Graphics.FillRectangle(brush, currentRegionLeft, 0, currentRegionWidth, e.Graphics.VisibleClipBounds.Height);

                        if (currentRegionWidth > 40)
                        {
                            SolidBrush textBrush = new SolidBrush(Color.Turquoise);
                            Font textFont = new Font(Font.FontFamily, 7);
                            e.Graphics.DrawString(string.Format("{0:0.###} {1}", (double)((double)currentRegionWidth / _wavePeaks.Header.SampleRate / _zoomFactor),
                                                                Configuration.Settings.Language.WaveForm.Seconds),
                                                  Font, textBrush, new PointF(currentRegionLeft + 3, Height - 32));
                        }
                    }
                }
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
            }
        }

        private void DrawBackground(Graphics graphics)
        {
            graphics.Clear(BackgroundColor);
            if (DrawGridLines)
            {
                Pen pen = new Pen(new SolidBrush(GridColor));
                for (int i = 0; i < Width; i += 10)
                {
                    graphics.DrawLine(pen, i, 0, i, Height);
                    graphics.DrawLine(pen, 0, i, Width, i);
                }
            }
        }

        private void DrawTimeLine(double startPositionSeconds, PaintEventArgs e)
        {
            int start = (int)Math.Round(startPositionSeconds + 0.5);
            double seconds = start - StartPositionSeconds;
            float position = SecondsToXPosition(seconds);
            Pen pen = new Pen(TextColor);
            SolidBrush textBrush = new SolidBrush(TextColor);
            Font textFont = new Font(Font.FontFamily, 7);
            while (position < Width)            
            {
                if (_zoomFactor > 0.3 || (int)Math.Round(StartPositionSeconds + seconds) % 5 == 0)
                { 
                    e.Graphics.DrawLine(pen, position, Height, position, Height-10);
                    e.Graphics.DrawString(GetDisplayTime(StartPositionSeconds + seconds), textFont, textBrush, new PointF(position + 2, Height - 13));
                }

                seconds += 0.5;
                position = SecondsToXPosition(seconds);

                if (_zoomFactor > 0.5)
                    e.Graphics.DrawLine(pen, position, Height, position, Height - 5);

                seconds += 0.5;
                position = SecondsToXPosition(seconds);
            }
        }

        private static string GetDisplayTime(double seconds)
        {
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            if (ts.Minutes == 0 && ts.Hours == 0)
                return ts.Seconds.ToString();
            else if (ts.Hours == 0)
                return string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
            else
                return string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
        }

        private void DrawParagraph(Paragraph paragraph, PaintEventArgs e, int begin)
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
                pen = new Pen(new SolidBrush(Color.FromArgb(135, 0, 100, 0)));
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                e.Graphics.DrawLine(pen, currentRegionRight, 0, currentRegionRight, e.Graphics.VisibleClipBounds.Height);

                SolidBrush textBrush = new SolidBrush(TextColor);
                if (_zoomFactor > 0.6)
                    e.Graphics.DrawString(paragraph.Text.Replace(Environment.NewLine, "  "), Font, textBrush, new PointF(currentRegionLeft + 3, 10));

                if (_zoomFactor > 0.6)
                    e.Graphics.DrawString("#" + paragraph.Number + "  " + paragraph.StartTime.ToShortString() + " --> " + paragraph.EndTime.ToShortString(), Font, textBrush, new PointF(currentRegionLeft + 3, Height - 32));
                else if (_zoomFactor > 0.4)
                    e.Graphics.DrawString("#" + paragraph.Number + "  " + paragraph.StartTime.ToShortString(), Font, textBrush, new PointF(currentRegionLeft + 3, Height - 32));
                else if (_zoomFactor > 0.2)
                    e.Graphics.DrawString("#" + paragraph.Number, Font, textBrush, new PointF(currentRegionLeft + 3, Height - 32));
            }
        }

        private double XPositionToSeconds(double x)
        {
            return StartPositionSeconds + (x / (double)_wavePeaks.Header.SampleRate) / _zoomFactor;
        }

        private int SecondsToXPosition(double seconds)
        {
            return (int)Math.Round(seconds * _wavePeaks.Header.SampleRate * _zoomFactor);
        }


        private void WaveForm_MouseDown(object sender, MouseEventArgs e)
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
                    else if (OnParagraphRightClicked != null)
                    {
                        NewSelectionParagraph = null;
                        Paragraph p = GetParagraphAtMilliseconds(milliseconds);
                        RightClickedParagraph = p;
                        RightClickedSeconds = seconds;
                        if (p != null)
                            OnParagraphRightClicked.Invoke(seconds, p);
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
                _mouseDownParagraph = paragraph;
                _mouseDownParagraphType = MouseDownParagraphType.Start;
                return true;
            }
            else if (Math.Abs(milliseconds - paragraph.EndTime.TotalMilliseconds) <= ClosenessForBorderSelection)
            {                
                _mouseDownParagraph = paragraph;
                _mouseDownParagraphType = MouseDownParagraphType.End;
                return true;
            }
            return false;
        }

        private void WaveForm_MouseMove(object sender, MouseEventArgs e)
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
            else if (e.X > Width && StartPositionSeconds + 0.1 < _wavePeaks.Header.LengthInSeconds && _mouseDown)
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
                                        OnTimeChanged.Invoke(seconds, _mouseDownParagraph);
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
                                        OnTimeChanged.Invoke(seconds, _mouseDownParagraph);
                                }
                            }
                        }
                        else if (_mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {                           
                            double durationMilliseconds = _mouseDownParagraph.Duration.TotalMilliseconds;
                            _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds - _moveWholeStartDifferenceMilliseconds;
                            _mouseDownParagraph.EndTime.TotalMilliseconds = _mouseDownParagraph.StartTime.TotalMilliseconds + durationMilliseconds;
                            if (OnTimeChanged != null)
                                OnTimeChanged.Invoke(seconds, _mouseDownParagraph);
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

        private void WaveForm_MouseUp(object sender, MouseEventArgs e)
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

        private void WaveForm_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
            _mouseDown = false;
            Invalidate();
        }

        private void WaveForm_MouseEnter(object sender, EventArgs e)
        {
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
                _mouseMoveStartX = SecondsToXPosition(NewSelectionParagraph.StartTime.TotalSeconds);
                _mouseMoveEndX = SecondsToXPosition(NewSelectionParagraph.EndTime.TotalSeconds);
            }

        }

        private void WaveForm_MouseDoubleClick(object sender, MouseEventArgs e)
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

        private void WaveForm_MouseClick(object sender, MouseEventArgs e)
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
                                _mouseDownParagraph = _selectedParagraph;
                                _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                                if (OnTimeChanged != null)
                                    OnTimeChanged.Invoke(seconds, _mouseDownParagraph);
                            }
                        }
                        return;
                    }
                    else if (ModifierKeys == Keys.Control && _selectedParagraph != null)
                    {
                        double seconds = XPositionToSeconds(e.X);
                        int milliseconds = (int)(seconds * 1000.0);
                        if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            if (seconds > _selectedParagraph.StartTime.TotalSeconds)
                            {
                                _mouseDownParagraph = _selectedParagraph;
                                _mouseDownParagraph.EndTime.TotalMilliseconds = milliseconds;
                                if (OnTimeChanged != null)
                                    OnTimeChanged.Invoke(seconds, _mouseDownParagraph);
                            }
                        }
                        return;
                    }
                    else if (ModifierKeys == (Keys.Control | Keys.Shift) && _selectedParagraph != null)
                    {
                        double seconds = XPositionToSeconds(e.X);
                        if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            _mouseDownParagraph = _selectedParagraph;
                            if (OnTimeChangedAndOffsetRest != null)
                                OnTimeChangedAndOffsetRest.Invoke(seconds, _mouseDownParagraph);
                        }
                        return;
                    }
                    else if (ModifierKeys == Keys.Alt && _selectedParagraph != null)
                    {
                        double seconds = XPositionToSeconds(e.X);
                        int milliseconds = (int)(seconds * 1000.0);
                        if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        {
                            _mouseDownParagraph = _selectedParagraph;
                            double durationMilliseconds = _mouseDownParagraph.Duration.TotalMilliseconds;
                            _mouseDownParagraph.StartTime.TotalMilliseconds = milliseconds;
                            _mouseDownParagraph.EndTime.TotalMilliseconds = _mouseDownParagraph.StartTime.TotalMilliseconds + durationMilliseconds;
                            if (OnTimeChanged != null)
                                OnTimeChanged.Invoke(seconds, _mouseDownParagraph);
                        }
                        return;
                    }

                    if (_mouseDownParagraphType == MouseDownParagraphType.None || _mouseDownParagraphType == MouseDownParagraphType.Whole)
                        OnSingleClick.Invoke(XPositionToSeconds(e.X), null);
                }
            }
        }

        private void WaveForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.None && e.KeyCode == Keys.Add)
            {
                ZoomFactor = ZoomFactor + 0.1;
                if (OnZoomedChanged != null)
                    OnZoomedChanged.Invoke(null, null);
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Subtract)
            {
                ZoomFactor = ZoomFactor - 0.1;
                if (OnZoomedChanged != null)
                    OnZoomedChanged.Invoke(null, null);
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

        void WaveForm_MouseWheel(object sender, MouseEventArgs e)
        {
            int delta = e.Delta;
            {
                if (Locked)
                {
                    OnPositionSelected.Invoke(_currentVideoPositionSeconds + (-delta / 256.0), null);
                }
                else
                {
                    StartPositionSeconds -= delta / 256.0;
                    if (_currentVideoPositionSeconds < StartPositionSeconds || _currentVideoPositionSeconds >= EndPositionSeconds)
                        OnPositionSelected.Invoke(StartPositionSeconds, null);
                }
                Invalidate();
            }
        }

    }
}
