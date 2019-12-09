#region #Disclaimer

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

#endregion #Disclaimer

#region Using directives

using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.ColorChooser;
using System;
using System.Drawing;
using System.Windows.Forms;

#endregion Using directives

namespace Nikse.SubtitleEdit.Forms
{
    /// <summary>
    ///   Summary description for ColorChooser.
    /// </summary>
    public sealed class ColorChooser : Form
    {
        /// <summary>
        ///   Required designer variable.
        /// </summary>
        private ColorHandler.ARGB argb;
        private ChangeStyle changeType = ChangeStyle.None;
        private FlowLayoutPanel flowLayoutPanel1;
        private ColorHandler.HSV hsv;
        private Label label1;
        private Label label2;
        private Label labelRed;
        private Label labelGreen;
        private Label label5;
        private Label labelBlue;
        private Label labelAlpha1;
        private Label lblAlpha2;
        private Label lblBlue;
        private Label lblGreen;
        private Label lblHue;
        private Label lblRed;
        private Label lblSaturation;
        private Label lblValue;
        private ColorWheel myColorWheel;
        private Panel pnlBrightness;
        private Panel pnlColor;
        private Panel pnlSelectedColor;
        private Point selectedPoint;
        private TrackBar tbAlpha;
        private TrackBar tbBlue;
        private TrackBar tbGreen;
        private TextBox tbHexCode;
        private TrackBar tbHue;
        private TrackBar tbRed;
        private TrackBar tbSaturation;
        private Button buttonCancel;
        private Button buttonOK;
        private TrackBar tbValue;
        private bool _showAlpha = true;

        public ColorChooser()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = Configuration.Settings.Language.ColorChooser.Title;
            labelRed.Text = Configuration.Settings.Language.ColorChooser.Red;
            labelGreen.Text = Configuration.Settings.Language.ColorChooser.Green;
            labelBlue.Text = Configuration.Settings.Language.ColorChooser.Blue;
            labelAlpha1.Text = Configuration.Settings.Language.ColorChooser.Alpha;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
        }

        public bool ShowAlpha
        {
            get
            {
                return _showAlpha;
            }
            set
            {
                if (!value && ShowAlpha)
                {
                    Height -= 40;
                    buttonOK.Top -= 40;
                    buttonCancel.Top -= 40;
                }
                else if (value && !ShowAlpha)
                {
                    Height += 40;
                    buttonOK.Top += 40;
                    buttonCancel.Top += 40;
                }
                labelAlpha1.Visible = value;
                lblAlpha2.Visible = value;
                tbAlpha.Visible = value;
                _showAlpha = value;
            }
        }

        public Color Color
        {
            // Get or set the color to be
            // displayed in the color wheel.
            get
            {
                return Color.FromArgb(tbAlpha.Value, tbRed.Value, tbGreen.Value, tbBlue.Value);
            }
            set
            {
                // Indicate the color change type. Either RGB or HSV
                // will cause the color wheel to update the position
                // of the pointer.
                changeType = ChangeStyle.RGB;
                argb = new ColorHandler.ARGB(value.A, value.R, value.G, value.B);
                hsv = ColorHandler.RGBtoHSV(argb);
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
            pnlSelectedColor.Visible = false;
            pnlBrightness.Visible = false;
            pnlColor.Visible = false;

            // Calculate the coordinates of the three
            // required regions on the form.
            Rectangle selectedColorRectangle = new Rectangle(pnlSelectedColor.Location, pnlSelectedColor.Size);
            Rectangle brightnessRectangle = new Rectangle(pnlBrightness.Location, pnlBrightness.Size);
            Rectangle colorRectangle = new Rectangle(pnlColor.Location, pnlColor.Size);

            // Create the new ColorWheel class, indicating
            // the locations of the color wheel itself, the
            // brightness area, and the position of the selected color.
            myColorWheel = new ColorWheel(colorRectangle, brightnessRectangle, selectedColorRectangle);
            myColorWheel.ColorChanged += MyColorWheelColorChanged;

            // Set the RGB and HSV values
            // of the NumericUpDown controls.
            SetRGB(argb);
            SetHSV(hsv);
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

            changeType = ChangeStyle.MouseMove;
            selectedPoint = new Point(e.X, e.Y);
            Invalidate();
        }

        private void FormMainMouseUp(object sender, MouseEventArgs e)
        {
            myColorWheel.SetMouseUp();
            changeType = ChangeStyle.None;
        }

        private void SetRGBLabels(ColorHandler.ARGB argb)
        {
            RefreshText(lblRed, argb.Red);
            RefreshText(lblBlue, argb.Blue);
            RefreshText(lblGreen, argb.Green);
            RefreshText(lblAlpha2, argb.Alpha);
            if (_showAlpha)
            {
                tbHexCode.Text = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", argb.Alpha, argb.Red, argb.Green, argb.Blue);
            }
            else
            {
                tbHexCode.Text = string.Format("{0:X2}{1:X2}{2:X2}", argb.Red, argb.Green, argb.Blue);
            }
        }

        private void SetHSVLabels(ColorHandler.HSV HSV)
        {
            RefreshText(lblHue, HSV.Hue);
            RefreshText(lblSaturation, HSV.Saturation);
            RefreshText(lblValue, HSV.Value);
            RefreshText(lblAlpha2, HSV.Alpha);
        }

        private void SetRGB(ColorHandler.ARGB argb)
        {
            // Update the RGB values on the form.
            RefreshValue(tbRed, argb.Red);
            RefreshValue(tbBlue, argb.Blue);
            RefreshValue(tbGreen, argb.Green);
            RefreshValue(tbAlpha, argb.Alpha);
            SetRGBLabels(argb);
        }

        private void SetHSV(ColorHandler.HSV HSV)
        {
            // Update the HSV values on the form.
            RefreshValue(tbHue, HSV.Hue);
            RefreshValue(tbSaturation, HSV.Saturation);
            RefreshValue(tbValue, HSV.Value);
            RefreshValue(tbAlpha, HSV.Alpha);
            SetHSVLabels(HSV);
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
            SetRGB(e.ARGB);
            SetHSV(e.HSV);
        }

        private void HandleHSVScroll(object sender, EventArgs e)
        // If the H, S, or V values change, use this
        // code to update the RGB values and invalidate
        // the color wheel (so it updates the pointers).
        // Check the isInUpdate flag to avoid recursive events
        // when you update the NumericUpdownControls.
        {
            changeType = ChangeStyle.HSV;
            hsv = new ColorHandler.HSV(tbAlpha.Value, tbHue.Value, tbSaturation.Value, tbValue.Value);
            SetRGB(ColorHandler.HSVtoRGB(hsv));
            SetHSVLabels(hsv);
            Invalidate();
        }

        private void HandleRGBScroll(object sender, EventArgs e)
        {
            // If the R, G, or B values change, use this
            // code to update the HSV values and invalidate
            // the color wheel (so it updates the pointers).
            // Check the isInUpdate flag to avoid recursive events
            // when you update the NumericUpdownControls.
            changeType = ChangeStyle.RGB;
            argb = new ColorHandler.ARGB(tbAlpha.Value, tbRed.Value, tbGreen.Value, tbBlue.Value);
            SetHSV(ColorHandler.RGBtoHSV(argb));
            SetRGBLabels(argb);
            Invalidate();
        }

        private void TbAlphaScroll(object sender, EventArgs e)
        {
            changeType = ChangeStyle.RGB;
            argb = new ColorHandler.ARGB(tbAlpha.Value, tbRed.Value, tbGreen.Value, tbBlue.Value);
            RefreshText(lblAlpha2, tbAlpha.Value);
            tbHexCode.Text = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", argb.Alpha, argb.Red, argb.Green, argb.Blue);
            Invalidate();
        }

        private void ColorChooserPaint(object sender, PaintEventArgs e)
        {
            // Depending on the circumstances, force a repaint
            // of the color wheel passing different information.
            switch (changeType)
            {
                case ChangeStyle.HSV:
                    myColorWheel.Draw(e.Graphics, hsv);
                    break;
                case ChangeStyle.MouseMove:
                case ChangeStyle.None:
                    myColorWheel.Draw(e.Graphics, selectedPoint);
                    break;
                case ChangeStyle.RGB:
                    myColorWheel.Draw(e.Graphics, argb);
                    break;
            }
        }

        private void TbHexCodeMouseDown(object sender, MouseEventArgs e)
        {
            tbHexCode.SelectionStart = 0;
            tbHexCode.SelectionLength = tbHexCode.Text.Length;
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///   Required method for Designer support - do not modify
        ///   the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblBlue = new System.Windows.Forms.Label();
            this.lblGreen = new System.Windows.Forms.Label();
            this.lblRed = new System.Windows.Forms.Label();
            this.lblValue = new System.Windows.Forms.Label();
            this.lblSaturation = new System.Windows.Forms.Label();
            this.lblHue = new System.Windows.Forms.Label();
            this.pnlColor = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.pnlBrightness = new System.Windows.Forms.Panel();
            this.lblAlpha2 = new System.Windows.Forms.Label();
            this.tbHexCode = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.labelRed = new System.Windows.Forms.Label();
            this.tbRed = new System.Windows.Forms.TrackBar();
            this.labelGreen = new System.Windows.Forms.Label();
            this.tbGreen = new System.Windows.Forms.TrackBar();
            this.labelBlue = new System.Windows.Forms.Label();
            this.tbBlue = new System.Windows.Forms.TrackBar();
            this.labelAlpha1 = new System.Windows.Forms.Label();
            this.tbAlpha = new System.Windows.Forms.TrackBar();
            this.tbHue = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.tbSaturation = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.tbValue = new System.Windows.Forms.TrackBar();
            this.pnlSelectedColor = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbRed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbBlue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbAlpha)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbHue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbSaturation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbValue)).BeginInit();
            this.SuspendLayout();
            // 
            // lblBlue
            // 
            this.lblBlue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBlue.Location = new System.Drawing.Point(340, 70);
            this.lblBlue.Name = "lblBlue";
            this.lblBlue.Size = new System.Drawing.Size(39, 23);
            this.lblBlue.TabIndex = 54;
            this.lblBlue.Text = "Blue";
            this.lblBlue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblGreen
            // 
            this.lblGreen.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGreen.Location = new System.Drawing.Point(340, 35);
            this.lblGreen.Name = "lblGreen";
            this.lblGreen.Size = new System.Drawing.Size(39, 23);
            this.lblGreen.TabIndex = 53;
            this.lblGreen.Text = "Green";
            this.lblGreen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblRed
            // 
            this.lblRed.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRed.Location = new System.Drawing.Point(340, 0);
            this.lblRed.Name = "lblRed";
            this.lblRed.Size = new System.Drawing.Size(39, 23);
            this.lblRed.TabIndex = 52;
            this.lblRed.Text = "Red";
            this.lblRed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblValue
            // 
            this.lblValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblValue.Location = new System.Drawing.Point(623, 217);
            this.lblValue.Name = "lblValue";
            this.lblValue.Size = new System.Drawing.Size(39, 23);
            this.lblValue.TabIndex = 51;
            this.lblValue.Text = "Value";
            this.lblValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblValue.Visible = false;
            // 
            // lblSaturation
            // 
            this.lblSaturation.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSaturation.Location = new System.Drawing.Point(623, 182);
            this.lblSaturation.Name = "lblSaturation";
            this.lblSaturation.Size = new System.Drawing.Size(39, 23);
            this.lblSaturation.TabIndex = 50;
            this.lblSaturation.Text = "Sat";
            this.lblSaturation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSaturation.Visible = false;
            // 
            // lblHue
            // 
            this.lblHue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHue.Location = new System.Drawing.Point(623, 155);
            this.lblHue.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.lblHue.Name = "lblHue";
            this.lblHue.Size = new System.Drawing.Size(41, 23);
            this.lblHue.TabIndex = 49;
            this.lblHue.Text = "Hue";
            this.lblHue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblHue.Visible = false;
            // 
            // pnlColor
            // 
            this.pnlColor.Location = new System.Drawing.Point(5, 8);
            this.pnlColor.Name = "pnlColor";
            this.pnlColor.Size = new System.Drawing.Size(224, 216);
            this.pnlColor.TabIndex = 38;
            this.pnlColor.Visible = false;
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(304, 155);
            this.label5.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 18);
            this.label5.TabIndex = 35;
            this.label5.Text = "Hue";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label5.Visible = false;
            // 
            // pnlBrightness
            // 
            this.pnlBrightness.Location = new System.Drawing.Point(254, 8);
            this.pnlBrightness.Name = "pnlBrightness";
            this.pnlBrightness.Size = new System.Drawing.Size(24, 216);
            this.pnlBrightness.TabIndex = 39;
            this.pnlBrightness.Visible = false;
            // 
            // lblAlpha2
            // 
            this.lblAlpha2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAlpha2.Location = new System.Drawing.Point(340, 111);
            this.lblAlpha2.Name = "lblAlpha2";
            this.lblAlpha2.Size = new System.Drawing.Size(39, 24);
            this.lblAlpha2.TabIndex = 57;
            this.lblAlpha2.Text = "Alpha";
            this.lblAlpha2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbHexCode
            // 
            this.tbHexCode.BackColor = System.Drawing.Color.White;
            this.tbHexCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbHexCode.Location = new System.Drawing.Point(295, 50);
            this.tbHexCode.MaxLength = 8;
            this.tbHexCode.Name = "tbHexCode";
            this.tbHexCode.ReadOnly = true;
            this.tbHexCode.Size = new System.Drawing.Size(96, 22);
            this.tbHexCode.TabIndex = 58;
            this.tbHexCode.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TbHexCodeMouseDown);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.labelRed);
            this.flowLayoutPanel1.Controls.Add(this.tbRed);
            this.flowLayoutPanel1.Controls.Add(this.lblRed);
            this.flowLayoutPanel1.Controls.Add(this.labelGreen);
            this.flowLayoutPanel1.Controls.Add(this.tbGreen);
            this.flowLayoutPanel1.Controls.Add(this.lblGreen);
            this.flowLayoutPanel1.Controls.Add(this.labelBlue);
            this.flowLayoutPanel1.Controls.Add(this.tbBlue);
            this.flowLayoutPanel1.Controls.Add(this.lblBlue);
            this.flowLayoutPanel1.Controls.Add(this.labelAlpha1);
            this.flowLayoutPanel1.Controls.Add(this.tbAlpha);
            this.flowLayoutPanel1.Controls.Add(this.lblAlpha2);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(5, 232);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(396, 157);
            this.flowLayoutPanel1.TabIndex = 59;
            // 
            // labelRed
            // 
            this.labelRed.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRed.Location = new System.Drawing.Point(3, 8);
            this.labelRed.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.labelRed.Name = "labelRed";
            this.labelRed.Size = new System.Drawing.Size(72, 18);
            this.labelRed.TabIndex = 42;
            this.labelRed.Text = "Red";
            this.labelRed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbRed
            // 
            this.tbRed.AutoSize = false;
            this.tbRed.LargeChange = 16;
            this.tbRed.Location = new System.Drawing.Point(78, 3);
            this.tbRed.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this.tbRed.Maximum = 255;
            this.tbRed.Name = "tbRed";
            this.tbRed.Size = new System.Drawing.Size(256, 32);
            this.tbRed.TabIndex = 43;
            this.tbRed.TickFrequency = 32;
            this.tbRed.Scroll += new System.EventHandler(this.HandleRGBScroll);
            // 
            // labelGreen
            // 
            this.labelGreen.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelGreen.Location = new System.Drawing.Point(3, 43);
            this.labelGreen.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.labelGreen.Name = "labelGreen";
            this.labelGreen.Size = new System.Drawing.Size(72, 18);
            this.labelGreen.TabIndex = 44;
            this.labelGreen.Text = "Green";
            this.labelGreen.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbGreen
            // 
            this.tbGreen.AutoSize = false;
            this.tbGreen.LargeChange = 16;
            this.tbGreen.Location = new System.Drawing.Point(78, 38);
            this.tbGreen.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this.tbGreen.Maximum = 255;
            this.tbGreen.Name = "tbGreen";
            this.tbGreen.Size = new System.Drawing.Size(256, 32);
            this.tbGreen.TabIndex = 45;
            this.tbGreen.TickFrequency = 32;
            this.tbGreen.Scroll += new System.EventHandler(this.HandleRGBScroll);
            // 
            // labelBlue
            // 
            this.labelBlue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBlue.Location = new System.Drawing.Point(3, 78);
            this.labelBlue.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.labelBlue.Name = "labelBlue";
            this.labelBlue.Size = new System.Drawing.Size(72, 18);
            this.labelBlue.TabIndex = 46;
            this.labelBlue.Text = "Blue";
            this.labelBlue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbBlue
            // 
            this.tbBlue.AutoSize = false;
            this.tbBlue.LargeChange = 16;
            this.tbBlue.Location = new System.Drawing.Point(78, 73);
            this.tbBlue.Margin = new System.Windows.Forms.Padding(0, 3, 3, 6);
            this.tbBlue.Maximum = 255;
            this.tbBlue.Name = "tbBlue";
            this.tbBlue.Size = new System.Drawing.Size(256, 32);
            this.tbBlue.TabIndex = 47;
            this.tbBlue.TickFrequency = 32;
            this.tbBlue.Scroll += new System.EventHandler(this.HandleRGBScroll);
            // 
            // labelAlpha1
            // 
            this.labelAlpha1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAlpha1.Location = new System.Drawing.Point(3, 119);
            this.labelAlpha1.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.labelAlpha1.Name = "labelAlpha1";
            this.labelAlpha1.Size = new System.Drawing.Size(72, 18);
            this.labelAlpha1.TabIndex = 55;
            this.labelAlpha1.Text = "Alpha";
            this.labelAlpha1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbAlpha
            // 
            this.tbAlpha.AutoSize = false;
            this.tbAlpha.LargeChange = 16;
            this.tbAlpha.Location = new System.Drawing.Point(78, 114);
            this.tbAlpha.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this.tbAlpha.Maximum = 255;
            this.tbAlpha.Name = "tbAlpha";
            this.tbAlpha.Size = new System.Drawing.Size(256, 32);
            this.tbAlpha.TabIndex = 56;
            this.tbAlpha.TickFrequency = 32;
            this.tbAlpha.Scroll += new System.EventHandler(this.TbAlphaScroll);
            // 
            // tbHue
            // 
            this.tbHue.AutoSize = false;
            this.tbHue.LargeChange = 16;
            this.tbHue.Location = new System.Drawing.Point(379, 150);
            this.tbHue.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this.tbHue.Maximum = 255;
            this.tbHue.Name = "tbHue";
            this.tbHue.Size = new System.Drawing.Size(238, 32);
            this.tbHue.TabIndex = 36;
            this.tbHue.TickFrequency = 32;
            this.tbHue.Visible = false;
            this.tbHue.Scroll += new System.EventHandler(this.HandleHSVScroll);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(304, 190);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 18);
            this.label1.TabIndex = 38;
            this.label1.Text = "Saturation";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1.Visible = false;
            // 
            // tbSaturation
            // 
            this.tbSaturation.AutoSize = false;
            this.tbSaturation.LargeChange = 16;
            this.tbSaturation.Location = new System.Drawing.Point(379, 185);
            this.tbSaturation.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this.tbSaturation.Maximum = 255;
            this.tbSaturation.Name = "tbSaturation";
            this.tbSaturation.Size = new System.Drawing.Size(238, 32);
            this.tbSaturation.TabIndex = 39;
            this.tbSaturation.TickFrequency = 32;
            this.tbSaturation.Visible = false;
            this.tbSaturation.Scroll += new System.EventHandler(this.HandleHSVScroll);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(304, 225);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 18);
            this.label2.TabIndex = 40;
            this.label2.Text = "Value";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label2.Visible = false;
            // 
            // tbValue
            // 
            this.tbValue.AutoSize = false;
            this.tbValue.LargeChange = 16;
            this.tbValue.Location = new System.Drawing.Point(379, 220);
            this.tbValue.Margin = new System.Windows.Forms.Padding(0, 3, 3, 6);
            this.tbValue.Maximum = 255;
            this.tbValue.Name = "tbValue";
            this.tbValue.Size = new System.Drawing.Size(238, 32);
            this.tbValue.TabIndex = 41;
            this.tbValue.TickFrequency = 32;
            this.tbValue.Visible = false;
            this.tbValue.Scroll += new System.EventHandler(this.HandleHSVScroll);
            // 
            // pnlSelectedColor
            // 
            this.pnlSelectedColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlSelectedColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlSelectedColor.Location = new System.Drawing.Point(295, 12);
            this.pnlSelectedColor.Name = "pnlSelectedColor";
            this.pnlSelectedColor.Size = new System.Drawing.Size(96, 32);
            this.pnlSelectedColor.TabIndex = 40;
            this.pnlSelectedColor.Visible = false;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(321, 406);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(80, 23);
            this.buttonCancel.TabIndex = 61;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(235, 406);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(80, 23);
            this.buttonOK.TabIndex = 60;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // ColorChooser
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(413, 441);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbHue);
            this.Controls.Add(this.tbHexCode);
            this.Controls.Add(this.lblHue);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbSaturation);
            this.Controls.Add(this.pnlColor);
            this.Controls.Add(this.lblSaturation);
            this.Controls.Add(this.pnlSelectedColor);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pnlBrightness);
            this.Controls.Add(this.tbValue);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.lblValue);
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
            this.flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tbRed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbBlue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbAlpha)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbHue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbSaturation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbValue)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion Windows Form Designer generated code

        #region Nested type: ChangeStyle

        private enum ChangeStyle
        {
            MouseMove,
            RGB,
            HSV,
            None
        }

        #endregion Nested type: ChangeStyle

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
                base.Dispose(true);
                if (myColorWheel != null)
                {
                    myColorWheel.Dispose();
                    myColorWheel = null;
                }
            }
        }

    }
}
