using System;
using System.Windows.Forms;

namespace PdfSearcher
{
    public partial class ExportBitmapsForm : Form
    {
        private int dpiX;
        private int dpiY;

        public int DpiX => dpiX;

        public int DpiY => dpiY;

        public ExportBitmapsForm()
        {
            InitializeComponent();
            UpdateEnabled();
        }

        private void AcceptButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void DpiX_TextChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void DpiY_TextChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            acceptButton.Enabled =
                int.TryParse(_dpiXTextBox.Text, out dpiX) &&
                int.TryParse(_dpiYTextBox.Text, out dpiY);
        }
    }

}

