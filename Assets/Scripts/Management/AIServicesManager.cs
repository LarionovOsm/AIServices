using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AIServicesManager : MonoBehaviour
{
    [SerializeField] private List<ServiceBase> _services = new List<ServiceBase>();
    
    public STTService STTService => _services.OfType<STTService>().FirstOrDefault();
    public TTSService TTSService => _services.OfType<TTSService>().FirstOrDefault();
    public event Action OnServicesStarted;
    
    public async UniTask StartServicesAsync()
    {
        foreach (var service in _services)
        {
            Debug.Log($"Starting {service.GetType().Name}...");
            await service.StartServiceAsync();
            
            // Подтверждение готовности сервиса
            if (service.IsReady())
                Debug.Log($"{service.GetType().Name} is ready!");
        }

        Debug.Log("All services have been started.");
        OnServicesStarted?.Invoke();
    }
    
    public async UniTask StopAllServicesAsync()
    {
        foreach (var service in _services)
        {
            await service.StopServiceAsync();
        }
        Debug.Log("All services stopped.");
    }
}