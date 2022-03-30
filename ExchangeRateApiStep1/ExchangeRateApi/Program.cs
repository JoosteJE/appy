// See https://aka.ms/new-console-template for more information
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ExchangeRateApi
{

    public class Program
    {
        public static void Main(string[] arguments)
        {
            BuildWebHost(arguments).Run();
        }

        public static IWebHost BuildWebHost(string[] arguments) => WebHost.CreateDefaultBuilder(arguments)
            .UseConfiguration(new ConfigurationBuilder()
                .Build())
            .UseUrls("http://localhost:8080")
            .UseStartup<Startup>()
            .Build();
    }
}