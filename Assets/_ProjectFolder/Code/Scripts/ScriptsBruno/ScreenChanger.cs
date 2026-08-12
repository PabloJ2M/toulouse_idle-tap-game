using UnityEngine;
using UnityEngine.UI;

public class ScreenChanger : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private Image buttonImage;
    [SerializeField] Color buttonSelected;
    [SerializeField] Color ButtonDeselected;

    [Header("Screen")]
    [SerializeField] private GameObject screen;    
    public void EnableScreen() => UIScreenChangerManager.Instance.EnableScreen(this);
    public void DeactiveScreen() 
    {
        buttonImage.color = ButtonDeselected;
        screen.SetActive(false);
    }
    public void ActiveScreen() 
    {
        buttonImage.color = buttonSelected;
        screen.SetActive(true);
    }
    
}
