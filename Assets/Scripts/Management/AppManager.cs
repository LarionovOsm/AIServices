using Cysharp.Threading.Tasks;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    public static AppManager instance = null;

    [SerializeField] private AIServicesManager _aiServicesManager;
   
    public AIServicesManager AIServicesManager => _aiServicesManager;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance == this)
        {
            Destroy(gameObject);
        }


        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
    }

    private void Start()
    {
        ResetAsync().Forget();
    }
    
    private void OnApplicationQuit()
    {
        Debug.Log("Application quitting, stopping services...");
        UniTask.Void(async () => await _aiServicesManager.StopAllServicesAsync());
    }

#if UNITY_EDITOR
    private void OnDisable()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("Exiting PlayMode in Editor, stopping services...");
            UniTask.Void(async () => await _aiServicesManager.StopAllServicesAsync());
        }
    }
#endif

    public async UniTask ResetAsync()
    {
        if (_aiServicesManager != null)
        {
            await _aiServicesManager.StartServicesAsync();
            Debug.Log("AI Services are ready!");
        }
    }
}
