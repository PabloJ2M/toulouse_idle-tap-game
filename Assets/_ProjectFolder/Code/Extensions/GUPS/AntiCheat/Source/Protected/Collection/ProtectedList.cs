// System
using System;
using System.Collections;
using System.Collections.Generic;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Integrity;

namespace GUPS.AntiCheat.Protected.Collection
{
    /// <summary>
    /// Represents a hash-protected <see cref="IList{T}"/> that detects external modification.
    /// </summary>
    /// <typeparam name="T">The value type of the elements in the list.</typeparam>
    /// <remarks>
    /// Call <see cref="CheckIntegrity"/> before reading the list to ensure no element has been tampered with.
    /// </remarks>
    /// <seealso cref="DataChain{T}"/>
    public class ProtectedList<T> : IList<T>, IDataIntegrity where T : struct
    {
        /// <summary>
        /// The underlying list of items.
        /// </summary>
        private readonly List<T> list;

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="_Index">The zero-based index of the element.</param>
        /// <returns>The element at <paramref name="_Index"/>.</returns>
        public T this[int _Index]
        {
            get => this.list[_Index];
            set
            {
                // Remove the old item from the rolling hash before swapping it for the new one.
                this.Hash = this.RemoveFromHashCode(this.Hash, this.list[_Index]);

                this.list[_Index] = value;

                this.Hash = this.AddToHashCode(this.Hash, value);
            }
        }

        /// <summary>
        /// Gets the number of elements in the list.
        /// </summary>
        public int Count => this.list.Count;

        /// <summary>
        /// Gets a value indicating whether the list is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the rolling hash that represents the current state of the list.
        /// </summary>
        public Int32 Hash { get; private set; }

        /// <inheritdoc/>
        public bool HasIntegrity { get; private set; } = true;

        /// <summary>
        /// Initializes a new empty <see cref="ProtectedList{T}"/>.
        /// </summary>
        public ProtectedList()
        {
            this.list = new List<T>();

            this.Hash = this.GetHashCode();
        }

        /// <summary>
        /// Appends an item to the end of the list.
        /// </summary>
        /// <param name="_Item">The item to append.</param>
        public void Add(T _Item)
        {
            this.list.Add(_Item);

            this.Hash = this.AddToHashCode(this.Hash, _Item);
        }

        /// <summary>
        /// Inserts an item at the specified index.
        /// </summary>
        /// <param name="_Index">The zero-based index at which to insert.</param>
        /// <param name="_Item">The item to insert.</param>
        public void Insert(int _Index, T _Item)
        {
            this.list.Insert(_Index, _Item);

            this.Hash = this.AddToHashCode(this.Hash, _Item);
        }

        /// <summary>
        /// Determines whether the list contains the specified item.
        /// </summary>
        /// <param name="_Item">The item to locate.</param>
        /// <returns><c>true</c> if the item is found; otherwise, <c>false</c>.</returns>
        public bool Contains(T _Item) => this.list.Contains(_Item);

        /// <summary>
        /// Copies the elements of the list to the given array, starting at the given index.
        /// </summary>
        /// <param name="_Array">The destination array.</param>
        /// <param name="_ArrayIndex">The zero-based index in <paramref name="_Array"/> at which to start copying.</param>
        public void CopyTo(T[] _Array, int _ArrayIndex) => this.list.CopyTo(_Array, _ArrayIndex);

        /// <summary>
        /// Returns the zero-based index of the first occurrence of the given item.
        /// </summary>
        /// <param name="_Item">The item to locate.</param>
        /// <returns>The index of the first occurrence, or <c>-1</c> if not found.</returns>
        public int IndexOf(T _Item) => this.list.IndexOf(_Item);

        /// <summary>
        /// Removes the first occurrence of the given item from the list.
        /// </summary>
        /// <param name="_Item">The item to remove.</param>
        /// <returns><c>true</c> if the item was found and removed; otherwise, <c>false</c>.</returns>
        public bool Remove(T _Item)
        {
            bool var_Removed = this.list.Remove(_Item);

            if (var_Removed)
            {
                this.Hash = this.RemoveFromHashCode(this.Hash, _Item);
            }

            return var_Removed;
        }

        /// <summary>
        /// Removes the element at the specified index.
        /// </summary>
        /// <param name="_Index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int _Index)
        {
            T var_Item = this.list[_Index];

            this.list.RemoveAt(_Index);

            this.Hash = this.RemoveFromHashCode(this.Hash, var_Item);
        }

        /// <summary>
        /// Removes all elements from the list and recomputes the hash.
        /// </summary>
        public void Clear()
        {
            this.list.Clear();

            this.Hash = this.GetHashCode();
        }

        /// <inheritdoc/>
        public bool CheckIntegrity()
        {
            Int32 currentHash = this.GetHashCode();

            if (this.Hash != currentHash)
            {
                this.HasIntegrity = false;
            }

            return this.HasIntegrity;
        }

        /// <summary>
        /// Computes a hash code derived from every element of the list.
        /// </summary>
        /// <returns>The computed hash code.</returns>
        public override int GetHashCode()
        {
            int var_Hash = 17;

            foreach (T var_Item in this.list)
            {
                var_Hash = this.AddToHashCode(var_Hash, var_Item);
            }

            return var_Hash;
        }

        /// <summary>
        /// Folds the given item into the running hash without recomputing the full hash.
        /// </summary>
        /// <param name="_HashCode">The current hash code.</param>
        /// <param name="_Item">The item to fold in.</param>
        /// <returns>The updated hash code.</returns>
        private int AddToHashCode(int _HashCode, T _Item)
        {
            unchecked
            {
                return _HashCode + _Item.GetHashCode() * 23;
            }
        }

        /// <summary>
        /// Removes the contribution of the given item from the running hash without recomputing the full hash.
        /// </summary>
        /// <param name="_HashCode">The current hash code.</param>
        /// <param name="_Item">The item to remove from the hash.</param>
        /// <returns>The updated hash code.</returns>
        private int RemoveFromHashCode(int _HashCode, T _Item)
        {
            unchecked
            {
                return _HashCode - _Item.GetHashCode() * 23;
            }
        }

        /// <summary>
        /// Returns an enumerator over the elements of the list.
        /// </summary>
        /// <returns>An enumerator over the list.</returns>
        public IEnumerator<T> GetEnumerator() => this.list.GetEnumerator();

        /// <summary>
        /// Returns a non-generic enumerator over the elements of the list.
        /// </summary>
        /// <returns>A non-generic enumerator over the list.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.list.GetEnumerator();
    }
}
