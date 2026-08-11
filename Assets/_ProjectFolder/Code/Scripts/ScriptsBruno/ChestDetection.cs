using System;
using UnityEngine;

public class ChestDetection : SingletonBasic<ChestDetection>,IClickable
{
    public event Action<bool> giveResources;
    public void click() => giveResources.Invoke(true);
}
