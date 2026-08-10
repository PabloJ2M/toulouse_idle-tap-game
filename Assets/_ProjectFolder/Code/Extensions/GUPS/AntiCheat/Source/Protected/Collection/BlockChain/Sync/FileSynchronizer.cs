// System
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// Unity
using UnityEngine;

namespace GUPS.AntiCheat.Protected.Collection.Chain
{
    /// <summary>
    /// Synchronizes a <see cref="BlockChain{T}"/> with a JSON-line file on the local file system.
    /// </summary>
    /// <typeparam name="T">The value type of the transaction content.</typeparam>
    /// <remarks>
    /// Each transaction is stored as a single JSON line in append order. Reads stop as soon as a transaction
    /// older than the requested sync timestamp is reached. Only primitive types or structs are supported.
    /// </remarks>
    /// <seealso cref="BlockChain{T}"/>
    public class FileSynchronizer<T> : ISynchronizer<T>
        where T : struct
    {
        /// <summary>
        /// Gets the path of the file used for synchronization.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// The timestamp at which the file was last read.
        /// </summary>
        private Int64 lastFileReadTimestamp = 0;

        /// <summary>
        /// The ordered cache of transactions read from the file.
        /// </summary>
        private List<ITransaction<T>> transactions = new List<ITransaction<T>>();

        /// <summary>
        /// Initializes a new <see cref="FileSynchronizer{T}"/> bound to the given file path.
        /// </summary>
        /// <param name="_FilePath">The local file path used for synchronization.</param>
        public FileSynchronizer(String _FilePath)
        {
            this.FilePath = _FilePath;
        }

        /// <summary>
        /// Reads all transactions from the file that are newer than <paramref name="_Timestamp"/>.
        /// </summary>
        /// <param name="_Timestamp">The lower bound on the transaction timestamp. Only transactions with a higher timestamp are returned.</param>
        /// <returns>The transactions read from the file, ordered from oldest to newest.</returns>
        private async Task<List<ITransaction<T>>> ReadFromFileAsync(Int64 _Timestamp)
        {
            List<ITransaction<T>> var_Result = new List<ITransaction<T>>();

            using(FileStream var_FileStream = new FileStream(this.FilePath, FileMode.Open, FileAccess.Read))
            {
                using(StreamReader var_StreamReader = new StreamReader(var_FileStream))
                {
                    List<String> var_Lines = new List<String>();

                    while(!var_StreamReader.EndOfStream)
                    {
                        String var_Line = await var_StreamReader.ReadLineAsync();

                        var_Lines.Add(var_Line);
                    }

                    // Walk lines from newest to oldest; stop as soon as we cross the timestamp boundary.
                    for(int i = var_Lines.Count - 1; i >= 0; i--)
                    {
                        Transaction<T> var_Transaction = JsonUtility.FromJson<Transaction<T>>(var_Lines[i]);

                        if(var_Transaction.timestamp > _Timestamp)
                        {
                            var_Result.Insert(0, var_Transaction);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return var_Result;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Refreshes the in-memory cache if the file was modified since the last read, then returns the cached
        /// transactions newer than <paramref name="_SyncTimestamp"/>.
        /// </remarks>
        public async Task<ITransaction<T>[]> ReadAsync(Int64 _SyncTimestamp)
        {
            // #1: Refresh the cache from disk if the file was modified.

            Int64 var_LastModifiedTimestamp = File.GetLastWriteTime(this.FilePath).ToFileTimeUtc();

            if(var_LastModifiedTimestamp > this.lastFileReadTimestamp)
            {
                List<ITransaction<T>> var_Transactions = await this.ReadFromFileAsync(this.transactions.Count > 0 ? this.transactions[this.transactions.Count - 1].Timestamp : 0);

                lock(this.transactions)
                {
                    this.transactions.AddRange(var_Transactions);
                }

                this.lastFileReadTimestamp = var_LastModifiedTimestamp;
            }

            // #2: Pick the cached transactions newer than the requested sync timestamp.

            List<ITransaction<T>> var_Result = new List<ITransaction<T>>();

            lock(this.transactions)
            {
                for(int i = this.transactions.Count - 1; i >= 0; i--)
                {
                    ITransaction<T> var_Transaction = this.transactions[i];

                    if (var_Transaction.Timestamp > _SyncTimestamp)
                    {
                        var_Result.Insert(0, var_Transaction);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return var_Result.ToArray();
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Rewrites the transaction with the current UTC tick count before appending it as a single JSON line.
        /// </remarks>
        public async Task WriteAsync(ITransaction<T> _Transaction)
        {
            // Stamp the transaction with the server-side write time so its order in the file is monotonic.
            Transaction<T> var_WrittenTransaction = new Transaction<T>(DateTime.UtcNow.Ticks, _Transaction.Content);

            using(FileStream var_FileStream = new FileStream(this.FilePath, FileMode.Append, FileAccess.Write))
            {
                using(StreamWriter var_StreamWriter = new StreamWriter(var_FileStream))
                {
                    String var_SerializedTransaction = JsonUtility.ToJson(var_WrittenTransaction);

                    await var_StreamWriter.WriteLineAsync(var_SerializedTransaction);

                    await var_StreamWriter.FlushAsync();
                }
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(ITransaction<T>[] _Transactions)
        {
            foreach(ITransaction<T> var_Transaction in _Transactions)
            {
                await this.WriteAsync(var_Transaction);
            }
        }
    }
}
