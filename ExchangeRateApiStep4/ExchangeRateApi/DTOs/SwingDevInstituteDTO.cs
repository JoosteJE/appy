using System.Text.Json.Serialization;

namespace ExchangeRateApi.DTOs;

public class SwingDevInstituteDTO
{
    [JsonPropertyName("base")]
    public string Base { get; set; }
    [JsonPropertyName("target")]
    public string Target { get; set; }
    [JsonPropertyName("timestamp")]
    public int Timestamp { get; set; }
    [JsonPropertyName("rate")]
    public decimal Rate { get; set; }
}