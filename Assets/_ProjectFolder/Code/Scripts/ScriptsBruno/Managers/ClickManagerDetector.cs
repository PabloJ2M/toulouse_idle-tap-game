using UnityEngine;
using UnityEngine.InputSystem;
public class ClickManagerDetector : MonoBehaviour // script general para detectar todos los objs interactuables
{    
    InputAction tap;
    InputAction test;
    TapTest control;
    private void Awake()
    {
        control = new();
        tap = control.FindAction("Tap");
        test = control.FindAction("testAction");
        test.performed += TestInput;
        tap.performed += DetectInput;
    }
    private void OnEnable() => control.Enable();
    private void OnDisable()
    {
        tap.performed -= DetectInput;
        test.performed -= TestInput;
        control.Disable();
    }
    private void TestInput(InputAction.CallbackContext ctx) => SlotUpgradeManager.Instance.ClearUpgrades();    
    private void DetectInput(InputAction.CallbackContext ctx) // ahora si funciona :)
    {        
        Vector2 position = Vector2.zero;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            position = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else 
        {
            position = Mouse.current.position.ReadValue();
        }

        var obj = InputUtils.GetTopUIElement(position);
        IClickable clickable = null;

        if (obj != null) clickable = obj.GetComponent<IClickable>();
        else return;

        clickable?.click();
    }
}
