// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Watch;

// GUPS - AntiCheat
using GUPS.AntiCheat.Detector;

namespace GUPS.AntiCheat.Protected.Collection.Chain
{
    /// <summary>
    /// Represents an integrity-checked chain of <see cref="Block{T}"/> instances that can optionally synchronize with a remote source.
    /// </summary>
    /// <typeparam name="T">The value type of the content stored in the block transactions.</typeparam>
    /// <remarks>
    /// Blocks are linked through hash chaining: each block's nonce is the hash of its predecessor. Every modification
    /// (append or remote sync) re-validates the affected block, and a broken hash notifies
    /// <see cref="PrimitiveCheatingDetector"/> through the <see cref="IWatchedSubject"/> contract.
    /// The structure is intended for small, integrity-critical data sets such as scores or progression — not bulk storage.
    /// Only primitive types or structs are supported.
    /// </remarks>
    /// <example>
    /// Append an item, verify the chain and (optionally) replicate to a remote server:
    /// <code>
    /// using GUPS.AntiCheat.Protected.Collection.Chain;
    ///
    /// // PlayerScore is a value type (struct).
    /// var chain = new BlockChain&lt;PlayerScore&gt;();
    ///
    /// chain.Append(new PlayerScore { Player = "Tim", Points = 1337 });
    ///
    /// if (!chain.CheckIntegrity())
    /// {
    ///     // The chain has been tampered with.
    /// }
    ///
    /// // Optional: replicate through a synchronizer.
    /// var sync = new WebSynchronizer&lt;PlayerScore&gt;(
    ///     "https://example.com/scores/read",
    ///     "https://example.com/scores/write");
    ///
    /// var remoteChain = new BlockChain&lt;PlayerScore&gt;(sync);
    /// await remoteChain.AppendAsync(new PlayerScore { Player = "Tim", Points = 1337 });
    /// </code>
    /// </example>
    public class BlockChain<T> : IDataChain<Block<T>>, IWatchedSubject 
        where T : struct
    {
        /// <summary>
        /// The chain of blocks backing the blockchain.
        /// </summary>
        private readonly LinkedList<Block<T>> chain;

        /// <inheritdoc/>
        public LinkedList<Block<T>> Chain => this.chain;

        /// <summary>
        /// The maximum number of transactions stored per block.
        /// </summary>
        private readonly int blockSize;

        /// <summary>
        /// Gets the last block of the chain, or <c>null</c> if the chain is empty.
        /// </summary>
        public Block<T> LastBlock => this.chain.Last?.Value ?? null;

        /// <summary>
        /// The synchronizer used to read and write transactions against a remote source.
        /// </summary>
        private ISynchronizer<T> synchronizer;

        /// <inheritdoc/>
        public bool HasIntegrity { get; private set; } = true;

        /// <summary>
        /// Initializes a new local <see cref="BlockChain{T}"/> with a block size of 10 and no synchronizer.
        /// </summary>
        public BlockChain()
            :this(null)
        {
        }

        /// <summary>
        /// Initializes a new local <see cref="BlockChain{T}"/> with the given block size and no synchronizer.
        /// </summary>
        /// <param name="_BlockSize">The maximum number of transactions per block.</param>
        public BlockChain(int _BlockSize)
            : this(_BlockSize, null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="BlockChain{T}"/> with a block size of 10 and the given synchronizer.
        /// </summary>
        /// <param name="_Synchronizer">The synchronizer used to replicate transactions, or <c>null</c> for a local-only chain.</param>
        public BlockChain(ISynchronizer<T> _Synchronizer)
            :this(10, _Synchronizer)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="BlockChain{T}"/> with the given block size and synchronizer.
        /// </summary>
        /// <param name="_BlockSize">The maximum number of transactions per block.</param>
        /// <param name="_Synchronizer">The synchronizer used to replicate transactions, or <c>null</c> for a local-only chain.</param>
        public BlockChain(int _BlockSize, ISynchronizer<T> _Synchronizer)
        {
            this.synchronizer = _Synchronizer;

            this.chain = new LinkedList<Block<T>>();
            this.blockSize = _BlockSize;
        }

        /// <summary>
        /// Pulls any new remote transactions since the last sync timestamp and appends them to the local chain.
        /// </summary>
        /// <returns><c>true</c> if the sync completed and the chain still has integrity; <c>false</c> if no synchronizer is set, integrity was lost, or the remote read failed.</returns>
        public async Task<bool> SynchronizeAsync()
        {
            if (this.synchronizer == null)
            {
                return false;
            }

            if (!this.CheckIntegrityOfLastBlock())
            {
                return false;
            }

            // Pull only the transactions newer than what we already have locally.
            ITransaction<T>[] var_RemoteTransactions = await this.synchronizer.ReadAsync(this.LastBlock?.Last?.Timestamp ?? 0);

            if (var_RemoteTransactions == null)
            {
                return false;
            }

            this.Append(var_RemoteTransactions);

            return true;
        }

        /// <summary>
        /// Appends a transaction to the local chain without remote sync or integrity verification.
        /// </summary>
        /// <param name="_Transaction">The transaction to append.</param>
        private void Append(ITransaction<T> _Transaction)
        {
            Block<T> var_LastBlock = null;

            if (this.chain.Count == 0)
            {
                var_LastBlock = new Block<T>(this.blockSize);
                this.chain.AddFirst(var_LastBlock);
            }
            else
            {
                var_LastBlock = this.chain.Last.Value;
            }

            // Roll over to a new block linked by the previous block's hash once the current block is full.
            if (var_LastBlock.Count == var_LastBlock.Size)
            {
                int var_Nonce = var_LastBlock.Hash;
                var_LastBlock = new Block<T>(this.blockSize, var_Nonce);
                this.chain.AddLast(var_LastBlock);
            }

            var_LastBlock.Append(_Transaction);
        }

        /// <summary>
        /// Appends multiple transactions to the local chain without remote sync or integrity verification.
        /// </summary>
        /// <param name="_Transactions">The transactions to append.</param>
        private void Append(ITransaction<T>[] _Transactions)
        {
            foreach (ITransaction<T> var_Transaction in _Transactions)
            {
                this.Append(var_Transaction);
            }
        }

        /// <summary>
        /// Wraps the item in a transaction and appends it to the chain, replicating it to the remote source when a synchronizer is set.
        /// </summary>
        /// <param name="_Item">The item to append.</param>
        /// <returns><c>true</c> if the item was appended and the chain still has integrity; otherwise, <c>false</c>.</returns>
        public bool Append(T _Item)
        {
            if(!this.CheckIntegrityOfLastBlock())
            {
                return false;
            }

            // Local-only chain: append directly.
            if(this.synchronizer == null)
            {
                this.Append(new Transaction<T>(_Item));

                return true;
            }

            // Remote-backed chain: write to the remote source first, then pull back the resulting state.
            var writeTask = Task.Run(async () => { await this.synchronizer.WriteAsync(new Transaction<T>(_Item)); });
            writeTask.Wait();

            var syncTask = Task.Run(async () => { return await this.SynchronizeAsync(); });
            syncTask.Wait();

            return syncTask.Result;
        }

        /// <summary>
        /// Wraps the item in a transaction and appends it to the chain asynchronously, replicating it to the remote source when a synchronizer is set.
        /// </summary>
        /// <param name="_Item">The item to append.</param>
        /// <returns><c>true</c> if the item was appended and the chain still has integrity; otherwise, <c>false</c>.</returns>
        public async Task<bool> AppendAsync(T _Item)
        {
            if (!this.CheckIntegrityOfLastBlock())
            {
                return false;
            }

            if (this.synchronizer == null)
            {
                this.Append(new Transaction<T>(_Item));

                return true;
            }

            await this.synchronizer.WriteAsync(new Transaction<T>(_Item));

            return await this.SynchronizeAsync();
        }

        /// <inheritdoc/>
        bool IDataChain<Block<T>>.Append(Block<T> _Item)
        {
            // Unfold the block into its transactions so each item runs through the normal append path.
            foreach (ITransaction<T> var_Transaction in _Item.Items)
            {
                this.Append(var_Transaction.Content);
            }

            return true;
        }

        /// <inheritdoc/>
        async Task<bool> IDataChain<Block<T>>.AppendAsync(Block<T> _Item)
        {
            foreach (ITransaction<T> var_Transaction in _Item.Items)
            {
                await this.AppendAsync(var_Transaction.Content);
            }

            return true;
        }

        /// <summary>
        /// Verifies a single block and its link to the previous block.
        /// </summary>
        /// <param name="_Node">The chain node holding the block to verify.</param>
        /// <returns><c>true</c> if the block hash is valid and its nonce matches the previous block's hash; otherwise, <c>false</c>.</returns>
        private bool CheckIntegrityOfBlock(LinkedListNode<Block<T>> _Node)
        {
            if (!_Node.Value.Verify())
            {
                return false;
            }

            // The nonce of a non-genesis block must equal the hash of its predecessor.
            if (_Node.Previous != null && _Node.Value.nonce != _Node.Previous.Value.hash)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Verifies every block in the chain and notifies the cheating detector when integrity is lost.
        /// </summary>
        /// <returns><c>true</c> if the chain still has integrity; otherwise, <c>false</c>.</returns>
        public bool CheckIntegrity()
        {
            if (!this.HasIntegrity)
            {
                return false;
            }

            // Walk the chain from newest to oldest so the most recent tampering surfaces first.
            var var_Node = this.chain.Last;

            while (var_Node != null)
            {
                if (!this.CheckIntegrityOfBlock(var_Node))
                {
                    this.HasIntegrity = false;
                    break;
                }

                var_Node = var_Node.Previous;
            }

            if (!this.HasIntegrity)
            {
                AntiCheatMonitor.Instance.GetDetector<PrimitiveCheatingDetector>()?.OnNext(this);
            }

            return this.HasIntegrity;
        }

        /// <summary>
        /// Verifies the last block of the chain and notifies the cheating detector when integrity is lost.
        /// </summary>
        /// <returns><c>true</c> if the last block is intact; otherwise, <c>false</c>.</returns>
        public bool CheckIntegrityOfLastBlock()
        {
            if (!this.HasIntegrity)
            {
                return false;
            }

            if (this.chain.Count > 0)
            {
                if(!this.CheckIntegrityOfBlock(this.chain.Last))
                {
                    this.HasIntegrity = false;
                }
            }

            if (!this.HasIntegrity)
            {
                AntiCheatMonitor.Instance.GetDetector<PrimitiveCheatingDetector>()?.OnNext(this);
            }

            return this.HasIntegrity;
        }

        /// <summary>
        /// Returns an enumerator over the blocks in the chain.
        /// </summary>
        /// <returns>An enumerator over the blocks.</returns>
        public IEnumerator<Block<T>> GetEnumerator()
        {
            return this.chain.GetEnumerator();
        }

        /// <summary>
        /// Returns a non-generic enumerator over the blocks in the chain.
        /// </summary>
        /// <returns>A non-generic enumerator over the blocks.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.chain.GetEnumerator();
        }

        /// <summary>
        /// Releases any resources held by the blockchain. No-op for the default implementation.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
