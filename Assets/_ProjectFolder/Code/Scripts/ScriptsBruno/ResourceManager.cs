using UnityEngine;
using System.Collections;

public class ResourceManager : MonoBehaviour
{
    [SerializeField] private ResourcesInventory inventory;
    [SerializeField] private ChestDetection tap;

    private Coroutine goldCoroutine;
    private void Start()
    {
        tap.giveResources += ActiveObtain;
    }
    private void ActiveObtain(bool chestDetect)
    {
        if (chestDetect)
            IncreaseGold(100);
        else
            inventory.GoldError();
    }
    private void PasiveObtain() { } // Solamente esta por referencia
    private void IncreaseGold(int amount)
    {
        if (goldCoroutine == null)
            goldCoroutine = StartCoroutine(AddGoldToInvent(amount));
    }
    private IEnumerator AddGoldToInvent(int amount)
    {
        inventory.AddGold(amount);
        yield return new WaitForSecondsRealtime(0.1f);
        goldCoroutine = null;
    }
}
