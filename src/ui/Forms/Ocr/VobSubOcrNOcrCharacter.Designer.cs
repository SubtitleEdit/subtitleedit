namespace Nikse.SubtitleEdit.Forms.Ocr
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
            this.contextMenuStripLetters = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelSubtitleImage = new System.Windows.Forms.Label();
            this.pictureBoxSubtitleImage = new System.Windows.Forms.PictureBox();
            this.groupBoxNewInput = new System.Windows.Forms.GroupBox();
            this.numericUpDownLinesToDraw = new System.Windows.Forms.NumericUpDown();
            this.labelLinestoDraw = new System.Windows.Forms.Label();
            this.buttonGuessAgain = new System.Windows.Forms.Button();
            this.checkBoxAutoSubmitOfFirstChar = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labelItalicOn = new System.Windows.Forms.Label();
            this.checkBoxItalic = new System.Windows.Forms.CheckBox();
            this.buttonZoomOut = new System.Windows.Forms.Button();
            this.buttonZoomIn = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButtonCold = new System.Windows.Forms.RadioButton();
            this.radioButtonHot = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.listBoxlinesBackground = new System.Windows.Forms.ListBox();
            this.contextMenuStripLinesBackground = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeBackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.labelLines = new System.Windows.Forms.Label();
            this.listBoxLinesForeground = new System.Windows.Forms.ListBox();
            this.contextMenuLinesForeground = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeForegroundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCharacter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSubtitleImage)).BeginInit();
            this.groupBoxNewInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLinesToDraw)).BeginInit();
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
            this.buttonShrinkSelection.Size = new System.Drawing.Size(112, 23);
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
            this.buttonExpandSelection.Size = new System.Drawing.Size(112, 23);
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
            this.buttonAbort.Location = new System.Drawing.Point(541, 613);
            this.buttonAbort.Name = "buttonAbort";
            this.buttonAbort.Size = new System.Drawing.Size(75, 23);
            this.buttonAbort.TabIndex = 25;
            this.buttonAbort.Text = "&Abort";
            this.buttonAbort.UseVisualStyleBackColor = true;
            this.buttonAbort.Click += new System.EventHandler(this.buttonAbort_Click);
            // 
            // labelCharacters
            // 
            this.labelCharacters.AutoSize = true;
            this.labelCharacters.Location = new System.Drawing.Point(185, 127);
            this.labelCharacters.Name = "labelCharacters";
            this.labelCharacters.Size = new System.Drawing.Size(64, 13);
            this.labelCharacters.TabIndex = 30;
            this.labelCharacters.Text = "Character(s)";
            // 
            // pictureBoxCharacter
            // 
            this.pictureBoxCharacter.BackColor = System.Drawing.Color.DimGray;
            this.pictureBoxCharacter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxCharacter.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pictureBoxCharacter.Location = new System.Drawing.Point(183, 148);
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
            this.labelCharactersAsText.Location = new System.Drawing.Point(185, 37);
            this.labelCharactersAsText.Name = "labelCharactersAsText";
            this.labelCharactersAsText.Size = new System.Drawing.Size(98, 13);
            this.labelCharactersAsText.TabIndex = 28;
            this.labelCharactersAsText.Text = "Character(s) as text";
            // 
            // textBoxCharacters
            // 
            this.textBoxCharacters.ContextMenuStrip = this.contextMenuStripLetters;
            this.textBoxCharacters.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxCharacters.Location = new System.Drawing.Point(188, 58);
            this.textBoxCharacters.Name = "textBoxCharacters";
            this.textBoxCharacters.Size = new System.Drawing.Size(107, 23);
            this.textBoxCharacters.TabIndex = 22;
            this.textBoxCharacters.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxCharacters_KeyDown);
            this.textBoxCharacters.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxCharacters_KeyUp);
            // 
            // contextMenuStripLetters
            // 
            this.contextMenuStripLetters.Name = "contextMenuStripLetters";
            this.contextMenuStripLetters.Size = new System.Drawing.Size(181, 26);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(379, 613);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
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
            this.buttonCancel.Location = new System.Drawing.Point(460, 613);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 24;
            this.buttonCancel.Text = "&Skip";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelSubtitleImage
            // 
            this.labelSubtitleImage.AutoSize = true;
            this.labelSubtitleImage.Location = new System.Drawing.Point(12, 17);
            this.labelSubtitleImage.Name = "labelSubtitleImage";
            this.labelSubtitleImage.Size = new System.Drawing.Size(73, 13);
            this.labelSubtitleImage.TabIndex = 27;
            this.labelSubtitleImage.Text = "Subtitle image";
            // 
            // pictureBoxSubtitleImage
            // 
            this.pictureBoxSubtitleImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxSubtitleImage.BackColor = System.Drawing.Color.DimGray;
            this.pictureBoxSubtitleImage.Location = new System.Drawing.Point(12, 36);
            this.pictureBoxSubtitleImage.Name = "pictureBoxSubtitleImage";
            this.pictureBoxSubtitleImage.Size = new System.Drawing.Size(604, 205);
            this.pictureBoxSubtitleImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxSubtitleImage.TabIndex = 26;
            this.pictureBoxSubtitleImage.TabStop = false;
            // 
            // groupBoxNewInput
            // 
            this.groupBoxNewInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxNewInput.Controls.Add(this.numericUpDownLinesToDraw);
            this.groupBoxNewInput.Controls.Add(this.labelLinestoDraw);
            this.groupBoxNewInput.Controls.Add(this.buttonGuessAgain);
            this.groupBoxNewInput.Controls.Add(this.checkBoxAutoSubmitOfFirstChar);
            this.groupBoxNewInput.Controls.Add(this.label1);
            this.groupBoxNewInput.Controls.Add(this.label4);
            this.groupBoxNewInput.Controls.Add(this.label3);
            this.groupBoxNewInput.Controls.Add(this.labelItalicOn);
            this.groupBoxNewInput.Controls.Add(this.checkBoxItalic);
            this.groupBoxNewInput.Controls.Add(this.buttonZoomOut);
            this.groupBoxNewInput.Controls.Add(this.buttonZoomIn);
            this.groupBoxNewInput.Controls.Add(this.groupBox2);
            this.groupBoxNewInput.Controls.Add(this.label2);
            this.groupBoxNewInput.Controls.Add(this.listBoxlinesBackground);
            this.groupBoxNewInput.Controls.Add(this.labelLines);
            this.groupBoxNewInput.Controls.Add(this.labelCharacters);
            this.groupBoxNewInput.Controls.Add(this.pictureBoxCharacter);
            this.groupBoxNewInput.Controls.Add(this.listBoxLinesForeground);
            this.groupBoxNewInput.Controls.Add(this.labelCharactersAsText);
            this.groupBoxNewInput.Controls.Add(this.textBoxCharacters);
            this.groupBoxNewInput.Location = new System.Drawing.Point(15, 247);
            this.groupBoxNewInput.Name = "groupBoxNewInput";
            this.groupBoxNewInput.Size = new System.Drawing.Size(600, 360);
            this.groupBoxNewInput.TabIndex = 33;
            this.groupBoxNewInput.TabStop = false;
            // 
            // numericUpDownLinesToDraw
            // 
            this.numericUpDownLinesToDraw.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownLinesToDraw.Location = new System.Drawing.Point(455, 99);
            this.numericUpDownLinesToDraw.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownLinesToDraw.Name = "numericUpDownLinesToDraw";
            this.numericUpDownLinesToDraw.Size = new System.Drawing.Size(65, 20);
            this.numericUpDownLinesToDraw.TabIndex = 40;
            // 
            // labelLinestoDraw
            // 
            this.labelLinestoDraw.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLinestoDraw.AutoSize = true;
            this.labelLinestoDraw.Location = new System.Drawing.Point(452, 81);
            this.labelLinestoDraw.Name = "labelLinestoDraw";
            this.labelLinestoDraw.Size = new System.Drawing.Size(70, 13);
            this.labelLinestoDraw.TabIndex = 39;
            this.labelLinestoDraw.Text = "Lines to draw";
            // 
            // buttonGuessAgain
            // 
            this.buttonGuessAgain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGuessAgain.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGuessAgain.Location = new System.Drawing.Point(455, 125);
            this.buttonGuessAgain.Name = "buttonGuessAgain";
            this.buttonGuessAgain.Size = new System.Drawing.Size(139, 23);
            this.buttonGuessAgain.TabIndex = 38;
            this.buttonGuessAgain.Text = "&Guess again";
            this.buttonGuessAgain.UseVisualStyleBackColor = true;
            this.buttonGuessAgain.Click += new System.EventHandler(this.buttonGuessAgain_Click);
            // 
            // checkBoxAutoSubmitOfFirstChar
            // 
            this.checkBoxAutoSubmitOfFirstChar.AutoSize = true;
            this.checkBoxAutoSubmitOfFirstChar.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxAutoSubmitOfFirstChar.Location = new System.Drawing.Point(188, 87);
            this.checkBoxAutoSubmitOfFirstChar.Name = "checkBoxAutoSubmitOfFirstChar";
            this.checkBoxAutoSubmitOfFirstChar.Size = new System.Drawing.Size(144, 17);
            this.checkBoxAutoSubmitOfFirstChar.TabIndex = 37;
            this.checkBoxAutoSubmitOfFirstChar.Text = "A&uto submit on first char";
            this.checkBoxAutoSubmitOfFirstChar.UseVisualStyleBackColor = true;
            this.checkBoxAutoSubmitOfFirstChar.CheckedChanged += new System.EventHandler(this.checkBoxAutoSubmitOfFirstChar_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(394, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 13);
            this.label1.TabIndex = 34;
            this.label1.Text = "Tips for drawing";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(394, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(122, 13);
            this.label4.TabIndex = 36;
            this.label4.Text = "Ctrl+z=undo, Ctrl+y=redo";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(394, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(151, 13);
            this.label3.TabIndex = 35;
            this.label3.Text = "Hold Ctrl down to continue line";
            // 
            // labelItalicOn
            // 
            this.labelItalicOn.AutoSize = true;
            this.labelItalicOn.Font = new System.Drawing.Font("Tahoma", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelItalicOn.ForeColor = System.Drawing.Color.Red;
            this.labelItalicOn.Location = new System.Drawing.Point(185, 13);
            this.labelItalicOn.Name = "labelItalicOn";
            this.labelItalicOn.Size = new System.Drawing.Size(51, 19);
            this.labelItalicOn.TabIndex = 34;
            this.labelItalicOn.Text = "Italic";
            // 
            // checkBoxItalic
            // 
            this.checkBoxItalic.AutoSize = true;
            this.checkBoxItalic.Location = new System.Drawing.Point(301, 60);
            this.checkBoxItalic.Name = "checkBoxItalic";
            this.checkBoxItalic.Size = new System.Drawing.Size(48, 17);
            this.checkBoxItalic.TabIndex = 33;
            this.checkBoxItalic.Text = "&Italic";
            this.checkBoxItalic.UseVisualStyleBackColor = true;
            this.checkBoxItalic.CheckedChanged += new System.EventHandler(this.checkBoxItalic_CheckedChanged);
            // 
            // buttonZoomOut
            // 
            this.buttonZoomOut.Location = new System.Drawing.Point(255, 116);
            this.buttonZoomOut.Name = "buttonZoomOut";
            this.buttonZoomOut.Size = new System.Drawing.Size(25, 23);
            this.buttonZoomOut.TabIndex = 32;
            this.buttonZoomOut.Text = "-";
            this.buttonZoomOut.UseVisualStyleBackColor = true;
            this.buttonZoomOut.Click += new System.EventHandler(this.buttonZoomOut_Click);
            // 
            // buttonZoomIn
            // 
            this.buttonZoomIn.Location = new System.Drawing.Point(286, 116);
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
            this.label2.Location = new System.Drawing.Point(9, 230);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Lines - not foreground";
            // 
            // listBoxlinesBackground
            // 
            this.listBoxlinesBackground.ContextMenuStrip = this.contextMenuStripLinesBackground;
            this.listBoxlinesBackground.FormattingEnabled = true;
            this.listBoxlinesBackground.Location = new System.Drawing.Point(12, 246);
            this.listBoxlinesBackground.Name = "listBoxlinesBackground";
            this.listBoxlinesBackground.Size = new System.Drawing.Size(151, 95);
            this.listBoxlinesBackground.TabIndex = 19;
            this.listBoxlinesBackground.SelectedIndexChanged += new System.EventHandler(this.listBoxLinesBackground_SelectedIndexChanged);
            this.listBoxlinesBackground.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxLinesBackground_KeyDown);
            // 
            // contextMenuStripLinesBackground
            // 
            this.contextMenuStripLinesBackground.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeBackToolStripMenuItem,
            this.clearToolStripMenuItem1});
            this.contextMenuStripLinesBackground.Name = "contextMenuStripLines";
            this.contextMenuStripLinesBackground.Size = new System.Drawing.Size(118, 48);
            // 
            // removeBackToolStripMenuItem
            // 
            this.removeBackToolStripMenuItem.Name = "removeBackToolStripMenuItem";
            this.removeBackToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.removeBackToolStripMenuItem.Text = "Remove";
            this.removeBackToolStripMenuItem.Click += new System.EventHandler(this.removeBackgroundLineToolStripMenuItem_Click);
            // 
            // clearToolStripMenuItem1
            // 
            this.clearToolStripMenuItem1.Name = "clearToolStripMenuItem1";
            this.clearToolStripMenuItem1.Size = new System.Drawing.Size(117, 22);
            this.clearToolStripMenuItem1.Text = "Clear";
            this.clearToolStripMenuItem1.Click += new System.EventHandler(this.clearToolStripMenuItem1_Click);
            // 
            // labelLines
            // 
            this.labelLines.AutoSize = true;
            this.labelLines.Location = new System.Drawing.Point(9, 93);
            this.labelLines.Name = "labelLines";
            this.labelLines.Size = new System.Drawing.Size(92, 13);
            this.labelLines.TabIndex = 18;
            this.labelLines.Text = "Lines - foreground";
            // 
            // listBoxLinesForeground
            // 
            this.listBoxLinesForeground.ContextMenuStrip = this.contextMenuLinesForeground;
            this.listBoxLinesForeground.FormattingEnabled = true;
            this.listBoxLinesForeground.Location = new System.Drawing.Point(12, 109);
            this.listBoxLinesForeground.Name = "listBoxLinesForeground";
            this.listBoxLinesForeground.Size = new System.Drawing.Size(151, 108);
            this.listBoxLinesForeground.TabIndex = 17;
            this.listBoxLinesForeground.SelectedIndexChanged += new System.EventHandler(this.listBoxLinesForeground_SelectedIndexChanged);
            this.listBoxLinesForeground.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxLinesForeground_KeyDown);
            this.listBoxLinesForeground.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.listBoxLinesForeground_KeyPress);
            // 
            // contextMenuLinesForeground
            // 
            this.contextMenuLinesForeground.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeForegroundToolStripMenuItem,
            this.clearToolStripMenuItem});
            this.contextMenuLinesForeground.Name = "contextMenuStripLines";
            this.contextMenuLinesForeground.Size = new System.Drawing.Size(118, 48);
            // 
            // removeForegroundToolStripMenuItem
            // 
            this.removeForegroundToolStripMenuItem.Name = "removeForegroundToolStripMenuItem";
            this.removeForegroundToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.removeForegroundToolStripMenuItem.Text = "Remove";
            this.removeForegroundToolStripMenuItem.Click += new System.EventHandler(this.removeForegroundToolStripMenuItem_Click);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // VobSubOcrNOcrCharacter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(648, 642);
            this.Controls.Add(this.groupBoxNewInput);
            this.Controls.Add(this.buttonShrinkSelection);
            this.Controls.Add(this.buttonExpandSelection);
            this.Controls.Add(this.buttonAbort);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.pictureBoxSubtitleImage);
            this.Controls.Add(this.labelSubtitleImage);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(660, 675);
            this.Name = "VobSubOcrNOcrCharacter";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "nOCR character";
            this.Load += new System.EventHandler(this.VobSubOcrNOcrCharacter_Load);
            this.Shown += new System.EventHandler(this.VobSubOcrNOcrCharacter_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VobSubOcrNOcrCharacter_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCharacter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSubtitleImage)).EndInit();
            this.groupBoxNewInput.ResumeLayout(false);
            this.groupBoxNewInput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLinesToDraw)).EndInit();
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
        private System.Windows.Forms.GroupBox groupBoxNewInput;
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
        private System.Windows.Forms.CheckBox checkBoxItalic;
        private System.Windows.Forms.Label labelItalicOn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxAutoSubmitOfFirstChar;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem1;
        private System.Windows.Forms.NumericUpDown numericUpDownLinesToDraw;
        private System.Windows.Forms.Label labelLinestoDraw;
        private System.Windows.Forms.Button buttonGuessAgain;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripLetters;
    }
}