// System
using System;
using System.Reflection;

// Unity
using UnityEngine;

namespace GUPS.AntiCheat.Protected.Collection.Chain
{
    /// <summary>
    /// Represents a single transaction inside a block, carrying a timestamp and a content value.
    /// </summary>
    /// <typeparam name="T">The value type of the transaction content.</typeparam>
    /// <seealso cref="BlockChain{T}"/>
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Transaction<T> : ITransaction<T>
        where T : struct
    {
        /// <summary>
        /// The timestamp at which the transaction was added to the blockchain (at least millisecond precision recommended).
        /// </summary>
        [SerializeField]
        public Int64 timestamp;

        /// <inheritdoc/>
        public Int64 Timestamp { get => timestamp; private set => timestamp = value; }

        /// <summary>
        /// The serializable content of the transaction.
        /// </summary>
        [SerializeField]
        public T content;

        /// <inheritdoc/>
        public T Content { get => content; private set => content = value; }

        /// <summary>
        /// Initializes a new transaction with the current UTC timestamp and the given content.
        /// </summary>
        /// <param name="_Content">The content of the transaction.</param>
        public Transaction(T _Content)
            :this(DateTimeOffset.UtcNow.Ticks, _Content)
        {
        }

        /// <summary>
        /// Initializes a new transaction with the given timestamp and content.
        /// </summary>
        /// <param name="_Timestamp">The timestamp of the transaction.</param>
        /// <param name="_Content">The content of the transaction.</param>
        public Transaction(Int64 _Timestamp, T _Content)
        {
            this.timestamp = _Timestamp;
            this.content = _Content;
        }

        /// <summary>
        /// Computes a hash code derived from the transaction's timestamp and content.
        /// </summary>
        /// <returns>The computed hash code.</returns>
        public override int GetHashCode()
        {
            return (Int32)this.timestamp ^ this.Content.GetHashCode();
        }
    }
}
