namespace Nikse.SubtitleEdit.Forms.Ocr
{
    sealed partial class VobSubCharactersImport
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.ColumnImport = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonImport = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.buttonFixesSelectAll = new System.Windows.Forms.Button();
            this.buttonFixesInverse = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.groupBoxCurrentImage = new System.Windows.Forms.GroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelCurrentImage = new System.Windows.Forms.Label();
            this.buttonDone = new System.Windows.Forms.Button();
            this.groupBoxCurrentImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listView1.CheckBoxes = true;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnImport,
            this.ColumnText});
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 56);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(450, 426);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // ColumnImport
            // 
            this.ColumnImport.Text = "Import";
            this.ColumnImport.Width = 80;
            // 
            // ColumnText
            // 
            this.ColumnText.Text = "Text";
            this.ColumnText.Width = 340;
            // 
            // buttonImport
            // 
            this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonImport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonImport.Location = new System.Drawing.Point(337, 493);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(125, 23);
            this.buttonImport.TabIndex = 4;
            this.buttonImport.Text = "I&mport";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(759, 493);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // buttonFixesSelectAll
            // 
            this.buttonFixesSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonFixesSelectAll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonFixesSelectAll.Location = new System.Drawing.Point(12, 493);
            this.buttonFixesSelectAll.Name = "buttonFixesSelectAll";
            this.buttonFixesSelectAll.Size = new System.Drawing.Size(75, 23);
            this.buttonFixesSelectAll.TabIndex = 105;
            this.buttonFixesSelectAll.Text = "Select &all";
            this.buttonFixesSelectAll.UseVisualStyleBackColor = true;
            this.buttonFixesSelectAll.Click += new System.EventHandler(this.buttonFixesSelectAll_Click);
            // 
            // buttonFixesInverse
            // 
            this.buttonFixesInverse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonFixesInverse.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonFixesInverse.Location = new System.Drawing.Point(93, 493);
            this.buttonFixesInverse.Name = "buttonFixesInverse";
            this.buttonFixesInverse.Size = new System.Drawing.Size(100, 23);
            this.buttonFixesInverse.TabIndex = 106;
            this.buttonFixesInverse.Text = "&Inverse selection";
            this.buttonFixesInverse.UseVisualStyleBackColor = true;
            this.buttonFixesInverse.Click += new System.EventHandler(this.buttonFixesInverse_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(12, 22);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(47, 13);
            this.labelInfo.TabIndex = 107;
            this.labelInfo.Text = "labelInfo";
            // 
            // groupBoxCurrentImage
            // 
            this.groupBoxCurrentImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxCurrentImage.Controls.Add(this.pictureBox1);
            this.groupBoxCurrentImage.Controls.Add(this.labelCurrentImage);
            this.groupBoxCurrentImage.Location = new System.Drawing.Point(469, 56);
            this.groupBoxCurrentImage.Name = "groupBoxCurrentImage";
            this.groupBoxCurrentImage.Size = new System.Drawing.Size(365, 426);
            this.groupBoxCurrentImage.TabIndex = 108;
            this.groupBoxCurrentImage.TabStop = false;
            this.groupBoxCurrentImage.Text = "Current image";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Red;
            this.pictureBox1.Location = new System.Drawing.Point(19, 107);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(52, 52);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 109;
            this.pictureBox1.TabStop = false;
            // 
            // labelCurrentImage
            // 
            this.labelCurrentImage.AutoSize = true;
            this.labelCurrentImage.Location = new System.Drawing.Point(16, 30);
            this.labelCurrentImage.Name = "labelCurrentImage";
            this.labelCurrentImage.Size = new System.Drawing.Size(88, 13);
            this.labelCurrentImage.TabIndex = 108;
            this.labelCurrentImage.Text = "CurrentImageInfo";
            // 
            // buttonDone
            // 
            this.buttonDone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDone.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonDone.Location = new System.Drawing.Point(628, 493);
            this.buttonDone.Name = "buttonDone";
            this.buttonDone.Size = new System.Drawing.Size(125, 23);
            this.buttonDone.TabIndex = 109;
            this.buttonDone.Text = "&Done";
            this.buttonDone.UseVisualStyleBackColor = true;
            this.buttonDone.Click += new System.EventHandler(this.buttonDone_Click);
            // 
            // VobSubCharactersImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(846, 526);
            this.Controls.Add(this.buttonDone);
            this.Controls.Add(this.groupBoxCurrentImage);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonFixesSelectAll);
            this.Controls.Add(this.buttonFixesInverse);
            this.Controls.Add(this.buttonImport);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.listView1);
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "VobSubCharactersImport";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Shown += new System.EventHandler(this.VobSubCharactersImport_Shown);
            this.groupBoxCurrentImage.ResumeLayout(false);
            this.groupBoxCurrentImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ColumnHeader ColumnImport;
        private System.Windows.Forms.ColumnHeader ColumnText;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button buttonFixesSelectAll;
        private System.Windows.Forms.Button buttonFixesInverse;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.GroupBox groupBoxCurrentImage;
        private System.Windows.Forms.Label labelCurrentImage;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button buttonDone;
    }
}