using System;
using System.Windows.Forms;

namespace PdfSearcher
{
    public partial class FormContent : Form
    {
        public string Content
        {
            get => this.textBoxContent.Text;
            set => this.textBoxContent.Text = value;
        }
        public FormContent()
        {
            InitializeComponent();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(Content);
        }
        private void ButtonCancel_Click(object sender, EventArgs e)
        {

        }

        private void FormContent_Load(object sender, EventArgs e)
        {

        }

    }
}
