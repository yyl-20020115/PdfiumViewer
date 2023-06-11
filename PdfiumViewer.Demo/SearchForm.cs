using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PdfiumViewer.Demo
{
    public partial class SearchForm : Form
    {
        private readonly PdfSearchManager searchManager;
        private bool findDirty;

        public SearchForm(PdfRenderer renderer)
        {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));

            InitializeComponent();

            searchManager = new PdfSearchManager(renderer);

            matchCase.Checked = searchManager.MatchCase;
            matchWholeWord.Checked = searchManager.MatchWholeWord;
            highlightAll.Checked = searchManager.HighlightAllMatches;
        }

        private void MatchCase_CheckedChanged(object sender, EventArgs e)
        {
            findDirty = true;
            searchManager.MatchCase = matchCase.Checked;
        }

        private void MatchWholeWord_CheckedChanged(object sender, EventArgs e)
        {
            findDirty = true;
            searchManager.MatchWholeWord = matchWholeWord.Checked;
        }

        private void HighlightAll_CheckedChanged(object sender, EventArgs e)
        {
            searchManager.HighlightAllMatches = highlightAll.Checked;
        }

        private void Find_TextChanged(object sender, EventArgs e)
        {
            findDirty = true;
        }

        private void FindPrevious_Click(object sender, EventArgs e)
        {
            Find(false);
        }

        private void FindNext_Click(object sender, EventArgs e)
        {
            Find(true);
        }

        private void Find(bool forward)
        {
            if (findDirty)
            {
                findDirty = false;

                if (!searchManager.Search(_find.Text))
                {
                    MessageBox.Show(this, "未能找到匹配");
                    return;
                }
            }

            if (!searchManager.FindNext(forward))
                MessageBox.Show(this, "查找已回到开始");
        }
    }
}
