using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ShowEarlierLater
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            Nikse.SubtitleEdit.Core.Common.TimeCode timeCode4 = new Nikse.SubtitleEdit.Core.Common.TimeCode();
            this.labelHourMinSecMilliSecond = new System.Windows.Forms.Label();
            this.buttonShowLater = new System.Windows.Forms.Button();
            this.buttonShowEarlier = new System.Windows.Forms.Button();
            this.labelTotalAdjustment = new System.Windows.Forms.Label();
            this.timerRefreshAllowSelection = new System.Windows.Forms.Timer(this.components);
            this.radioButtonAllLines = new System.Windows.Forms.RadioButton();
            this.radioButtonSelectedLinesOnly = new System.Windows.Forms.RadioButton();
            this.timeUpDownAdjust = new Nikse.SubtitleEdit.Controls.NikseTimeUpDown();
            this.radioButtonSelectedLineAndForward = new System.Windows.Forms.RadioButton();
            this.panelFooter = new System.Windows.Forms.Panel();
            this.flowLayoutPanelMainContent = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBoxSelection = new System.Windows.Forms.GroupBox();
            this.groupBoxCustomOffset = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanelQuickbar = new System.Windows.Forms.FlowLayoutPanel();
            this.panelQuick10s = new System.Windows.Forms.Panel();
            this.labelQuick10s = new System.Windows.Forms.Label();
            this.buttonQuick10sEarlier = new System.Windows.Forms.Button();
            this.buttonQuick10sLater = new System.Windows.Forms.Button();
            this.panelQuick1s = new System.Windows.Forms.Panel();
            this.buttonQuick1sLater = new System.Windows.Forms.Button();
            this.buttonQuick1sEarlier = new System.Windows.Forms.Button();
            this.labelQuick1s = new System.Windows.Forms.Label();
            this.panelQuick500ms = new System.Windows.Forms.Panel();
            this.buttonQuick500msLater = new System.Windows.Forms.Button();
            this.buttonQuick500msEarlier = new System.Windows.Forms.Button();
            this.labelQuick500ms = new System.Windows.Forms.Label();
            this.panelQuick100ms = new System.Windows.Forms.Panel();
            this.buttonQuick100msLater = new System.Windows.Forms.Button();
            this.buttonQuick100msEarlier = new System.Windows.Forms.Button();
            this.labelQuick100ms = new System.Windows.Forms.Label();
            this.panelQuick10ms = new System.Windows.Forms.Panel();
            this.buttonQuick10msLater = new System.Windows.Forms.Button();
            this.buttonQuick10msEarlier = new System.Windows.Forms.Button();
            this.labelQuick10ms = new System.Windows.Forms.Label();
            this.panelQuick1ms = new System.Windows.Forms.Panel();
            this.buttonQuick1msLater = new System.Windows.Forms.Button();
            this.buttonQuick1msEarlier = new System.Windows.Forms.Button();
            this.labelQuick1ms = new System.Windows.Forms.Label();
            this.panelFooter.SuspendLayout();
            this.flowLayoutPanelMainContent.SuspendLayout();
            this.groupBoxSelection.SuspendLayout();
            this.groupBoxCustomOffset.SuspendLayout();
            this.flowLayoutPanelQuickbar.SuspendLayout();
            this.panelQuick10s.SuspendLayout();
            this.panelQuick1s.SuspendLayout();
            this.panelQuick500ms.SuspendLayout();
            this.panelQuick100ms.SuspendLayout();
            this.panelQuick10ms.SuspendLayout();
            this.panelQuick1ms.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelHourMinSecMilliSecond
            // 
            this.labelHourMinSecMilliSecond.AutoSize = true;
            this.labelHourMinSecMilliSecond.Location = new System.Drawing.Point(3, 17);
            this.labelHourMinSecMilliSecond.Name = "labelHourMinSecMilliSecond";
            this.labelHourMinSecMilliSecond.Size = new System.Drawing.Size(108, 13);
            this.labelHourMinSecMilliSecond.TabIndex = 18;
            this.labelHourMinSecMilliSecond.Text = "Hours:min:sec.msecs";
            // 
            // buttonShowLater
            // 
            this.buttonShowLater.Location = new System.Drawing.Point(6, 94);
            this.buttonShowLater.Name = "buttonShowLater";
            this.buttonShowLater.Size = new System.Drawing.Size(105, 23);
            this.buttonShowLater.TabIndex = 20;
            this.buttonShowLater.Text = "Show later";
            this.buttonShowLater.UseVisualStyleBackColor = true;
            this.buttonShowLater.Click += new System.EventHandler(this.ButtonShowLaterClick);
            // 
            // buttonShowEarlier
            // 
            this.buttonShowEarlier.Location = new System.Drawing.Point(6, 65);
            this.buttonShowEarlier.Name = "buttonShowEarlier";
            this.buttonShowEarlier.Size = new System.Drawing.Size(105, 23);
            this.buttonShowEarlier.TabIndex = 19;
            this.buttonShowEarlier.Text = "Show earlier";
            this.buttonShowEarlier.UseVisualStyleBackColor = true;
            this.buttonShowEarlier.Click += new System.EventHandler(this.ButtonShowEarlierClick);
            // 
            // labelTotalAdjustment
            // 
            this.labelTotalAdjustment.AutoSize = true;
            this.labelTotalAdjustment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelTotalAdjustment.Location = new System.Drawing.Point(3, 3);
            this.labelTotalAdjustment.Name = "labelTotalAdjustment";
            this.labelTotalAdjustment.Size = new System.Drawing.Size(108, 13);
            this.labelTotalAdjustment.TabIndex = 38;
            this.labelTotalAdjustment.Text = "labelTotalAdjustment";
            this.labelTotalAdjustment.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // timerRefreshAllowSelection
            // 
            this.timerRefreshAllowSelection.Interval = 250;
            this.timerRefreshAllowSelection.Tick += new System.EventHandler(this.timerRefreshAllowSelection_Tick);
            // 
            // radioButtonAllLines
            // 
            this.radioButtonAllLines.AutoSize = true;
            this.radioButtonAllLines.Location = new System.Drawing.Point(16, 20);
            this.radioButtonAllLines.Name = "radioButtonAllLines";
            this.radioButtonAllLines.Size = new System.Drawing.Size(60, 17);
            this.radioButtonAllLines.TabIndex = 39;
            this.radioButtonAllLines.TabStop = true;
            this.radioButtonAllLines.Text = "All lines";
            this.radioButtonAllLines.UseVisualStyleBackColor = true;
            this.radioButtonAllLines.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // radioButtonSelectedLinesOnly
            // 
            this.radioButtonSelectedLinesOnly.AutoSize = true;
            this.radioButtonSelectedLinesOnly.Location = new System.Drawing.Point(16, 43);
            this.radioButtonSelectedLinesOnly.Name = "radioButtonSelectedLinesOnly";
            this.radioButtonSelectedLinesOnly.Size = new System.Drawing.Size(113, 17);
            this.radioButtonSelectedLinesOnly.TabIndex = 40;
            this.radioButtonSelectedLinesOnly.TabStop = true;
            this.radioButtonSelectedLinesOnly.Text = "Selected lines only";
            this.radioButtonSelectedLinesOnly.UseVisualStyleBackColor = true;
            this.radioButtonSelectedLinesOnly.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // timeUpDownAdjust
            // 
            this.timeUpDownAdjust.BackColor = System.Drawing.SystemColors.Window;
            this.timeUpDownAdjust.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.timeUpDownAdjust.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.timeUpDownAdjust.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.timeUpDownAdjust.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.timeUpDownAdjust.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.timeUpDownAdjust.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.timeUpDownAdjust.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.timeUpDownAdjust.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.timeUpDownAdjust.Location = new System.Drawing.Point(6, 35);
            this.timeUpDownAdjust.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownAdjust.Name = "timeUpDownAdjust";
            this.timeUpDownAdjust.Size = new System.Drawing.Size(105, 23);
            this.timeUpDownAdjust.TabIndex = 21;
            this.timeUpDownAdjust.TabStop = false;
            timeCode4.Hours = 0;
            timeCode4.Milliseconds = 0;
            timeCode4.Minutes = 0;
            timeCode4.Seconds = 0;
            timeCode4.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode4.TotalMilliseconds = 0D;
            timeCode4.TotalSeconds = 0D;
            this.timeUpDownAdjust.TimeCode = timeCode4;
            this.timeUpDownAdjust.UseVideoOffset = false;
            // 
            // radioButtonSelectedLineAndForward
            // 
            this.radioButtonSelectedLineAndForward.AutoSize = true;
            this.radioButtonSelectedLineAndForward.Location = new System.Drawing.Point(16, 66);
            this.radioButtonSelectedLineAndForward.Name = "radioButtonSelectedLineAndForward";
            this.radioButtonSelectedLineAndForward.Size = new System.Drawing.Size(160, 17);
            this.radioButtonSelectedLineAndForward.TabIndex = 41;
            this.radioButtonSelectedLineAndForward.TabStop = true;
            this.radioButtonSelectedLineAndForward.Text = "Selected line(s) and forward";
            this.radioButtonSelectedLineAndForward.UseVisualStyleBackColor = true;
            this.radioButtonSelectedLineAndForward.CheckedChanged += new System.EventHandler(this.RadioButtonCheckedChanged);
            // 
            // panelFooter
            // 
            this.panelFooter.Controls.Add(this.labelTotalAdjustment);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFooter.Location = new System.Drawing.Point(0, 266);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Padding = new System.Windows.Forms.Padding(3);
            this.panelFooter.Size = new System.Drawing.Size(391, 24);
            this.panelFooter.TabIndex = 42;
            // 
            // flowLayoutPanelMainContent
            // 
            this.flowLayoutPanelMainContent.Controls.Add(this.groupBoxSelection);
            this.flowLayoutPanelMainContent.Controls.Add(this.flowLayoutPanelQuickbar);
            this.flowLayoutPanelMainContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelMainContent.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelMainContent.Name = "flowLayoutPanelMainContent";
            this.flowLayoutPanelMainContent.Size = new System.Drawing.Size(391, 266);
            this.flowLayoutPanelMainContent.TabIndex = 43;
            // 
            // groupBoxSelection
            // 
            this.groupBoxSelection.AutoSize = true;
            this.groupBoxSelection.Controls.Add(this.radioButtonAllLines);
            this.groupBoxSelection.Controls.Add(this.groupBoxCustomOffset);
            this.groupBoxSelection.Controls.Add(this.radioButtonSelectedLinesOnly);
            this.groupBoxSelection.Controls.Add(this.radioButtonSelectedLineAndForward);
            this.groupBoxSelection.Location = new System.Drawing.Point(3, 3);
            this.groupBoxSelection.Name = "groupBoxSelection";
            this.groupBoxSelection.Size = new System.Drawing.Size(182, 235);
            this.groupBoxSelection.TabIndex = 44;
            this.groupBoxSelection.TabStop = false;
            // 
            // groupBoxCustomOffset
            // 
            this.groupBoxCustomOffset.Controls.Add(this.timeUpDownAdjust);
            this.groupBoxCustomOffset.Controls.Add(this.labelHourMinSecMilliSecond);
            this.groupBoxCustomOffset.Controls.Add(this.buttonShowEarlier);
            this.groupBoxCustomOffset.Controls.Add(this.buttonShowLater);
            this.groupBoxCustomOffset.Location = new System.Drawing.Point(28, 89);
            this.groupBoxCustomOffset.Name = "groupBoxCustomOffset";
            this.groupBoxCustomOffset.Size = new System.Drawing.Size(119, 126);
            this.groupBoxCustomOffset.TabIndex = 45;
            this.groupBoxCustomOffset.TabStop = false;
            // 
            // flowLayoutPanelQuickbar
            // 
            this.flowLayoutPanelQuickbar.AutoSize = true;
            this.flowLayoutPanelQuickbar.Controls.Add(this.panelQuick10s);
            this.flowLayoutPanelQuickbar.Controls.Add(this.panelQuick1s);
            this.flowLayoutPanelQuickbar.Controls.Add(this.panelQuick500ms);
            this.flowLayoutPanelQuickbar.Controls.Add(this.panelQuick100ms);
            this.flowLayoutPanelQuickbar.Controls.Add(this.panelQuick10ms);
            this.flowLayoutPanelQuickbar.Controls.Add(this.panelQuick1ms);
            this.flowLayoutPanelQuickbar.Location = new System.Drawing.Point(191, 3);
            this.flowLayoutPanelQuickbar.Name = "flowLayoutPanelQuickbar";
            this.flowLayoutPanelQuickbar.Size = new System.Drawing.Size(162, 108);
            this.flowLayoutPanelQuickbar.TabIndex = 0;
            // 
            // panelQuick10s
            // 
            this.panelQuick10s.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelQuick10s.Controls.Add(this.buttonQuick10sLater);
            this.panelQuick10s.Controls.Add(this.buttonQuick10sEarlier);
            this.panelQuick10s.Controls.Add(this.labelQuick10s);
            this.panelQuick10s.Location = new System.Drawing.Point(3, 3);
            this.panelQuick10s.MaximumSize = new System.Drawing.Size(48, 48);
            this.panelQuick10s.MinimumSize = new System.Drawing.Size(48, 48);
            this.panelQuick10s.Name = "panelQuick10s";
            this.panelQuick10s.Size = new System.Drawing.Size(48, 48);
            this.panelQuick10s.TabIndex = 0;
            // 
            // labelQuick10s
            // 
            this.labelQuick10s.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelQuick10s.Location = new System.Drawing.Point(0, 0);
            this.labelQuick10s.Name = "labelQuick10s";
            this.labelQuick10s.Size = new System.Drawing.Size(46, 15);
            this.labelQuick10s.TabIndex = 0;
            this.labelQuick10s.Text = "10s";
            this.labelQuick10s.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // buttonQuick10sEarlier
            // 
            this.buttonQuick10sEarlier.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonQuick10sEarlier.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonQuick10sEarlier.Location = new System.Drawing.Point(0, 15);
            this.buttonQuick10sEarlier.Name = "buttonQuick10sEarlier";
            this.buttonQuick10sEarlier.Size = new System.Drawing.Size(24, 31);
            this.buttonQuick10sEarlier.TabIndex = 20;
            this.buttonQuick10sEarlier.Text = "<";
            this.buttonQuick10sEarlier.UseVisualStyleBackColor = true;
            this.buttonQuick10sEarlier.Click += new System.EventHandler(this.buttonQuick10sEarlier_Click);
            // 
            // buttonQuick10sLater
            // 
            this.buttonQuick10sLater.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonQuick10sLater.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonQuick10sLater.Location = new System.Drawing.Point(24, 15);
            this.buttonQuick10sLater.Name = "buttonQuick10sLater";
            this.buttonQuick10sLater.Size = new System.Drawing.Size(22, 31);
            this.buttonQuick10sLater.TabIndex = 21;
            this.buttonQuick10sLater.Text = ">";
            this.buttonQuick10sLater.UseVisualStyleBackColor = true;
            this.buttonQuick10sLater.Click += new System.EventHandler(this.buttonQuick10sLater_Click);
            // 
            // panelQuick1s
            // 
            this.panelQuick1s.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelQuick1s.Controls.Add(this.buttonQuick1sLater);
            this.panelQuick1s.Controls.Add(this.buttonQuick1sEarlier);
            this.panelQuick1s.Controls.Add(this.labelQuick1s);
            this.panelQuick1s.Location = new System.Drawing.Point(57, 3);
            this.panelQuick1s.MaximumSize = new System.Drawing.Size(48, 48);
            this.panelQuick1s.MinimumSize = new System.Drawing.Size(48, 48);
            this.panelQuick1s.Name = "panelQuick1s";
            this.panelQuick1s.Size = new System.Drawing.Size(48, 48);
            this.panelQuick1s.TabIndex = 1;
            // 
            // buttonQuick1sLater
            // 
            this.buttonQuick1sLater.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonQuick1sLater.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonQuick1sLater.Location = new System.Drawing.Point(24, 15);
            this.buttonQuick1sLater.Name = "buttonQuick1sLater";
            this.buttonQuick1sLater.Size = new System.Drawing.Size(22, 31);
            this.buttonQuick1sLater.TabIndex = 21;
            this.buttonQuick1sLater.Text = ">";
            this.buttonQuick1sLater.UseVisualStyleBackColor = true;
            this.buttonQuick1sLater.Click += new System.EventHandler(this.buttonQuick1sLater_Click);
            // 
            // buttonQuick1sEarlier
            // 
            this.buttonQuick1sEarlier.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonQuick1sEarlier.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonQuick1sEarlier.Location = new System.Drawing.Point(0, 15);
            this.buttonQuick1sEarlier.Name = "buttonQuick1sEarlier";
            this.buttonQuick1sEarlier.Size = new System.Drawing.Size(24, 31);
            this.buttonQuick1sEarlier.TabIndex = 20;
            this.buttonQuick1sEarlier.Text = "<";
            this.buttonQuick1sEarlier.UseVisualStyleBackColor = true;
            this.buttonQuick1sEarlier.Click += new System.EventHandler(this.buttonQuick1sEarlier_Click);
            // 
            // labelQuick1s
            // 
            this.labelQuick1s.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelQuick1s.Location = new System.Drawing.Point(0, 0);
            this.labelQuick1s.Name = "labelQuick1s";
            this.labelQuick1s.Size = new System.Drawing.Size(46, 15);
            this.labelQuick1s.TabIndex = 0;
            this.labelQuick1s.Text = "1s";
            this.labelQuick1s.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panelQuick500ms
            // 
            this.panelQuick500ms.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelQuick500ms.Controls.Add(this.buttonQuick500msLater);
            this.panelQuick500ms.Controls.Add(this.buttonQuick500msEarlier);
            this.panelQuick500ms.Controls.Add(this.labelQuick500ms);
            this.flowLayoutPanelQuickbar.SetFlowBreak(this.panelQuick500ms, true);
            this.panelQuick500ms.Location = new System.Drawing.Point(111, 3);
            this.panelQuick500ms.MaximumSize = new System.Drawing.Size(48, 48);
            this.panelQuick500ms.MinimumSize = new System.Drawing.Size(48, 48);
            this.panelQuick500ms.Name = "panelQuick500ms";
            this.panelQuick500ms.Size = new System.Drawing.Size(48, 48);
            this.panelQuick500ms.TabIndex = 2;
            // 
            // buttonQuick500msLater
            // 
            this.buttonQuick500msLater.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonQuick500msLater.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonQuick500msLater.Location = new System.Drawing.Point(24, 15);
            this.buttonQuick500msLater.Name = "buttonQuick500msLater";
            this.buttonQuick500msLater.Size = new System.Drawing.Size(22, 31);
            this.buttonQuick500msLater.TabIndex = 21;
            this.buttonQuick500msLater.Text = ">";
            this.buttonQuick500msLater.UseVisualStyleBackColor = true;
            this.buttonQuick500msLater.Click += new System.EventHandler(this.buttonQuick500msLater_Click);
            // 
            // buttonQuick500msEarlier
            // 
            this.buttonQuick500msEarlier.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonQuick500msEarlier.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonQuick500msEarlier.Location = new System.Drawing.Point(0, 15);
            this.buttonQuick500msEarlier.Name = "buttonQuick500msEarlier";
            this.buttonQuick500msEarlier.Size = new System.Drawing.Size(24, 31);
            this.buttonQuick500msEarlier.TabIndex = 20;
            this.buttonQuick500msEarlier.Text = "<";
            this.buttonQuick500msEarlier.UseVisualStyleBackColor = true;
            this.buttonQuick500msEarlier.Click += new System.EventHandler(this.buttonQuick500msEarlier_Click);
            // 
            // labelQuick500ms
            // 
            this.labelQuick500ms.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelQuick500ms.Location = new System.Drawing.Point(0, 0);
            this.labelQuick500ms.Name = "labelQuick500ms";
            this.labelQuick500ms.Size = new System.Drawing.Size(46, 15);
            this.labelQuick500ms.TabIndex = 0;
            this.labelQuick500ms.Text = "500ms";
            this.labelQuick500ms.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panelQuick100ms
            // 
            this.panelQuick100ms.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelQuick100ms.Controls.Add(this.buttonQuick100msLater);
            this.panelQuick100ms.Controls.Add(this.buttonQuick100msEarlier);
            this.panelQuick100ms.Controls.Add(this.labelQuick100ms);
            this.panelQuick100ms.Location = new System.Drawing.Point(3, 57);
            this.panelQuick100ms.MaximumSize = new System.Drawing.Size(48, 48);
            this.panelQuick100ms.MinimumSize = new System.Drawing.Size(48, 48);
            this.panelQuick100ms.Name = "panelQuick100ms";
            this.panelQuick100ms.Size = new System.Drawing.Size(48, 48);
            this.panelQuick100ms.TabIndex = 3;
            // 
            // buttonQuick100msLater
            // 
            this.buttonQuick100msLater.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonQuick100msLater.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonQuick100msLater.Location = new System.Drawing.Point(24, 15);
            this.buttonQuick100msLater.Name = "buttonQuick100msLater";
            this.buttonQuick100msLater.Size = new System.Drawing.Size(22, 31);
            this.buttonQuick100msLater.TabIndex = 21;
            this.buttonQuick100msLater.Text = ">";
            this.buttonQuick100msLater.UseVisualStyleBackColor = true;
            this.buttonQuick100msLater.Click += new System.EventHandler(this.buttonQuick100msLater_Click);
            // 
            // buttonQuick100msEarlier
            // 
            this.buttonQuick100msEarlier.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonQuick100msEarlier.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonQuick100msEarlier.Location = new System.Drawing.Point(0, 15);
            this.buttonQuick100msEarlier.Name = "buttonQuick100msEarlier";
            this.buttonQuick100msEarlier.Size = new System.Drawing.Size(24, 31);
            this.buttonQuick100msEarlier.TabIndex = 20;
            this.buttonQuick100msEarlier.Text = "<";
            this.buttonQuick100msEarlier.UseVisualStyleBackColor = true;
            this.buttonQuick100msEarlier.Click += new System.EventHandler(this.buttonQuick100msEarlier_Click);
            // 
            // labelQuick100ms
            // 
            this.labelQuick100ms.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelQuick100ms.Location = new System.Drawing.Point(0, 0);
            this.labelQuick100ms.Name = "labelQuick100ms";
            this.labelQuick100ms.Size = new System.Drawing.Size(46, 15);
            this.labelQuick100ms.TabIndex = 0;
            this.labelQuick100ms.Text = "100ms";
            this.labelQuick100ms.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panelQuick10ms
            // 
            this.panelQuick10ms.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelQuick10ms.Controls.Add(this.buttonQuick10msLater);
            this.panelQuick10ms.Controls.Add(this.buttonQuick10msEarlier);
            this.panelQuick10ms.Controls.Add(this.labelQuick10ms);
            this.panelQuick10ms.Location = new System.Drawing.Point(57, 57);
            this.panelQuick10ms.MaximumSize = new System.Drawing.Size(48, 48);
            this.panelQuick10ms.MinimumSize = new System.Drawing.Size(48, 48);
            this.panelQuick10ms.Name = "panelQuick10ms";
            this.panelQuick10ms.Size = new System.Drawing.Size(48, 48);
            this.panelQuick10ms.TabIndex = 4;
            // 
            // buttonQuick10msLater
            // 
            this.buttonQuick10msLater.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonQuick10msLater.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonQuick10msLater.Location = new System.Drawing.Point(24, 15);
            this.buttonQuick10msLater.Name = "buttonQuick10msLater";
            this.buttonQuick10msLater.Size = new System.Drawing.Size(22, 31);
            this.buttonQuick10msLater.TabIndex = 21;
            this.buttonQuick10msLater.Text = ">";
            this.buttonQuick10msLater.UseVisualStyleBackColor = true;
            this.buttonQuick10msLater.Click += new System.EventHandler(this.buttonQuick10msLater_Click);
            // 
            // buttonQuick10msEarlier
            // 
            this.buttonQuick10msEarlier.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonQuick10msEarlier.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonQuick10msEarlier.Location = new System.Drawing.Point(0, 15);
            this.buttonQuick10msEarlier.Name = "buttonQuick10msEarlier";
            this.buttonQuick10msEarlier.Size = new System.Drawing.Size(24, 31);
            this.buttonQuick10msEarlier.TabIndex = 20;
            this.buttonQuick10msEarlier.Text = "<";
            this.buttonQuick10msEarlier.UseVisualStyleBackColor = true;
            this.buttonQuick10msEarlier.Click += new System.EventHandler(this.buttonQuick10msEarlier_Click);
            // 
            // labelQuick10ms
            // 
            this.labelQuick10ms.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelQuick10ms.Location = new System.Drawing.Point(0, 0);
            this.labelQuick10ms.Name = "labelQuick10ms";
            this.labelQuick10ms.Size = new System.Drawing.Size(46, 15);
            this.labelQuick10ms.TabIndex = 0;
            this.labelQuick10ms.Text = "10ms";
            this.labelQuick10ms.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panelQuick1ms
            // 
            this.panelQuick1ms.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelQuick1ms.Controls.Add(this.buttonQuick1msLater);
            this.panelQuick1ms.Controls.Add(this.buttonQuick1msEarlier);
            this.panelQuick1ms.Controls.Add(this.labelQuick1ms);
            this.panelQuick1ms.Location = new System.Drawing.Point(111, 57);
            this.panelQuick1ms.MaximumSize = new System.Drawing.Size(48, 48);
            this.panelQuick1ms.MinimumSize = new System.Drawing.Size(48, 48);
            this.panelQuick1ms.Name = "panelQuick1ms";
            this.panelQuick1ms.Size = new System.Drawing.Size(48, 48);
            this.panelQuick1ms.TabIndex = 5;
            // 
            // buttonQuick1msLater
            // 
            this.buttonQuick1msLater.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonQuick1msLater.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonQuick1msLater.Location = new System.Drawing.Point(24, 15);
            this.buttonQuick1msLater.Name = "buttonQuick1msLater";
            this.buttonQuick1msLater.Size = new System.Drawing.Size(22, 31);
            this.buttonQuick1msLater.TabIndex = 21;
            this.buttonQuick1msLater.Text = ">";
            this.buttonQuick1msLater.UseVisualStyleBackColor = true;
            this.buttonQuick1msLater.Click += new System.EventHandler(this.buttonQuick1msLater_Click);
            // 
            // buttonQuick1msEarlier
            // 
            this.buttonQuick1msEarlier.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonQuick1msEarlier.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonQuick1msEarlier.Location = new System.Drawing.Point(0, 15);
            this.buttonQuick1msEarlier.Name = "buttonQuick1msEarlier";
            this.buttonQuick1msEarlier.Size = new System.Drawing.Size(24, 31);
            this.buttonQuick1msEarlier.TabIndex = 20;
            this.buttonQuick1msEarlier.Text = "<";
            this.buttonQuick1msEarlier.UseVisualStyleBackColor = true;
            this.buttonQuick1msEarlier.Click += new System.EventHandler(this.buttonQuick1msEarlier_Click);
            // 
            // labelQuick1ms
            // 
            this.labelQuick1ms.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelQuick1ms.Location = new System.Drawing.Point(0, 0);
            this.labelQuick1ms.Name = "labelQuick1ms";
            this.labelQuick1ms.Size = new System.Drawing.Size(46, 15);
            this.labelQuick1ms.TabIndex = 0;
            this.labelQuick1ms.Text = "1ms";
            this.labelQuick1ms.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ShowEarlierLater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(391, 290);
            this.Controls.Add(this.flowLayoutPanelMainContent);
            this.Controls.Add(this.panelFooter);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ShowEarlierLater";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Show selected lines earlier/later";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ShowEarlierLater_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ShowEarlierLater_KeyDown);
            this.panelFooter.ResumeLayout(false);
            this.panelFooter.PerformLayout();
            this.flowLayoutPanelMainContent.ResumeLayout(false);
            this.flowLayoutPanelMainContent.PerformLayout();
            this.groupBoxSelection.ResumeLayout(false);
            this.groupBoxSelection.PerformLayout();
            this.groupBoxCustomOffset.ResumeLayout(false);
            this.groupBoxCustomOffset.PerformLayout();
            this.flowLayoutPanelQuickbar.ResumeLayout(false);
            this.panelQuick10s.ResumeLayout(false);
            this.panelQuick1s.ResumeLayout(false);
            this.panelQuick500ms.ResumeLayout(false);
            this.panelQuick100ms.ResumeLayout(false);
            this.panelQuick10ms.ResumeLayout(false);
            this.panelQuick1ms.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Nikse.SubtitleEdit.Controls.NikseTimeUpDown timeUpDownAdjust;
        private System.Windows.Forms.Label labelHourMinSecMilliSecond;
        private System.Windows.Forms.Button buttonShowLater;
        private System.Windows.Forms.Button buttonShowEarlier;
        private System.Windows.Forms.Label labelTotalAdjustment;
        private System.Windows.Forms.Timer timerRefreshAllowSelection;
        private System.Windows.Forms.RadioButton radioButtonAllLines;
        private System.Windows.Forms.RadioButton radioButtonSelectedLinesOnly;
        private System.Windows.Forms.RadioButton radioButtonSelectedLineAndForward;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMainContent;
        private System.Windows.Forms.GroupBox groupBoxSelection;
        private System.Windows.Forms.GroupBox groupBoxCustomOffset;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelQuickbar;
        private System.Windows.Forms.Panel panelQuick10s;
        private System.Windows.Forms.Label labelQuick10s;
        private System.Windows.Forms.Button buttonQuick10sLater;
        private System.Windows.Forms.Button buttonQuick10sEarlier;
        private System.Windows.Forms.Panel panelQuick1s;
        private System.Windows.Forms.Button buttonQuick1sLater;
        private System.Windows.Forms.Button buttonQuick1sEarlier;
        private System.Windows.Forms.Label labelQuick1s;
        private System.Windows.Forms.Panel panelQuick500ms;
        private System.Windows.Forms.Button buttonQuick500msLater;
        private System.Windows.Forms.Button buttonQuick500msEarlier;
        private System.Windows.Forms.Label labelQuick500ms;
        private System.Windows.Forms.Panel panelQuick100ms;
        private System.Windows.Forms.Button buttonQuick100msLater;
        private System.Windows.Forms.Button buttonQuick100msEarlier;
        private System.Windows.Forms.Label labelQuick100ms;
        private System.Windows.Forms.Panel panelQuick10ms;
        private System.Windows.Forms.Button buttonQuick10msLater;
        private System.Windows.Forms.Button buttonQuick10msEarlier;
        private System.Windows.Forms.Label labelQuick10ms;
        private System.Windows.Forms.Panel panelQuick1ms;
        private System.Windows.Forms.Button buttonQuick1msLater;
        private System.Windows.Forms.Button buttonQuick1msEarlier;
        private System.Windows.Forms.Label labelQuick1ms;
    }
}