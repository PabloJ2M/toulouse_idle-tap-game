using UnityEngine;

public class UIScreenChangerManager : SingletonBasic<UIScreenChangerManager>
{
    [SerializeField] ScreenChanger[] screens;
    private void Start() => EnableScreen(screens[0]);
    public void EnableScreen(ScreenChanger screen) => UpdateScreen(screen);    
    private void UpdateScreen(ScreenChanger newScreen) 
    {
        foreach (var screen in screens)
            screen.DeactiveScreen();

        newScreen.ActiveScreen();
    }
}
