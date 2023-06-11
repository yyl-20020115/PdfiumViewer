using System;
using System.Drawing;

#pragma warning disable 1591

namespace PdfiumViewer
{
    public struct PdfRectangle : IEquatable<PdfRectangle>
    {
        public static readonly PdfRectangle Empty = new PdfRectangle();

        // _page is offset by 1 so that Empty returns an invalid rectangle.
        private readonly int page;

        public int Page => page - 1;

        public RectangleF Bounds { get; }

        public bool IsValid => page != 0;

        public PdfRectangle(int page, RectangleF bounds)
        {
            this.page = page + 1;
            Bounds = bounds;
        }

        public bool Equals(PdfRectangle other) => Page == other.Page &&
                Bounds == other.Bounds;

        public override bool Equals(object obj) => obj is PdfRectangle rectangle &&
                Equals(rectangle);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Page * 397) ^ Bounds.GetHashCode();
            }
        }

        public static bool operator ==(PdfRectangle left, PdfRectangle right) => left.Equals(right);

        public static bool operator !=(PdfRectangle left, PdfRectangle right) => !left.Equals(right);
    }
}
