using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField, Min(1f)] private float speedPerTap;
    [SerializeField, Min(0f)] private float decreasedSpeed = 1;
    [SerializeField] private AnimationCurve speedCurve;
    [Space]
    [SerializeField] private UnityEvent<float> onSpeedUpdate;
    
    private float _globalSpeed;

    private void Start() => UpdateSpeed(); 
    private void FixedUpdate()
    {
        if (_globalSpeed == 0) return;
        
        DecreaseSpeed(decreasedSpeed * Time.fixedDeltaTime);
        UpdateSpeed();
    }

    private void UpdateSpeed() => onSpeedUpdate.Invoke(speedCurve.Evaluate(_globalSpeed));
    private void DecreaseSpeed(float delta) => _globalSpeed = Mathf.MoveTowards(_globalSpeed, 0, delta);
    
    public void Tap()
    {
        _globalSpeed += speedPerTap * Time.fixedDeltaTime;
        _globalSpeed = Mathf.Clamp01(_globalSpeed);
    }
}