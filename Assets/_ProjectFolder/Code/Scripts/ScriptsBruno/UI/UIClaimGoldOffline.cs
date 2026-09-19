using UnityEngine;

public class UIClaimGoldOffline : MonoBehaviour
{
    [SerializeField] private ResourcesOffline manager;
    public void ClaimResourcesNormal() => manager.ClaimResourcesManager(1);
    public void ClaimResourcesDouble() => manager.ClaimResourcesManager(2);
}
