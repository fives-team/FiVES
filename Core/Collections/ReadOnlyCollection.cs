using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Read-only wrapper around a list. Write operations are implemented explicitly only (can only be accessed via the
    /// pointer to the base interface) and will result in an NotSupportedException exception if executed.
    /// </summary>
    /// <typeparam name="T">Type of the elements stored in the list.</typeparam>
    public class ReadOnlyCollection<T> : ICollection<T>
    {
        public ReadOnlyCollection(ICollection<T> collection)
        {
            writableCollection = collection;
        }

        #region ICollection implementation

        public bool Contains(T item)
        {
            return writableCollection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            writableCollection.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return writableCollection.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return writableCollection.GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return writableCollection.GetEnumerator();
        }

        #endregion

        private ICollection<T> writableCollection;
    }
}
