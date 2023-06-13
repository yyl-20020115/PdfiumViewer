using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PdfiumViewer;

namespace PdfSearcher
{
    public partial class MainForm : Form
    {
        public class PdfRecord
        {
            public string Name;
            public string PdfPath;
            public PdfMatches PdfMatches;
            public PdfSearchManager PdfSearchManager;
        }
        protected string pdfpath ="";
        private TreeNode WorkingNodes;
        private TreeNode FoundNodes;
        private TreeNode AllNodes;

        public string PdfDatabasePath
        {
            get => this.pdfpath;
            set => this.watcher.Path = this.pdfpath = value;
        }

        protected SearchForm searchForm;

        protected TabPage CurrentTabPage
        {
            get => this.tabControlBooks.SelectedTab;
            set => this.tabControlBooks.SelectedTab = value;
        }
        protected PdfViewer PdfViewer
            => this.CurrentTabPage != null 
            && this.CurrentTabPage.Controls.Count > 0
                ? this.CurrentTabPage.Controls[0] as PdfViewer
                : null;


        protected FileSystemWatcher watcher = new FileSystemWatcher();

        protected ContextMenu deleteMenu = new ContextMenu();
        protected ContextMenu appendMenu = new ContextMenu();
        private void ToolStripButtonTextSearch_Click(object sender, EventArgs e)
        {
            var text = this.toolStripTextBoxTextSearch.Text;
            text = (text ?? string.Empty).Trim();
            if (text.Length == 0)
            {
                return;
            }
            var adding = this.WorkingNodes.Nodes.Count == 0;
            //this.WorkingNodes.Nodes.Clear();
            this.FoundNodes.Nodes.Clear();
            this.tabControlBooks.TabPages.Clear();

            if (!Directory.Exists(PdfDatabasePath)) return;

            if(this.toolStripButtonInWorking.Checked)
            {
                
                var found = FindPdfs(this.GetFiles(this.WorkingNodes), text);
                foreach (var p in found.OrderBy(p => p.Key))
                {
                    var file = p.Key;
                    var name = Path.GetFileNameWithoutExtension(file);

                    var record = new PdfRecord() { Name = name, PdfPath = file, PdfMatches = p.Value };
                    var treeNode = new TreeNode(name);

                    foreach (var match in p.Value.Items)
                    {
                        var matchNode = new TreeNode(
                            $"{match.Text}: 页码={match.Page}, 开始位置={match.TextSpan.Offset}, 长度={match.TextSpan.Length}")
                        {
                            Tag = match
                        };
                        treeNode.Nodes.Add(matchNode);
                    }
                    treeNode.ContextMenu = this.appendMenu;
                    this.FoundNodes.Nodes.Add(treeNode);

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
                this.FoundNodes.Expand();
            }
            else
            {
                var found = FindPdfs(this.PdfDatabasePath, text);

                foreach (var p in found.OrderBy(p => p.Key))
                {
                    var file = p.Key;
                    var name = Path.GetFileNameWithoutExtension(file);

                    var record = new PdfRecord() { Name = name, PdfPath = file, PdfMatches = p.Value };
                    var treeNode = new TreeNode(name);

                    foreach (var match in p.Value.Items)
                    {
                        var matchNode = new TreeNode(
                            $"{match.Text}: 页码={match.Page}, 开始位置={match.TextSpan.Offset}, 长度={match.TextSpan.Length}")
                        {
                            Tag = match
                        };
                        treeNode.Nodes.Add(matchNode);
                    }
                    treeNode.ContextMenu = this.appendMenu;
                    this.FoundNodes.Nodes.Add(treeNode);

                    var tabPage = new TabPage(name) { Tag = record, ToolTipText = file };
                    treeNode.Tag = tabPage;
                    if (adding)
                    {
                        var clone = treeNode.Clone() as TreeNode;
                        this.WorkingNodes.Nodes.Add(clone);
                        clone.ContextMenu = this.deleteMenu;
                    }
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
                this.FoundNodes.Expand();
                if (adding)
                {
                    this.WorkingNodes.Expand();
                    this.toolStripButtonInWorking.Checked = true;
                }
            }

        }
        public string[] GetFiles(TreeNode workingNode)
        {
            var files = new List<string>();
            for (int i = 0; i < workingNode.Nodes.Count; i++)
            {
                var fn = workingNode.Nodes[i];
                if (fn.Tag is TabPage page && page.Tag is PdfRecord record)
                {
                    files.Add(record.PdfPath);
                }
            }
            return files.ToArray();
        }
        public Dictionary<string, PdfMatches> FindPdfs(string directory, string text, bool matchCase =false, bool wholeWord = false)
            => FindPdfs(Directory.GetFiles(directory, "*.pdf", SearchOption.AllDirectories), text,matchCase,wholeWord);

        public Dictionary<string, PdfMatches> FindPdfs(string[] files, string text, bool matchCase =false, bool wholeWord = false)
        {
            var found = new ConcurrentBag<(string, PdfMatches)>();
            files.AsParallel().ForAll(f =>
            {
                using (var doc = PdfDocument.Load(f))
                {
                    var matches = doc.Search(text, matchCase, wholeWord);
                    if (matches.Items.Any())
                        found.Add((f, matches));
                }

            });
            var ret = new Dictionary<string, PdfMatches>();
            foreach (var (file, matches) in found)
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

            this.Disposed += (s, e) => pdfViewer?.Document?.Dispose();
            showBookmarks.Checked = pdfViewer.ShowBookmarks;
            showToolbar.Checked = pdfViewer.ShowToolbar;

        }
        public MainForm()
        {
            InitializeComponent();

            this.WorkingNodes = new TreeNode("关注中的PDF文档");
            this.FoundNodes = new TreeNode("已找到的PDF文档");
            this.AllNodes = new TreeNode("全部PDF文档");

            this.treeViewBooks.Nodes.Add(FoundNodes);
            this.treeViewBooks.Nodes.Add(WorkingNodes);
            this.treeViewBooks.Nodes.Add(AllNodes);

        }

        private void Watcher_Operations(object sender, FileSystemEventArgs e)
        {
            this.Invoke(new Action(() =>{ this.UpdateFileList(); }));
        }

        private void UpdateFileList()
        {
            this.AllNodes.Nodes.Clear();
            var files = Directory.GetFiles(this.PdfDatabasePath, "*.pdf", SearchOption.AllDirectories);
            foreach (var file in files.OrderBy(f=>f))
            {
                var fileNode = new TreeNode(
                    Path.GetFileNameWithoutExtension(file));
                this.AllNodes.Nodes.Add(fileNode);
            }
            this.AllNodes.ExpandAll();
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
                pageToolStripLabel.Text = null;
                coordinatesToolStripLabel.Text = null;
            }
            else
            {
                pageToolStripLabel.Text = (point.Page + 1).ToString();
                coordinatesToolStripLabel.Text = point.Location.X + "," + point.Location.Y;
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
            this.PdfDatabasePath = Program.GlobalConfigure.PDFDatabasePath;
            if (string.IsNullOrEmpty(this.PdfDatabasePath)||!Directory.Exists(this.PdfDatabasePath))
                SettingsToolStripMenuItem_Click(sender, e);

            var appendMenuItem = this.appendMenu.MenuItems.Add("加入(&A)");

            var deleteMenuItem = this.deleteMenu.MenuItems.Add("删除(&D)");

            appendMenuItem.Click += AppendMenuItem_Click;
            deleteMenuItem.Click += DeleteMenuItem_Click;
            this.treeViewBooks.MouseDown += TreeViewBooks_MouseDown;
            this.watcher.Filter = "*.pdf";
            this.watcher.IncludeSubdirectories = true;
            this.watcher.EnableRaisingEvents = true;
            this.watcher.Created += Watcher_Operations;
            this.watcher.Deleted += Watcher_Operations;
            this.watcher.Changed += Watcher_Operations;
            this.watcher.Renamed += Watcher_Operations;

            this.UpdateFileList();
        }

        private void TreeViewBooks_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)//判断你点的是不是右键
            {
                var ClickPoint = new Point(e.X, e.Y);
                var CurrentNode = this.treeViewBooks.GetNodeAt(ClickPoint);
                if (CurrentNode != null)//判断你点的是不是一个节点
                {
                    this.treeViewBooks.SelectedNode = CurrentNode;//选中这个节点
                }
            }
        }
        private bool HasAny(TreeNode node,string text)
        {
            for(int i = 0; i < node.Nodes.Count; i++)
            {
                if (node.Nodes[i].Text == text) return true;
            }
            return false;
        }
        private void AppendMenuItem_Click(object sender, EventArgs e)
        {
            var node = this.treeViewBooks.SelectedNode;
            if (node != null)
            {
                if (!this.HasAny(this.WorkingNodes,node.Text))
                {
                    node = node.Clone() as TreeNode;
                    node.ContextMenu = deleteMenu;
                    this.WorkingNodes.Nodes.Add(node);
                }
            }
        }

        private void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            this.treeViewBooks.SelectedNode?.Remove();
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
            
        }

        private void RenderToBitmapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;
            int dpiX = 0;
            int dpiY = 0;

            using (var form = new ExportBitmapsForm())
            {
                if (form.ShowDialog() != DialogResult.OK)
                    return;

                dpiX = form.DpiX;
                dpiY = form.DpiY;
            }

            var path = this.PdfDatabasePath;

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

            PdfViewer.ShowToolbar = showToolbar.Checked;
        }

        private void HideBookmarks_Click(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;

            PdfViewer.ShowBookmarks = showBookmarks.Checked;
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
            var caption = string.Format("页码 {0}: 包含 {1} 个字符", page + 1, text.Length);
            var cform = new FormContent() { Content = text, Text = caption };
            cform.ShowDialog(this);
        }

        private void FindToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PdfViewer == null) return;

            if (this.searchForm == null)
            {
                this.searchForm = new SearchForm(PdfViewer.Renderer);
                this.searchForm.Disposed += (s, ea) => searchForm = null;
                this.searchForm.Show(this);
            }

            this.searchForm.Focus();
        }

        private void PrintMultiplePagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(PdfViewer== null) return;
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
            if (e.Node.Tag is TabPage page)
            {
                this.CurrentTabPage = page;
            }
            else if (e.Node.Tag is PdfMatch match)
            {
                if (this.PdfViewer == null)
                    return;
                var parent = e.Node.Parent;
                if (parent.Tag is TabPage cp && cp.Tag is PdfRecord record)
                {
                    this.CurrentTabPage = cp;
                    record.PdfSearchManager.ScrollIntoView(match);
                }

            }
        }

        private void ToolStripTextBoxTextSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                ToolStripButtonTextSearch_Click(sender, e);
            }
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "请选择PDF文件所在的文件夹";

                var result = dialog.ShowDialog(this);

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    Program.GlobalConfigure.PDFDatabasePath = this.PdfDatabasePath = dialog.SelectedPath;
                    Program.SaveGlobalConfigure();
                }
            }
        }

        private void NetworkPDFDatabasePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var input = new NetworkPDFDataPathInputForm())
            {
                input.Path = this.PdfDatabasePath;
                var dr = input.ShowDialog(this);
                var path = input.Path;
                if(dr == DialogResult.OK)
                {
                    if (string.IsNullOrEmpty(path) || !path.StartsWith("\\\\"))
                        MessageBox.Show("网络路径应当以\\\\开头", "错误");
                    
                    Program.GlobalConfigure.PDFDatabasePath = this.PdfDatabasePath = path;
                    Program.SaveGlobalConfigure();
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void ToolStripButtonInWorking_Click(object sender, EventArgs e)
        {
            this.toolStripButtonInWorking.Checked =!this.toolStripButtonInWorking.Checked;

        }
    }
}
