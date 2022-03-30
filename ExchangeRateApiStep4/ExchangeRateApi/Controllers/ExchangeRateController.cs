using ExchangeRateApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ExchangeRateApi.Controllers;

[Route("exchange-rate")]
[ApiController]
public class ExchangeRateController
{
    private readonly IMemoryCache _memoryCache;
    private readonly IExchangeRateService _exchangeRateService;
    private MemoryCacheEntryOptions _cacheOptions;
    public ExchangeRateController(IMemoryCache memoryCache, IExchangeRateService exchangeRateService)
    {
        _memoryCache = memoryCache;
        _exchangeRateService = exchangeRateService;
        // Cache expiry times can be toggled to more desirable values. I set the sliding expiration to 10 seconds to make sure the API is never called more than 6 times a minute.
        // You can likely set that to 6 seconds if the API allows for 10 calls per minute.
        // I also set an absolute expiration to make sure that even if the API gets called constantly the cache will refresh the value at least once a minute
        _cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
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

        try
        {
            //I just moved the more involved business logic to the service layer. I personally prefer to keep BL out of the controller as much as possible.
            //In prod I would also add a database to persist the values and add a repository layer to access that data.
            //I would then add a background task to fetch data for all the currency pairs at set intervals and save that to the DB.
            //That way I need less calls to the 3rd party API and I can extend my service to provide historic data.
            var returnDto = await _exchangeRateService.GetSwingDevInstituteResult(from, to);
            _memoryCache.Set(cacheKey, returnDto, _cacheOptions);
            return new OkObjectResult(returnDto);
        }
        catch (Exception e)
        {
            if (from != "USD")
            {
                return new BadRequestResult();
            }

            var returnDto = await _exchangeRateService.GetSwingExchangeCentralResult(to, 2);
            _memoryCache.Set(cacheKey, returnDto, _cacheOptions);
            return new OkObjectResult(returnDto);
        }
    }
}