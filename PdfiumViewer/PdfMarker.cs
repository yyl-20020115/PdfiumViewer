using System;
using System.Drawing;

#pragma warning disable 1591

namespace PdfiumViewer
{
    public class PdfMarker : IPdfMarker
    {
        public int Page { get; }
        public int CharIndex { get; }
        public RectangleF Bounds { get; set; }
        public Color Color { get; }
        public Color BorderColor { get; }
        public float BorderWidth { get; }

        public PdfMarker(int page, RectangleF bounds, Color color, int charIndex = -1)
            : this(page, bounds, color, Color.Transparent, 0, charIndex)
        {
        }

        public PdfMarker(int page, RectangleF bounds, Color color, Color borderColor, float borderWidth, int charIndex = -1)
        {
            Page = page;
            Bounds = bounds;
            Color = color;
            BorderColor = borderColor;
            BorderWidth = borderWidth;
            CharIndex = charIndex;
        }

        public void Draw(PdfRenderer renderer, Graphics graphics)
        {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));

            var bounds = renderer.BoundsFromPdf(
                new PdfRectangle(Page, Bounds));

            using (var brush = new SolidBrush(Color))
            {
                graphics.FillRectangle(brush, bounds);
            }

            if (BorderWidth > 0)
            {
                using (var pen = new Pen(BorderColor, BorderWidth))
                {
                    graphics.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                }
            }
        }
        public override bool Equals(object o)
            => o is IPdfMarker marker 
            ? this.Page == marker.Page && this.CharIndex == marker.CharIndex 
            : base.Equals(o);
        public override int GetHashCode()
            => (this.Page << 16) + this.CharIndex;
    }
}
