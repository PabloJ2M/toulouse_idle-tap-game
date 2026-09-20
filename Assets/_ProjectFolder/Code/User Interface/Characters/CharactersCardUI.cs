using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Gameplay.Characters.UI
{
    using Database;
    
    public class CharactersCardUI : VirtualObject
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI text;

        public void Setup(ScriptableCharacter character)
        {
            // image.sprite = character.
            text.SetText(character.Name);
        }
    }
}