using UnityEngine.UI;

namespace Gameplay.Characters.UI
{
    using Database;
    
    public class CharactersDisplayUI : VirtualScrollBehaviour<ScriptableCharacter>
    {
        protected override IVirtualLayout CreateLayout() => new VirtualVerticalGrid();
        protected override void BindInstanceData(VirtualObject item, int dataIndex)
        {
            
        }

        protected override float GetDynamicSize(int index)
        {
            return 100f;
        }
    }
}