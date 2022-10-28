using Coravel;
using MessagingWorkerService.Schedules;
using MessagingWorkerService.Services;
using MessagingWorkerService.Tasks;
using Serilog;
using Serilog.Formatting.Json;
using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace MessagingWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = BuildPreliminaryConfig(new ConfigurationBuilder());

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config.GetSection("SeriLog"))
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(new JsonFormatter(),
                    config.GetSection("LogFileLocation").Value,
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var busConfiguration = config.GetSection("BusConfiguration").Get<Configuration.BusConfiguration>();
            IHost host = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices(services =>
                {
                    services.AddTransient(x =>
                    {
                        var conf = x.GetService<IConfiguration>();
                        var settings = conf.GetSection("BusConfiguration").Get<Configuration.BusConfiguration>();
                        return settings;
                    });
                    services.AddTransient<IBusService, BusService>();
                    services.AddTransient<ConfigurationUpdateService>();
                    services.AddScheduler();
                    services.AddQueue();
                    services.AddCache();
                    services.AddTransient<ScheduleStageOne>();
                    services.AddTransient<TaskStageOne>();
                    services.AddTransient<ScheduleStageTwo>();
                    services.AddTransient<TaskStageTwo>();
                   
                })
                .Build();

            host.Services.UseScheduler(scheduler =>
            {
                var stageOne = scheduler
                    .OnWorker("TaskStageOne")
                    .Schedule<ScheduleStageOne>()
                    .EveryFiveSeconds()
                    .Weekday()
                    .Zoned(TimeZoneInfo.Local)
                    .PreventOverlapping("TaskStageOne");

                var stageTwo = scheduler
                    .OnWorker("TaskStageTwo")
                    .Schedule<ScheduleStageTwo>()
                    .EveryFifteenSeconds()
                    .Weekday()
                    .Zoned(TimeZoneInfo.Local)
                    .PreventOverlapping("TaskStageTwo");

            });

            host.Run();
        }

        private static IConfigurationRoot BuildPreliminaryConfig(ConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.SetBasePath(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static void Splash()
        {

        }
    }
}