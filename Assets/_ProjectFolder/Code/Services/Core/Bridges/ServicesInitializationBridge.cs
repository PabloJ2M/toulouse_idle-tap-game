using UnityEngine;

namespace Unity.Services.Core
{
    using Components;
    
    [RequireComponent(typeof(ServicesInitialization))]
    [AddComponentMenu("Services/Services Initialization Bridge")]
    public class ServicesInitializationBridge : Singleton<ServicesInitializationBridge>
    {
        private ServicesInitialization _initialization;
        private IServiceModule[] _services;

        protected override void Awake()
        {
            base.Awake();
            _initialization = GetComponent<ServicesInitialization>();
            _services = GetComponentsInChildren<IServiceModule>();
        }

        private void OnEnable() => _initialization.Events.Initialized.AddListener(InitializeModules);
        private void OnDisable() => _initialization.Events.Initialized.RemoveListener(InitializeModules);

        private void InitializeModules()
        {
            ServicesStatus.IsInitialized = true;
            
            foreach (var service in _services)
                service.OnInitialized();
        }
    }
}