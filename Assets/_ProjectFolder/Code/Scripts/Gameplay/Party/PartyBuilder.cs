using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Characters
{
    using Database;
    using UI;
    
    public class PartyBuilder : MonoBehaviour
    {
        [SerializeField] private PartyBuilderUI uiBuilder;

        private void OnEnable() => uiBuilder.onCharacterSelected += OnUpdateSelection;
        private void OnDisable() => uiBuilder.onCharacterSelected -= OnUpdateSelection;

        private void OnUpdateSelection(IReadOnlyList<ScriptableCharacter> characters)
        {
            //replace characters in gameplay
        }
    }
}