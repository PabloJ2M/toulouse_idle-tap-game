using System;
using Unity.Services.Economy;
using Unity.Services.Economy.Samples;
using UnityEngine;

public class ResourcesOffline : MonoBehaviour
{
    [SerializeField] private TaskLoopAsync offlineRewards = new(86400, 60);
    [SerializeField] private GameObject rewardCanva;
    private int amountToClaim;
    void Awake()
    {
        amountToClaim = 0;
        DateTime lastClaimTime = GetLastClaimTime();
        this.LoopTaskOffline(offlineRewards, lastClaimTime, () => amountToClaim++, () => amountToClaim = 1440);        
    }
    private void OnEnable()
    {
        rewardCanva.SetActive(true);
    }
    private void OnDisable()
    {
        rewardCanva.SetActive(false);
    }
    private DateTime GetLastClaimTime()
    {
        string saved = PlayerPrefs.GetString("LastClaimTime", DateTime.UtcNow.ToString("o"));
        return DateTime.Parse(saved, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }
    private void SaveClaimTime(DateTime time)
    {
        PlayerPrefs.SetString("LastClaimTime", time.ToString("o"));
        PlayerPrefs.Save();
    }
    public string GetTotalTime() 
    {
        ushort hours = (ushort)(amountToClaim / 60);
        ushort minutes = (ushort)(amountToClaim % 60);
        return $"{hours}hrs {minutes}m";
    }
    public string GetGoldAmount() => $"$ {SlotUpgradeManager.Instance.GetStat(SlotID.Cash) * amountToClaim}";
    public void ClaimResourcesManager(ushort multiplier = 1)
    {
        EconomyManager.Instance.AddBalanceID(BalanceType.GOLD, (uint)(SlotUpgradeManager.Instance.GetStat(SlotID.Cash) * amountToClaim) * multiplier);
        rewardCanva.SetActive(false);
    }
    private void OnApplicationQuit() => SaveClaimTime(DateTime.UtcNow);
}
