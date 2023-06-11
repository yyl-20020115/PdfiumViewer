using System;
using System.Collections.Generic;
using System.IO;

namespace PdfiumViewer
{
    internal static class StreamManager
    {
        private static readonly object syncRoot = new object();
        private static int nextId = 1;
        private static readonly Dictionary<int, Stream> files = new Dictionary<int, Stream>();

        public static int Register(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            lock (syncRoot)
            {
                int id = nextId++;
                files.Add(id, stream);
                return id;
            }
        }

        public static void Unregister(int id)
        {
            lock (syncRoot)
            {
                files.Remove(id);
            }
        }

        public static Stream Get(int id)
        {
            lock (syncRoot)
            {
                files.TryGetValue(id, out Stream stream);
                return stream;
            }
        }
    }
}
