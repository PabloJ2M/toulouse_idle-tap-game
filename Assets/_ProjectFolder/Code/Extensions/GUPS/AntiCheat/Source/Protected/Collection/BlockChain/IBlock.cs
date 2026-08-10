// System
using System;

namespace GUPS.AntiCheat.Protected.Collection.Chain
{
    /// <summary>
    /// Represents a block of transactions inside a blockchain.
    /// </summary>
    /// <typeparam name="T">The value type of the content stored in the block transactions.</typeparam>
    /// <seealso cref="BlockChain{T}"/>
    public interface IBlock<T>
    {
        /// <summary>
        /// Gets the maximum number of transactions this block can store.
        /// </summary>
        Int32 Size { get; }

        /// <summary>
        /// Gets the transactions currently stored in the block.
        /// </summary>
        ITransaction<T>[] Items { get; }

        /// <summary>
        /// Gets the transaction at the specified index.
        /// </summary>
        /// <param name="_Index">The zero-based index of the transaction to retrieve.</param>
        /// <returns>The transaction at <paramref name="_Index"/>.</returns>
        ITransaction<T> this[Int32 _Index] { get; }

        /// <summary>
        /// Gets the number of transactions currently stored in the block.
        /// </summary>
        Int32 Count { get; }

        /// <summary>
        /// Gets the most recently appended transaction, or <c>null</c> if the block is empty.
        /// </summary>
        ITransaction<T> Last { get; }

        /// <summary>
        /// Gets the nonce of the block, which equals the hash of the previous block in the chain.
        /// </summary>
        Int32 Nonce { get; }

        /// <summary>
        /// Gets the hash of the block, computed from its nonce and transactions.
        /// </summary>
        Int32 Hash { get; }
    }
}
