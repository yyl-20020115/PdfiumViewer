using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace PdfiumViewer
{
    /// <summary>
    /// Control to host PDF documents with support for printing.
    /// </summary>
    public partial class PdfViewer : UserControl
    {
        private IPdfDocument document;
        private bool showBookmarks;

        /// <summary>
        /// Gets or sets the PDF document.
        /// </summary>
        [DefaultValue(null)]
        public IPdfDocument Document
        {
            get => document;
            set
            {
                if (document != value)
                {
                    document = value;

                    if (document != null)
                    {
                        renderer.Load(document);
                        UpdateBookmarks();
                    }

                    UpdateEnabled();
                }
            }
        }

        /// <summary>
        /// Get the <see cref="PdfRenderer"/> that renders the PDF document.
        /// </summary>
        public PdfRenderer Renderer
        {
            get { return renderer; }
        }
        public bool IsSelecting
        {
            get=>this.renderer.IsSelecting;
            set
            {
                this.renderer.IsSelecting = value;
                if (value) this.Focus();
            }
        }
        public bool RowSelecting
        {
            get => this.renderer.RowSelecting;
            set
            {
                this.renderer.RowSelecting = value;
            }
        }
        /// <summary>
        /// Gets or sets the default document name used when saving the document.
        /// </summary>
        [DefaultValue(null)]
        public string DefaultDocumentName { get; set; }

        /// <summary>
        /// Gets or sets the default print mode.
        /// </summary>
        [DefaultValue(PdfPrintMode.CutMargin)]
        public PdfPrintMode DefaultPrintMode { get; set; }

        /// <summary>
        /// Gets or sets the way the document should be zoomed initially.
        /// </summary>
        [DefaultValue(PdfViewerZoomMode.FitHeight)]
        public PdfViewerZoomMode ZoomMode
        {
            get { return renderer.ZoomMode; }
            set { renderer.ZoomMode = value; }
        }

        /// <summary>
        /// Gets or sets whether the toolbar should be shown.
        /// </summary>
        [DefaultValue(true)]
        public bool ShowToolbar
        {
            get { return _toolStrip.Visible; }
            set { _toolStrip.Visible = value; }
        }

        /// <summary>
        /// Gets or sets whether the bookmarks panel should be shown.
        /// </summary>
        [DefaultValue(true)]
        public bool ShowBookmarks
        {
            get { return showBookmarks; }
            set
            {
                showBookmarks = value;
                UpdateBookmarks();
            }
        }

        /// <summary>
        /// Gets or sets the pre-selected printer to be used when the print
        /// dialog shows up.
        /// </summary>
        [DefaultValue(null)]
        public string DefaultPrinter { get; set; }

        /// <summary>
        /// Occurs when a link in the pdf document is clicked.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when a link in the pdf document is clicked.")]
        public event LinkClickEventHandler LinkClick;

        /// <summary>
        /// Called when a link is clicked.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnLinkClick(LinkClickEventArgs e)
        {
            LinkClick?.Invoke(this, e);
        }

        private void UpdateBookmarks()
        {
            bool visible = showBookmarks && document != null && document.Bookmarks.Count > 0;

            container.Panel1Collapsed = !visible;

            if (visible)
            {
                container.Panel1Collapsed = false;

                bookmarks.Nodes.Clear();
                foreach (var bookmark in document.Bookmarks)
                    bookmarks.Nodes.Add(GetBookmarkNode(bookmark));
            }
        }
        public bool ToolStripVisible
        {
            get => this._toolStrip.Visible;
            set => this._toolStrip.Visible = false;
        }
        /// <summary>
        /// Initializes a new instance of the PdfViewer class.
        /// </summary>
        public PdfViewer()
        {
            DefaultPrintMode = PdfPrintMode.CutMargin;

            InitializeComponent();

            ShowToolbar = true;
            ShowBookmarks = true;
            this.IsSelecting = true;
            this.ToolStripVisible = false;
            this.RowSelecting = true;
            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            _toolStrip.Enabled = document != null;
        }

        private void ZoomInButton_Click(object sender, EventArgs e)
        {
            renderer.ZoomIn();
        }

        private void ZoomOutButton_Click(object sender, EventArgs e)
        {
            renderer.ZoomOut();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            using (var form = new SaveFileDialog())
            {
                form.DefaultExt = ".pdf";
                form.Filter = Properties.Resources.SaveAsFilter;
                form.RestoreDirectory = true;
                form.Title = Properties.Resources.SaveAsTitle;
                form.FileName = DefaultDocumentName;

                if (form.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    try
                    {
                        document.Save(form.FileName);
                    }
                    catch
                    {
                        MessageBox.Show(
                            FindForm(),
                            Properties.Resources.SaveAsFailedText,
                            Properties.Resources.SaveAsFailedTitle,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
            }
        }

        private void PrintButton_Click(object sender, EventArgs e)
        {
            using (var form = new PrintDialog())
            using (var document = this.document.CreatePrintDocument(DefaultPrintMode))
            {
                form.AllowSomePages = true;
                form.Document = document;
                form.UseEXDialog = true;
                form.Document.PrinterSettings.FromPage = 1;
                form.Document.PrinterSettings.ToPage = this.document.PageCount;
                if (DefaultPrinter != null)
                    form.Document.PrinterSettings.PrinterName = DefaultPrinter;

                if (form.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    try
                    {
                        if (form.Document.PrinterSettings.FromPage <= this.document.PageCount)
                            form.Document.Print();
                    }
                    catch
                    {
                        // Ignore exceptions; the printer dialog should take care of this.
                    }
                }
            }
        }

        private TreeNode GetBookmarkNode(PdfBookmark bookmark)
        {
            var node = new TreeNode(bookmark.Title)
            {
                Tag = bookmark
            };
            if (bookmark.Children != null)
            {
                foreach (var child in bookmark.Children)
                    node.Nodes.Add(GetBookmarkNode(child));
            }
            return node;
        }

        private void Bookmarks_AfterSelect(object sender, TreeViewEventArgs e)
        {
            renderer.Page = ((PdfBookmark)e.Node.Tag).PageIndex;
        }

        private void Renderer_LinkClick(object sender, LinkClickEventArgs e)
        {
            OnLinkClick(e);
        }
    }
}
