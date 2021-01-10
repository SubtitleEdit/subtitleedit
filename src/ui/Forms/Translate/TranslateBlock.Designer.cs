
namespace Nikse.SubtitleEdit.Forms.Translate
{
    partial class TranslateBlock
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
            this.labelInfo = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonGetTargetGet = new System.Windows.Forms.Button();
            this.buttonCopySourceTextToClipboard = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelInfo
            // 
            this.labelInfo.Location = new System.Drawing.Point(12, 19);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(524, 33);
            this.labelInfo.TabIndex = 8;
            this.labelInfo.Text = "Go to translator and paste text, copy result back to clipboard and click the butt" +
    "on below";
            this.labelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(452, 165);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonGetTargetGet
            // 
            this.buttonGetTargetGet.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonGetTargetGet.Location = new System.Drawing.Point(118, 55);
            this.buttonGetTargetGet.Name = "buttonGetTargetGet";
            this.buttonGetTargetGet.Size = new System.Drawing.Size(294, 67);
            this.buttonGetTargetGet.TabIndex = 0;
            this.buttonGetTargetGet.Text = "Get translated text from clipboard\r\n(Ctrl+V)";
            this.buttonGetTargetGet.UseVisualStyleBackColor = true;
            this.buttonGetTargetGet.Click += new System.EventHandler(this.buttonGetTargetGet_Click);
            // 
            // buttonCopySourceTextToClipboard
            // 
            this.buttonCopySourceTextToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCopySourceTextToClipboard.Location = new System.Drawing.Point(211, 165);
            this.buttonCopySourceTextToClipboard.Name = "buttonCopySourceTextToClipboard";
            this.buttonCopySourceTextToClipboard.Size = new System.Drawing.Size(235, 23);
            this.buttonCopySourceTextToClipboard.TabIndex = 1;
            this.buttonCopySourceTextToClipboard.Text = "Copy source text clipboard again";
            this.buttonCopySourceTextToClipboard.UseVisualStyleBackColor = true;
            this.buttonCopySourceTextToClipboard.Click += new System.EventHandler(this.buttonCopySourceTextToClipboard_Click);
            // 
            // TranslateBlock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 200);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonGetTargetGet);
            this.Controls.Add(this.buttonCopySourceTextToClipboard);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TranslateBlock";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TranslateBlock";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TranslateBlock_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonGetTargetGet;
        private System.Windows.Forms.Button buttonCopySourceTextToClipboard;
    }
}