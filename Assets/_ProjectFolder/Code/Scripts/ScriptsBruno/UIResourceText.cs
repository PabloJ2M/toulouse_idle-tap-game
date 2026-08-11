using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIResourceText : MonoBehaviour
{
    [SerializeField] private ResourcesInventory inventory;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI diamondText;
    private void Start() => ResourcesInventory._instance.UIText += ShowText;
    private void ShowText(int amount, char type)
    {
        if (type == 'g')
           goldText.text = $"Gold: {amount}";        
        else if (type == 'd') 
           diamondText.text = $"Diamond: {amount}";        
    }
}
