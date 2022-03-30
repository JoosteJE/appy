using ExchangeRateApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExchangeRateApi;

public class Startup
{
    public IConfiguration Configuration { get; }
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        //In PROD I would probably rather use a database for persisting exchange values fetched.
        //I might still use this in combination with the MemoryCache if the API gets heavy traffic so I limit calls to the database and speed up returns
        services.AddMemoryCache();
        services.AddControllers()
            .AddNewtonsoftJson();

        services.AddScoped<IExchangeRateService, ExchangeRateService>();
    }

    public void Configure(IApplicationBuilder applicationBuilder, IWebHostEnvironment environment)
    {
        applicationBuilder.UseRouting();
        applicationBuilder.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}