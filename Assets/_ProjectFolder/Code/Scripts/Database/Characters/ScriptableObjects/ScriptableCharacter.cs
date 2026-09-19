using UnityEngine;

namespace Gameplay.Characters.Database
{
    [CreateAssetMenu(fileName = "character", menuName = "system/database/characters")]
    public class ScriptableCharacter : ScriptableObject
    {
        [SerializeField] private string characterName;
        [SerializeField] private int health;
        [SerializeField] private int attack;
        [SerializeField] private int defence;

        public string CharacterName => characterName;

        public int Health => health;
        public int Attack => attack;
        public int Defence => defence;
    }
}