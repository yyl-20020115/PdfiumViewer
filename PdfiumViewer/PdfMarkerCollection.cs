using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#pragma warning disable 1591

namespace PdfiumViewer
{
    public class PdfMarkerCollection : Collection<IPdfMarker>
    {
        public event EventHandler CollectionChanged;

        public PdfMarkerCollection AddRange(IEnumerable<IPdfMarker> markers)
        {
            foreach(var marker in markers)
            {
                this.Add(marker);
            }
            return this;
        }
        protected override void ClearItems()
        {
            base.ClearItems();

            OnCollectionChanged(EventArgs.Empty);
        }

        protected override void InsertItem(int index, IPdfMarker item)
        {
            base.InsertItem(index, item);

            OnCollectionChanged(EventArgs.Empty);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);

            OnCollectionChanged(EventArgs.Empty);
        }

        protected override void SetItem(int index, IPdfMarker item)
        {
            base.SetItem(index, item);

            OnCollectionChanged(EventArgs.Empty);
        }

        protected virtual void OnCollectionChanged(EventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }
}
