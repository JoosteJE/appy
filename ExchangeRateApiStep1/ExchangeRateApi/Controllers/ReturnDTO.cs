namespace ExchangeRateApi.Controllers;

public class ReturnDTO
{
    public string From { get; set; }
    public string To { get; set; }
    public int Timestamp { get; set; }
    public decimal Rate { get; set; }
}