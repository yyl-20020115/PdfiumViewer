using System;
using System.Windows.Forms;
using PdfiumViewer;

namespace PdfSearcher
{
    public partial class PrintMultiplePagesForm : Form
    {
        private readonly PdfViewer viewer;

        public PrintMultiplePagesForm(PdfViewer viewer)
        {
            this.viewer = viewer ?? throw new ArgumentNullException(nameof(viewer));

            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(_horizontal.Text, out int horizontal))
            {
                MessageBox.Show(this, "Invalid horizontal");
            }
            else if (!int.TryParse(_vertical.Text, out int vertical))
            {
                MessageBox.Show(this, "Invalid vertical");
            }
            else if (!float.TryParse(_margin.Text, out float margin))
            {
                MessageBox.Show(this, "Invalid margin");
            }
            else
            {
                var settings = new PdfPrintSettings(
                    viewer.DefaultPrintMode,
                    new PdfPrintMultiplePages(
                        horizontal,
                        vertical,
                        _horizontalOrientation.Checked ? Orientation.Horizontal : Orientation.Vertical,
                        margin
                    )
                );

                using (var form = new PrintPreviewDialog())
                {
                    form.Document = viewer.Document.CreatePrintDocument(settings);
                    form.ShowDialog(this);
                }

                DialogResult = DialogResult.OK;
            }
        }
    }
}
