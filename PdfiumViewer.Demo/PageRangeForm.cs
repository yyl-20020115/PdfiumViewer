using System;
using System.Windows.Forms;
using PdfiumViewer;

namespace PdfSearcher
{
    public partial class PageRangeForm : Form
    {
        private readonly IPdfDocument document;

        public IPdfDocument Document { get; private set; }

        public PageRangeForm(IPdfDocument document)
        {
            this.document = document;

            InitializeComponent();

            startPage.Text = "1";
            endPage.Text = document.PageCount.ToString();
        }

        private void AcceptButton_Click(object sender, EventArgs e)
        {
            if (
                !int.TryParse(this.startPage.Text, out int startPage) ||
                !int.TryParse(this.endPage.Text, out int endPage) ||
                startPage < 1 ||
                endPage > document.PageCount ||
                startPage > endPage
            )
            {
                MessageBox.Show(this, "Invalid start/end page");
            }
            else
            {
                Document = PdfRangeDocument.FromDocument(document, startPage - 1, endPage - 1);

                DialogResult = DialogResult.OK;
            }
        }
    }
}
