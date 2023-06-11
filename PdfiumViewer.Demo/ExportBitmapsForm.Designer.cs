namespace PdSearcher
{
    partial class ExportBitmapsForm
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
            this.label1 = new System.Windows.Forms.Label();
            this._dpiXTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this._dpiYTextBox = new System.Windows.Forms.TextBox();
            this._acceptButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "横向 DPI:";
            // 
            // _dpiXTextBox
            // 
            this._dpiXTextBox.Location = new System.Drawing.Point(106, 11);
            this._dpiXTextBox.Name = "_dpiXTextBox";
            this._dpiXTextBox.Size = new System.Drawing.Size(240, 21);
            this._dpiXTextBox.TabIndex = 1;
            this._dpiXTextBox.Text = "96";
            this._dpiXTextBox.TextChanged += new System.EventHandler(this.DpiX_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "纵向 DPI:";
            // 
            // _dpiYTextBox
            // 
            this._dpiYTextBox.Location = new System.Drawing.Point(106, 35);
            this._dpiYTextBox.Name = "_dpiYTextBox";
            this._dpiYTextBox.Size = new System.Drawing.Size(240, 21);
            this._dpiYTextBox.TabIndex = 3;
            this._dpiYTextBox.Text = "96";
            this._dpiYTextBox.TextChanged += new System.EventHandler(this.DpiY_TextChanged);
            // 
            // _acceptButton
            // 
            this._acceptButton.Location = new System.Drawing.Point(190, 59);
            this._acceptButton.Name = "_acceptButton";
            this._acceptButton.Size = new System.Drawing.Size(75, 21);
            this._acceptButton.TabIndex = 4;
            this._acceptButton.Text = "确定(&O)";
            this._acceptButton.UseVisualStyleBackColor = true;
            this._acceptButton.Click += new System.EventHandler(this.AcceptButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(271, 59);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 21);
            this._cancelButton.TabIndex = 5;
            this._cancelButton.Text = "取消(&C)";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // ExportBitmapsForm
            // 
            this.AcceptButton = this._acceptButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(358, 91);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._acceptButton);
            this.Controls.Add(this._dpiYTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._dpiXTextBox);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportBitmapsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "输出位图";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _dpiXTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _dpiYTextBox;
        private System.Windows.Forms.Button _acceptButton;
        private System.Windows.Forms.Button _cancelButton;
    }
}