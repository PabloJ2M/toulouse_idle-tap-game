using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


public class ChestDetection : MonoBehaviour
{
    public event Action<bool> giveResources;
    public static ChestDetection instance;
    InputAction tap;

    TapTest control;
    private void Awake()
    {
        control = new();
        instance = this;
        tap = control.FindAction("Tap");
        tap.performed += DetectInput; 
    }
    private void OnEnable()
    {
        control.Enable();
    }
    private void OnDisable()
    {
        control.Disable();
    }
    private void DetectInput(InputAction.CallbackContext ctx) 
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Chest"))
            {
                giveResources.Invoke(true);
            }
            else 
            {
                giveResources.Invoke(false);            
            }
        }        
    } 
}
