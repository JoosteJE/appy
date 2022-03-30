using System.Text.Json.Serialization;

namespace ExchangeRateApi.DTOs;

public class SwingDevCentralDTO
{
    [JsonPropertyName("PLN")]
    public BidDTO PLN { get; set; }
    [JsonPropertyName("SWD")]
    public BidDTO SWD { get; set; }
    [JsonPropertyName("EUR")]
    public BidDTO EUR { get; set; }
    [JsonPropertyName("time")]
    public int Time { get; set; }
}

public class BidDTO
{
    [JsonPropertyName("bid")]
    public decimal Bid { get; set; }
    [JsonPropertyName("ask")]
    public decimal Ask { get; set; }
}