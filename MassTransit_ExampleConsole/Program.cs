using MassTransit;
using Serilog;
using Serilog.Formatting.Json;
using System;
using Microsoft.Extensions.DependencyInjection;
using SettlementApiMiddleware.Core.ExchangeId;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MassTransit_ExampleConsole
{
    public interface IValueEntered
    {
        string Value { get; }        
    }

    class Program
    {
        static void Main(string[] args)
        {
            ConfigureLogger();
            var provider = ConfigureServices(new ServiceCollection());

            var busControl = provider.GetService<IHostedService>();

            var cancelToken = new System.Threading.CancellationToken();

            busControl.StartAsync(cancelToken);
            Log.Information("Bus started");

            var shouldRun = true;
            do
            {
                var key = Console.ReadKey();

                shouldRun = key.Key != ConsoleKey.Escape;
            }
            while (shouldRun);

            busControl.StopAsync(cancelToken);
        }

        private static ServiceProvider ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder().Build();
            services.AddTransient<IExchangeIdHelper>(s => new ExchangeIdHelper(s.GetService<IHttpContextAccessor>(), configuration));
            services.AddMassTransit(x =>
            {
                x.AddConsumer<ValueEnteredConsumer>();

                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    var host = cfg.Host("localhost", 5672, "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ReceiveEndpoint(host, "value_entered_queue", e =>
                    {
                        e.Consumer<ValueEnteredConsumer>();
                    });
                }));

            });

            services.AddSingleton<IHostedService, BusService>();

            return services.BuildServiceProvider();
        }
        private static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(new JsonFormatter(), "log.txt")
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Properties:j} {Message:lj}{NewLine}{Exception}")
                .Enrich.FromLogContext()
                .CreateLogger();

            Log.Information("The global logger has been configured.");
        }

        private static IBusControl ConfigureBus()
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host("localhost", 5672,"/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint(host, "value_entered_queue", e =>
                {
                    e.Consumer<ValueEnteredConsumer>();
                });

            });
        }
    }
}
