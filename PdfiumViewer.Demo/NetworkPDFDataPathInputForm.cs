using System;
using System.Windows.Forms;

namespace PdfSearcher
{
    public partial class NetworkPDFDataPathInputForm : Form
    {
        public string Path
        {
            get=>this.textBoxInput.Text;
            set => this.textBoxInput.Text = value;
        }
        public NetworkPDFDataPathInputForm()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {

        }
    }
}
