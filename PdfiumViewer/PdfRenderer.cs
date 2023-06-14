using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PdfiumViewer
{
    /// <summary>
    /// Control to render PDF documents.
    /// </summary>
    public class PdfRenderer : PanningZoomingScrollControl
    {
        private static readonly Padding PageMargin = new Padding(4);

        private int height;
        private int maxWidth;
        private int maxHeight;
        private double documentScaleFactor;
        private bool disposed;
        private double _scaleFactor;
        private ShadeBorder shadeBorder = new ShadeBorder();
        private int _suspendPaintCount;
        private ToolTip _toolTip;
        private PdfViewerZoomMode _zoomMode;
        private bool _pageCacheValid;
        private readonly List<PageCache> _pageCache = new List<PageCache>();
        private int _visiblePageStart;
        private int _visiblePageEnd;
        private PageLink _cachedLink;
        private DragState _dragState;
        private PdfRotation _rotation;
        private List<IPdfMarker>[] displayingMarkers;

        /// <summary>
        /// The associated PDF document.
        /// </summary>
        public IPdfDocument Document { get; private set; }
        public string SelectedText { get; protected set; }
        public override bool IsSelecting
        {
            get => base.IsSelecting;
            set
            {
                base.IsSelecting = value;
                if (!value) this.dragged = false;
            }
        }
        public virtual bool RowSelecting
        {
            get;
            set;
        }
        public Color SelectionColor { get; set; } = Color.FromArgb(0x40, Color.Yellow);
        public Color SelectionBorderColor { get; set; } = Color.FromArgb(0x80, Color.Blue);// Color.FromArgb(0x80, Color.Yellow);

        public bool RightClickCopy { get; protected set; }
        /// <summary>
        /// Gets or sets a value indicating whether the user can give the focus to this control using the TAB key.
        /// </summary>
        /// 
        /// <returns>
        /// true if the user can give the focus to the control using the TAB key; otherwise, false. The default is true.Note:This property will always return true for an instance of the <see cref="T:System.Windows.Forms.Form"/> class.
        /// </returns>
        /// <filterpriority>1</filterpriority><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        [DefaultValue(true)]
        public new bool TabStop
        {
            get { return base.TabStop; }
            set { base.TabStop = value; }
        }

        /// <summary>
        /// Gets or sets the currently focused page.
        /// </summary>
        public int Page
        {
            get
            {
                if (Document == null || !_pageCacheValid)
                    return 0;

                int top = -DisplayRectangle.Top;
                int bottom = top + GetScrollClientArea().Height;

                for (int page = 0; page < Document.PageSizes.Count; page++)
                {
                    var pageCache = _pageCache[page].OuterBounds;
                    if (top - 10 < pageCache.Top)
                    {
                        // If more than 50% of the page is hidden, return the previous page.

                        int hidden = pageCache.Bottom - bottom;
                        if (hidden > 0 && (double)hidden / pageCache.Height > 0.5 && page > 0)
                            return page - 1;

                        return page;
                    }
                }

                return Document.PageCount - 1;
            }
            set
            {
                if (Document == null)
                {
                    SetDisplayRectLocation(new Point(0, 0));
                }
                else
                {
                    int page = Math.Min(Math.Max(value, 0), Document.PageCount - 1);

                    SetDisplayRectLocation(new Point(0, -_pageCache[page].OuterBounds.Top));
                }
            }
        }

        /// <summary>
        /// Get the outer bounds of the page.
        /// </summary>
        /// <param name="page">The page to get the bounds for.</param>
        /// <returns>The bounds of the page.</returns>
        public Rectangle GetOuterBounds(int page)
        {
            if (Document == null || !_pageCacheValid)
                return Rectangle.Empty;

            page = Math.Min(Math.Max(page, 0), Document.PageCount - 1);
            return _pageCache[page].OuterBounds;
        }

        /// <summary>
        /// Gets or sets the way the document should be zoomed initially.
        /// </summary>
        public PdfViewerZoomMode ZoomMode
        {
            get { return _zoomMode; }
            set
            {
                _zoomMode = value;
                PerformLayout();
            }
        }

        /// <summary>
        /// Gets or sets the current rotation of the PDF document.
        /// </summary>
        public PdfRotation Rotation
        {
            get { return _rotation; }
            set
            {
                if (_rotation != value)
                {
                    _rotation = value;
                    ResetFromRotation();
                }
            }
        }

        /// <summary>
        /// Gets a collection with all markers.
        /// </summary>
        public PdfMarkerCollection Markers { get; }
        public readonly List<PdfMarker> LastMarkers = new List<PdfMarker>();

        /// <summary>
        /// Initializes a new instance of the PdfRenderer class.
        /// </summary>
        public PdfRenderer()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);

            TabStop = true;
            RightClickCopy = true;
            _toolTip = new ToolTip();

            Markers = new PdfMarkerCollection();
            Markers.CollectionChanged += Markers_CollectionChanged;
        }

        private void Markers_CollectionChanged(object sender, EventArgs e)
        {
            RedrawMarkers();
        }

        /// <summary>
        /// Converts client coordinates to PDF coordinates.
        /// </summary>
        /// <param name="location">Client coordinates to get the PDF location for.</param>
        /// <returns>The location in a PDF page or a PdfPoint with IsValid false when the coordinates do not match a PDF page.</returns>
        public PdfPoint PointToPdf(Point location)
        {
            if (Document == null)
                return PdfPoint.Empty;

            var offset = GetScrollOffset();
            location.Offset(-offset.Width, -offset.Height);

            for (int page = 0; page < Document.PageSizes.Count; page++)
            {
                var pageCache = _pageCache[page];
                if (pageCache.OuterBounds.Contains(location))
                {
                    if (pageCache.Bounds.Contains(location))
                    {
                        location = new Point(
                            location.X - pageCache.Bounds.X,
                            location.Y - pageCache.Bounds.Y
                        );
                        var translated = TranslatePointToPdf(pageCache.Bounds.Size, Document.PageSizes[page], location);
                        translated = Document.PointToPdf(page, new Point((int)translated.X, (int)translated.Y));

                        return new PdfPoint(page, translated);
                    }

                    break;
                }
            }

            return PdfPoint.Empty;
        }

        /// <summary>
        /// Converts a PDF point to a client point.
        /// </summary>
        /// <param name="point">The PDF point to convert.</param>
        /// <returns>The location of the point in client coordinates.</returns>
        public Point PointFromPdf(PdfPoint point)
        {
            var offset = GetScrollOffset();
            var pageBounds = _pageCache[point.Page].Bounds;

            var translated = Document.PointFromPdf(point.Page, point.Location);
            var location = TranslatePointFromPdf(pageBounds.Size, Document.PageSizes[point.Page], translated);

            return new Point(
                pageBounds.Left + offset.Width + location.X,
                pageBounds.Top + offset.Height + location.Y
            );
        }

        /// <summary>
        /// Converts client coordinates to PDF bounds.
        /// </summary>
        /// <param name="bounds">The client coordinates to convert.</param>
        /// <returns>The PDF bounds.</returns>
        public PdfRectangle BoundsToPdf(Rectangle bounds)
        {
            if (Document == null)
                return PdfRectangle.Empty;

            var offset = GetScrollOffset();
            bounds.Offset(-offset.Width, -offset.Height);

            for (int page = 0; page < Document.PageSizes.Count; page++)
            {
                var pageCache = _pageCache[page];
                if (pageCache.OuterBounds.Contains(bounds.Location))
                {
                    if (pageCache.Bounds.Contains(bounds.Location))
                    {
                        var topLeft = new Point(
                            bounds.Left - pageCache.Bounds.Left,
                            bounds.Top - pageCache.Bounds.Top
                        );
                        var bottomRight = new Point(
                            bounds.Right - pageCache.Bounds.Left,
                            bounds.Bottom - pageCache.Bounds.Top
                        );

                        var translatedTopLeft = TranslatePointToPdf(pageCache.Bounds.Size, Document.PageSizes[page], topLeft);
                        var translatedBottomRight = TranslatePointToPdf(pageCache.Bounds.Size, Document.PageSizes[page], bottomRight);

                        var translated = Document.RectangleToPdf(
                            page,
                            new Rectangle(
                                (int)translatedTopLeft.X,
                                (int)translatedTopLeft.Y,
                                (int)(translatedBottomRight.X - translatedTopLeft.X),
                                (int)(translatedBottomRight.Y - translatedTopLeft.Y)
                            )
                        );

                        return new PdfRectangle(page, translated);
                    }

                    break;
                }
            }

            return PdfRectangle.Empty;
        }
        public List<PdfRectangle> BoundsToPdfList(Rectangle bounds)
        {
            if (Document == null)
                return new List<PdfRectangle>();

            var list = new List<PdfRectangle>();

            var offset = GetScrollOffset();
            bounds.Offset(-offset.Width, -offset.Height);
            var offseted = false;
            for (int page = 0; page < Document.PageSizes.Count; page++)
            {
                var pageCache = _pageCache[page];
                if (pageCache.OuterBounds.Contains(bounds.Location))
                {
                    if (pageCache.Bounds.Contains(bounds.Location))
                    {
                        var topLeft = new Point(
                            bounds.Left - pageCache.Bounds.Left,
                            bounds.Top - pageCache.Bounds.Top
                        );
                        var bottomRight = new Point(
                            bounds.Right - pageCache.Bounds.Left,
                            bounds.Bottom - pageCache.Bounds.Top
                        );

                        var translatedTopLeft = TranslatePointToPdf(pageCache.Bounds.Size, Document.PageSizes[page], topLeft);
                        var translatedBottomRight = TranslatePointToPdf(pageCache.Bounds.Size, Document.PageSizes[page], bottomRight);

                        var translated = Document.RectangleToPdf(
                            page,
                            new Rectangle(
                                (int)translatedTopLeft.X,
                                (int)translatedTopLeft.Y,
                                (int)(translatedBottomRight.X - translatedTopLeft.X),
                                (int)(translatedBottomRight.Y - translatedTopLeft.Y)
                            )
                        );

                        list.Add(new PdfRectangle(page, translated));
                        if (!offseted)
                        {
                            bounds.Y += bounds.Height;
                            offseted = true;
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Converts PDF bounds to client bounds.
        /// </summary>
        /// <param name="bounds">The PDF bounds to convert.</param>
        /// <returns>The bounds of the PDF bounds in client coordinates.</returns>
        //public Rectangle BoundsFromPdf(PdfRectangle bounds)
        //{
        //    return BoundsFromPdf(bounds, true);
        //}

        public Rectangle BoundsFromPdf(PdfRectangle bounds, bool translateOffset = true)
        {
            var offset = translateOffset ? GetScrollOffset() : Size.Empty;
            var pageBounds = _pageCache[bounds.Page].Bounds;
            var pageSize = Document.PageSizes[bounds.Page];

            var translated = Document.RectangleFromPdf(
                bounds.Page,
                bounds.Bounds
            );

            var topLeft = TranslatePointFromPdf(pageBounds.Size, pageSize, new PointF(translated.Left, translated.Top));
            var bottomRight = TranslatePointFromPdf(pageBounds.Size, pageSize, new PointF(translated.Right, translated.Bottom));

            return new Rectangle(
                pageBounds.Left + offset.Width + Math.Min(topLeft.X, bottomRight.X),
                pageBounds.Top + offset.Height + Math.Min(topLeft.Y, bottomRight.Y),
                Math.Abs(bottomRight.X - topLeft.X),
                Math.Abs(bottomRight.Y - topLeft.Y)
            );
        }

        private PointF TranslatePointToPdf(Size size, SizeF pageSize, Point point)
        {
            switch (Rotation)
            {
                case PdfRotation.Rotate90:
                    point = new Point(size.Height - point.Y, point.X);
                    size = new Size(size.Height, size.Width);
                    break;
                case PdfRotation.Rotate180:
                    point = new Point(size.Width - point.X, size.Height - point.Y);
                    break;
                case PdfRotation.Rotate270:
                    point = new Point(point.Y, size.Width - point.X);
                    size = new Size(size.Height, size.Width);
                    break;
            }

            return new PointF(
                ((float)point.X / size.Width) * pageSize.Width,
                ((float)point.Y / size.Height) * pageSize.Height
            );
        }

        private Point TranslatePointFromPdf(Size size, SizeF pageSize, PointF point)
        {
            switch (Rotation)
            {
                case PdfRotation.Rotate90:
                    point = new PointF(pageSize.Height - point.Y, point.X);
                    pageSize = new SizeF(pageSize.Height, pageSize.Width);
                    break;
                case PdfRotation.Rotate180:
                    point = new PointF(pageSize.Width - point.X, pageSize.Height - point.Y);
                    break;
                case PdfRotation.Rotate270:
                    point = new PointF(point.Y, pageSize.Width - point.X);
                    pageSize = new SizeF(pageSize.Height, pageSize.Width);
                    break;
            }

            return new Point(
                (int)((point.X / pageSize.Width) * size.Width),
                (int)((point.Y / pageSize.Height) * size.Height)
            );
        }

        private Size GetScrollOffset()
        {
            var bounds = GetScrollClientArea();
            int maxWidth = (int)(this.maxWidth * _scaleFactor) + ShadeBorder.Size.Horizontal + PageMargin.Horizontal;
            int leftOffset = (HScroll ? DisplayRectangle.X : (bounds.Width - maxWidth) / 2) + maxWidth / 2;
            int topOffset = VScroll ? DisplayRectangle.Y : 0;

            return new Size(leftOffset, topOffset);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Layout"/> event.
        /// </summary>
        /// <param name="levent">A <see cref="T:System.Windows.Forms.LayoutEventArgs"/> that contains the event data. </param>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            UpdateScrollbars();
        }

        /// <summary>
        /// Called when the zoom level changes.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnZoomChanged(EventArgs e)
        {
            base.OnZoomChanged(e);

            UpdateScrollbars();
        }

        /// <summary>
        /// Load a <see cref="IPdfDocument"/> into the control.
        /// </summary>
        /// <param name="document">Document to load.</param>
        public void Load(IPdfDocument document)
        {
            if (document == null)
                throw new ArgumentNullException("document");
            if (document.PageCount == 0)
                throw new ArgumentException("Document does not contain any pages", "document");

            Document = document;

            SetDisplayRectLocation(new Point(0, 0));

            ReloadDocument();
        }

        private void ReloadDocument()
        {
            height = 0;
            maxWidth = 0;
            maxHeight = 0;

            foreach (var size in Document.PageSizes)
            {
                var translated = TranslateSize(size);
                height += (int)translated.Height;
                maxWidth = Math.Max((int)translated.Width, maxWidth);
                maxHeight = Math.Max((int)translated.Height, maxHeight);
            }

            documentScaleFactor = maxHeight != 0 ? (double)maxWidth / maxHeight : 0D;

            displayingMarkers = null;

            UpdateScrollbars();

            Invalidate();
        }

        private void UpdateScrollbars()
        {
            if (Document == null)
                return;

            UpdateScaleFactor(ScrollBars.Both);

            var bounds = GetScrollClientArea(ScrollBars.Both);

            var documentSize = GetDocumentBounds().Size;

            bool horizontalVisible = documentSize.Width > bounds.Width;

            if (!horizontalVisible)
            {
                UpdateScaleFactor(ScrollBars.Vertical);

                documentSize = GetDocumentBounds().Size;
            }

            _suspendPaintCount++;

            try
            {
                SetDisplaySize(documentSize);
            }
            finally
            {
                _suspendPaintCount--;
            }

            RebuildPageCache();
        }

        private void RebuildPageCache()
        {
            if (Document == null || _suspendPaintCount > 0)
                return;

            _pageCacheValid = true;

            int maxWidth = (int)(this.maxWidth * _scaleFactor) + ShadeBorder.Size.Horizontal + PageMargin.Horizontal;
            int leftOffset = -maxWidth / 2;

            int offset = 0;

            for (int page = 0; page < Document.PageSizes.Count; page++)
            {
                var size = TranslateSize(Document.PageSizes[page]);
                int height = (int)(size.Height * _scaleFactor);
                int fullHeight = height + ShadeBorder.Size.Vertical + PageMargin.Vertical;
                int width = (int)(size.Width * _scaleFactor);
                int maxFullWidth = (int)(this.maxWidth * _scaleFactor) + ShadeBorder.Size.Horizontal + PageMargin.Horizontal;
                int fullWidth = width + ShadeBorder.Size.Horizontal + PageMargin.Horizontal;
                int thisLeftOffset = leftOffset + (maxFullWidth - fullWidth) / 2;

                while (_pageCache.Count <= page)
                {
                    _pageCache.Add(new PageCache());
                }

                var pageCache = _pageCache[page];

                if (pageCache.Image != null)
                {
                    pageCache.Image.Dispose();
                    pageCache.Image = null;
                }

                pageCache.Links = null;
                pageCache.Bounds = new Rectangle(
                    thisLeftOffset + ShadeBorder.Size.Left + PageMargin.Left,
                    offset + ShadeBorder.Size.Top + PageMargin.Top,
                    width,
                    height
                );
                pageCache.OuterBounds = new Rectangle(
                    thisLeftOffset,
                    offset,
                    width + ShadeBorder.Size.Horizontal + PageMargin.Horizontal,
                    height + ShadeBorder.Size.Vertical + PageMargin.Vertical
                );

                offset += fullHeight;
            }
        }

        private List<PageLink> GetPageLinks(int page)
        {
            var pageCache = _pageCache[page];
            if (pageCache.Links == null)
            {
                pageCache.Links = new List<PageLink>();
                foreach (var link in Document.GetPageLinks(page, pageCache.Bounds.Size).Links)
                {
                    pageCache.Links.Add(new PageLink(link, BoundsFromPdf(new PdfRectangle(page, link.Bounds), false)));
                }
            }

            return pageCache.Links;
        }

        private Rectangle GetScrollClientArea()
        {
            ScrollBars scrollBarsVisible;

            if (HScroll && VScroll)
                scrollBarsVisible = ScrollBars.Both;
            else if (HScroll)
                scrollBarsVisible = ScrollBars.Horizontal;
            else if (VScroll)
                scrollBarsVisible = ScrollBars.Vertical;
            else
                scrollBarsVisible = ScrollBars.None;

            return GetScrollClientArea(scrollBarsVisible);
        }

        private Rectangle GetScrollClientArea(ScrollBars scrollbars)
        {
            return new Rectangle(
                0,
                0,
                scrollbars == ScrollBars.Vertical || scrollbars == ScrollBars.Both ? Width - SystemInformation.VerticalScrollBarWidth : Width,
                scrollbars == ScrollBars.Horizontal || scrollbars == ScrollBars.Both ? Height - SystemInformation.HorizontalScrollBarHeight : Height
            );
        }

        private void UpdateScaleFactor(ScrollBars scrollBars)
        {
            var bounds = GetScrollClientArea(scrollBars);

            // Scale factor determines what we need to multiply the dimensions
            // of the metafile with to get the size in the control.

            var zoomMode = CalculateZoomModeForFitBest(bounds);

            if (zoomMode == PdfViewerZoomMode.FitHeight)
            {
                int height = bounds.Height - ShadeBorder.Size.Vertical - PageMargin.Vertical;

                _scaleFactor = ((double)height / maxHeight) * Zoom;
            }
            else
            {
                int width = bounds.Width - ShadeBorder.Size.Horizontal - PageMargin.Horizontal;

                _scaleFactor = ((double)width / maxWidth) * Zoom;
            }
        }

        private PdfViewerZoomMode CalculateZoomModeForFitBest(Rectangle bounds)
        {
            if (ZoomMode != PdfViewerZoomMode.FitBest)
            {
                return ZoomMode;
            }

            var controlScaleFactor = (double)bounds.Width / bounds.Height;

            return controlScaleFactor >= documentScaleFactor ? PdfViewerZoomMode.FitHeight : PdfViewerZoomMode.FitWidth;
        }

        public string GetCurrentSelectedText()
        {
            var builder = new StringBuilder();
            foreach(var marker in this.LastMarkers)
            {
                builder.Append(marker.Text);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data. </param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Document == null || _suspendPaintCount > 0 || !_pageCacheValid)
                return;

            EnsureMarkers();

            var offset = GetScrollOffset();
            var bounds = GetScrollClientArea();

            using (var brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(brush, e.ClipRectangle);
            }

            //using (var pen = new Pen(this.SelectionBorderColor))
            //{
            //    var outerBound = this.UnionAllRects(
            //            this.Markers.Select(
            //                m => this.BoundsFromPdf(new PdfRectangle(m.Page, m.Bounds))).ToArray());
            //    e.Graphics.DrawRectangle(pen, outerBound);
            //}
            _visiblePageStart = -1;
            _visiblePageEnd = -1;

            for (int page = 0; page < Document.PageSizes.Count; page++)
            {
                var pageCache = _pageCache[page];
                var rectangle = pageCache.OuterBounds;
                rectangle.Offset(offset.Width, offset.Height);

                if (_visiblePageStart == -1)
                {
                    if (rectangle.Bottom >= 0)
                    {
                        _visiblePageStart = page;
                    }
                    else if (pageCache.Image != null)
                    {
                        pageCache.Image.Dispose();
                        pageCache.Image = null;
                    }
                }

                if (rectangle.Top > bounds.Height)
                {
                    if (_visiblePageEnd == -1)
                        _visiblePageEnd = page - 1;

                    if (pageCache.Image != null)
                    {
                        pageCache.Image.Dispose();
                        pageCache.Image = null;
                    }
                }

                if (e.ClipRectangle.IntersectsWith(rectangle))
                {
                    var pageBounds = pageCache.Bounds;
                    pageBounds.Offset(offset.Width, offset.Height);

                    e.Graphics.FillRectangle(Brushes.White, pageBounds);

                    DrawPageImage(e.Graphics, page, pageBounds);

                    shadeBorder.Draw(e.Graphics, pageBounds);

                    DrawMarkers(e.Graphics, page);
                }
            }

            if (_visiblePageStart == -1)
                _visiblePageStart = 0;
            if (_visiblePageEnd == -1)
                _visiblePageEnd = Document.PageCount - 1;

        }

        private void DrawPageImage(Graphics graphics, int page, Rectangle pageBounds)
        {
            var pageCache = _pageCache[page];

            if (pageCache.Image == null)
                pageCache.Image = Document.Render(page, pageBounds.Width, pageBounds.Height, graphics.DpiX, graphics.DpiY, Rotation, PdfRenderFlags.Annotations);

            graphics.DrawImageUnscaled(pageCache.Image, pageBounds.Location);
        }

        /// <summary>
        /// Gets the document bounds.
        /// </summary>
        /// <returns>The document bounds.</returns>
        protected override Rectangle GetDocumentBounds()
        {
            int height = (int)(this.height * _scaleFactor + (ShadeBorder.Size.Vertical + PageMargin.Vertical) * Document.PageCount);
            int width = (int)(maxWidth * _scaleFactor + ShadeBorder.Size.Horizontal + PageMargin.Horizontal);

            var center = new Point(
                DisplayRectangle.Width / 2,
                DisplayRectangle.Height / 2
            );

            if (
                DisplayRectangle.Width > ClientSize.Width ||
                DisplayRectangle.Height > ClientSize.Height
            )
            {
                center.X += DisplayRectangle.Left;
                center.Y += DisplayRectangle.Top;
            }

            return new Rectangle(
                center.X - width / 2,
                center.Y - height / 2,
                width,
                height
            );
        }

        /// <summary>
        /// Called whent the cursor changes.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnSetCursor(SetCursorEventArgs e)
        {
            _cachedLink = null;

            if (_pageCacheValid)
            {
                var offset = GetScrollOffset();

                var location = new Point(
                    e.Location.X - offset.Width,
                    e.Location.Y - offset.Height
                );

                for (int page = _visiblePageStart; page <= _visiblePageEnd; page++)
                {
                    foreach (var link in GetPageLinks(page))
                    {
                        if (link.Bounds.Contains(location))
                        {
                            _cachedLink = link;
                            e.Cursor = Cursors.Hand;
                            return;
                        }
                    }
                }
            }

            base.OnSetCursor(e);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                this.SelectedText
                    = this.GetCurrentSelectedText();
                Clipboard.Clear();
                if (!string.IsNullOrEmpty(SelectedText))
                    Clipboard.SetText(this.SelectedText);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.dragged = false;
                this.Invalidate();
            }
            base.OnPreviewKeyDown(e);
        }
        protected Point sp = new Point();
        protected Point ep = new Point();
        protected bool dragging = false;
        protected bool dragged = false;
        protected List<PdfRectangle> currentPdfRectanges = new List<PdfRectangle>();
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
        }

        protected Rectangle UnionAllRects(Rectangle[] rects)
        {
            var result = new Rectangle();
            for (var i = 0; i < rects.Length; i++)
            {
                if (i == 0)
                {
                    result = rects[i];
                }
                else
                {
                    result = Rectangle.Union(rects[i], result);
                }
            }
            return result;
        }
        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.MouseDown" /> event.</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data. </param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (this.IsSelecting)
            {
                if (e.Button == MouseButtons.Right)
                {
                    this.SelectedText
                        = this.GetCurrentSelectedText();
                    if (this.RightClickCopy)
                    {
                        Clipboard.Clear();
                        if(!string.IsNullOrEmpty(SelectedText))
                            Clipboard.SetText(this.SelectedText);
                    }
                    dragging = false;
                    dragged = false;
                    this.Markers.Clear();
                    this.Invalidate();
                }
                else if (e.Button == MouseButtons.Left)
                {
                    var ob = this.UnionAllRects(
                            this.Markers.Select(
                                m => this.BoundsFromPdf(new PdfRectangle(m.Page, m.Bounds))).ToArray());

                    if (dragged && ob.Contains(e.Location))
                    {
                        this.SelectedText
                            = this.GetCurrentSelectedText();
                        this.DoDragDrop(this.SelectedText, DragDropEffects.Copy);
                    }
                    else
                    {
                        if (!this.Focused) this.Focus();
                        this.Markers.Clear();
                        sp = e.Location;
                        dragging = true;
                        dragged = false;
                    }
                }
            }
            else
            {
                base.OnMouseDown(e);
                _dragState = null;

                if (_cachedLink != null)
                {
                    _dragState = new DragState
                    {
                        Link = _cachedLink.Link,
                        Location = e.Location
                    };
                }
            }
        }
        protected static bool InvRectContains(RectangleF rect, float y)
        {
            float top = Math.Min(rect.Top, rect.Bottom);
            float bottom = Math.Max(rect.Top, rect.Bottom);
            return y >= top && y <= bottom;
        }
        protected static RectangleF InvRectUnion(RectangleF rect1, RectangleF rect2)
        {
            float top1 = Math.Min(rect1.Top, rect1.Bottom);
            float bottom1 = Math.Max(rect1.Top, rect1.Bottom);
            float left1 = Math.Min(rect1.Left, rect1.Right);
            float right1 = Math.Max(rect1.Left, rect1.Right);

            float top2 = Math.Min(rect2.Top, rect2.Bottom);
            float bottom2 = Math.Max(rect2.Top, rect2.Bottom);
            float left2 = Math.Min(rect2.Left, rect2.Right);
            float right2 = Math.Max(rect2.Left, rect2.Right);

            float top = Math.Min(top1, top2);
            float bottom = Math.Max(bottom1, bottom2);
            float left = Math.Min(left1, left2);
            float right = Math.Max(right1, right2);


            return RectangleF.FromLTRB(right, bottom, left, top);
        }
        protected List<(int page, RectangleF rect, string line)> Compose(List<(int page, List<(int,char,RectangleF rf)> line)> lines, int left, int right)
        {
            var rets = new List<(int, RectangleF, string)>();
            for (int n = 0; n < lines.Count; n++)
            {
                var builder = new StringBuilder();
                RectangleF? ret = null;
                var rects = lines[n];
                for (int i = 0; i < rects.line.Count; i++)
                {
                    var rect = rects.line[i];
                    if (n == 0 && i >= left
                        || n == lines.Count - 1 && i <= right
                        || n >= 1 && n < lines.Count - 1)
                    {
                        if (ret == null) ret = rect.rf;
                        ret = InvRectUnion(ret.Value, rect.rf);
                        builder.Append(rect.Item2);
                    }
                }
                builder.AppendLine();
                builder.AppendLine();
                rets.Add((rects.page, ret ?? new RectangleF(),builder.ToString()));
            }
            return rets;
        }
        protected virtual void BuildSelection()
        {
            var x = Math.Min(sp.X, ep.X);
            var y = Math.Min(sp.Y, ep.Y);
            var w = Math.Abs(ep.X - sp.X);
            var h = Math.Abs(ep.Y - sp.Y);

            var selectionRect = new Rectangle(x, y, w, h);
            var originalSelectionRect = selectionRect;
            var left = -1;
            var right = -1;
            var markers = new List<PdfMarker>();
            this.currentPdfRectanges = BoundsToPdfList(selectionRect);
            if (this.RowSelecting)
            {
                selectionRect = new Rectangle(0, selectionRect.Y, ClientRectangle.Width, selectionRect.Height);
            }

            var linesRects = new List<(int page, List<(int, char c, RectangleF)> lineRects)>();
            foreach (var currentPdfRectangle in this.currentPdfRectanges)
            {
                int page = currentPdfRectangle.Page;

                if (page < 0) continue;
                var col = -1;
                var charRects = new List<(int i,char c, RectangleF r)>();
                var text = Document.GetPdfText(page);
                RectangleF? lastRect = null;
                RectangleF? fullRect = null;
                for (int i = 0; i < text.Length; i++)
                {
                forChar:
                    var pdfCharSpan = new PdfTextSpan(page, i, 1);
                    var pdfCharBounds = Document.GetTextBounds(pdfCharSpan);
                    if (pdfCharBounds.Count == 1)
                    {
                    retry:
                        var pdfCharBound = pdfCharBounds[0];
                        var devCharBound = this.BoundsFromPdf(pdfCharBound, true);
                        if (selectionRect.IntersectsWith(devCharBound))
                        {
                            col++;
                            if (left == -1 && devCharBound.IntersectsWith(originalSelectionRect))
                            {
                                left = col;
                            }
                            if (right == -1 && devCharBound.Contains(new Point(originalSelectionRect.Right, originalSelectionRect.Bottom)))
                            {
                                right = col;
                            }
                            var rect = new RectangleF(
                                pdfCharBound.Bounds.Left - 1,
                                pdfCharBound.Bounds.Top + 1,
                                pdfCharBound.Bounds.Width + 2,
                                pdfCharBound.Bounds.Height - 2);

                            if (lastRect == null)
                            {
                                lastRect = rect;
                            }

                            if (fullRect == null)
                            {
                                fullRect = rect;
                            }

                            float middleY = (rect.Top + rect.Bottom) / 2;

                            if (InvRectContains(fullRect.Value, middleY))
                            {
                                fullRect = InvRectUnion(rect, fullRect.Value);
                                charRects.Add((i, text[i], rect));
                            }
                            else //not following, save last and empty new
                            {
                                col = -1;
                                linesRects.Add((page, charRects));
                                charRects = new List<(int,char,RectangleF)>();

                                if (RowSelecting)
                                {
                                    i = text.LastIndexOf('\n', i) + 1;
                                    lastRect = null;
                                    fullRect = null;
                                    goto forChar;
                                }
                                else
                                {
                                    fullRect = null;
                                    goto retry;
                                }
                            }
                            lastRect = rect;
                        }
                    }
                }
                if (fullRect != null)
                {
                    linesRects.Add((page, charRects.ToList()));
                    charRects = new List<(int,char,RectangleF)>();
                    fullRect = null;
                }
            }
            foreach (var line in Compose(linesRects, left, right))
            {
                markers.Add(new PdfMarker(
                    line.page,
                    line.rect,
                    this.SelectionColor,
                    SelectionBorderColor, 1)
                { Text = line.line });
            }

            //if (markers.Count != this.Markers.Count
            //    || !Enumerable.SequenceEqual(markers, this.Markers))
            {
                this.Markers.Clear();
                this.LastMarkers.Clear();
                foreach (var m in markers)
                {
                    this.Markers.Add(m);
                    this.LastMarkers.Add(m);
                }
                this.RedrawMarkers();
                this.Refresh();
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.IsSelecting && e.Button == MouseButtons.Left)
            {
                this.ep = e.Location;
                this.BuildSelection();
                this.Invalidate();
            }
            else
            {
                base.OnMouseMove(e);
            }
        }
        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.MouseUp" /> event.</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data. </param>
        protected override void OnMouseUp(MouseEventArgs e)
        {

            if (this.IsSelecting && e.Button == MouseButtons.Left)
            {
                this.ep = e.Location;
                this.dragging = false;
                this.dragged = true;
            }
            else
            {
                base.OnMouseUp(e);
                if (_dragState == null)
                    return;

                int dx = Math.Abs(e.Location.X - _dragState.Location.X);
                int dy = Math.Abs(e.Location.Y - _dragState.Location.Y);

                var link = _dragState.Link;
                _dragState = null;

                if (link == null)
                    return;

                if (dx <= SystemInformation.DragSize.Width && dy <= SystemInformation.DragSize.Height)
                {
                    var linkClickEventArgs = new LinkClickEventArgs(link);
                    HandleLinkClick(linkClickEventArgs);
                }
            }
        }

        private void HandleLinkClick(LinkClickEventArgs e)
        {
            OnLinkClick(e);

            if (e.Handled)
                return;

            if (e.Link.TargetPage.HasValue)
                Page = e.Link.TargetPage.Value;

            if (e.Link.Uri != null)
            {
                try
                {
                    Process.Start(e.Link.Uri);
                }
                catch
                {
                    // Some browsers (Firefox) will cause an exception to
                    // be thrown (when it auto-updates).
                }
            }
        }

        /// <summary>
        /// Occurs when a link in the pdf document is clicked.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when a link in the pdf document is clicked.")]
        public event LinkClickEventHandler LinkClick;

        /// <summary>
        /// Called when a link is clicked.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnLinkClick(LinkClickEventArgs e)
        {
            LinkClick?.Invoke(this, e);
        }

        /// <summary>
        /// Rotate the PDF document left.
        /// </summary>
        public void RotateLeft()
        {
            Rotation = (PdfRotation)(((int)Rotation + 3) % 4);
        }

        /// <summary>
        /// Rotate the PDF document right.
        /// </summary>
        public void RotateRight()
        {
            Rotation = (PdfRotation)(((int)Rotation + 1) % 4);
        }

        private void ResetFromRotation()
        {
            var offsetX = (double)DisplayRectangle.Left / DisplayRectangle.Width;
            var offsetY = (double)DisplayRectangle.Top / DisplayRectangle.Height;

            ReloadDocument();

            SetDisplayRectLocation(new Point(
                (int)(DisplayRectangle.Width * offsetX),
                (int)(DisplayRectangle.Height * offsetY)
            ));
        }

        private SizeF TranslateSize(SizeF size)
        {
            switch (Rotation)
            {
                case PdfRotation.Rotate90:
                case PdfRotation.Rotate270:
                    return new SizeF(size.Height, size.Width);

                default:
                    return size;
            }
        }

        /// <summary>
        /// Called when the zoom level changes.
        /// </summary>
        /// <param name="zoom">The new zoom level.</param>
        /// <param name="focus">The location to focus on.</param>
        protected override void SetZoom(double zoom, Point? focus)
        {
            Point location;

            if (focus.HasValue)
            {
                var bounds = GetDocumentBounds();

                location = new Point(
                    focus.Value.X - bounds.X,
                    focus.Value.Y - bounds.Y
                );
            }
            else
            {
                var bounds = _pageCacheValid
                    ? _pageCache[Page].Bounds
                    : GetDocumentBounds();

                location = new Point(
                    bounds.X,
                    bounds.Y
                );
            }

            double oldScale = Zoom;

            base.SetZoom(zoom, null);

            var newLocation = new Point(
                (int)(location.X * (zoom / oldScale)),
                (int)(location.Y * (zoom / oldScale))
            );

            SetDisplayRectLocation(
                new Point(
                    DisplayRectangle.Left - (newLocation.X - location.X),
                    DisplayRectangle.Top - (newLocation.Y - location.Y)
                ),
                false
            );
        }

        private void RedrawMarkers()
        {
            displayingMarkers = null;

            Invalidate();
        }

        private void EnsureMarkers()
        {
            if (displayingMarkers != null)
                return;

            displayingMarkers = new List<IPdfMarker>[_pageCache.Count];

            foreach (var marker in Markers)
            {
                if (marker.Page < 0 || marker.Page >= displayingMarkers.Length)
                    continue;

                if (displayingMarkers[marker.Page] == null)
                    displayingMarkers[marker.Page] = new List<IPdfMarker>();

                displayingMarkers[marker.Page].Add(marker);
            }
        }

        private void DrawMarkers(Graphics graphics, int page)
        {
            var markers = displayingMarkers[page];
            if (markers == null)
                return;

            foreach (var marker in markers)
            {
                marker.Draw(this, graphics);
            }
        }

        /// <summary>
        /// Scroll the PDF bounds into view.
        /// </summary>
        /// <param name="bounds">The PDF bounds to scroll into view.</param>
        public void ScrollIntoView(PdfRectangle bounds)
        {
            ScrollIntoView(BoundsFromPdf(bounds));
        }

        /// <summary>
        /// Scroll the client rectangle into view.
        /// </summary>
        /// <param name="rectangle">The client rectangle to scroll into view.</param>
        public void ScrollIntoView(Rectangle rectangle)
        {
            var clientArea = GetScrollClientArea();

            if (rectangle.Top < 0 || rectangle.Bottom > clientArea.Height)
            {
                var displayRectangle = DisplayRectangle;
                int center = rectangle.Top + rectangle.Height / 2;
                int documentCenter = center - displayRectangle.Y;
                int displayCenter = clientArea.Height / 2;
                int offset = documentCenter - displayCenter;

                SetDisplayRectLocation(new Point(
                    displayRectangle.X,
                    -offset
                ));
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.Control"/> and its child controls and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
        protected override void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                if (shadeBorder != null)
                {
                    shadeBorder.Dispose();
                    shadeBorder = null;
                }

                if (_toolTip != null)
                {
                    _toolTip.Dispose();
                    _toolTip = null;
                }

                foreach (var pageCache in _pageCache)
                {
                    if (pageCache.Image != null)
                    {
                        pageCache.Image.Dispose();
                        pageCache.Image = null;
                    }
                }

                disposed = true;
            }

            base.Dispose(disposing);
        }

        private class PageCache
        {
            public List<PageLink> Links { get; set; }
            public Rectangle Bounds { get; set; }
            public Rectangle OuterBounds { get; set; }
            public Image Image { get; set; }
        }

        private class PageLink
        {
            public PdfPageLink Link { get; }
            public Rectangle Bounds { get; }

            public PageLink(PdfPageLink link, Rectangle bounds)
            {
                Link = link;
                Bounds = bounds;
            }
        }

        private class DragState
        {
            public PdfPageLink Link { get; set; }
            public Point Location { get; set; }
        }
    }
}
