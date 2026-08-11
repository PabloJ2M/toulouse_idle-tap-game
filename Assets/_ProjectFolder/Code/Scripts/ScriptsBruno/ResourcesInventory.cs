using System;
using UnityEngine;

public class ResourcesInventory : MonoBehaviour
{
    public static ResourcesInventory instance;

    [SerializeField] private int gold;
    [SerializeField] private int diamonds;

    public event Action<int, char> UIText;

    private void Awake() => instance = this;
    
    public void AddGold(int amount)
    {
        gold += amount;
        UIText.Invoke(gold, 'g');
    }
    public void AddDiamonds(int amount)
    {
        diamonds += amount;
        UIText.Invoke(diamonds, 'd');
    }

    public void GetResources() => print($"Gold: {gold} /n Diamond: {diamonds}");
    public void GoldError() => print("No estas presinando el cofre");
}
