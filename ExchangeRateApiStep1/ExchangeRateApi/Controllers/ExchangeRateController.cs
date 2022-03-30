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
            var resultStream = await _httpClient.GetStreamAsync($"https://federal-institute.sandbox.swing.dev/rates?base={from}&target={to}");
            var result = await JsonSerializer.DeserializeAsync<SwingDevInstituteDTO>(resultStream);
            if (result == null)
            {
                return new BadRequestResult();
            }
            var returnDto = new ReturnDTO
            {
                From = result.Base,
                To = result.Target,
                Rate = result.Rate,
                Timestamp = result.Timestamp,
            };
            return new OkObjectResult(returnDto);
        }
        catch (Exception e)
        {
            return new BadRequestResult();
        }
        
    }
}