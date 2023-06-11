using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PdfiumViewer.Demo
{
    public partial class MainForm : Form
    {
        private string PdfDatabasePath = string.Empty;

        private SearchForm searchForm;

        private PdfViewer PdfViewer 
            => this.tabControlBooks?.SelectedTab?.Controls?[0] as PdfViewer;

        public class PdfRecord
        {
            public string PdfPath;
        }


        public MainForm(string dir)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(dir) && Directory.Exists(PdfDatabasePath = dir))
            {
                var files = Directory.GetFiles(PdfDatabasePath,"*.pdf", SearchOption.AllDirectories);
                foreach(var file in files)
                {
                    var record = new PdfRecord() { PdfPath = file };
                    var ti = new TreeNode(Path.GetFileNameWithoutExtension(file)) { Tag = record };
                    this.treeViewBooks.Nodes.Add(ti);
                    
                    var tabPage = new TabPage() { Tag = record };
                    
                }


            }


            renderToBitmapsToolStripMenuItem.Enabled = false;

            PdfViewer.Renderer.DisplayRectangleChanged += Renderer_DisplayRectangleChanged;
            PdfViewer.Renderer.ZoomChanged += Renderer_ZoomChanged;

            PdfViewer.Renderer.MouseMove += Renderer_MouseMove;
            PdfViewer.Renderer.MouseLeave += Renderer_MouseLeave;
            ShowPdfLocation(PdfPoint.Empty);

            cutMarginsWhenPrintingToolStripMenuItem.PerformClick();

            zoom.Text = PdfViewer.Renderer.Zoom.ToString();

            Disposed += (s, e) => PdfViewer?.Document?.Dispose();
        }

        private void Renderer_MouseLeave(object sender, EventArgs e)
        {
            ShowPdfLocation(PdfPoint.Empty);
        }

        private void Renderer_MouseMove(object sender, MouseEventArgs e)
        {
            ShowPdfLocation(PdfViewer.Renderer.PointToPdf(e.Location));
        }

        private void ShowPdfLocation(PdfPoint point)
        {
            if (!point.IsValid)
            {
                _pageToolStripLabel.Text = null;
                _coordinatesToolStripLabel.Text = null;
            }
            else
            {
                _pageToolStripLabel.Text = (point.Page + 1).ToString();
                _coordinatesToolStripLabel.Text = point.Location.X + "," + point.Location.Y;
            }
        }

        void Renderer_ZoomChanged(object sender, EventArgs e)
        {
            zoom.Text = PdfViewer.Renderer.Zoom.ToString();
        }

        void Renderer_DisplayRectangleChanged(object sender, EventArgs e)
        {
            page.Text = (PdfViewer.Renderer.Page + 1).ToString();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            var args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {


                PdfViewer.Document?.Dispose();
                PdfViewer.Document = OpenDocument(args[1]);
                renderToBitmapsToolStripMenuItem.Enabled = true;
            }
            else
            {
                OpenFile();
            }

            _showBookmarks.Checked = PdfViewer.ShowBookmarks;
            _showToolbar.Checked = PdfViewer.ShowToolbar;
        }

        private PdfDocument OpenDocument(string fileName)
        {
            try
            {
                return PdfDocument.Load(this, fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void OpenFile()
        {
            using (var form = new OpenFileDialog())
            {
                form.Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*";
                form.RestoreDirectory = true;
                form.Title = "Open PDF File";

                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    Dispose();
                    return;
                }

                PdfViewer.Document?.Dispose();
                PdfViewer.Document = OpenDocument(form.FileName);
                renderToBitmapsToolStripMenuItem.Enabled = true;
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void RenderToBitmapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int dpiX;
            int dpiY;

            using (var form = new ExportBitmapsForm())
            {
                if (form.ShowDialog() != DialogResult.OK)
                    return;

                dpiX = form.DpiX;
                dpiY = form.DpiY;
            }

            string path;

            using (var form = new FolderBrowserDialog())
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                    return;

                path = form.SelectedPath;
            }

            var document = PdfViewer.Document;

            for (int i = 0; i < document.PageCount; i++)
            {
                using (var image = document.Render(i, (int)document.PageSizes[i].Width, (int)document.PageSizes[i].Height, dpiX, dpiY, false))
                {
                    image.Save(Path.Combine(path, "Page " + i + ".png"));
                }
            }
        }

        private void ToolStripButton1_Click_MM(object sender, EventArgs e)
        {
            PdfViewer.Renderer.Page--;
        }

        private void ToolStripButton2_Click_PP(object sender, EventArgs e)
        {
            PdfViewer.Renderer.Page++;
        }

        private void CutMarginsWhenPrintingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cutMarginsWhenPrintingToolStripMenuItem.Checked = true;
            shrinkToMarginsWhenPrintingToolStripMenuItem.Checked = false;

            PdfViewer.DefaultPrintMode = PdfPrintMode.CutMargin;
        }

        private void ShrinkToMarginsWhenPrintingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            shrinkToMarginsWhenPrintingToolStripMenuItem.Checked = true;
            cutMarginsWhenPrintingToolStripMenuItem.Checked = false;

            PdfViewer.DefaultPrintMode = PdfPrintMode.ShrinkToMargin;
        }

        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new PrintPreviewDialog())
            {
                form.Document = PdfViewer.Document.CreatePrintDocument(PdfViewer.DefaultPrintMode);
                form.ShowDialog(this);
            }
        }

        private void FitWidth_Click(object sender, EventArgs e)
        {
            FitPage(PdfViewerZoomMode.FitWidth);
        }

        private void FitPage(PdfViewerZoomMode zoomMode)
        {
            int page = PdfViewer.Renderer.Page;
            PdfViewer.ZoomMode = zoomMode;
            PdfViewer.Renderer.Zoom = 1;
            PdfViewer.Renderer.Page = page;
        }

        private void FitHeight_Click(object sender, EventArgs e)
        {
            FitPage(PdfViewerZoomMode.FitHeight);
        }

        private void FitBest_Click(object sender, EventArgs e)
        {
            FitPage(PdfViewerZoomMode.FitBest);
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;

                if (int.TryParse(this.page.Text, out int page))
                    PdfViewer.Renderer.Page = page - 1;
            }
        }

        private void Zoom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;

                if (float.TryParse(this.zoom.Text, out float zoom))
                    PdfViewer.Renderer.Zoom = zoom;
            }
        }

        private void ToolStripButton4_Click(object sender, EventArgs e)
        {
            PdfViewer.Renderer.ZoomIn();
        }

        private void ToolStripButton3_Click(object sender, EventArgs e)
        {
            PdfViewer.Renderer.ZoomOut();
        }

        private void RotateLeft_Click(object sender, EventArgs e)
        {
            PdfViewer.Renderer.RotateLeft();
        }

        private void RotateRight_Click(object sender, EventArgs e)
        {
            PdfViewer.Renderer.RotateRight();
        }

        private void HideToolbar_Click(object sender, EventArgs e)
        {
            PdfViewer.ShowToolbar = _showToolbar.Checked;
        }

        private void HideBookmarks_Click(object sender, EventArgs e)
        {
            PdfViewer.ShowBookmarks = _showBookmarks.Checked;
        }

        private void DeleteCurrentPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // PdfRenderer does not support changes to the loaded document,
            // so we fake it by reloading the document into the renderer.

            int page = PdfViewer.Renderer.Page;
            var document = PdfViewer.Document;
            PdfViewer.Document = null;
            document.DeletePage(page);
            PdfViewer.Document = document;
            PdfViewer.Renderer.Page = page;
        }

        private void Rotate0ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Rotate(PdfRotation.Rotate0);
        }

        private void Rotate90ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Rotate(PdfRotation.Rotate90);
        }

        private void Rotate180ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Rotate(PdfRotation.Rotate180);
        }

        private void Rotate270ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Rotate(PdfRotation.Rotate270);
        }

        private void Rotate(PdfRotation rotate)
        {
            // PdfRenderer does not support changes to the loaded document,
            // so we fake it by reloading the document into the renderer.

            int page = PdfViewer.Renderer.Page;
            var document = PdfViewer.Document;
            PdfViewer.Document = null;
            document.RotatePage(page, rotate);
            PdfViewer.Document = document;
            PdfViewer.Renderer.Page = page;
        }

        private void ShowRangeOfPagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new PageRangeForm(PdfViewer.Document))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    PdfViewer.Document = form.Document;
                }
            }
		}
			
        private void InformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PdfInformation info = PdfViewer.Document.GetInformation();
            StringBuilder sz = new StringBuilder();
            sz.AppendLine($"Author: {info.Author}");
            sz.AppendLine($"Creator: {info.Creator}");
            sz.AppendLine($"Keywords: {info.Keywords}");
            sz.AppendLine($"Producer: {info.Producer}");
            sz.AppendLine($"Subject: {info.Subject}");
            sz.AppendLine($"Title: {info.Title}");
            sz.AppendLine($"Create Date: {info.CreationDate}");
            sz.AppendLine($"Modified Date: {info.ModificationDate}");

            MessageBox.Show(sz.ToString(), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}	

        private void GetTextFromPage_Click(object sender, EventArgs e)
        {
            int page = PdfViewer.Renderer.Page;
            string text = PdfViewer.Document.GetPdfText(page);
            string caption = string.Format("Page {0} contains {1} character(s):", page + 1, text.Length);
            var cform = new FormContent() { Content = text, Text = text };
            cform.ShowDialog(this);
            //if (text.Length > 128) text = text.Substring(0, 125) + "...\n\n\n\n..." + text.Substring(text.Length - 125);
            //MessageBox.Show(this, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void FindToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (searchForm == null)
            {
                searchForm = new SearchForm(PdfViewer.Renderer);
                searchForm.Disposed += (s, ea) => searchForm = null;
                searchForm.Show(this);
            }

            searchForm.Focus();
        }

        private void PrintMultiplePagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new PrintMultiplePagesForm(PdfViewer))
            {
                form.ShowDialog(this);
            }
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutForm().ShowDialog(this);
        }
    }
}
