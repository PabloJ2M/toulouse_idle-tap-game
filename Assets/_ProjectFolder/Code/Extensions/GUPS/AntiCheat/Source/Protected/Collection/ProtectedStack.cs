// System
using System;
using System.Collections;
using System.Collections.Generic;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Integrity;

namespace GUPS.AntiCheat.Protected.Collection
{
    /// <summary>
    /// Represents a hash-protected LIFO stack that detects external modification.
    /// </summary>
    /// <typeparam name="T">The value type of the elements in the stack.</typeparam>
    /// <remarks>
    /// Call <see cref="CheckIntegrity"/> before reading the stack to ensure no element has been tampered with.
    /// </remarks>
    /// <seealso cref="DataChain{T}"/>
    public class ProtectedStack<T> : IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ICollection, IDataIntegrity where T : struct
    {
        /// <summary>
        /// The underlying stack of items.
        /// </summary>
        private readonly Stack<T> stack;

        /// <summary>
        /// Gets a value indicating whether access to the stack is synchronized.
        /// </summary>
        bool ICollection.IsSynchronized => ((ICollection)this.stack).IsSynchronized;

        /// <summary>
        /// Gets the object used to synchronize access to the stack.
        /// </summary>
        object ICollection.SyncRoot => ((ICollection)this.stack).SyncRoot;

        /// <summary>
        /// Gets the rolling hash that represents the current state of the stack.
        /// </summary>
        public Int32 Hash { get; private set; }

        /// <inheritdoc/>
        public bool HasIntegrity { get; private set; } = true;

        /// <summary>
        /// Initializes a new empty <see cref="ProtectedStack{T}"/>.
        /// </summary>
        public ProtectedStack()
        {
            this.stack = new Stack<T>();

            this.Hash = this.GetHashCode();
        }

        /// <summary>
        /// Initializes a new <see cref="ProtectedStack{T}"/> populated from the given collection.
        /// </summary>
        /// <param name="_Collection">The source collection.</param>
        public ProtectedStack(IEnumerable<T> _Collection)
        {
            this.stack = new Stack<T>(_Collection);

            this.Hash = this.GetHashCode();
        }

        /// <summary>
        /// Initializes a new empty <see cref="ProtectedStack{T}"/> with the given initial capacity.
        /// </summary>
        /// <param name="_Capacity">The initial capacity of the stack.</param>
        public ProtectedStack(int _Capacity)
        {
            this.stack = new Stack<T>(_Capacity);

            this.Hash = this.GetHashCode();
        }

        /// <summary>
        /// Gets the number of elements in the stack.
        /// </summary>
        public int Count => this.stack.Count;

        /// <summary>
        /// Determines whether the stack contains the specified item.
        /// </summary>
        /// <param name="_Item">The item to locate.</param>
        /// <returns><c>true</c> if the item is found; otherwise, <c>false</c>.</returns>
        public bool Contains(T _Item) => this.stack.Contains(_Item);

        /// <summary>
        /// Copies the elements of the stack to the given array, starting at the given index.
        /// </summary>
        /// <param name="_Array">The destination array.</param>
        /// <param name="_ArrayIndex">The zero-based index in <paramref name="_Array"/> at which to start copying.</param>
        public void CopyTo(T[] _Array, int _ArrayIndex) => this.stack.CopyTo(_Array, _ArrayIndex);

        /// <summary>
        /// Copies the elements of the stack to the given array, starting at the given index.
        /// </summary>
        /// <param name="_Array">The destination array.</param>
        /// <param name="_Index">The zero-based index in <paramref name="_Array"/> at which to start copying.</param>
        void ICollection.CopyTo(Array _Array, int _Index) => ((ICollection)this.stack).CopyTo(_Array, _Index);

        /// <summary>
        /// Pushes an item onto the top of the stack.
        /// </summary>
        /// <param name="_Item">The item to push.</param>
        public void Push(T _Item)
        {
            this.stack.Push(_Item);

            this.Hash = this.AddToHashCode(this.Hash, _Item);
        }

        /// <summary>
        /// Returns the item at the top of the stack without removing it.
        /// </summary>
        /// <returns>The item at the top of the stack.</returns>
        public T Peek() => this.stack.Peek();

        /// <summary>
        /// Removes and returns the item at the top of the stack.
        /// </summary>
        /// <returns>The removed item.</returns>
        public T Pop()
        {
            T var_Item = this.stack.Pop();

            this.Hash = this.RemoveFromHashCode(this.Hash, var_Item);

            return var_Item;
        }

        /// <summary>
        /// Trims the unused capacity of the stack if it is below an internal threshold.
        /// </summary>
        public void TrimExcess() => this.stack.TrimExcess();

        /// <summary>
        /// Tries to return the item at the top of the stack without removing it.
        /// </summary>
        /// <param name="_Result">On success, the item at the top of the stack; otherwise, the default value of <typeparamref name="T"/>.</param>
        /// <returns><c>true</c> if the stack was not empty; otherwise, <c>false</c>.</returns>
        public bool TryPeek(out T _Result) => this.stack.TryPeek(out _Result);

        /// <summary>
        /// Tries to remove and return the item at the top of the stack.
        /// </summary>
        /// <param name="_Result">On success, the removed item; otherwise, the default value of <typeparamref name="T"/>.</param>
        /// <returns><c>true</c> if the stack was not empty; otherwise, <c>false</c>.</returns>
        public bool TryPop(out T _Result)
        {
            if (this.stack.Count > 0)
            {
                _Result = this.Pop();
                return true;
            }
            _Result = default;
            return false;
        }

        /// <summary>
        /// Copies the elements of the stack to a new array.
        /// </summary>
        /// <returns>A new array containing the elements of the stack.</returns>
        public T[] ToArray() => this.stack.ToArray();

        /// <summary>
        /// Removes all elements from the stack.
        /// </summary>
        public void Clear() => this.stack.Clear();

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
        /// Computes a hash code derived from every element of the stack.
        /// </summary>
        /// <returns>The computed hash code.</returns>
        public override int GetHashCode()
        {
            int var_Hash = 17;

            foreach (T var_Item in this.stack)
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
        /// Returns an enumerator over the elements of the stack.
        /// </summary>
        /// <returns>An enumerator over the stack.</returns>
        public IEnumerator<T> GetEnumerator() => this.stack.GetEnumerator();

        /// <summary>
        /// Returns a non-generic enumerator over the elements of the stack.
        /// </summary>
        /// <returns>A non-generic enumerator over the stack.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.stack.GetEnumerator();
    }
}
