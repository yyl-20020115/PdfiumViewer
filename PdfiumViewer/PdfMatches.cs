﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#pragma warning disable 1591

namespace PdfiumViewer
{
    public class PdfMatches
    {
        public int StartPage { get; private set; }

        public int EndPage { get; private set; }

        public IList<PdfMatch> Items { get; private set; }

        public PdfMatches(int startPage, int endPage, IList<PdfMatch> matches)
        {
            Items = new ReadOnlyCollection<PdfMatch>(
                matches ?? throw new ArgumentNullException(nameof(matches)));
            StartPage = startPage;
            EndPage = endPage;
        }
    }
}
