using UnityEngine;
public class Autoclicker : MonoBehaviour
{
    [SerializeField] private float time;
    [SerializeField] private float delayTime;
    private void Update() => AutoClick();
    private void AutoClick() 
    {
        time += Time.deltaTime;
        if (time >= delayTime)
        {
            time -= delayTime;
            StatUtil.Instance.AddGold(SlotID.AutoClicker);
        }
    }
}
