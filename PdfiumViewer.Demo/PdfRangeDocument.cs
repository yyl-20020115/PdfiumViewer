using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using PdfiumViewer;

namespace PdfSearcher
{
    public class PdfRangeDocument : IPdfDocument
    {
        public static PdfRangeDocument FromDocument(IPdfDocument document, int startPage, int endPage)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (endPage < startPage)
                throw new ArgumentException("End page cannot be less than start page");
            if (startPage < 0)
                throw new ArgumentException("Start page cannot be less than zero");
            if (endPage >= document.PageCount)
                throw new ArgumentException("End page cannot be more than the number of pages in the document");

            return new PdfRangeDocument(
                document,
                startPage,
                endPage
            );
        }

        private readonly IPdfDocument document;
        private readonly int startPage;
        private readonly int endPage;
        private PdfBookmarkCollection bookmarks;
        private IList<SizeF> sizes;

        private PdfRangeDocument(IPdfDocument document, int startPage, int endPage)
        {
            this.document = document;
            this.startPage = startPage;
            this.endPage = endPage;
        }

        public int PageCount => endPage - startPage + 1;

        public PdfBookmarkCollection Bookmarks 
            => bookmarks = bookmarks ?? TranslateBookmarks(document.Bookmarks);

        private PdfBookmarkCollection TranslateBookmarks(PdfBookmarkCollection bookmarks)
        {
            var result = new PdfBookmarkCollection();

            TranslateBookmarks(result, bookmarks);

            return result;
        }

        private void TranslateBookmarks(PdfBookmarkCollection result, PdfBookmarkCollection bookmarks)
        {
            foreach (var bookmark in bookmarks)
            {
                if (bookmark.PageIndex >= startPage && bookmark.PageIndex <= endPage)
                {
                    var resultBookmark = new PdfBookmark
                    {
                        PageIndex = bookmark.PageIndex - startPage,
                        Title = bookmark.Title
                    };

                    TranslateBookmarks(resultBookmark.Children, bookmark.Children);

                    result.Add(resultBookmark);
                }
            }
        }

        public IList<SizeF> PageSizes 
            => sizes = sizes ?? TranslateSizes(document.PageSizes);

        private IList<SizeF> TranslateSizes(IList<SizeF> pageSizes)
        {
            var result = new List<SizeF>();

            for (var i = startPage; i <= endPage; i++)
            {
                result.Add(pageSizes[i]);
            }

            return result;
        }

        public void Render(int page, Graphics graphics, float dpiX, float dpiY, Rectangle bounds, bool forPrinting) => document.Render(TranslatePage(page), graphics, dpiX, dpiY, bounds, forPrinting);

        public void Render(int page, Graphics graphics, float dpiX, float dpiY, Rectangle bounds, PdfRenderFlags flags) => document.Render(TranslatePage(page), graphics, dpiX, dpiY, bounds, flags);

        public Image Render(int page, float dpiX, float dpiY, bool forPrinting) => document.Render(TranslatePage(page), dpiX, dpiY, forPrinting);

        public Image Render(int page, float dpiX, float dpiY, PdfRenderFlags flags) => document.Render(TranslatePage(page), dpiX, dpiY, flags);

        public Image Render(int page, int width, int height, float dpiX, float dpiY, bool forPrinting) => document.Render(TranslatePage(page), width, height, dpiX, dpiY, forPrinting);

        public Image Render(int page, int width, int height, float dpiX, float dpiY, PdfRenderFlags flags) => document.Render(TranslatePage(page), width, height, dpiX, dpiY, flags);

        public Image Render(int page, int width, int height, float dpiX, float dpiY, PdfRotation rotate, PdfRenderFlags flags) => document.Render(page, width, height, dpiX, dpiY, rotate, flags);

        public void Save(string path) => document.Save(path);

        public void Save(Stream stream) => document.Save(stream);

        public PdfMatches Search(string text, bool matchCase, bool wholeWord) => TranslateMatches(document.Search(text, matchCase, wholeWord));

        public PdfMatches Search(string text, bool matchCase, bool wholeWord, int page) => TranslateMatches(document.Search(text, matchCase, wholeWord, page));

        public PdfMatches Search(string text, bool matchCase, bool wholeWord, int startPage, int endPage) => TranslateMatches(document.Search(text, matchCase, wholeWord, startPage, endPage));

        private PdfMatches TranslateMatches(PdfMatches search)
        {
            if (search == null)
                return null;

            var matches = new List<PdfMatch>();

            foreach (var match in search.Items)
            {
                matches.Add(new PdfMatch(
                    match.Text,
                    new PdfTextSpan(match.TextSpan.Page + startPage, match.TextSpan.Offset, match.TextSpan.Length),
                    match.Page + startPage
                ));
            }

            return new PdfMatches(
                search.StartPage + startPage,
                search.EndPage + startPage,
                matches
            );
        }

        public PrintDocument CreatePrintDocument() => document.CreatePrintDocument();

        public PrintDocument CreatePrintDocument(PdfPrintMode printMode) => document.CreatePrintDocument(printMode);

        public PrintDocument CreatePrintDocument(PdfPrintSettings settings) => document.CreatePrintDocument(settings);

        public PdfPageLinks GetPageLinks(int pageNumber, Size pageSize) => TranslateLinks(document.GetPageLinks(pageNumber + startPage, pageSize));

        private PdfPageLinks TranslateLinks(PdfPageLinks pageLinks)
        {
            if (pageLinks == null)
                return null;

            var links = new List<PdfPageLink>();

            foreach (var link in pageLinks.Links)
            {
                links.Add(new PdfPageLink(
                    link.Bounds,
                    link.TargetPage + startPage,
                    link.Uri
                ));
            }

            return new PdfPageLinks(links);
        }

        public void DeletePage(int pageNumber) => document.DeletePage(TranslatePage(pageNumber));

        public void RotatePage(int pageNumber, PdfRotation rotation) => document.RotatePage(TranslatePage(pageNumber), rotation);

        public PdfInformation GetInformation() => document.GetInformation();

        public string GetPdfText(int page) => document.GetPdfText(TranslatePage(page));

        public string GetPdfText(PdfTextSpan textSpan) => document.GetPdfText(textSpan);

        public IList<PdfRectangle> GetTextBounds(PdfTextSpan textSpan)
        {
            var result = new List<PdfRectangle>();

            foreach (var rectangle in document.GetTextBounds(textSpan))
            {
                result.Add(new PdfRectangle(
                    rectangle.Page + startPage,
                    rectangle.Bounds
                ));
            }

            return result;
        }

        public PointF PointToPdf(int page, Point point) => document.PointToPdf(TranslatePage(page), point);

        public Point PointFromPdf(int page, PointF point) => document.PointFromPdf(TranslatePage(page), point);

        public RectangleF RectangleToPdf(int page, Rectangle rect) => document.RectangleToPdf(TranslatePage(page), rect);

        public Rectangle RectangleFromPdf(int page, RectangleF rect) => document.RectangleFromPdf(TranslatePage(page), rect);

        private int TranslatePage(int page) => page < 0 || page >= PageCount ? throw new ArgumentException("Page number out of range") : page + startPage;

        public void Dispose() => document.Dispose();
    }
}
