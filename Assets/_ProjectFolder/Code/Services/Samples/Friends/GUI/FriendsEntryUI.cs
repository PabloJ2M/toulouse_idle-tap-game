using UnityEngine;
using TMPro;

namespace Unity.Services.Friends.Samples.UI
{
    using Models;
    
    public abstract class FriendsEntryUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        protected string ID;
        
        public virtual void Setup(Member member)
        {
            nameText.SetText(member.Profile.Name);
            ID = member.Id;
        }
    }
}