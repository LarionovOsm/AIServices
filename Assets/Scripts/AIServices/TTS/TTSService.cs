using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class TTSService : ServiceBase
{
    [SerializeField]
    [Tooltip("Преобразование и воспроизведение синтезированной речи")]
    private TTSSpeaker _ttsSpeaker;
    
    [SerializeField]
    [Tooltip("Голос для синтеза речи")]
    private string _voice;

    [SerializeField]
    [Range(0.1f, 3.0f)]
    [Tooltip("Скорость воспроизведения речи (игнорируется TTS моделью, используется для аудиоплеера)")]
    private float _speed = 1.0f;

    [SerializeField]
    [Range(0.25f, 2.0f)]
    [Tooltip("Интенсивность эмоций в голосе")]
    private float _exaggeration = 0.25f;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("Контроль темпа и чёткости речи")]
    private float _cfgWeight = 1.0f;

    [SerializeField]
    [Range(0.05f, 5.0f)]
    [Tooltip("Вариативность генерации речи")]
    private float _temperature = 0.05f;

    private Process _serverProcess;
    private string _serverUrl = "http://127.0.0.1:4123";
    private string _directoryPath = "AIServices/TTS";
    private int _port = 4123;
    private bool _isReady = false;
    private bool _enableLogs = false;

    public override bool IsReady() => _isReady;
    public TTSSpeaker TTSSpeaker => _ttsSpeaker;

    #region ServiceControl

    public override async UniTask StartServiceAsync()
    {
        // Освобождаем порт перед запуском
        await PortUtils.FreePortAsync(_port);

        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(_directoryPath, "Python/venv/Scripts/python.exe"),
            Arguments = "-m uvicorn app.main:app --host 127.0.0.1 --port 4123",
            WorkingDirectory = _directoryPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        startInfo.Environment["PYTHONUNBUFFERED"] = "1";
        startInfo.Environment["PYTHONIOENCODING"] = "utf-8";

        _serverProcess = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        _serverProcess.OutputDataReceived += (sender, args) =>
        {
            if (string.IsNullOrEmpty(args.Data)) return;

            if (_enableLogs) UnityEngine.Debug.Log("[TTS LOG] " + args.Data);

            if (args.Data.Contains("Model initialized successfully") && !_isReady) _isReady = true;
        };

        _serverProcess.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data) && _enableLogs) UnityEngine.Debug.LogError("[TTS ERR] " + args.Data);
        };

        _serverProcess.Start();
        _serverProcess.BeginOutputReadLine();
        _serverProcess.BeginErrorReadLine();

        while (!_isReady)
        {
            await UniTask.Yield();
        }
    }

    public override async UniTask StopServiceAsync()
    {
        try
        {
            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                _serverProcess.Kill();
                _serverProcess.Dispose();
                _serverProcess = null;

                if (_enableLogs)
                    UnityEngine.Debug.Log("TTSService stopped.");
            }

            await PortUtils.FreePortAsync(_port);
        }
        catch (Exception e)
        {
            if (_enableLogs)
                UnityEngine.Debug.LogError("[TTS ERR] " + e.Message);
        }
        finally
        {
            _isReady = false;
        }
    }

    #endregion

    #region TTS API

    public async UniTask<AudioClip> GenerateAudioAsync(string text)
    {
        if (!_isReady)
            throw new InvalidOperationException("TTSService not ready");

        var requestData = new
        {
            input = text,
            voice = _voice,
            response_format = "wav",
            exaggeration = _exaggeration,
            cfg_weight = _cfgWeight,
            temperature = _temperature,
            streaming_strategy = "paragraph",
            streaming_buffer_size = 1,
            streaming_quality = "balanced"
        };

        string json = JsonConvert.SerializeObject(requestData);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using var request =
            new UnityWebRequest(_serverUrl + "/audio/speech", "POST");

        request.uploadHandler = new UploadHandlerRaw(body);

        request.downloadHandler =
            new DownloadHandlerAudioClip(
                _serverUrl + "/audio/speech",
                AudioType.WAV);

        request.SetRequestHeader("Content-Type", "application/json");

        await request.SendWebRequest().ToUniTask();

        if (request.result != UnityWebRequest.Result.Success)
        {
            UnityEngine.Debug.LogError($"[TTS] Request error: {request.error}");
            return null;
        }

        return DownloadHandlerAudioClip.GetContent(request);
    }
    
    // Новый метод ТОЛЬКО для сохранения файла
public async UniTask<bool> GenerateAndSaveAudioAsync(string text, string fileName = null)
{
    if (!_isReady)
        throw new InvalidOperationException("TTSService not ready");

    var requestData = new
    {
        input = text,
        voice = _voice,
        response_format = "wav",
        exaggeration = _exaggeration,
        cfg_weight = _cfgWeight,
        temperature = _temperature,
        streaming_strategy = "paragraph",
        streaming_buffer_size = 1,
        streaming_quality = "balanced"
    };

    string json = JsonConvert.SerializeObject(requestData);
    byte[] body = Encoding.UTF8.GetBytes(json);

    using var request =
        new UnityWebRequest(_serverUrl + "/audio/speech", "POST");

    request.uploadHandler = new UploadHandlerRaw(body);
    
    // Используем DownloadHandlerBuffer для получения сырых байтов
    DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer();
    request.downloadHandler = downloadHandler;
    
    request.SetRequestHeader("Content-Type", "application/json");

    await request.SendWebRequest().ToUniTask();

    if (request.result != UnityWebRequest.Result.Success)
    {
        UnityEngine.Debug.LogError($"[TTS] Save request error: {request.error}");
        return false;
    }

    // Получаем байты WAV файла
    byte[] wavData = downloadHandler.data;
    
    if (wavData == null || wavData.Length == 0)
    {
        UnityEngine.Debug.LogError("[TTS] Получены пустые данные WAV");
        return false;
    }

    // Сохраняем в файл
    return SaveWavFile(wavData, fileName ?? $"tts_{DateTime.Now:yyyyMMdd_HHmmss}");
}

private bool SaveWavFile(byte[] wavData, string fileName)
{
    try
    {
        // Создаем папку для сохранения
        string savePath = Path.Combine(Application.persistentDataPath, "TTSAudio");
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        
        // Добавляем расширение .wav если его нет
        if (!fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".wav";
        }
        
        string filePath = Path.Combine(savePath, fileName);
        
        // Сохраняем WAV файл
        File.WriteAllBytes(filePath, wavData);
        
        UnityEngine.Debug.Log($"[TTS] WAV файл сохранен: {filePath} (Размер: {wavData.Length} байт)");
        
        #if UNITY_EDITOR
        // В редакторе можно сразу открыть папку
        UnityEditor.EditorUtility.RevealInFinder(savePath);
        #endif
        
        return true;
    }
    catch (Exception e)
    {
        UnityEngine.Debug.LogError($"[TTS] Ошибка сохранения WAV файла: {e.Message}");
        return false;
    }
}
    #endregion
}
