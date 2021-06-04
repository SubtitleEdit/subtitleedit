// Author: Adalberto L. Simeone (Taranto, Italy)
// E-Mail: avengerdragon@gmail.com
// Website: http://www.avengersutd.com/blog
//
// This source code is Intellectual property of the Author
// and is released under the Creative Commons Attribution
// NonCommercial License, available at:
// http://creativecommons.org/licenses/by-nc/3.0/
// You can alter and use this source code as you wish,
// provided that you do not use the results in commercial
// projects, without the express and written consent of
// the Author.

using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.ColorChooser;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed class ColorChooser : Form
    {
        /// <summary>
        ///   Required designer variable.
        /// </summary>
        private ColorHandler.Argb _alphaRedGreenBlue;
        private ChangeStyle _changeType = ChangeStyle.None;
        private FlowLayoutPanel _flowLayoutPanel1;
        private ColorHandler.Hsv _hsv;
        private Label _label1;
        private Label _label2;
        private Label _labelRed;
        private Label _labelGreen;
        private Label _label5;
        private Label _labelBlue;
        private Label _labelAlpha1;
        private Label _lblAlpha2;
        private Label _lblBlue;
        private Label _lblGreen;
        private Label _lblHue;
        private Label _lblRed;
        private Label _lblSaturation;
        private Label _lblValue;
        private ColorWheel _myColorWheel;
        private Panel _pnlBrightness;
        private Panel _pnlColor;
        private Panel _pnlSelectedColor;
        private Point _selectedPoint;
        private TrackBar _tbAlpha;
        private TrackBar _tbBlue;
        private TrackBar _tbGreen;
        private TextBox _tbHexCode;
        private TrackBar _tbHue;
        private TrackBar _tbRed;
        private TrackBar _tbSaturation;
        private Button _buttonCancel;
        private Button _buttonOk;
        private TrackBar _tbValue;
        private bool _showAlpha = true;
        private readonly Timer _hexCodeEditTimer;
        private bool _hexEditOn;

        public ColorChooser()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.ColorChooser.Title;
            _labelRed.Text = LanguageSettings.Current.ColorChooser.Red;
            _labelGreen.Text = LanguageSettings.Current.ColorChooser.Green;
            _labelBlue.Text = LanguageSettings.Current.ColorChooser.Blue;
            _labelAlpha1.Text = LanguageSettings.Current.ColorChooser.Alpha;
            _buttonOk.Text = LanguageSettings.Current.General.Ok;
            _buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            _hexCodeEditTimer = new Timer { Interval = 100 };
            _hexCodeEditTimer.Tick += (sender, args) =>
            {
                if (_hexEditOn)
                {
                    CheckValidHexInput();
                }
            };
        }

        public bool ShowAlpha
        {
            get => _showAlpha;
            set
            {
                if (!value && ShowAlpha)
                {
                    Height -= 40;
                    _buttonOk.Top -= 40;
                    _buttonCancel.Top -= 40;
                }
                else if (value && !ShowAlpha)
                {
                    Height += 40;
                    _buttonOk.Top += 40;
                    _buttonCancel.Top += 40;
                }
                _labelAlpha1.Visible = value;
                _lblAlpha2.Visible = value;
                _tbAlpha.Visible = value;
                _showAlpha = value;
            }
        }

        public Color Color
        {
            // Get or set the color to be
            // displayed in the color wheel.
            get => Color.FromArgb(_tbAlpha.Value, _tbRed.Value, _tbGreen.Value, _tbBlue.Value);
            set
            {
                // Indicate the color change type. Either RGB or HSV
                // will cause the color wheel to update the position
                // of the pointer.
                _changeType = ChangeStyle.RGB;
                _alphaRedGreenBlue = new ColorHandler.Argb(value.A, value.R, value.G, value.B);
                _hsv = ColorHandler.RgbToHsv(_alphaRedGreenBlue);
            }
        }

        private void ColorChooserLoad(object sender, EventArgs e)
        {
            // Turn on double-buffering, so the form looks better.
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            // These properties are set in design view, as well, but they
            // have to be set to false in order for the Paint
            // event to be able to display their contents.
            // Never hurts to make sure they're invisible.
            _pnlSelectedColor.Visible = false;
            _pnlBrightness.Visible = false;
            _pnlColor.Visible = false;

            // Calculate the coordinates of the three
            // required regions on the form.
            var selectedColorRectangle = new Rectangle(_pnlSelectedColor.Location, _pnlSelectedColor.Size);
            var brightnessRectangle = new Rectangle(_pnlBrightness.Location, _pnlBrightness.Size);
            var colorRectangle = new Rectangle(_pnlColor.Location, _pnlColor.Size);

            // Create the new ColorWheel class, indicating
            // the locations of the color wheel itself, the
            // brightness area, and the position of the selected color.
            _myColorWheel = new ColorWheel(colorRectangle, brightnessRectangle, selectedColorRectangle);
            _myColorWheel.ColorChanged += MyColorWheelColorChanged;

            // Set the RGB and HSV values
            // of the NumericUpDown controls.
            SetRgb(_alphaRedGreenBlue);
            SetHsv(_hsv);
        }

        private void HandleMouse(object sender, MouseEventArgs e)
        {
            // If you have the left mouse button down,
            // then update the selectedPoint value and
            // force a repaint of the color wheel.
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            _changeType = ChangeStyle.MouseMove;
            _selectedPoint = new Point(e.X, e.Y);
            Invalidate();
        }

        private void FormMainMouseUp(object sender, MouseEventArgs e)
        {
            _myColorWheel.SetMouseUp();
            _changeType = ChangeStyle.None;
        }

        private void SetRgbLabels(ColorHandler.Argb argb)
        {
            RefreshText(_lblRed, argb.Red);
            RefreshText(_lblBlue, argb.Blue);
            RefreshText(_lblGreen, argb.Green);
            RefreshText(_lblAlpha2, argb.Alpha);
            if (_showAlpha)
            {
                _tbHexCode.Text = $"{argb.Alpha:X2}{argb.Red:X2}{argb.Green:X2}{argb.Blue:X2}";
            }
            else
            {
                _tbHexCode.Text = $"{argb.Red:X2}{argb.Green:X2}{argb.Blue:X2}";
            }
        }

        private void SetHsvLabels(ColorHandler.Hsv hsv)
        {
            RefreshText(_lblHue, hsv.Hue);
            RefreshText(_lblSaturation, hsv.Saturation);
            RefreshText(_lblValue, hsv.Value);
            RefreshText(_lblAlpha2, hsv.Alpha);
        }

        private void SetRgb(ColorHandler.Argb argb)
        {
            // Update the RGB values on the form.
            RefreshValue(_tbRed, argb.Red);
            RefreshValue(_tbBlue, argb.Blue);
            RefreshValue(_tbGreen, argb.Green);
            RefreshValue(_tbAlpha, argb.Alpha);
            SetRgbLabels(argb);
        }

        private void SetHsv(ColorHandler.Hsv hsv)
        {
            // Update the HSV values on the form.
            RefreshValue(_tbHue, hsv.Hue);
            RefreshValue(_tbSaturation, hsv.Saturation);
            RefreshValue(_tbValue, hsv.Value);
            RefreshValue(_tbAlpha, hsv.Alpha);
            SetHsvLabels(hsv);
        }

        private static void RefreshValue(TrackBar hsv, int value)
        {
            hsv.Value = value;
        }

        private static void RefreshText(Control lbl, int value)
        {
            lbl.Text = value.ToString();
        }

        private void MyColorWheelColorChanged(object sender, ColorChangedEventArgs e)
        {
            SetRgb(e.ARGB);
            SetHsv(e.HSV);
        }

        private void HandleHsvScroll(object sender, EventArgs e)
        // If the H, S, or V values change, use this
        // code to update the RGB values and invalidate
        // the color wheel (so it updates the pointers).
        // Check the isInUpdate flag to avoid recursive events
        // when you update the NumericUpdownControls.
        {
            _changeType = ChangeStyle.HSV;
            _hsv = new ColorHandler.Hsv(_tbAlpha.Value, _tbHue.Value, _tbSaturation.Value, _tbValue.Value);
            SetRgb(ColorHandler.HsvToRgb(_hsv));
            SetHsvLabels(_hsv);
            Invalidate();
        }

        private void HandleRgbScroll(object sender, EventArgs e)
        {
            // If the R, G, or B values change, use this
            // code to update the HSV values and invalidate
            // the color wheel (so it updates the pointers).
            // Check the isInUpdate flag to avoid recursive events
            // when you update the NumericUpdownControls.
            _changeType = ChangeStyle.RGB;
            _alphaRedGreenBlue = new ColorHandler.Argb(_tbAlpha.Value, _tbRed.Value, _tbGreen.Value, _tbBlue.Value);
            SetHsv(ColorHandler.RgbToHsv(_alphaRedGreenBlue));
            SetRgbLabels(_alphaRedGreenBlue);
            Invalidate();
        }

        private void TbAlphaScroll(object sender, EventArgs e)
        {
            _changeType = ChangeStyle.RGB;
            _alphaRedGreenBlue = new ColorHandler.Argb(_tbAlpha.Value, _tbRed.Value, _tbGreen.Value, _tbBlue.Value);
            RefreshText(_lblAlpha2, _tbAlpha.Value);
            _tbHexCode.Text = $"{_alphaRedGreenBlue.Alpha:X2}{_alphaRedGreenBlue.Red:X2}{_alphaRedGreenBlue.Green:X2}{_alphaRedGreenBlue.Blue:X2}";
            Invalidate();
        }

        private void ColorChooserPaint(object sender, PaintEventArgs e)
        {
            // Depending on the circumstances, force a repaint
            // of the color wheel passing different information.
            switch (_changeType)
            {
                case ChangeStyle.HSV:
                    _myColorWheel.Draw(e.Graphics, _hsv);
                    break;
                case ChangeStyle.MouseMove:
                case ChangeStyle.None:
                    _myColorWheel.Draw(e.Graphics, _selectedPoint);
                    break;
                case ChangeStyle.RGB:
                    _myColorWheel.Draw(e.Graphics, _alphaRedGreenBlue);
                    break;
            }
        }

        private void TbHexCodeMouseDown(object sender, MouseEventArgs e)
        {
            _tbHexCode.SelectionStart = 0;
            _tbHexCode.SelectionLength = _tbHexCode.Text.Length;
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///   Required method for Designer support - do not modify
        ///   the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._lblBlue = new System.Windows.Forms.Label();
            this._lblGreen = new System.Windows.Forms.Label();
            this._lblRed = new System.Windows.Forms.Label();
            this._lblValue = new System.Windows.Forms.Label();
            this._lblSaturation = new System.Windows.Forms.Label();
            this._lblHue = new System.Windows.Forms.Label();
            this._pnlColor = new System.Windows.Forms.Panel();
            this._label5 = new System.Windows.Forms.Label();
            this._pnlBrightness = new System.Windows.Forms.Panel();
            this._lblAlpha2 = new System.Windows.Forms.Label();
            this._tbHexCode = new System.Windows.Forms.TextBox();
            this._flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this._labelRed = new System.Windows.Forms.Label();
            this._tbRed = new System.Windows.Forms.TrackBar();
            this._labelGreen = new System.Windows.Forms.Label();
            this._tbGreen = new System.Windows.Forms.TrackBar();
            this._labelBlue = new System.Windows.Forms.Label();
            this._tbBlue = new System.Windows.Forms.TrackBar();
            this._labelAlpha1 = new System.Windows.Forms.Label();
            this._tbAlpha = new System.Windows.Forms.TrackBar();
            this._tbHue = new System.Windows.Forms.TrackBar();
            this._label1 = new System.Windows.Forms.Label();
            this._tbSaturation = new System.Windows.Forms.TrackBar();
            this._label2 = new System.Windows.Forms.Label();
            this._tbValue = new System.Windows.Forms.TrackBar();
            this._pnlSelectedColor = new System.Windows.Forms.Panel();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this._flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._tbRed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._tbGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._tbBlue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._tbAlpha)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._tbHue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._tbSaturation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._tbValue)).BeginInit();
            this.SuspendLayout();
            // 
            // _lblBlue
            // 
            this._lblBlue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblBlue.Location = new System.Drawing.Point(340, 70);
            this._lblBlue.Name = "_lblBlue";
            this._lblBlue.Size = new System.Drawing.Size(39, 23);
            this._lblBlue.TabIndex = 54;
            this._lblBlue.Text = "Blue";
            this._lblBlue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _lblGreen
            // 
            this._lblGreen.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblGreen.Location = new System.Drawing.Point(340, 35);
            this._lblGreen.Name = "_lblGreen";
            this._lblGreen.Size = new System.Drawing.Size(39, 23);
            this._lblGreen.TabIndex = 53;
            this._lblGreen.Text = "Green";
            this._lblGreen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _lblRed
            // 
            this._lblRed.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblRed.Location = new System.Drawing.Point(340, 0);
            this._lblRed.Name = "_lblRed";
            this._lblRed.Size = new System.Drawing.Size(39, 23);
            this._lblRed.TabIndex = 52;
            this._lblRed.Text = "Red";
            this._lblRed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _lblValue
            // 
            this._lblValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblValue.Location = new System.Drawing.Point(623, 217);
            this._lblValue.Name = "_lblValue";
            this._lblValue.Size = new System.Drawing.Size(39, 23);
            this._lblValue.TabIndex = 51;
            this._lblValue.Text = "Value";
            this._lblValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._lblValue.Visible = false;
            // 
            // _lblSaturation
            // 
            this._lblSaturation.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblSaturation.Location = new System.Drawing.Point(623, 182);
            this._lblSaturation.Name = "_lblSaturation";
            this._lblSaturation.Size = new System.Drawing.Size(39, 23);
            this._lblSaturation.TabIndex = 50;
            this._lblSaturation.Text = "Sat";
            this._lblSaturation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._lblSaturation.Visible = false;
            // 
            // _lblHue
            // 
            this._lblHue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblHue.Location = new System.Drawing.Point(623, 155);
            this._lblHue.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this._lblHue.Name = "_lblHue";
            this._lblHue.Size = new System.Drawing.Size(41, 23);
            this._lblHue.TabIndex = 49;
            this._lblHue.Text = "Hue";
            this._lblHue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._lblHue.Visible = false;
            // 
            // _pnlColor
            // 
            this._pnlColor.Location = new System.Drawing.Point(5, 8);
            this._pnlColor.Name = "_pnlColor";
            this._pnlColor.Size = new System.Drawing.Size(224, 216);
            this._pnlColor.TabIndex = 38;
            this._pnlColor.Visible = false;
            // 
            // _label5
            // 
            this._label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._label5.Location = new System.Drawing.Point(304, 155);
            this._label5.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this._label5.Name = "_label5";
            this._label5.Size = new System.Drawing.Size(72, 18);
            this._label5.TabIndex = 35;
            this._label5.Text = "Hue";
            this._label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._label5.Visible = false;
            // 
            // _pnlBrightness
            // 
            this._pnlBrightness.Location = new System.Drawing.Point(254, 8);
            this._pnlBrightness.Name = "_pnlBrightness";
            this._pnlBrightness.Size = new System.Drawing.Size(24, 216);
            this._pnlBrightness.TabIndex = 39;
            this._pnlBrightness.Visible = false;
            // 
            // _lblAlpha2
            // 
            this._lblAlpha2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblAlpha2.Location = new System.Drawing.Point(340, 111);
            this._lblAlpha2.Name = "_lblAlpha2";
            this._lblAlpha2.Size = new System.Drawing.Size(39, 24);
            this._lblAlpha2.TabIndex = 57;
            this._lblAlpha2.Text = "Alpha";
            this._lblAlpha2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _tbHexCode
            // 
            this._tbHexCode.BackColor = System.Drawing.Color.White;
            this._tbHexCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._tbHexCode.Location = new System.Drawing.Point(295, 50);
            this._tbHexCode.MaxLength = 8;
            this._tbHexCode.Name = "_tbHexCode";
            this._tbHexCode.ReadOnly = true;
            this._tbHexCode.Size = new System.Drawing.Size(96, 22);
            this._tbHexCode.TabIndex = 58;
            this._tbHexCode.TextChanged += new System.EventHandler(this._tbHexCode_TextChanged);
            this._tbHexCode.Enter += new System.EventHandler(this._tbHexCode_Enter);
            this._tbHexCode.Leave += new System.EventHandler(this._tbHexCode_Leave);
            this._tbHexCode.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TbHexCodeMouseDown);
            // 
            // _flowLayoutPanel1
            // 
            this._flowLayoutPanel1.Controls.Add(this._labelRed);
            this._flowLayoutPanel1.Controls.Add(this._tbRed);
            this._flowLayoutPanel1.Controls.Add(this._lblRed);
            this._flowLayoutPanel1.Controls.Add(this._labelGreen);
            this._flowLayoutPanel1.Controls.Add(this._tbGreen);
            this._flowLayoutPanel1.Controls.Add(this._lblGreen);
            this._flowLayoutPanel1.Controls.Add(this._labelBlue);
            this._flowLayoutPanel1.Controls.Add(this._tbBlue);
            this._flowLayoutPanel1.Controls.Add(this._lblBlue);
            this._flowLayoutPanel1.Controls.Add(this._labelAlpha1);
            this._flowLayoutPanel1.Controls.Add(this._tbAlpha);
            this._flowLayoutPanel1.Controls.Add(this._lblAlpha2);
            this._flowLayoutPanel1.Location = new System.Drawing.Point(5, 232);
            this._flowLayoutPanel1.Name = "_flowLayoutPanel1";
            this._flowLayoutPanel1.Size = new System.Drawing.Size(396, 157);
            this._flowLayoutPanel1.TabIndex = 59;
            // 
            // _labelRed
            // 
            this._labelRed.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labelRed.Location = new System.Drawing.Point(3, 8);
            this._labelRed.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this._labelRed.Name = "_labelRed";
            this._labelRed.Size = new System.Drawing.Size(72, 18);
            this._labelRed.TabIndex = 42;
            this._labelRed.Text = "Red";
            this._labelRed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _tbRed
            // 
            this._tbRed.AutoSize = false;
            this._tbRed.LargeChange = 16;
            this._tbRed.Location = new System.Drawing.Point(78, 3);
            this._tbRed.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this._tbRed.Maximum = 255;
            this._tbRed.Name = "_tbRed";
            this._tbRed.Size = new System.Drawing.Size(256, 32);
            this._tbRed.TabIndex = 43;
            this._tbRed.TickFrequency = 32;
            this._tbRed.Scroll += new System.EventHandler(this.HandleRgbScroll);
            // 
            // _labelGreen
            // 
            this._labelGreen.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labelGreen.Location = new System.Drawing.Point(3, 43);
            this._labelGreen.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this._labelGreen.Name = "_labelGreen";
            this._labelGreen.Size = new System.Drawing.Size(72, 18);
            this._labelGreen.TabIndex = 44;
            this._labelGreen.Text = "Green";
            this._labelGreen.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _tbGreen
            // 
            this._tbGreen.AutoSize = false;
            this._tbGreen.LargeChange = 16;
            this._tbGreen.Location = new System.Drawing.Point(78, 38);
            this._tbGreen.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this._tbGreen.Maximum = 255;
            this._tbGreen.Name = "_tbGreen";
            this._tbGreen.Size = new System.Drawing.Size(256, 32);
            this._tbGreen.TabIndex = 45;
            this._tbGreen.TickFrequency = 32;
            this._tbGreen.Scroll += new System.EventHandler(this.HandleRgbScroll);
            // 
            // _labelBlue
            // 
            this._labelBlue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labelBlue.Location = new System.Drawing.Point(3, 78);
            this._labelBlue.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this._labelBlue.Name = "_labelBlue";
            this._labelBlue.Size = new System.Drawing.Size(72, 18);
            this._labelBlue.TabIndex = 46;
            this._labelBlue.Text = "Blue";
            this._labelBlue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _tbBlue
            // 
            this._tbBlue.AutoSize = false;
            this._tbBlue.LargeChange = 16;
            this._tbBlue.Location = new System.Drawing.Point(78, 73);
            this._tbBlue.Margin = new System.Windows.Forms.Padding(0, 3, 3, 6);
            this._tbBlue.Maximum = 255;
            this._tbBlue.Name = "_tbBlue";
            this._tbBlue.Size = new System.Drawing.Size(256, 32);
            this._tbBlue.TabIndex = 47;
            this._tbBlue.TickFrequency = 32;
            this._tbBlue.Scroll += new System.EventHandler(this.HandleRgbScroll);
            // 
            // _labelAlpha1
            // 
            this._labelAlpha1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labelAlpha1.Location = new System.Drawing.Point(3, 119);
            this._labelAlpha1.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this._labelAlpha1.Name = "_labelAlpha1";
            this._labelAlpha1.Size = new System.Drawing.Size(72, 18);
            this._labelAlpha1.TabIndex = 55;
            this._labelAlpha1.Text = "Alpha";
            this._labelAlpha1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _tbAlpha
            // 
            this._tbAlpha.AutoSize = false;
            this._tbAlpha.LargeChange = 16;
            this._tbAlpha.Location = new System.Drawing.Point(78, 114);
            this._tbAlpha.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this._tbAlpha.Maximum = 255;
            this._tbAlpha.Name = "_tbAlpha";
            this._tbAlpha.Size = new System.Drawing.Size(256, 32);
            this._tbAlpha.TabIndex = 56;
            this._tbAlpha.TickFrequency = 32;
            this._tbAlpha.Scroll += new System.EventHandler(this.TbAlphaScroll);
            // 
            // _tbHue
            // 
            this._tbHue.AutoSize = false;
            this._tbHue.LargeChange = 16;
            this._tbHue.Location = new System.Drawing.Point(379, 150);
            this._tbHue.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this._tbHue.Maximum = 255;
            this._tbHue.Name = "_tbHue";
            this._tbHue.Size = new System.Drawing.Size(238, 32);
            this._tbHue.TabIndex = 36;
            this._tbHue.TickFrequency = 32;
            this._tbHue.Visible = false;
            this._tbHue.Scroll += new System.EventHandler(this.HandleHsvScroll);
            // 
            // _label1
            // 
            this._label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._label1.Location = new System.Drawing.Point(304, 190);
            this._label1.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this._label1.Name = "_label1";
            this._label1.Size = new System.Drawing.Size(72, 18);
            this._label1.TabIndex = 38;
            this._label1.Text = "Saturation";
            this._label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._label1.Visible = false;
            // 
            // _tbSaturation
            // 
            this._tbSaturation.AutoSize = false;
            this._tbSaturation.LargeChange = 16;
            this._tbSaturation.Location = new System.Drawing.Point(379, 185);
            this._tbSaturation.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this._tbSaturation.Maximum = 255;
            this._tbSaturation.Name = "_tbSaturation";
            this._tbSaturation.Size = new System.Drawing.Size(238, 32);
            this._tbSaturation.TabIndex = 39;
            this._tbSaturation.TickFrequency = 32;
            this._tbSaturation.Visible = false;
            this._tbSaturation.Scroll += new System.EventHandler(this.HandleHsvScroll);
            // 
            // _label2
            // 
            this._label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._label2.Location = new System.Drawing.Point(304, 225);
            this._label2.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this._label2.Name = "_label2";
            this._label2.Size = new System.Drawing.Size(72, 18);
            this._label2.TabIndex = 40;
            this._label2.Text = "Value";
            this._label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._label2.Visible = false;
            // 
            // _tbValue
            // 
            this._tbValue.AutoSize = false;
            this._tbValue.LargeChange = 16;
            this._tbValue.Location = new System.Drawing.Point(379, 220);
            this._tbValue.Margin = new System.Windows.Forms.Padding(0, 3, 3, 6);
            this._tbValue.Maximum = 255;
            this._tbValue.Name = "_tbValue";
            this._tbValue.Size = new System.Drawing.Size(238, 32);
            this._tbValue.TabIndex = 41;
            this._tbValue.TickFrequency = 32;
            this._tbValue.Visible = false;
            this._tbValue.Scroll += new System.EventHandler(this.HandleHsvScroll);
            // 
            // _pnlSelectedColor
            // 
            this._pnlSelectedColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this._pnlSelectedColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._pnlSelectedColor.Location = new System.Drawing.Point(295, 12);
            this._pnlSelectedColor.Name = "_pnlSelectedColor";
            this._pnlSelectedColor.Size = new System.Drawing.Size(96, 32);
            this._pnlSelectedColor.TabIndex = 40;
            this._pnlSelectedColor.Visible = false;
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonCancel.Location = new System.Drawing.Point(321, 406);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(80, 23);
            this._buttonCancel.TabIndex = 61;
            this._buttonCancel.Text = "C&ancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            this._buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOk.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonOk.Location = new System.Drawing.Point(235, 406);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(80, 23);
            this._buttonOk.TabIndex = 60;
            this._buttonOk.Text = "&OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // ColorChooser
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(413, 441);
            this.Controls.Add(this._buttonCancel);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._label5);
            this.Controls.Add(this._tbHue);
            this.Controls.Add(this._tbHexCode);
            this.Controls.Add(this._lblHue);
            this.Controls.Add(this._label1);
            this.Controls.Add(this._tbSaturation);
            this.Controls.Add(this._pnlColor);
            this.Controls.Add(this._lblSaturation);
            this.Controls.Add(this._pnlSelectedColor);
            this.Controls.Add(this._label2);
            this.Controls.Add(this._pnlBrightness);
            this.Controls.Add(this._tbValue);
            this.Controls.Add(this._flowLayoutPanel1);
            this.Controls.Add(this._lblValue);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ColorChooser";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select color";
            this.Load += new System.EventHandler(this.ColorChooserLoad);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ColorChooserPaint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ColorChooser_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HandleMouse);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouse);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FormMainMouseUp);
            this._flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._tbRed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._tbGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._tbBlue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._tbAlpha)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._tbHue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._tbSaturation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._tbValue)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion Windows Form Designer generated code

        private enum ChangeStyle
        {
            MouseMove,
            RGB,
            HSV,
            None
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ColorChooser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hexCodeEditTimer.Dispose();
                base.Dispose(true);
                if (_myColorWheel != null)
                {
                    _myColorWheel.Dispose();
                    _myColorWheel = null;
                }
            }
        }

        private void _tbHexCode_TextChanged(object sender, EventArgs e)
        {
            if (_hexEditOn)
            {
                CheckValidHexInput();
            }
        }

        private void _tbHexCode_Enter(object sender, EventArgs e)
        {
            _hexEditOn = true;
            _tbHexCode.ReadOnly = false;
        }

        private void _tbHexCode_Leave(object sender, EventArgs e)
        {
            _hexEditOn = false;
            _tbHexCode.BackColor = UiUtil.BackColor;
        }

        private void CheckValidHexInput()
        {
            var hexString = _tbHexCode.Text.Trim();
            if (hexString.Length == 6 && !_showAlpha && IsValidHexString(hexString))
            {
                UpdateRgb("ff" + hexString);
            }
            else if (hexString.Length == 8 && _showAlpha && IsValidHexString(hexString))
            {
                UpdateRgb(hexString);
            }
            else
            {
                _tbHexCode.BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
            }
        }

        private void UpdateRgb(string hexString)
        {
            _tbAlpha.Value = Convert.ToInt32(hexString.Substring(0, 2), 16);
            _tbRed.Value = Convert.ToInt32(hexString.Substring(2, 2), 16);
            _tbGreen.Value = Convert.ToInt32(hexString.Substring(4, 2), 16);
            _tbBlue.Value = Convert.ToInt32(hexString.Substring(6, 2), 16);
            _tbHexCode.BackColor = UiUtil.BackColor;
            HandleRgbScroll(null, null);
        }

        private bool IsValidHexString(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                return false;
            }

            foreach (var ch in hexString)
            {
                if (!CharUtils.IsHexadecimal(ch))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
