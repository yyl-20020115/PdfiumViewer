using System;
using System.Collections.Generic;
using System.Drawing;

namespace PdfiumViewer
{
    /// <summary>
    /// Helper class for searching through PDF documents.
    /// </summary>
    public class PdfSearchManager
    {
        private bool highlightAllMatches;
        private PdfMatches matches;
        private List<(PdfMatch match,IList<PdfRectangle> rects)> bounds;
        private int firstMatch;
        private int offset;

        /// <summary>
        /// The renderer associated with the search manager.
        /// </summary>
        public PdfRenderer Renderer { get; }

        /// <summary>
        /// Gets or sets whether to match case.
        /// </summary>
        public bool MatchCase { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to match whole words.
        /// </summary>
        public bool MatchWholeWord { get; set; } = false;

        /// <summary>
        /// Gets or sets the color of matched search terms.
        /// </summary>
        public Color MatchColor { get; }

        /// <summary>
        /// Gets or sets the border color of matched search terms.
        /// </summary>
        public Color MatchBorderColor { get; }

        /// <summary>
        /// Gets or sets the border width of matched search terms.
        /// </summary>
        public float MatchBorderWidth { get; }

        /// <summary>
        /// Gets or sets the color of the current match.
        /// </summary>
        public Color CurrentMatchColor { get; }

        /// <summary>
        /// Gets or sets the border color of the current match.
        /// </summary>
        public Color CurrentMatchBorderColor { get; }

        /// <summary>
        /// Gets or sets the border width of the current match.
        /// </summary>
        public float CurrentMatchBorderWidth { get; }

        /// <summary>
        /// Gets or sets whether all matches should be highlighted.
        /// </summary>
        public bool HighlightAllMatches
        {
            get { return highlightAllMatches; }
            set
            {
                if (highlightAllMatches != value)
                {
                    highlightAllMatches = value;
                    UpdateHighlights();
                }
            }
        } 

        /// <summary>
        /// Creates a new instance of the search manager.
        /// </summary>
        /// <param name="renderer">The renderer to create the search manager for.</param>
        public PdfSearchManager(PdfRenderer renderer)
        {
            Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

            HighlightAllMatches = true;
            MatchColor = Color.FromArgb(0x80, Color.Yellow);
            CurrentMatchColor = Color.FromArgb(0x80, SystemColors.Highlight);
        }

        /// <summary>
        /// Searches for the specified text.
        /// </summary>
        /// <param name="text">The text to search.</param>
        /// <returns>Whether any matches were found.</returns>
        public bool Search(string text)
        {
            Renderer.Markers.Clear();

            if (string.IsNullOrEmpty(text))
            {
                matches = null;
                bounds = null;
            }
            else
            {
                matches = Renderer.Document.Search(text, MatchCase, MatchWholeWord);
                bounds = GetAllBounds();
            }

            offset = -1;

            UpdateHighlights();

            return matches != null && matches.Items.Count > 0;
        }

        private List<(PdfMatch,IList<PdfRectangle>)> GetAllBounds()
        {
            var result = new List<(PdfMatch,IList<PdfRectangle>)>();

            foreach (var match in matches.Items)
            {
                result.Add((match,Renderer.Document.GetTextBounds(match.TextSpan)));
            }

            return result;
        }

        /// <summary>
        /// Find the next matched term.
        /// </summary>
        /// <param name="forward">Whether or not to search forward.</param>
        /// <returns>False when the first match was found again; otherwise true.</returns>
        public bool FindNext(bool forward)
        {
            if (matches == null || matches.Items.Count == 0)
                return false;

            if (offset == -1)
            {
                offset = FindFirstFromCurrentPage();
                firstMatch = offset;

                UpdateHighlights();
                ScrollCurrentIntoView();

                return true;
            }

            if (forward)
            {
                offset++;
                if (offset >= matches.Items.Count)
                    offset = 0;
            }
            else
            {
                offset--;
                if (offset < 0)
                    offset = matches.Items.Count - 1;
            }

            UpdateHighlights();
            ScrollCurrentIntoView();

            return offset != firstMatch;
        }

        public void ScrollIntoView(PdfMatch match)
        {
            var rects = Renderer.Document.GetTextBounds(match.TextSpan);
            if (rects.Count > 0)
            {
                Renderer.ScrollIntoView(rects[0]);

                Renderer.Markers.Clear();

                foreach (var pdfBounds in rects)
                {
                    var bounds = new RectangleF(
                        pdfBounds.Bounds.Left - 1,
                        pdfBounds.Bounds.Top + 1,
                        pdfBounds.Bounds.Width + 2,
                        pdfBounds.Bounds.Height - 2
                    );

                    var marker = new PdfMarker(
                        pdfBounds.Page,
                        bounds,
                        CurrentMatchColor,
                        CurrentMatchBorderColor,
                        CurrentMatchBorderWidth
                    );

                    Renderer.Markers.Add(marker);
                }
            }
        }
        private void ScrollCurrentIntoView()
        {
            var (_, rects) = bounds[offset];
            if (rects.Count > 0)
                Renderer.ScrollIntoView(rects[0]);
        }

        private int FindFirstFromCurrentPage()
        {
            for (int i = 0; i < Renderer.Document.PageCount; i++)
            {
                int page = (i + Renderer.Page) % Renderer.Document.PageCount;

                for (int j = 0; j < matches.Items.Count; j++)
                {
                    var match = matches.Items[j];
                    if (match.Page == page)
                        return j;
                }
            }

            return 0;
        }

        /// <summary>
        /// Resets the search manager.
        /// </summary>
        public void Reset()
        {
            Search(null);
        }

        private void UpdateHighlights()
        {
            Renderer.Markers.Clear();

            if (matches == null)
                return;

            if (highlightAllMatches)
            {
                for (int i = 0; i < matches.Items.Count; i++)
                {
                    AddMatch(i, i == offset);
                }
            }
            else if (offset != -1)
            {
                AddMatch(offset, true);
            }
        }

        private void AddMatch(int index, bool current)
        {
            foreach (var pdfBounds in bounds[index].rects)
            {
                var bounds = new RectangleF(
                    pdfBounds.Bounds.Left - 1,
                    pdfBounds.Bounds.Top + 1,
                    pdfBounds.Bounds.Width + 2,
                    pdfBounds.Bounds.Height - 2
                );

                var marker = new PdfMarker(
                    pdfBounds.Page,
                    bounds,
                    current ? CurrentMatchColor : MatchColor,
                    current ? CurrentMatchBorderColor : MatchBorderColor,
                    current ? CurrentMatchBorderWidth : MatchBorderWidth
                );

                Renderer.Markers.Add(marker);
            }
        }
    }
}
