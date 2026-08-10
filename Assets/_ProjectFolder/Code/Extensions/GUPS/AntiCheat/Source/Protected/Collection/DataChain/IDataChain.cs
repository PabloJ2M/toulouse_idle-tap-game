// System
using System.Collections.Generic;
using System.Threading.Tasks;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Integrity;

namespace GUPS.AntiCheat.Protected.Collection.Chain
{
    /// <summary>
    /// Represents an integrity-checked linked list of items of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the data chain.</typeparam>
    /// <seealso cref="DataChain{T}"/>
    public interface IDataChain<T> : IEnumerable<T>, IDataIntegrity
    {
        /// <summary>
        /// Gets the underlying linked list of items.
        /// </summary>
        LinkedList<T> Chain { get; }

        /// <summary>
        /// Appends an item to the end of the chain after verifying its integrity.
        /// </summary>
        /// <param name="_Item">The item to append.</param>
        /// <returns><c>true</c> if the item was appended and the chain still has integrity; otherwise, <c>false</c>.</returns>
        bool Append(T _Item);

        /// <summary>
        /// Appends an item to the end of the chain after verifying its integrity, asynchronously.
        /// </summary>
        /// <param name="_Item">The item to append.</param>
        /// <returns><c>true</c> if the item was appended and the chain still has integrity; otherwise, <c>false</c>.</returns>
        Task<bool> AppendAsync(T _Item);
    }
}
