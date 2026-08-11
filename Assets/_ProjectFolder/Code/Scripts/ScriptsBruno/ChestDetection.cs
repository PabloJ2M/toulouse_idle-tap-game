using System;
using UnityEngine;

public class ChestDetection : MonoBehaviour, IClickable
{
    public event Action<bool> giveResources;
    public static ChestDetection _instance;
    private void Awake() => _instance = this;
    public void click() => giveResources.Invoke(true);
}
