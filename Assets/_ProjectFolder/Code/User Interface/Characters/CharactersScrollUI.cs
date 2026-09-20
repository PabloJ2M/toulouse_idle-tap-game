using UnityEngine.UI;

namespace Gameplay.Characters.UI
{
    using Database;
    
    public class CharactersScrollUI : VirtualScrollBehaviour<ScriptableCharacter>
    {
        protected override IVirtualLayout CreateLayout() => new VirtualVerticalGrid();
        protected override void BindInstanceData(VirtualObject item, int dataIndex)
        {
            if (item is not CharactersCardUI card) return;
            card.Setup(Data[dataIndex]);
        }

        protected override float GetDynamicSize(int index)
        {
            if (Spawned[index] is VirtualObject vObject)
                return vObject.RectTransform.rect.height;

            return 100f;
        }
    }
}