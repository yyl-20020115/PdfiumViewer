using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PdfiumViewer.Demo
{
    public partial class MainForm : Form
    {
        private string PdfDatabasePath;

        private SearchForm searchForm;

        private PdfViewer PdfViewer
            => this.tabControlBooks.SelectedTab!=null && this.tabControlBooks.SelectedTab.Controls.Count > 0
                ? this.tabControlBooks.SelectedTab.Controls?[0] as PdfViewer
                : null;

        public class PdfRecord
        {
            public string Name;
            public string PdfPath;
            public PdfMatches PdfMatches;
            public PdfSearchManager PdfSearchManager;
        }
        private void ToolStripButtonTextSearch_Click(object sender, EventArgs e)
        {
            this.treeViewBooks.Nodes.Clear();
            this.tabControlBooks.TabPages.Clear();

            var text = this.toolStripTextBoxTextSearch.Text;
            text = (text ?? "").Trim();
            if (text.Length == 0) return;

            var found = FindPdfs(this.PdfDatabasePath, text);
            foreach(var p in found.OrderBy(p=>p.Key))
            {
                var file = p.Key;
                var name = Path.GetFileNameWithoutExtension(file);

                
                var record = new PdfRecord() { Name=name,  PdfPath = file ,PdfMatches = p.Value};
                var treeNode = new TreeNode(name);

                foreach(var match in p.Value.Items)
                {
                    var matchNode = new TreeNode(
                        $"{match.Text}: 页码={match.Page}, 开始位置={match.TextSpan.Offset}, 长度={match.TextSpan.Length}")
                    {
                        Tag = match
                    };
                    treeNode.Nodes.Add(matchNode);
                }

                this.treeViewBooks.Nodes.Add(treeNode);

                var tabPage = new TabPage(name) { Tag = record, ToolTipText = file };
                treeNode.Tag = tabPage;
                var pdfViwer = new PdfViewer
                {
                    Dock = DockStyle.Fill,
                    Tag = record
                };
                InitPdfViewer(pdfViwer);
                pdfViwer.Document = OpenDocument(record.PdfPath);
                tabPage.Controls.Add(pdfViwer);
                this.tabControlBooks.TabPages.Add(tabPage);

                record.PdfSearchManager = new PdfSearchManager(pdfViwer.Renderer);
            }
        }

        public Dictionary<string, PdfMatches> FindPdfs(string directory, string text)
            => FindPdfs(Directory.GetFiles(directory, "*.pdf", SearchOption.AllDirectories), text);


        public Dictionary<string,PdfMatches> FindPdfs(string[] files, string text)
        {
            var found = new ConcurrentBag<(string,PdfMatches)>();    
            files.AsParallel().ForAll(f => {
                using (var doc = PdfDocument.Load(f))
                {
                    var matches = doc.Search(text, false, false);
                    if (matches.Items.Any()) 
                        found.Add((f,matches));
                }

            });
            var ret = new Dictionary<string, PdfMatches>();
            foreach(var (file,matches) in found)
            {
                ret[file] = matches;
            }
            return ret;
        }


        public void InitPdfViewer(PdfViewer pdfViewer)
        {

            renderToBitmapsToolStripMenuItem.Enabled = false;

            pdfViewer.Renderer.DisplayRectangleChanged += Renderer_DisplayRectangleChanged;
            pdfViewer.Renderer.ZoomChanged += Renderer_ZoomChanged;

            pdfViewer.Renderer.MouseMove += Renderer_MouseMove;
            pdfViewer.Renderer.MouseLeave += Renderer_MouseLeave;
            ShowPdfLocation(PdfPoint.Empty);

            cutMarginsWhenPrintingToolStripMenuItem.PerformClick();

            zoom.Text = pdfViewer.Renderer.Zoom.ToString();

            Disposed += (s, e) => pdfViewer?.Document?.Dispose();
            _showBookmarks.Checked = pdfViewer.ShowBookmarks;
            _showToolbar.Checked = pdfViewer.ShowToolbar;

        }
        public MainForm(string PdfDatabasePath)
        {
            this.PdfDatabasePath = PdfDatabasePath;
            InitializeComponent();
        }

        private void Renderer_MouseLeave(object sender, EventArgs e)
        {
            ShowPdfLocation(PdfPoint.Empty);
        }

        private void Renderer_MouseMove(object sender, MouseEventArgs e)
        {
            if (PdfViewer == null) return;
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
            if (PdfViewer == null) return;
            zoom.Text = PdfViewer.Renderer.Zoom.ToString();
        }

        void Renderer_DisplayRectangleChanged(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;
            page.Text = (PdfViewer.Renderer.Page + 1).ToString();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {

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

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //OpenFile();
        }

        private void RenderToBitmapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;
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
            if (PdfViewer == null) return;
            PdfViewer.Renderer.Page--;
        }

        private void ToolStripButton2_Click_PP(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;
            PdfViewer.Renderer.Page++;
        }

        private void CutMarginsWhenPrintingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;
            cutMarginsWhenPrintingToolStripMenuItem.Checked = true;
            shrinkToMarginsWhenPrintingToolStripMenuItem.Checked = false;
            PdfViewer.DefaultPrintMode = PdfPrintMode.CutMargin;
        }

        private void ShrinkToMarginsWhenPrintingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;
            shrinkToMarginsWhenPrintingToolStripMenuItem.Checked = true;
            cutMarginsWhenPrintingToolStripMenuItem.Checked = false;

            PdfViewer.DefaultPrintMode = PdfPrintMode.ShrinkToMargin;
        }

        private void PrintPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;
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
            if (PdfViewer == null) return;

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
            if (PdfViewer == null) return;

            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;

                if (int.TryParse(this.page.Text, out int page))
                    PdfViewer.Renderer.Page = page - 1;
            }
        }

        private void Zoom_KeyDown(object sender, KeyEventArgs e)
        {
            if (PdfViewer == null) return;

            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;

                if (float.TryParse(this.zoom.Text, out float zoom))
                    PdfViewer.Renderer.Zoom = zoom;
            }
        }

        private void ToolStripButton4_Click(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;

            PdfViewer.Renderer.ZoomIn();
        }

        private void ToolStripButton3_Click(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;

            PdfViewer.Renderer.ZoomOut();
        }

        private void RotateLeft_Click(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;

            PdfViewer.Renderer.RotateLeft();
        }

        private void RotateRight_Click(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;

            PdfViewer.Renderer.RotateRight();
        }

        private void HideToolbar_Click(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;

            PdfViewer.ShowToolbar = _showToolbar.Checked;
        }

        private void HideBookmarks_Click(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;

            PdfViewer.ShowBookmarks = _showBookmarks.Checked;
        }

        private void DeleteCurrentPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // PdfRenderer does not support changes to the loaded document,
            // so we fake it by reloading the document into the renderer.
            if (PdfViewer == null) return;

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
            if (PdfViewer == null) return;

            int page = PdfViewer.Renderer.Page;
            var document = PdfViewer.Document;
            PdfViewer.Document = null;
            document.RotatePage(page, rotate);
            PdfViewer.Document = document;
            PdfViewer.Renderer.Page = page;
        }

        private void ShowRangeOfPagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;

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
            if (PdfViewer == null) return;

            var info = PdfViewer.Document.GetInformation();
            var builder = new StringBuilder();
            builder.AppendLine($"作者: {info.Author}");
            builder.AppendLine($"创建者: {info.Creator}");
            builder.AppendLine($"关键字: {info.Keywords}");
            builder.AppendLine($"出品人: {info.Producer}");
            builder.AppendLine($"主题: {info.Subject}");
            builder.AppendLine($"标题: {info.Title}");
            builder.AppendLine($"创建日期: {info.CreationDate}");
            builder.AppendLine($"修改日期: {info.ModificationDate}");

            MessageBox.Show(builder.ToString(), "相关信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}	

        private void GetTextFromPage_Click(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;
            var page = PdfViewer.Renderer.Page;
            var text = PdfViewer.Document.GetPdfText(page);
            var caption = string.Format("页面 {0} 包含 {1} 字符:", page + 1, text.Length);
            var cform = new FormContent() { Content = text, Text = text };
            cform.ShowDialog(this);
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

        private void TreeViewBooks_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if(e.Node.Tag is TabPage page)
            {
                this.tabControlBooks.SelectedTab = page;
            }else if(e.Node.Tag is PdfMatch match)
            {
                if (this.PdfViewer == null)
                    return;
                var parent = e.Node.Parent;
                if(parent.Tag is TabPage cp && cp.Tag is PdfRecord record)
                {
                    this.tabControlBooks.SelectedTab = cp;
                    record.PdfSearchManager.ScrollIntoView(match);
                }

            }
        }

        private void ToolStripTextBoxTextSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                ToolStripButtonTextSearch_Click(sender, e);
            }
        }
    }
}
