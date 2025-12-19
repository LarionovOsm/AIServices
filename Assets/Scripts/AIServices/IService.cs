using Cysharp.Threading.Tasks;

public interface IService
{
    UniTask StartServiceAsync();
    UniTask StopServiceAsync();
    bool IsReady();
}