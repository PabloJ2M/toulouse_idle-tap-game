using UnityEngine;

public class UIScreenChangerManager : SingletonBasic<UIScreenChangerManager>
{
    [SerializeField] ScreenChanger[] screens;
    public void EnableScreen(GameObject screen) => UpdateScreen(screen);    
    private void UpdateScreen(GameObject newScreen) 
    {
        foreach (var screen in screens)
            screen.GetScreen.SetActive(false);

        newScreen.SetActive(true);
    }
}
