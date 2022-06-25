using Newtonsoft.Json;

namespace SignalRTestDotNet.DOAs;

public record PlayerDAO
{
    [JsonProperty("country")]
    public string Country { get; init; }

    [JsonProperty("url")]
    public string Url { get; init; }

    [JsonProperty("sessionId")]
    public string SessionId { get; init; }

    [JsonProperty("playerId")]
    public string PlayerId { get; init; }
}

