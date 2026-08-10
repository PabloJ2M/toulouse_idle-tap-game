// System
using System;
using System.Threading.Tasks;

namespace GUPS.AntiCheat.Protected.Collection.Chain
{
    /// <summary>
    /// Synchronizes a blockchain with a remote source by reading new transactions and uploading local ones.
    /// </summary>
    /// <typeparam name="T">The value type of the transaction content.</typeparam>
    /// <seealso cref="BlockChain{T}"/>
    public interface ISynchronizer<T>
    {
        /// <summary>
        /// Reads all transactions from the remote source that are newer than <paramref name="_SyncTimestamp"/>.
        /// </summary>
        /// <param name="_SyncTimestamp">The timestamp of the last successful synchronization.</param>
        /// <returns>The transactions newer than <paramref name="_SyncTimestamp"/>, or an empty array when none are available.</returns>
        Task<ITransaction<T>[]> ReadAsync(Int64 _SyncTimestamp);

        /// <summary>
        /// Uploads a single transaction to the remote source.
        /// </summary>
        /// <param name="_Transaction">The transaction to upload.</param>
        Task WriteAsync(ITransaction<T> _Transaction);

        /// <summary>
        /// Uploads a batch of transactions to the remote source.
        /// </summary>
        /// <param name="_Transactions">The transactions to upload.</param>
        Task WriteAsync(ITransaction<T>[] _Transactions);
    }
}
