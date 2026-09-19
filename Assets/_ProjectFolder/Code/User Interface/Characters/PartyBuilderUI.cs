using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Characters.UI
{
    using Database;
    
    public class PartyBuilderUI : MonoBehaviour
    {
        [SerializeField] private List<ScriptableCharacter> characters;
        
        public event Action<IReadOnlyList<ScriptableCharacter>> onCharacterSelected;

        public void SelectCharacter(int index, ScriptableCharacter character)
        {
            if (index < characters.Count)
                characters.Insert(index, character);
            else
                characters[index] = character;
            
            onCharacterSelected?.Invoke(characters);
        }
    }
}