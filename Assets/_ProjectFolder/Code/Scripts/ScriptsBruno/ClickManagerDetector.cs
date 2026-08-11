using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


public class ClickManagerDetector : MonoBehaviour // script general para detectar todos los objs interactuables
{    
    InputAction tap;
    TapTest control;
    private void Awake()
    {
        control = new();
        tap = control.FindAction("Tap");
        tap.performed += DetectInput;
    }
    private void OnEnable() => control.Enable();
    private void OnDisable()
    {
        tap.performed -= DetectInput;
        control.Disable();
    }
    private void DetectInput(InputAction.CallbackContext ctx) // ahora si funciona :)
    {        
        PointerEventData pointerData = new PointerEventData(EventSystem.current);

        Vector2 position = Vector2.zero;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            position = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else 
        {
            position = Mouse.current.position.ReadValue();
        }

        pointerData.position = position;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results) 
        {
            IClickable clickable = result.gameObject.transform.GetComponent<IClickable>();
            if (clickable != null)
            {
                clickable.click();
                print("Esta entrando!");
            }
            else 
            {
                print("No hay nada que clickear");
            }
        }
        print("al menos hace algo");        
    }
}
