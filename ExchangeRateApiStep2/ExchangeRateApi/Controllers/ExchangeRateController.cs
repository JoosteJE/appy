using System.Text.Json;
using ExchangeRateApi.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeRateApi.Controllers;

[Route("exchange-rate")]
[ApiController]
public class ExchangeRateController
{
    private readonly HttpClient _httpClient;
    public ExchangeRateController()
    {
        _httpClient = new HttpClient();
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery(Name = "from")]string from, [FromQuery(Name = "to")]string to)
    {
        try
        {
            var returnDto = await GetSwingDevInstituteResult(from, to);
            return new OkObjectResult(returnDto);
        }
        catch (Exception e)
        {
            if (from != "USD")
            {
                return new BadRequestResult();
            }

            var returnDto = await GetSwingExchangeCentralResult(to);
            return new OkObjectResult(returnDto);
        }
    }

    private async Task<ReturnDTO?> GetSwingExchangeCentralResult(string target)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("X-APIKEY", "SWING");
        var resultStream = await _httpClient.GetStreamAsync($"https://central-bank.sandbox.swing.dev/exchange/v1");
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

    private async Task<ReturnDTO?> GetSwingDevInstituteResult(string from, string to)
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