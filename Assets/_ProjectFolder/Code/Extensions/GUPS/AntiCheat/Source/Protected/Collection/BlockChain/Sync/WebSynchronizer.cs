// System
using System;
using System.Threading.Tasks;

// Unity
using UnityEngine;
using UnityEngine.Networking;

namespace GUPS.AntiCheat.Protected.Collection.Chain
{
    /// <summary>
    /// Synchronizes a <see cref="BlockChain{T}"/> with a remote web server through a read and a write endpoint.
    /// </summary>
    /// <typeparam name="T">The value type of the transaction content.</typeparam>
    /// <remarks>
    /// The read endpoint must accept an <c>Int64</c> <c>timestamp</c> query parameter and return the transactions
    /// newer than that timestamp as a JSON array. The write endpoint must accept a JSON array of transactions
    /// and assign each received transaction a server-side timestamp with at least millisecond precision.
    /// </remarks>
    /// <seealso cref="BlockChain{T}"/>
    public class WebSynchronizer<T> : ISynchronizer<T>
        where T : struct
    {
        /// <summary>
        /// Gets the URL of the read endpoint, which returns transactions as a JSON array.
        /// </summary>
        public String ReadEndpoint { get; private set; }

        /// <summary>
        /// Gets the URL of the write endpoint, which accepts transactions as a JSON array.
        /// </summary>
        public String WriteEndpoint { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="WebSynchronizer{T}"/> with the given read and write endpoints.
        /// </summary>
        /// <param name="_ReadEndpoint">The URL of the read endpoint. Must accept a <c>timestamp</c> query parameter and return a JSON array of transactions.</param>
        /// <param name="_WriteEndpoint">The URL of the write endpoint. Must accept a JSON array of transactions and set each received transaction's timestamp.</param>
        public WebSynchronizer(String _ReadEndpoint, String _WriteEndpoint)
        {
            this.ReadEndpoint = _ReadEndpoint;
            this.WriteEndpoint = _WriteEndpoint;
        }

        /// <inheritdoc/>
        /// <exception cref="Exception">Thrown if the web request fails.</exception>
        public async Task<ITransaction<T>[]> ReadAsync(Int64 _SyncTimestamp)
        {
            using (UnityWebRequest var_Request = UnityWebRequest.Get(this.ReadEndpoint + "?timestamp=" + _SyncTimestamp))
            {
                var var_RequestWaiter = var_Request.SendWebRequest();

                while (!var_RequestWaiter.isDone)
                {
                    await Task.Delay(100);
                }

                if (var_Request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception(var_Request.error);
                }

                String var_Content = var_Request.downloadHandler.text;

                if (String.IsNullOrEmpty(var_Content))
                {
                    return new ITransaction<T>[0];
                }

                Transaction<T>[] var_Transactions = JsonUtility.FromJson<Transaction<T>[]>(var_Content);

                return var_Transactions;
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(ITransaction<T> _Transaction)
        {
            await this.WriteAsync(new ITransaction<T>[] { _Transaction });
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Posts the transactions to the remote server as a single JSON payload.
        /// </remarks>
        public async Task WriteAsync(ITransaction<T>[] _Transactions)
        {
            String var_Content = JsonUtility.ToJson(_Transactions);

#if UNITY_2022_2_OR_NEWER
            using (UnityWebRequest var_Request = UnityWebRequest.Post(this.WriteEndpoint, var_Content, "application/json"))
            {
                var var_RequestWaiter = var_Request.SendWebRequest();

                while (!var_RequestWaiter.isDone)
                {
                    await Task.Delay(100);
                }

                if (var_Request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception(var_Request.error);
                }
            }
#else
            using (UnityWebRequest var_Request = UnityWebRequest.Post(this.WriteEndpoint, var_Content))
            {
                var var_RequestWaiter = var_Request.SendWebRequest();

                while (!var_RequestWaiter.isDone)
                {
                    await Task.Delay(100);
                }

                if (var_Request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception(var_Request.error);
                }
            }
#endif
        }
    }
}
