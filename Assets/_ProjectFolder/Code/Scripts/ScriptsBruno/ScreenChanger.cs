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
    [HideInInspector] public GameObject GetScreen { private set; get; }
    private void Start() => GetScreen = screen;
    
    public void EnableScreen() => UIScreenChangerManager.Instance.EnableScreen(screen);
    public void DeactiveScreen() 
    {
        buttonImage.color = ButtonDeselected;
        screen.SetActive(false);
    }
    
}
