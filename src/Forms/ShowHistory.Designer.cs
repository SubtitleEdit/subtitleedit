namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ShowHistory
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
            this.listViewHistory = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonRollback = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonCompare = new System.Windows.Forms.Button();
            this.buttonCompareHistory = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listViewHistory
            // 
            this.listViewHistory.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listViewHistory.FullRowSelect = true;
            this.listViewHistory.HideSelection = false;
            this.listViewHistory.Location = new System.Drawing.Point(12, 37);
            this.listViewHistory.Name = "listViewHistory";
            this.listViewHistory.Size = new System.Drawing.Size(472, 324);
            this.listViewHistory.TabIndex = 0;
            this.listViewHistory.UseCompatibleStateImageBehavior = false;
            this.listViewHistory.View = System.Windows.Forms.View.Details;
            this.listViewHistory.SelectedIndexChanged += new System.EventHandler(this.ListViewHistorySelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Time";
            this.columnHeader1.Width = 80;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Description";
            this.columnHeader2.Width = 365;
            // 
            // buttonRollback
            // 
            this.buttonRollback.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRollback.Location = new System.Drawing.Point(489, 338);
            this.buttonRollback.Name = "buttonRollback";
            this.buttonRollback.Size = new System.Drawing.Size(98, 23);
            this.buttonRollback.TabIndex = 6;
            this.buttonRollback.Text = "&Rollback";
            this.buttonRollback.UseVisualStyleBackColor = true;
            this.buttonRollback.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(594, 338);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(171, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Select time/description for rollback";
            // 
            // buttonCompare
            // 
            this.buttonCompare.Enabled = false;
            this.buttonCompare.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCompare.Location = new System.Drawing.Point(489, 200);
            this.buttonCompare.Name = "buttonCompare";
            this.buttonCompare.Size = new System.Drawing.Size(179, 23);
            this.buttonCompare.TabIndex = 4;
            this.buttonCompare.Text = "&Compare with current";
            this.buttonCompare.UseVisualStyleBackColor = true;
            this.buttonCompare.Click += new System.EventHandler(this.ButtonCompareClick);
            // 
            // buttonCompareHistory
            // 
            this.buttonCompareHistory.Enabled = false;
            this.buttonCompareHistory.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCompareHistory.Location = new System.Drawing.Point(490, 171);
            this.buttonCompareHistory.Name = "buttonCompareHistory";
            this.buttonCompareHistory.Size = new System.Drawing.Size(179, 23);
            this.buttonCompareHistory.TabIndex = 2;
            this.buttonCompareHistory.Text = "&Compare history items";
            this.buttonCompareHistory.UseVisualStyleBackColor = true;
            this.buttonCompareHistory.Click += new System.EventHandler(this.ButtonCompareHistoryClick);
            // 
            // ShowHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 373);
            this.Controls.Add(this.buttonCompareHistory);
            this.Controls.Add(this.buttonCompare);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonRollback);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.listViewHistory);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ShowHistory";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "History (for undo)";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormShowHistory_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewHistory;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button buttonRollback;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonCompare;
        private System.Windows.Forms.Button buttonCompareHistory;
    }
}