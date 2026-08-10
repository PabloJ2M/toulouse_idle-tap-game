// System
using System;
using System.Collections;
using System.Collections.Generic;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Integrity;

namespace GUPS.AntiCheat.Protected.Collection
{
    /// <summary>
    /// Represents a hash-protected FIFO queue that detects external modification.
    /// </summary>
    /// <typeparam name="T">The value type of the elements in the queue.</typeparam>
    /// <remarks>
    /// Call <see cref="CheckIntegrity"/> before reading the queue to ensure no element has been tampered with.
    /// </remarks>
    /// <seealso cref="DataChain{T}"/>
    public class ProtectedQueue<T> : IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ICollection, IDataIntegrity where T : struct
    {
        /// <summary>
        /// The underlying queue of items.
        /// </summary>
        private readonly Queue<T> queue;

        /// <summary>
        /// Gets the rolling hash that represents the current state of the queue.
        /// </summary>
        public Int32 Hash { get; private set; }

        /// <summary>
        /// Gets the number of elements in the queue.
        /// </summary>
        public int Count => this.queue.Count;

        /// <summary>
        /// Gets a value indicating whether access to the queue is synchronized.
        /// </summary>
        bool ICollection.IsSynchronized => ((ICollection)this.queue).IsSynchronized;

        /// <summary>
        /// Gets the object used to synchronize access to the queue.
        /// </summary>
        object ICollection.SyncRoot => ((ICollection)this.queue).SyncRoot;

        /// <inheritdoc/>
        public bool HasIntegrity { get; private set; } = true;

        /// <summary>
        /// Initializes a new empty <see cref="ProtectedQueue{T}"/>.
        /// </summary>
        public ProtectedQueue()
        {
            this.queue = new Queue<T>();

            this.Hash = this.GetHashCode();
        }

        /// <summary>
        /// Initializes a new <see cref="ProtectedQueue{T}"/> populated from the given collection.
        /// </summary>
        /// <param name="_Collection">The source collection.</param>
        public ProtectedQueue(IEnumerable<T> _Collection)
        {
            this.queue = new Queue<T>(_Collection);

            this.Hash = this.GetHashCode();
        }

        /// <summary>
        /// Initializes a new empty <see cref="ProtectedQueue{T}"/> with the given initial capacity.
        /// </summary>
        /// <param name="_Capacity">The initial capacity of the queue.</param>
        public ProtectedQueue(int _Capacity)
        {
            this.queue = new Queue<T>(_Capacity);

            this.Hash = this.GetHashCode();
        }

        /// <summary>
        /// Determines whether the queue contains the specified item.
        /// </summary>
        /// <param name="_Item">The item to locate.</param>
        /// <returns><c>true</c> if the item is found; otherwise, <c>false</c>.</returns>
        public bool Contains(T _Item) => this.queue.Contains(_Item);

        /// <summary>
        /// Adds an item to the end of the queue.
        /// </summary>
        /// <param name="_Item">The item to enqueue.</param>
        public void Enqueue(T _Item)
        {
            this.queue.Enqueue(_Item);

            this.Hash = this.AddToHashCode(this.Hash, _Item);
        }

        /// <summary>
        /// Returns the item at the beginning of the queue without removing it.
        /// </summary>
        /// <returns>The item at the beginning of the queue.</returns>
        public T Peek() => this.queue.Peek();

        /// <summary>
        /// Removes and returns the item at the beginning of the queue.
        /// </summary>
        /// <returns>The removed item.</returns>
        public T Dequeue()
        {
            T var_Item = this.queue.Dequeue();

            this.Hash = this.RemoveFromHashCode(this.Hash, var_Item);

            return var_Item;
        }

        /// <summary>
        /// Copies the elements of the queue to the given array, starting at the given index.
        /// </summary>
        /// <param name="_Array">The destination array.</param>
        /// <param name="_Index">The zero-based index in <paramref name="_Array"/> at which to start copying.</param>
        void ICollection.CopyTo(Array _Array, int _Index) => ((ICollection)this.queue).CopyTo(_Array, _Index);

        /// <summary>
        /// Copies the elements of the queue to the given array, starting at the given index.
        /// </summary>
        /// <param name="_Array">The destination array.</param>
        /// <param name="_ArrayIndex">The zero-based index in <paramref name="_Array"/> at which to start copying.</param>
        public void CopyTo(T[] _Array, int _ArrayIndex) => this.queue.CopyTo(_Array, _ArrayIndex);

        /// <summary>
        /// Trims the unused capacity of the queue if it is below an internal threshold.
        /// </summary>
        public void TrimExcess() => this.queue.TrimExcess();

        /// <summary>
        /// Tries to return the item at the beginning of the queue without removing it.
        /// </summary>
        /// <param name="_Result">On success, the item at the beginning of the queue; otherwise, the default value of <typeparamref name="T"/>.</param>
        /// <returns><c>true</c> if the queue was not empty; otherwise, <c>false</c>.</returns>
        public bool TryPeek(out T _Result)
        {
            if (this.queue.Count > 0)
            {
                _Result = this.Peek();
                return true;
            }
            _Result = default;
            return false;
        }

        /// <summary>
        /// Tries to remove and return the item at the beginning of the queue.
        /// </summary>
        /// <param name="_Result">On success, the removed item; otherwise, the default value of <typeparamref name="T"/>.</param>
        /// <returns><c>true</c> if the queue was not empty; otherwise, <c>false</c>.</returns>
        public bool TryDequeue(out T _Result)
        {
            if (this.queue.Count > 0)
            {
                _Result = this.Dequeue();
                return true;
            }
            _Result = default;
            return false;
        }

        /// <summary>
        /// Copies the elements of the queue to a new array.
        /// </summary>
        /// <returns>A new array containing the elements of the queue.</returns>
        public T[] ToArray() => this.queue.ToArray();

        /// <summary>
        /// Removes all elements from the queue and recomputes the hash.
        /// </summary>
        public void Clear()
        {
            this.queue.Clear();

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
        /// Computes a hash code derived from every element of the queue.
        /// </summary>
        /// <returns>The computed hash code.</returns>
        public override int GetHashCode()
        {
            int var_Hash = 17;

            foreach (T var_Item in this.queue)
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
        /// Returns an enumerator over the elements of the queue.
        /// </summary>
        /// <returns>An enumerator over the queue.</returns>
        public IEnumerator<T> GetEnumerator() => this.queue.GetEnumerator();

        /// <summary>
        /// Returns a non-generic enumerator over the elements of the queue.
        /// </summary>
        /// <returns>A non-generic enumerator over the queue.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.queue.GetEnumerator();
    }
}
