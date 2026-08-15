using UnityEngine;
using UnityEngine.Events;

namespace Unity.Services.LevelPlay
{
    public class RewardedAdTrigger : AdTriggerBehaviour
    {
        [SerializeField] private UnityEvent onRewardGranted;
        [SerializeField] private UnityEvent onAdNotReady;
        [SerializeField] private UnityEvent onAdClosed;
        
        public override void TriggerAd()
        {
            if (!RewardedAd.Instance.TryDisplayAd(HandleReward, onAdClosed.Invoke))
                onAdNotReady.Invoke();
        }
        
        private void HandleReward(LevelPlayReward reward) => onRewardGranted.Invoke();
    }
}