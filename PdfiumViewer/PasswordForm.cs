using System;
using System.Windows.Forms;

namespace PdfiumViewer
{
    internal partial class PasswordForm : Form
    {
        public string Password => _password.Text;

        public PasswordForm()
        {
            InitializeComponent();

            UpdateEnabled();
        }

        private void Password_TextChanged(object sender, EventArgs e)
        {
            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            acceptButton.Enabled = _password.Text.Length > 0;
        }

        private void AcceptButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
