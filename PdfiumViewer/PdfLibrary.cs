using System;

namespace PdfiumViewer
{
    internal class PdfLibrary : IDisposable
    {
        private static readonly object syncRoot = new object();
        private static PdfLibrary library;

        public static void EnsureLoaded()
        {
            lock (syncRoot)
            {
                if (library == null)
                    library = new PdfLibrary();
            }
        }

        private bool disposed;

        private PdfLibrary()
        {
            NativeMethods.FPDF_AddRef();
        }

        ~PdfLibrary()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                NativeMethods.FPDF_Release();

                disposed = true;
            }
        }
    }
}
