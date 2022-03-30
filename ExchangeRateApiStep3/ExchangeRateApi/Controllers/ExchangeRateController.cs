using System.Text.Json;
using ExchangeRateApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ExchangeRateApi.Controllers;

[Route("exchange-rate")]
[ApiController]
public class ExchangeRateController
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    public ExchangeRateController(IMemoryCache memoryCache)
    {
        _httpClient = new HttpClient();
        _memoryCache = memoryCache;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery(Name = "from")]string from, [FromQuery(Name = "to")]string to)
    {
        //I added a cacheKey of the currency pairs to get any previously saved currency check in memory if one is available rather than calling the API
        var cacheKey = $"{from}-{to}";
        var cached = _memoryCache.TryGetValue(cacheKey, out ReturnDTO cachedResult);
        if (cached)
        {
            return new OkObjectResult(cachedResult);
        }

        // Cache expiry times can be toggled to more desirable values. I set the sliding expiration to 10 seconds to make sure the API is never called more than 6 times a minute.
        // You can likely set that to 6 seconds if the API allows for 10 calls per minute.
        // I also set an absolute expiration to make sure that even if the API gets called constantly the cache will refresh the value at least once a minute
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
        
        try
        {
            var returnDto = await GetSwingDevInstituteResult(from, to);
            _memoryCache.Set(cacheKey, returnDto, cacheOptions);
            return new OkObjectResult(returnDto);
        }
        catch (Exception e)
        {
            if (from != "USD")
            {
                return new BadRequestResult();
            }

            var returnDto = await GetSwingExchangeCentralResult(to, 2);
            _memoryCache.Set(cacheKey, returnDto, cacheOptions);
            return new OkObjectResult(returnDto);
        }
    }

    private async Task<ReturnDTO?> GetSwingExchangeCentralResult(string target, int apiVersion = 1)
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