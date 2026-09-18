using System;
using Unity.Services.Economy;
using UnityEngine;

public class ChestDetection : SingletonBasic<ChestDetection>, IClickable
{
    public void click() => StatUtil.Instance.AddGold(SlotID.Cash);
}
