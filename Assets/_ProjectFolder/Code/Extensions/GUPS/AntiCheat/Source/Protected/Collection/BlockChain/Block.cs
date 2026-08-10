// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

// Unity
using UnityEngine;

namespace GUPS.AntiCheat.Protected.Collection.Chain
{
    /// <summary>
    /// Represents a fixed-size block of transactions that can be linked into a blockchain.
    /// </summary>
    /// <typeparam name="T">The value type of the content stored in the block transactions.</typeparam>
    /// <remarks>
    /// Each block carries a nonce equal to the hash of the previous block, and a hash computed from its nonce
    /// and transactions. The combination allows the chain to detect any tampering with either order or content.
    /// Only primitive types or structs are supported.
    /// </remarks>
    /// <seealso cref="BlockChain{T}"/>
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Block<T> : IBlock<T>, IEnumerable<ITransaction<T>> 
        where T : struct
    {
        /// <summary>
        /// The shared random number generator used to seed the initial nonce.
        /// </summary>
        private static readonly System.Random random = new System.Random();

        /// <summary>
        /// The maximum number of transactions this block can store.
        /// </summary>
        [SerializeField]
        private int size;

        /// <inheritdoc/>
        public int Size { get => size; private set => size = value; }

        /// <summary>
        /// The fixed-size array of transactions stored in this block.
        /// </summary>
        [SerializeReference]
        private readonly ITransaction<T>[] transactions;

        /// <inheritdoc/>
        public ITransaction<T>[] Items => transactions;

        /// <inheritdoc/>
        public ITransaction<T> this[int _Index] { get => this.transactions[_Index]; }

        /// <summary>
        /// The number of transactions currently appended to the block.
        /// </summary>
        [SerializeField]
        private int count;

        /// <inheritdoc/>
        public int Count { get => count; private set => count = value; }

        /// <inheritdoc/>
        public ITransaction<T> Last => this.transactions.Length > 0 ? this.transactions[this.Count - 1] : null;

        /// <summary>
        /// Gets the timestamp of the last transaction in the block, or <c>0</c> if the block is empty.
        /// </summary>
        public Int64 LastTransactionTimestamp => this.Last?.Timestamp ?? 0;

        /// <summary>
        /// The nonce of the block, which equals the hash of the previous block in the chain.
        /// </summary>
        [SerializeField]
        public int nonce;

        /// <inheritdoc/>
        public int Nonce { get => nonce; private set => nonce = value; }

        /// <summary>
        /// The hash of the block, computed from its nonce and transactions.
        /// </summary>
        [SerializeField]
        public int hash;

        /// <inheritdoc/>
        public int Hash { get => hash; private set => hash = value; }

        /// <summary>
        /// Initializes a new genesis block of the given size with a random nonce.
        /// </summary>
        /// <param name="_Size">The maximum number of transactions the block can hold.</param>
        public Block(int _Size)
        {
            this.size = _Size;
            this.transactions = new ITransaction<T>[this.size];

            this.nonce = random.Next(Int32.MaxValue);

            this.hash = this.GetHashCode();
        }

        /// <summary>
        /// Initializes a new block of the given size, linked to a previous block via the supplied nonce.
        /// </summary>
        /// <param name="_Size">The maximum number of transactions the block can hold.</param>
        /// <param name="_Nonce">The nonce for the block — typically the hash of the previous block.</param>
        public Block(int _Size, int _Nonce)
        {
            this.size = _Size;
            this.transactions = new ITransaction<T>[this.size];
            this.nonce = _Nonce;

            this.hash = this.GetHashCode();
        }

        /// <summary>
        /// Appends a transaction to the block and refreshes its hash.
        /// </summary>
        /// <param name="_Transaction">The transaction to append.</param>
        /// <returns><c>true</c> if the transaction was appended; <c>false</c> if the block is already full.</returns>
        public bool Append(ITransaction<T> _Transaction)
        {
            if (this.count == this.size)
            {
                return false;
            }

            this.transactions[Count] = _Transaction;

            this.count++;

            // Refresh the stored hash so a later Verify() call can detect tampering.
            this.hash = this.GetHashCode();

            return true;
        }

        /// <summary>
        /// Verifies the integrity of the block by recomputing its hash and comparing it to the stored value.
        /// </summary>
        /// <returns><c>true</c> if the stored hash still matches the computed hash; otherwise, <c>false</c>.</returns>
        public bool Verify()
        {
            return this.hash == this.GetHashCode();
        }

        /// <summary>
        /// Computes a hash code derived from the nonce and every appended transaction.
        /// </summary>
        /// <returns>The computed hash code.</returns>
        public override int GetHashCode()
        {
            // Seed the hash with the nonce so the block is bound to its predecessor.
            int var_Hash = this.nonce;

            // Wrap on overflow instead of throwing.
            unchecked
            {
                for (int i = 0; i < Count; i++)
                {
                    var_Hash = var_Hash + this.transactions[i].GetHashCode() * 23;
                }
            }

            return var_Hash;
        }

        /// <summary>
        /// Determines whether the specified object is a block with the same transactions in the same order.
        /// </summary>
        /// <param name="_Obj">The object to compare with the current block.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
        public override bool Equals(object _Obj)
        {
            if (_Obj == null || GetType() != _Obj.GetType())
            {
                return false;
            }

            Block<T> var_Other = (Block<T>)_Obj;

            if (this.count != var_Other.Count)
            {
                return false;
            }

            for (int i = 0; i < Count; i++)
            {
                if (!this.transactions[i].Equals(var_Other.transactions[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns an enumerator over the transactions in the block, skipping trailing empty slots.
        /// </summary>
        /// <returns>An enumerator over the block's transactions.</returns>
        public IEnumerator<ITransaction<T>> GetEnumerator()
        {
            foreach (ITransaction<T> var_Transaction in this.transactions)
            {
                if (var_Transaction == null)
                {
                    break;
                }

                yield return var_Transaction;
            }
        }

        /// <summary>
        /// Returns a non-generic enumerator over the transactions in the block.
        /// </summary>
        /// <returns>A non-generic enumerator over the block's transactions.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
