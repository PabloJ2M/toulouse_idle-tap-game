using UnityEngine;
using UnityEngine.Pool;

namespace Entity.AI
{
    [RequireComponent(typeof(PoolObject))]
    public class Enemy : MonoBehaviour
    {
        private PoolObject _poolObject;
        
        private void Awake() => _poolObject = GetComponent<PoolObject>();
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                _poolObject.Release();
        }
    }
}