using System;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.LevelPlay
{
    [RequireComponent(typeof(Button))]
    public class RewardedAdTriggerButton : RewardedAdTrigger
    {
        private Button _button;

        private void Awake() => _button = GetComponent<Button>();
        private void OnEnable() => _button.onClick.AddListener(TriggerAd);
        private void OnDisable() => _button.onClick.RemoveListener(TriggerAd);
    }
}