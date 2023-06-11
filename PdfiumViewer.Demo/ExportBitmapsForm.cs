using System;
using System.Windows.Forms;

namespace PdfSearcher
{
    public partial class ExportBitmapsForm : Form
    {
        private int _dpiX;
        private int _dpiY;

        public int DpiX => _dpiX;

        public int DpiY => _dpiY;

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
            _acceptButton.Enabled =
                int.TryParse(_dpiXTextBox.Text, out _dpiX) &&
                int.TryParse(_dpiYTextBox.Text, out _dpiY);
        }
    }

}

