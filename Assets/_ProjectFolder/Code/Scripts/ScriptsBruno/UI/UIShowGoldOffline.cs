using System.Resources;
using TMPro;
using UnityEngine;

public class UIShowGoldOffline : MonoBehaviour
{
    [SerializeField] private ResourcesOffline manager;
    [SerializeField] private TextMeshProUGUI textTime;
    [SerializeField] private TextMeshProUGUI textGold;
    private void OnEnable()
    {
        ShowTime();
        ShowGold();
    }
    private void ShowTime() => textTime.SetText(manager.GetTotalTime());
    private void ShowGold() => textGold.SetText(manager.GetGoldAmount());
}
