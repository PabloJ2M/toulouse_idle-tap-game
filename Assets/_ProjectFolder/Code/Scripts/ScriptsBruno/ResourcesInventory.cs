using UnityEngine;

public class ResourcesInventory : MonoBehaviour
{
    [SerializeField] private int gold;
    [SerializeField] private int diamonds;

    public void AddGold(int amount) => gold += amount;
    public void AddDiamons(int amount) => diamonds += amount;

    public void GetResources() => print($"Gold: {gold} /n Diamond: {diamonds}");
    public void GoldError() => print("No estas presinando el cofre");
}
