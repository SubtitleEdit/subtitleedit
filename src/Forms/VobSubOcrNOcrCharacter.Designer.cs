namespace Nikse.SubtitleEdit.Forms
{
    partial class VobSubOcrNOcrCharacter
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
            this.buttonShrinkSelection = new System.Windows.Forms.Button();
            this.buttonExpandSelection = new System.Windows.Forms.Button();
            this.buttonAbort = new System.Windows.Forms.Button();
            this.labelCharacters = new System.Windows.Forms.Label();
            this.pictureBoxCharacter = new System.Windows.Forms.PictureBox();
            this.labelCharactersAsText = new System.Windows.Forms.Label();
            this.textBoxCharacters = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelSubtitleImage = new System.Windows.Forms.Label();
            this.pictureBoxSubtitleImage = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonZoomOut = new System.Windows.Forms.Button();
            this.buttonZoomIn = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButtonCold = new System.Windows.Forms.RadioButton();
            this.radioButtonHot = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.listBoxlinesBackground = new System.Windows.Forms.ListBox();
            this.labelLines = new System.Windows.Forms.Label();
            this.listBoxLinesForeground = new System.Windows.Forms.ListBox();
            this.contextMenuStripLinesBackground = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeBackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuLinesForeground = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeForegroundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCharacter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSubtitleImage)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.contextMenuStripLinesBackground.SuspendLayout();
            this.contextMenuLinesForeground.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonShrinkSelection
            // 
            this.buttonShrinkSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonShrinkSelection.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonShrinkSelection.Location = new System.Drawing.Point(385, 9);
            this.buttonShrinkSelection.Name = "buttonShrinkSelection";
            this.buttonShrinkSelection.Size = new System.Drawing.Size(112, 21);
            this.buttonShrinkSelection.TabIndex = 32;
            this.buttonShrinkSelection.Text = "Shrink selection";
            this.buttonShrinkSelection.UseVisualStyleBackColor = true;
            this.buttonShrinkSelection.Click += new System.EventHandler(this.buttonShrinkSelection_Click);
            // 
            // buttonExpandSelection
            // 
            this.buttonExpandSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExpandSelection.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonExpandSelection.Location = new System.Drawing.Point(503, 9);
            this.buttonExpandSelection.Name = "buttonExpandSelection";
            this.buttonExpandSelection.Size = new System.Drawing.Size(112, 21);
            this.buttonExpandSelection.TabIndex = 31;
            this.buttonExpandSelection.Text = "Expand selection";
            this.buttonExpandSelection.UseVisualStyleBackColor = true;
            this.buttonExpandSelection.Click += new System.EventHandler(this.buttonExpandSelection_Click);
            // 
            // buttonAbort
            // 
            this.buttonAbort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAbort.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.buttonAbort.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAbort.Location = new System.Drawing.Point(541, 523);
            this.buttonAbort.Name = "buttonAbort";
            this.buttonAbort.Size = new System.Drawing.Size(75, 21);
            this.buttonAbort.TabIndex = 25;
            this.buttonAbort.Text = "&Abort";
            this.buttonAbort.UseVisualStyleBackColor = true;
            // 
            // labelCharacters
            // 
            this.labelCharacters.AutoSize = true;
            this.labelCharacters.Location = new System.Drawing.Point(185, 38);
            this.labelCharacters.Name = "labelCharacters";
            this.labelCharacters.Size = new System.Drawing.Size(64, 13);
            this.labelCharacters.TabIndex = 30;
            this.labelCharacters.Text = "Character(s)";
            // 
            // pictureBoxCharacter
            // 
            this.pictureBoxCharacter.Location = new System.Drawing.Point(183, 59);
            this.pictureBoxCharacter.Name = "pictureBoxCharacter";
            this.pictureBoxCharacter.Size = new System.Drawing.Size(99, 47);
            this.pictureBoxCharacter.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxCharacter.TabIndex = 29;
            this.pictureBoxCharacter.TabStop = false;
            this.pictureBoxCharacter.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBoxCharacter_Paint);
            this.pictureBoxCharacter.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxCharacter_MouseClick);
            this.pictureBoxCharacter.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxCharacter_MouseMove);
            // 
            // labelCharactersAsText
            // 
            this.labelCharactersAsText.AutoSize = true;
            this.labelCharactersAsText.Location = new System.Drawing.Point(9, 89);
            this.labelCharactersAsText.Name = "labelCharactersAsText";
            this.labelCharactersAsText.Size = new System.Drawing.Size(98, 13);
            this.labelCharactersAsText.TabIndex = 28;
            this.labelCharactersAsText.Text = "Character(s) as text";
            // 
            // textBoxCharacters
            // 
            this.textBoxCharacters.Location = new System.Drawing.Point(12, 110);
            this.textBoxCharacters.Name = "textBoxCharacters";
            this.textBoxCharacters.Size = new System.Drawing.Size(107, 20);
            this.textBoxCharacters.TabIndex = 22;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(379, 523);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 23;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(460, 523);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 24;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelSubtitleImage
            // 
            this.labelSubtitleImage.AutoSize = true;
            this.labelSubtitleImage.Location = new System.Drawing.Point(33, 15);
            this.labelSubtitleImage.Name = "labelSubtitleImage";
            this.labelSubtitleImage.Size = new System.Drawing.Size(73, 13);
            this.labelSubtitleImage.TabIndex = 27;
            this.labelSubtitleImage.Text = "Subtitle image";
            // 
            // pictureBoxSubtitleImage
            // 
            this.pictureBoxSubtitleImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxSubtitleImage.Location = new System.Drawing.Point(36, 36);
            this.pictureBoxSubtitleImage.Name = "pictureBoxSubtitleImage";
            this.pictureBoxSubtitleImage.Size = new System.Drawing.Size(580, 205);
            this.pictureBoxSubtitleImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxSubtitleImage.TabIndex = 26;
            this.pictureBoxSubtitleImage.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonZoomOut);
            this.groupBox1.Controls.Add(this.buttonZoomIn);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.listBoxlinesBackground);
            this.groupBox1.Controls.Add(this.labelLines);
            this.groupBox1.Controls.Add(this.labelCharacters);
            this.groupBox1.Controls.Add(this.pictureBoxCharacter);
            this.groupBox1.Controls.Add(this.listBoxLinesForeground);
            this.groupBox1.Controls.Add(this.labelCharactersAsText);
            this.groupBox1.Controls.Add(this.textBoxCharacters);
            this.groupBox1.Location = new System.Drawing.Point(36, 247);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(579, 270);
            this.groupBox1.TabIndex = 33;
            this.groupBox1.TabStop = false;
            // 
            // buttonZoomOut
            // 
            this.buttonZoomOut.Location = new System.Drawing.Point(255, 27);
            this.buttonZoomOut.Name = "buttonZoomOut";
            this.buttonZoomOut.Size = new System.Drawing.Size(25, 23);
            this.buttonZoomOut.TabIndex = 32;
            this.buttonZoomOut.Text = "-";
            this.buttonZoomOut.UseVisualStyleBackColor = true;
            this.buttonZoomOut.Click += new System.EventHandler(this.buttonZoomOut_Click);
            // 
            // buttonZoomIn
            // 
            this.buttonZoomIn.Location = new System.Drawing.Point(286, 27);
            this.buttonZoomIn.Name = "buttonZoomIn";
            this.buttonZoomIn.Size = new System.Drawing.Size(25, 23);
            this.buttonZoomIn.TabIndex = 31;
            this.buttonZoomIn.Text = "+";
            this.buttonZoomIn.UseVisualStyleBackColor = true;
            this.buttonZoomIn.Click += new System.EventHandler(this.buttonZoomIn_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioButtonCold);
            this.groupBox2.Controls.Add(this.radioButtonHot);
            this.groupBox2.Location = new System.Drawing.Point(6, 19);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(157, 61);
            this.groupBox2.TabIndex = 21;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "New lines are";
            // 
            // radioButtonCold
            // 
            this.radioButtonCold.AutoSize = true;
            this.radioButtonCold.Location = new System.Drawing.Point(6, 40);
            this.radioButtonCold.Name = "radioButtonCold";
            this.radioButtonCold.Size = new System.Drawing.Size(102, 17);
            this.radioButtonCold.TabIndex = 1;
            this.radioButtonCold.Text = "NOT foreground";
            this.radioButtonCold.UseVisualStyleBackColor = true;
            // 
            // radioButtonHot
            // 
            this.radioButtonHot.AutoSize = true;
            this.radioButtonHot.Checked = true;
            this.radioButtonHot.Location = new System.Drawing.Point(6, 17);
            this.radioButtonHot.Name = "radioButtonHot";
            this.radioButtonHot.Size = new System.Drawing.Size(79, 17);
            this.radioButtonHot.TabIndex = 0;
            this.radioButtonHot.TabStop = true;
            this.radioButtonHot.Text = "Foreground";
            this.radioButtonHot.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(410, 146);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Lines - not foreground";
            // 
            // listBoxlinesBackground
            // 
            this.listBoxlinesBackground.ContextMenuStrip = this.contextMenuStripLinesBackground;
            this.listBoxlinesBackground.FormattingEnabled = true;
            this.listBoxlinesBackground.Location = new System.Drawing.Point(410, 162);
            this.listBoxlinesBackground.Name = "listBoxlinesBackground";
            this.listBoxlinesBackground.Size = new System.Drawing.Size(160, 95);
            this.listBoxlinesBackground.TabIndex = 19;
            // 
            // labelLines
            // 
            this.labelLines.AutoSize = true;
            this.labelLines.Location = new System.Drawing.Point(407, 19);
            this.labelLines.Name = "labelLines";
            this.labelLines.Size = new System.Drawing.Size(92, 13);
            this.labelLines.TabIndex = 18;
            this.labelLines.Text = "Lines - foreground";
            // 
            // listBoxLinesForeground
            // 
            this.listBoxLinesForeground.ContextMenuStrip = this.contextMenuLinesForeground;
            this.listBoxLinesForeground.FormattingEnabled = true;
            this.listBoxLinesForeground.Location = new System.Drawing.Point(410, 35);
            this.listBoxLinesForeground.Name = "listBoxLinesForeground";
            this.listBoxLinesForeground.Size = new System.Drawing.Size(160, 95);
            this.listBoxLinesForeground.TabIndex = 17;
            // 
            // contextMenuStripLinesBackground
            // 
            this.contextMenuStripLinesBackground.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeBackToolStripMenuItem});
            this.contextMenuStripLinesBackground.Name = "contextMenuStripLines";
            this.contextMenuStripLinesBackground.Size = new System.Drawing.Size(118, 26);
            // 
            // removeBackToolStripMenuItem
            // 
            this.removeBackToolStripMenuItem.Name = "removeBackToolStripMenuItem";
            this.removeBackToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.removeBackToolStripMenuItem.Text = "Remove";
            this.removeBackToolStripMenuItem.Click += new System.EventHandler(this.removeForegroundToolStripMenuItem_Click);
            // 
            // contextMenuLinesForeground
            // 
            this.contextMenuLinesForeground.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeForegroundToolStripMenuItem});
            this.contextMenuLinesForeground.Name = "contextMenuStripLines";
            this.contextMenuLinesForeground.Size = new System.Drawing.Size(153, 48);
            // 
            // removeForegroundToolStripMenuItem
            // 
            this.removeForegroundToolStripMenuItem.Name = "removeForegroundToolStripMenuItem";
            this.removeForegroundToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.removeForegroundToolStripMenuItem.Text = "Remove";
            this.removeForegroundToolStripMenuItem.Click += new System.EventHandler(this.removeForegroundToolStripMenuItem_Click_1);
            // 
            // VobSubOcrNOcrCharacter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(648, 552);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonShrinkSelection);
            this.Controls.Add(this.buttonExpandSelection);
            this.Controls.Add(this.buttonAbort);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelSubtitleImage);
            this.Controls.Add(this.pictureBoxSubtitleImage);
            this.KeyPreview = true;
            this.Name = "VobSubOcrNOcrCharacter";
            this.Text = "VobSubOcrNOcrCharacter";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VobSubOcrNOcrCharacter_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCharacter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSubtitleImage)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.contextMenuStripLinesBackground.ResumeLayout(false);
            this.contextMenuLinesForeground.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonShrinkSelection;
        private System.Windows.Forms.Button buttonExpandSelection;
        private System.Windows.Forms.Button buttonAbort;
        private System.Windows.Forms.Label labelCharacters;
        private System.Windows.Forms.PictureBox pictureBoxCharacter;
        private System.Windows.Forms.Label labelCharactersAsText;
        private System.Windows.Forms.TextBox textBoxCharacters;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelSubtitleImage;
        private System.Windows.Forms.PictureBox pictureBoxSubtitleImage;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonZoomOut;
        private System.Windows.Forms.Button buttonZoomIn;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButtonCold;
        private System.Windows.Forms.RadioButton radioButtonHot;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox listBoxlinesBackground;
        private System.Windows.Forms.Label labelLines;
        private System.Windows.Forms.ListBox listBoxLinesForeground;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripLinesBackground;
        private System.Windows.Forms.ToolStripMenuItem removeBackToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuLinesForeground;
        private System.Windows.Forms.ToolStripMenuItem removeForegroundToolStripMenuItem;

    }
}