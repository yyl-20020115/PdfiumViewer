using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PdfiumViewer.Demo
{
    public static class Program
    {
        public static string DefaultPDFDatabasePath = "C:\\Working\\PdfDatabase";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(DefaultPDFDatabasePath));
        }
    }
}
