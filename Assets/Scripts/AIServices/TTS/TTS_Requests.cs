using System;
using Newtonsoft.Json;

[Serializable]
public abstract class TtsRequestBase
{
    // Приватные поля
    private string _input;
    private string _voice;
    private string _responseFormat = "wav";
    private float _speed = 1.0f;
    private string _streamFormat = "audio";
    private float _exaggeration = 0.25f;
    private float _cfgWeight = 1.0f;
    private float _temperature = 0.05f;

    // Свойства с правильными именами для JSON
    [JsonProperty("input")]
    public string Input
    {
        get => _input;
        set => _input = value;
    }

    [JsonProperty("voice")]
    public string Voice
    {
        get => _voice;
        set => _voice = value;
    }

    [JsonProperty("response_format")]
    public string ResponseFormat
    {
        get => _responseFormat;
        set => _responseFormat = value;
    }

    [JsonProperty("speed")]
    public float Speed
    {
        get => _speed;
        set => _speed = value;
    }

    [JsonProperty("stream_format")]
    public string StreamFormat
    {
        get => _streamFormat;
        set => _streamFormat = value;
    }

    [JsonProperty("exaggeration")]
    public float Exaggeration
    {
        get => _exaggeration;
        set => _exaggeration = Math.Clamp(value, 0.25f, 2.0f);
    }

    [JsonProperty("cfg_weight")]
    public float CfgWeight
    {
        get => _cfgWeight;
        set => _cfgWeight = Math.Clamp(value, 0.0f, 1.0f);
    }

    [JsonProperty("temperature")]
    public float Temperature
    {
        get => _temperature;
        set => _temperature = Math.Clamp(value, 0.05f, 5.0f);
    }

    protected TtsRequestBase(string input, string voice = null)
    {
        _input = input;
        _voice = voice;
    }
}

[Serializable]
public class TtsRequest : TtsRequestBase
{
    private int _streamingChunkSize = 50;
    private string _streamingStrategy = null;
    private int _streamingBufferSize = 1;
    private string _streamingQuality = null;

    [JsonProperty("streaming_chunk_size")]
    public int StreamingChunkSize
    {
        get => _streamingChunkSize;
        set => _streamingChunkSize = Math.Clamp(value, 50, 500);
    }

    [JsonProperty("streaming_strategy")]
    public string StreamingStrategy
    {
        get => _streamingStrategy;
        set => _streamingStrategy = value;
    }

    [JsonProperty("streaming_buffer_size")]
    public int StreamingBufferSize
    {
        get => _streamingBufferSize;
        set => _streamingBufferSize = Math.Max(1, value);
    }

    [JsonProperty("streaming_quality")]
    public string StreamingQuality
    {
        get => _streamingQuality;
        set => _streamingQuality = value;
    }

    public TtsRequest(string input, string voice = "alloy")
        : base(input, voice) { }
}

[Serializable]
public class TtsUploadRequest : TtsRequestBase
{
    private int _streamingChunkSize = 100;
    private string _streamingStrategy = "sentence";
    private string _streamingQuality = "balanced";
    private string _voiceFilePath;

    [JsonProperty("streaming_chunk_size")]
    public int StreamingChunkSize
    {
        get => _streamingChunkSize;
        set => _streamingChunkSize = Math.Clamp(value, 50, 500);
    }

    [JsonProperty("streaming_strategy")]
    public string StreamingStrategy
    {
        get => _streamingStrategy;
        set => _streamingStrategy = value;
    }

    [JsonProperty("streaming_quality")]
    public string StreamingQuality
    {
        get => _streamingQuality;
        set => _streamingQuality = value;
    }

    // поле для файла не сериализуем в JSON, это используется только для multipart
    public string VoiceFilePath
    {
        get => _voiceFilePath;
        set => _voiceFilePath = value;
    }

    public TtsUploadRequest(string input, string voiceFilePath = null, string voice = null)
        : base(input, voice)
    {
        _voiceFilePath = voiceFilePath;
    }
}

[Serializable]
public class TtsLongTextRequest : TtsRequestBase
{
    private string _sessionID;
    [JsonProperty("session_id")] public string SessionId { get { return _sessionID;} set {_sessionID = value; } }

    public TtsLongTextRequest(string input, string voice = null, string sessionId = null)
        : base(input, voice)
    {
        SessionId = sessionId ?? Guid.NewGuid().ToString();
    }
}


