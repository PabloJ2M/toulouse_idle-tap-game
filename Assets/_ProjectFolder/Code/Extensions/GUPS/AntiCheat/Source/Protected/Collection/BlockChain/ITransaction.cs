// System
using System;

namespace GUPS.AntiCheat.Protected.Collection.Chain
{
    /// <summary>
    /// Represents a single transaction stored inside a block of a blockchain.
    /// </summary>
    /// <typeparam name="T">The value type of the transaction content.</typeparam>
    /// <seealso cref="BlockChain{T}"/>
    public interface ITransaction<T>
    {
        /// <summary>
        /// Gets the timestamp at which the transaction was added to the blockchain. Use at least millisecond precision.
        /// </summary>
        Int64 Timestamp { get; }

        /// <summary>
        /// Gets the serializable content of the transaction.
        /// </summary>
        T Content { get; }
    }
}
