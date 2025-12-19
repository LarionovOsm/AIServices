using System;
using Newtonsoft.Json;

[Serializable]
public class LongTextJobResponse
{
    private string _jobId;
    private string _status;
    private string _message;
    private int _estimatedProcessingTimeSeconds;
    private int _totalChunks;
    private string _statusUrl;
    private string _sseUrl;

    [JsonProperty("job_id")]
    public string JobId { get => _jobId; set => _jobId = value; }

    [JsonProperty("status")]
    public string Status { get => _status; set => _status = value; }

    [JsonProperty("message")]
    public string Message { get => _message; set => _message = value; }

    [JsonProperty("estimated_processing_time_seconds")]
    public int EstimatedProcessingTimeSeconds { get => _estimatedProcessingTimeSeconds; set => _estimatedProcessingTimeSeconds = value; }

    [JsonProperty("total_chunks")]
    public int TotalChunks { get => _totalChunks; set => _totalChunks = value; }

    [JsonProperty("status_url")]
    public string StatusUrl { get => _statusUrl; set => _statusUrl = value; }

    [JsonProperty("sse_url")]
    public string SseUrl { get => _sseUrl; set => _sseUrl = value; }
}
