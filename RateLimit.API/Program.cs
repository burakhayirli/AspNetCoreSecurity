using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RateLimit.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHost = CreateHostBuilder(args).Build();

            //Ip Rate Limit
            //var ipPolicy = webHost.Services.GetRequiredService<IIpPolicyStore>();
            
            //ClientId Rate Limit
            var ipPolicy = webHost.Services.GetRequiredService<IClientPolicyStore>();

            ipPolicy.SeedAsync().Wait();

            webHost.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
