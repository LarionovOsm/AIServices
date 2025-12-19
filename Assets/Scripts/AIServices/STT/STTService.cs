using Cysharp.Threading.Tasks;
using UnityEngine;
using Whisper;
using Whisper.Utils;
using System;
using System.Text.RegularExpressions;

public class STTService : ServiceBase
{
    [Header("Dependencies")]
    [SerializeField] private WhisperManager _whisperManager;
    [SerializeField] private MicrophoneRecord _microphoneRecord;
    
    private bool _isReady = false;

    public override bool IsReady() => _isReady;
    public MicrophoneRecord MicrophoneRecord => _microphoneRecord;

    /// <summary>
    /// Событие для стриминга сегментов текста
    /// </summary>
    public event Action<string> OnTranscription;
    public event Action<string> OnRecordEnd;

    #region ServiceControl
    public override async UniTask StartServiceAsync()
    {
        await _whisperManager.InitModel();
        if (!_whisperManager.IsLoaded)
        {
            Debug.LogError("STTService: Whisper model failed to load.");
            return;
        }

        // Подписываемся на события
        _whisperManager.OnNewSegment += OnNewSegment;
        _whisperManager.OnProgress += OnProgressHandler;
        _microphoneRecord.OnRecordStop += OnRecordStop;
        _microphoneRecord.OnChunkReady += OnChunkReady;
        _microphoneRecord.OnVadChanged += OnVadChanged;

        AppManager.instance.AIServicesManager.OnServicesStarted += StartMicRecord;
        
        _isReady = true;
    }

    public override async UniTask StopServiceAsync()
    {
        StopMicRecord();

        // Отписываемся от событий
        _whisperManager.OnNewSegment -= OnNewSegment;
        _whisperManager.OnProgress -= OnProgressHandler;
        _microphoneRecord.OnRecordStop -= OnRecordStop;
        _microphoneRecord.OnChunkReady -= OnChunkReady;
        _microphoneRecord.OnVadChanged -= OnVadChanged;

        _isReady = false;
        await UniTask.Yield();
    }
    #endregion

    #region MicrophoneControl
    public void StartMicRecord()
    {
        if (!_microphoneRecord.IsRecording)
            _microphoneRecord.StartRecord();
    }

    public void StopMicRecord()
    {
        if (_microphoneRecord.IsRecording)
            _microphoneRecord.StopRecord();
    }
    #endregion

    #region Handlers
    private async void OnRecordStop(AudioChunk recordedAudio)
    {
        // Полная транскрипция после окончания записи
        var result = await _whisperManager.GetTextAsync(
            recordedAudio.Data,
            recordedAudio.Frequency,
            recordedAudio.Channels
        );

        if (result != null)
        {
            string cleanedText = CleanTranscription(result.Result);
            if (!String.IsNullOrEmpty(cleanedText))
            {
                OnRecordEnd?.Invoke(cleanedText);
            }
            else _microphoneRecord.StartRecord();
        }
    }

    private void OnChunkReady(AudioChunk chunk)
    {
        // Можно добавить поддержку стриминга через WhisperStream в будущем
    }

    private void OnVadChanged(bool isSpeechDetected)
    {
        Debug.Log($"[STTService] VAD: {(isSpeechDetected ? "speech" : "silence")}");
    }

    private void OnNewSegment(WhisperSegment segment)
    {
        // Вызываем событие для стриминга текста по сегментам
        OnTranscription?.Invoke(segment.Text);
    }

    private void OnProgressHandler(int progress)
    {
        // Можно логировать прогресс, если нужно
        Debug.Log($"[STTService] Whisper progress: {progress}%");
    }
    #endregion
    
    #region Utils
    string CleanTranscription(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Удаляем всё внутри [], (), {}, и *...*
        string pattern = @"\[[^\]]*\]|\([^)]*\)|\{[^}]*\}|\*[^*]*\*";
        string result = Regex.Replace(input, pattern, "");

        // Убираем лишние пробелы
        result = Regex.Replace(result, @"\s{2,}", " ").Trim();

        return result;
    }
    #endregion
}
