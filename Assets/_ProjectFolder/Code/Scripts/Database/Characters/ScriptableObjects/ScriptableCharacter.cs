using UnityEngine;
using Unity.Services.Economy.Samples;

namespace Gameplay.Characters.Database
{
    [CreateAssetMenu(fileName = "character", menuName = "system/database/characters")]
    public class ScriptableCharacter : ScriptablePurchase
    {
        [SerializeField] private string characterName;
        [SerializeField] private int health;
        [SerializeField] private int attack;
        [SerializeField] private int defence;

        public string Name => characterName;

        public int Health => health;
        public int Attack => attack;
        public int Defence => defence;
    }
}