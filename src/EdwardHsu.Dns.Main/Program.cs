using EdwardHsu.Dns.RedisStore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;

namespace EdwardHsu.Dns.Main
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration((builder) =>
                {
                    builder.AddEnvironmentVariables("EHDNS_");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.Configure<RedisStoreOptions>(option =>
                    {
                        option.ConnectionString = hostContext.Configuration.GetValue<string>("RedisConnectionString");
                        option.DBNumber = hostContext.Configuration.GetValue<int>("RedisDBNumber");
                    });
                    services.AddSingleton<IStoreProvider, RedisStoreProvider>();
                    services.AddHostedService<DnsWorker>();
                });
    }
}
