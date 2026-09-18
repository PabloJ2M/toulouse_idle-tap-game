using System;
using Unity.Services.Economy;
using Unity.Services.Economy.Samples;
using UnityEngine;

public class ResourcesOffline : MonoBehaviour
{
    [SerializeField] private TaskLoopAsync offlineRewards;
    private int amountToClaim;
    void Awake()
    {
        DateTime lastClaimTime = GetLastClaimTime();
        this.LoopTaskOffline(offlineRewards, lastClaimTime, () => amountToClaim++, () => amountToClaim = 1440);        
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
    public void ClaimResources() =>  EconomyManager.Instance.AddBalanceID(BalanceType.GOLD, (uint)(SlotUpgradeManager.Instance.GetStat(SlotID.Cash) * amountToClaim));
    private void OnApplicationQuit() => SaveClaimTime(DateTime.Now);
}
