namespace Nikse.SubtitleEdit.Forms
{
    partial class Interjections
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
            this.groupBoxNamesIgonoreLists = new System.Windows.Forms.GroupBox();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.listBoxInterjections = new System.Windows.Forms.ListBox();
            this.textBoxInterjection = new System.Windows.Forms.TextBox();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxNamesIgonoreLists.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxNamesIgonoreLists
            // 
            this.groupBoxNamesIgonoreLists.Controls.Add(this.buttonRemove);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.listBoxInterjections);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.textBoxInterjection);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.buttonAdd);
            this.groupBoxNamesIgonoreLists.Location = new System.Drawing.Point(12, 12);
            this.groupBoxNamesIgonoreLists.Name = "groupBoxNamesIgonoreLists";
            this.groupBoxNamesIgonoreLists.Size = new System.Drawing.Size(241, 250);
            this.groupBoxNamesIgonoreLists.TabIndex = 3;
            this.groupBoxNamesIgonoreLists.TabStop = false;
            // 
            // buttonRemove
            // 
            this.buttonRemove.Enabled = false;
            this.buttonRemove.Location = new System.Drawing.Point(159, 16);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(75, 23);
            this.buttonRemove.TabIndex = 22;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // listBoxInterjections
            // 
            this.listBoxInterjections.FormattingEnabled = true;
            this.listBoxInterjections.Location = new System.Drawing.Point(3, 16);
            this.listBoxInterjections.Name = "listBoxInterjections";
            this.listBoxInterjections.Size = new System.Drawing.Size(150, 199);
            this.listBoxInterjections.TabIndex = 20;
            this.listBoxInterjections.SelectedIndexChanged += new System.EventHandler(this.listBoxInterjections_SelectedIndexChanged);
            // 
            // textBoxInterjection
            // 
            this.textBoxInterjection.Location = new System.Drawing.Point(2, 220);
            this.textBoxInterjection.Name = "textBoxInterjection";
            this.textBoxInterjection.Size = new System.Drawing.Size(151, 20);
            this.textBoxInterjection.TabIndex = 24;
            this.textBoxInterjection.TextChanged += new System.EventHandler(this.textBoxInterjection_TextChanged);
            this.textBoxInterjection.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxInterjection_KeyDown);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Enabled = false;
            this.buttonAdd.Location = new System.Drawing.Point(159, 219);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonAdd.TabIndex = 26;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(178, 281);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(99, 281);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // Interjections
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(268, 315);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxNamesIgonoreLists);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Interjections";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Interjections";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Interjections_KeyDown);
            this.groupBoxNamesIgonoreLists.ResumeLayout(false);
            this.groupBoxNamesIgonoreLists.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxNamesIgonoreLists;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.ListBox listBoxInterjections;
        private System.Windows.Forms.TextBox textBoxInterjection;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
    }
}