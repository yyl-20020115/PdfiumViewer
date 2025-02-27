﻿using System;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Media;

namespace PdfiumViewer.WPFDemo
{
    internal class BitmapHelper
    {

        public static BitmapSource ToBitmapSource(Image image) => ToBitmapSource(image as Bitmap);

        /// <summary>
        /// Convert an IImage to a WPF BitmapSource. The result can be used in the Set Property of Image.Source
        /// </summary>
        /// <param name="bitmap">The Source Bitmap</param>
        /// <returns>The equivalent BitmapSource</returns>
        public static BitmapSource ToBitmapSource(Bitmap bitmap)
        {
            if (bitmap == null) return null;

            using (var source = (Bitmap)bitmap.Clone())
            {
                var ptr = source.GetHbitmap(); //obtain the Hbitmap

                var bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    sourceRect: System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                NativeMethods.DeleteObject(ptr); //release the HBitmap
                bs.Freeze();
                return bs;
            }
        }

        public static BitmapSource ToBitmapSource(byte[] bytes, int width, int height, int dpiX, int dpiY)
        {
            var result = BitmapSource.Create(
                            width,
                            height,
                            dpiX,
                            dpiY,
                            PixelFormats.Bgra32,
                            null /* palette */,
                            bytes,
                            width * 4 /* stride */);
            result.Freeze();

            return result;
        }
    }
}
