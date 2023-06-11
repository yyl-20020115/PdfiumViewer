namespace PdSearcher
{
    partial class SearchForm
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
            this._find = new System.Windows.Forms.TextBox();
            this.matchCase = new System.Windows.Forms.CheckBox();
            this.matchWholeWord = new System.Windows.Forms.CheckBox();
            this.highlightAll = new System.Windows.Forms.CheckBox();
            this._findPrevious = new System.Windows.Forms.Button();
            this._findNext = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "文本:";
            // 
            // _find
            // 
            this._find.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._find.Location = new System.Drawing.Point(65, 11);
            this._find.Name = "_find";
            this._find.Size = new System.Drawing.Size(321, 21);
            this._find.TabIndex = 1;
            this._find.TextChanged += new System.EventHandler(this.Find_TextChanged);
            // 
            // _matchCase
            // 
            this.matchCase.AutoSize = true;
            this.matchCase.Location = new System.Drawing.Point(65, 35);
            this.matchCase.Name = "_matchCase";
            this.matchCase.Size = new System.Drawing.Size(84, 16);
            this.matchCase.TabIndex = 2;
            this.matchCase.Text = "大小写匹配";
            this.matchCase.UseVisualStyleBackColor = true;
            this.matchCase.CheckedChanged += new System.EventHandler(this.MatchCase_CheckedChanged);
            // 
            // _matchWholeWord
            // 
            this.matchWholeWord.AutoSize = true;
            this.matchWholeWord.Location = new System.Drawing.Point(65, 56);
            this.matchWholeWord.Name = "_matchWholeWord";
            this.matchWholeWord.Size = new System.Drawing.Size(72, 16);
            this.matchWholeWord.TabIndex = 3;
            this.matchWholeWord.Text = "全字匹配";
            this.matchWholeWord.UseVisualStyleBackColor = true;
            this.matchWholeWord.CheckedChanged += new System.EventHandler(this.MatchWholeWord_CheckedChanged);
            // 
            // _highlightAll
            // 
            this.highlightAll.AutoSize = true;
            this.highlightAll.Location = new System.Drawing.Point(65, 78);
            this.highlightAll.Name = "_highlightAll";
            this.highlightAll.Size = new System.Drawing.Size(120, 16);
            this.highlightAll.TabIndex = 4;
            this.highlightAll.Text = "高亮显示所有匹配";
            this.highlightAll.UseVisualStyleBackColor = true;
            this.highlightAll.CheckedChanged += new System.EventHandler(this.HighlightAll_CheckedChanged);
            // 
            // _findPrevious
            // 
            this._findPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._findPrevious.Location = new System.Drawing.Point(186, 106);
            this._findPrevious.Name = "_findPrevious";
            this._findPrevious.Size = new System.Drawing.Size(97, 21);
            this._findPrevious.TabIndex = 5;
            this._findPrevious.Text = "查找上一个(&P)";
            this._findPrevious.UseVisualStyleBackColor = true;
            this._findPrevious.Click += new System.EventHandler(this.FindPrevious_Click);
            // 
            // _findNext
            // 
            this._findNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._findNext.Location = new System.Drawing.Point(289, 106);
            this._findNext.Name = "_findNext";
            this._findNext.Size = new System.Drawing.Size(97, 21);
            this._findNext.TabIndex = 6;
            this._findNext.Text = "查找下一个(&N)";
            this._findNext.UseVisualStyleBackColor = true;
            this._findNext.Click += new System.EventHandler(this.FindNext_Click);
            // 
            // SearchForm
            // 
            this.AcceptButton = this._findNext;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(398, 138);
            this.Controls.Add(this._findNext);
            this.Controls.Add(this._findPrevious);
            this.Controls.Add(this.highlightAll);
            this.Controls.Add(this.matchWholeWord);
            this.Controls.Add(this.matchCase);
            this.Controls.Add(this._find);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SearchForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "搜索文本";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _find;
        private System.Windows.Forms.CheckBox matchCase;
        private System.Windows.Forms.CheckBox matchWholeWord;
        private System.Windows.Forms.CheckBox highlightAll;
        private System.Windows.Forms.Button _findPrevious;
        private System.Windows.Forms.Button _findNext;
    }
}