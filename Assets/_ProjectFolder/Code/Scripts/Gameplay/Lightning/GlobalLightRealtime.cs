using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace Gameplay.Environment.Lightning
{
    [RequireComponent(typeof(Light2D))]
    public class GlobalLightRealtime : MonoBehaviour
    {
        [SerializeField] private Light2D light2D;
        [SerializeField] private Gradient ambientColor;
        [SerializeField] private AnimationCurve intensityThreshold;

        [SerializeField, Range(0f, 24f)] private float currentTime = 0f;
        [SerializeField] private UnityEvent<Color> onValueChanged;
        
        private const float MaxDayTime = 24f;

        private static readonly TaskLoopAsync TaskLoop = new(-1f, 3600f);
        
        private void Awake() => light2D ??= GetComponent<Light2D>();
        private void Reset() => light2D = GetComponent<Light2D>();
        private void OnValidate() => SetLightIntensity();
        
        private void Start()
        {
            this.LoopTask(TaskLoop, OnUpdate);
            OnUpdate();
        }
        private void OnUpdate()
        {
            currentTime = DateTime.Now.Hour;
            SetLightIntensity();
        }

        private void SetLightIntensity()
        {
            if (!light2D) return;
            var time = currentTime / MaxDayTime;
            
            light2D.color = ambientColor.Evaluate(time);
            light2D.intensity = intensityThreshold.Evaluate(time);
            onValueChanged.Invoke(light2D.color);
        }
    }
}