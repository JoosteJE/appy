using System.Text.Json;
using ExchangeRateApi.Controllers;
using ExchangeRateApi.DTOs;

namespace ExchangeRateApi.Services;

public interface IExchangeRateService
{
    Task<ReturnDTO?> GetSwingExchangeCentralResult(string target, int apiVersion = 1);
    Task<ReturnDTO?> GetSwingDevInstituteResult(string from, string to);
}

public class ExchangeRateService: IExchangeRateService
{
    private readonly HttpClient _httpClient;
    public ExchangeRateService()
    {
        _httpClient = new HttpClient();
        
    }
    public async Task<ReturnDTO?> GetSwingExchangeCentralResult(string target, int apiVersion = 1)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("X-APIKEY", "SWING");
        var resultStream = await _httpClient.GetStreamAsync($"https://central-bank.sandbox.swing.dev/exchange/v{apiVersion}");
        var result = await JsonSerializer.DeserializeAsync<SwingDevCentralDTO>(resultStream);
        if (result == null)
        {
            return null;
        }

        BidDTO bid;
        switch (target.ToLower())
        {
            case "eur":
                bid = result.EUR;
                break;
            case "pln":
                bid = result.PLN;
                break;
            case "swd":
                bid = result.SWD;
                break;
            default:
                throw new NotSupportedException();
        }
        return new ReturnDTO
        {
            From = "USD",
            To = target,
            Rate = bid.Ask,
            Timestamp = result.Time,
        };
    }

    public async Task<ReturnDTO?> GetSwingDevInstituteResult(string from, string to)
    {
        var resultStream = await _httpClient.GetStreamAsync($"https://federal-institute.sandbox.swing.dev/rates?base={from}&target={to}");
        var result = await JsonSerializer.DeserializeAsync<SwingDevInstituteDTO>(resultStream);
        if (result == null)
        {
            return null;
        }
        return new ReturnDTO
        {
            From = result.Base,
            To = result.Target,
            Rate = result.Rate,
            Timestamp = result.Timestamp,
        };
    }
}