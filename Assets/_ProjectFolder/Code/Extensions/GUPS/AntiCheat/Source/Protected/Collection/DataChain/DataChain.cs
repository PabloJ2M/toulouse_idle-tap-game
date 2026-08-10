// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

// GUPS - AntiCheat
using GUPS.AntiCheat.Detector;

namespace GUPS.AntiCheat.Protected.Collection.Chain
{
    /// <summary>
    /// Represents an integrity-checked linked list of value-type items.
    /// </summary>
    /// <typeparam name="T">The value type of the elements stored in the chain.</typeparam>
    /// <remarks>
    /// Every append or remove recomputes the full chain hash and compares it against the stored value, so any
    /// out-of-band modification flips <see cref="HasIntegrity"/> to <c>false</c> and notifies
    /// <see cref="PrimitiveCheatingDetector"/>. The full rehash is O(n), so use this only for small data sets.
    /// </remarks>
    /// <example>
    /// Track an integrity-checked sequence of player action codes:
    /// <code>
    /// using GUPS.AntiCheat.Protected.Collection.Chain;
    ///
    /// var actions = new DataChain&lt;int&gt;();
    /// actions.Append(1); // login
    /// actions.Append(2); // buy potion
    /// actions.Append(3); // logout
    ///
    /// if (!actions.CheckIntegrity())
    /// {
    ///     // Chain hash mismatch — the action log was tampered with.
    /// }
    /// </code>
    /// </example>
    public class DataChain<T> : IDataChain<T>, IWatchedSubject where T: struct
    {
        /// <summary>
        /// The linked list backing the chain.
        /// </summary>
        private readonly LinkedList<T> chain;

        /// <summary>
        /// The hash of the chain at the time of the last successful integrity check.
        /// </summary>
        private Int32 hash;

        /// <inheritdoc/>
        public LinkedList<T> Chain => this.chain;

        /// <inheritdoc/>
        public bool HasIntegrity { get; private set; } = true;

        /// <summary>
        /// Initializes a new empty <see cref="DataChain{T}"/>.
        /// </summary>
        public DataChain()
        {
            this.chain = new LinkedList<T>();

            this.hash = this.GetHashCode();
        }

        /// <inheritdoc/>
        public bool Append(T _Item)
        {
            if (!this.CheckIntegrity())
            {
                return false;
            }

            this.chain.AddLast(_Item);

            this.hash = this.GetHashCode();

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> AppendAsync(T _Item)
        {
            bool var_HasIntegrity = await Task.Run(() => this.CheckIntegrity()).ConfigureAwait(true);

            if (!var_HasIntegrity)
            {
                return false;
            }

            this.chain.AddLast(_Item);

            this.hash = await Task.Run(() => this.GetHashCode()).ConfigureAwait(true);

            return true;
        }

        /// <summary>
        /// Removes the last item from the chain after verifying its integrity.
        /// </summary>
        /// <returns><c>true</c> if the item was removed and the chain still has integrity; otherwise, <c>false</c>.</returns>
        public bool RemoveLast()
        {
            if (!this.CheckIntegrity())
            {
                return false;
            }

            this.chain.RemoveLast();

            this.hash = this.GetHashCode();

            return true;
        }

        /// <summary>
        /// Removes the last item from the chain after verifying its integrity, asynchronously.
        /// </summary>
        /// <returns><c>true</c> if the item was removed and the chain still has integrity; otherwise, <c>false</c>.</returns>
        public async Task<bool> RemoveLastAsync()
        {
            bool var_HasIntegrity = await Task.Run(() => this.CheckIntegrity()).ConfigureAwait(true);

            if (!var_HasIntegrity)
            {
                return false;
            }

            this.chain.RemoveLast();

            this.hash = this.GetHashCode();

            return true;
        }

        /// <summary>
        /// Verifies the integrity of the chain and notifies the cheating detector when integrity is lost.
        /// </summary>
        /// <returns><c>true</c> if the chain still has integrity; otherwise, <c>false</c>.</returns>
        public bool CheckIntegrity()
        {
            if (!this.HasIntegrity)
            {
                return false;
            }

            Int32 currentHash = this.GetHashCode();

            if (this.hash != currentHash)
            {
                this.HasIntegrity = false;
            }

            if (!this.HasIntegrity)
            {
                AntiCheatMonitor.Instance.GetDetector<PrimitiveCheatingDetector>()?.OnNext(this);
            }

            return this.HasIntegrity;
        }

        /// <summary>
        /// Computes a hash code derived from every element of the chain.
        /// </summary>
        /// <returns>The computed hash code.</returns>
        public override Int32 GetHashCode()
        {
            int var_Hash = 17;

            // Wrap on overflow instead of throwing.
            unchecked
            {
                foreach (T item in this.chain)
                {
                    var_Hash = var_Hash + item.GetHashCode() * 23;
                }
            }

            return var_Hash;
        }

        /// <summary>
        /// Determines whether the specified object is a data chain with the same elements in the same order.
        /// </summary>
        /// <param name="_Obj">The object to compare with the current data chain.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
        public override bool Equals(object _Obj)
        {
            if (_Obj == null || GetType() != _Obj.GetType())
            {
                return false;
            }

            DataChain<T> other = (DataChain<T>)_Obj;

            if (this.chain.Count != other.chain.Count)
            {
                return false;
            }

            LinkedListNode<T> thisNode = this.chain.First;
            LinkedListNode<T> otherNode = other.chain.First;

            while (thisNode != null && otherNode != null)
            {
                if (!thisNode.Value.Equals(otherNode.Value))
                {
                    return false;
                }

                thisNode = thisNode.Next;
                otherNode = otherNode.Next;
            }

            return true;
        }

        /// <summary>
        /// Returns an enumerator over the elements of the chain.
        /// </summary>
        /// <returns>An enumerator over the chain.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.chain.GetEnumerator();
        }

        /// <summary>
        /// Returns a non-generic enumerator over the elements of the chain.
        /// </summary>
        /// <returns>A non-generic enumerator over the chain.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.chain.GetEnumerator();
        }

        /// <summary>
        /// Releases any resources held by the data chain. No-op for the default implementation.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
