using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class ServiceBase : MonoBehaviour, IService
{
    public abstract bool IsReady();
    public abstract UniTask StartServiceAsync();
    public abstract UniTask StopServiceAsync();
}