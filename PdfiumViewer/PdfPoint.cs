using System;
using System.Drawing;

#pragma warning disable 1591

namespace PdfiumViewer
{
    public struct PdfPoint : IEquatable<PdfPoint>
    {
        public static readonly PdfPoint Empty = new PdfPoint();

        // _page is offset by 1 so that Empty returns an invalid point.
        private readonly int page;

        public int Page => page - 1;

        public PointF Location { get; }

        public bool IsValid => page != 0;

        public PdfPoint(int page, PointF location)
        {
            this.page = page + 1;
            Location = location;
        }

        public bool Equals(PdfPoint other) => Page == other.Page &&
                Location == other.Location;

        public override bool Equals(object obj) => obj is PdfPoint point && Equals(point);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Page * 397) ^ Location.GetHashCode();
            }
        }

        public static bool operator ==(PdfPoint left, PdfPoint right) => left.Equals(right);

        public static bool operator !=(PdfPoint left, PdfPoint right) => !left.Equals(right);
    }
}
