using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


public class ChestDetection : MonoBehaviour, IPointerDownHandler
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
        //tap.performed += DetectInput; 
    }
    private void OnEnable()
    {
        control.Enable();
    }
    private void OnDisable()
    {
        control.Disable();
    }
   
    //private void DetectInput(InputAction.CallbackContext ctx) // intento de hacerlo asi (no funciono :c)
    //{
    //    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

    //    if (Physics.Raycast(ray, out RaycastHit hit))
    //    {
    //        print("Entra aca?");
    //        if (hit.transform.CompareTag("Chest"))
    //        {
    //            giveResources.Invoke(true);
    //            print("si da");
    //        }
    //        else 
    //        {
    //            giveResources.Invoke(false);
    //            print("no da");
    //        }
    //    }
    //    print("Detecta algo");
    //}

    public void OnPointerDown(PointerEventData eventData)
    {
        giveResources.Invoke(true);
    }
}
