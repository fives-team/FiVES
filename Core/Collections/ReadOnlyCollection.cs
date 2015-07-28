// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    /// <summary>
    /// Read-only wrapper around a list. Write operations are implemented explicitly only (can only be accessed via the
    /// pointer to the base interface) and will result in an NotSupportedException exception if executed. Underlying
    /// implementation creates a copy of the collection at the time of construction to allow lock-free usage. However,
    /// users must be aware that changes in the original collection will not be reflected.
    /// </summary>
    /// <typeparam name="T">Type of the elements stored in the list.</typeparam>
    public class ReadOnlyCollection<T> : ICollection<T>
    {
        public ReadOnlyCollection(ICollection<T> collection)
        {
            collectionClone = new List<T>(collection);
        }

        #region ICollection implementation

        public bool Contains(T item)
        {
            return collectionClone.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            collectionClone.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return collectionClone.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return collectionClone.GetEnumerator();
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
            return collectionClone.GetEnumerator();
        }

        #endregion

        private ICollection<T> collectionClone;
    }
}
