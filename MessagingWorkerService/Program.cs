using Coravel;
using MessagingWorkerService.Schedules;
using MessagingWorkerService.Services;
using MessagingWorkerService.Tasks;
using Serilog;
using Serilog.Formatting.Json;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Serilog.Events;

namespace MessagingWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = BuildPreliminaryConfig(new ConfigurationBuilder());

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config.GetSection("SeriLog"))
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
                .WriteTo.File(new JsonFormatter(),
                    config.GetSection("LogFileLocation").Value,
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();
            
            IHost host = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseWindowsService()
                .Build();

            var busConfiguration = config.GetSection("BusConfiguration").Get<Configuration.BusConfiguration>();
            host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddTransient(x =>
                    {
                        var conf = x.GetService<IConfiguration>();
                        if (conf == null) throw new ApplicationException("Configuration Missing!");
                        var settings = conf.GetSection("OutputFileConfiguration").Get<Configuration.OutputFileConfiguration>();
                        return settings;
                    });
                    services.AddTransient(x =>
                    {
                        var conf = x.GetService<IConfiguration>();
                        if (conf == null) throw new ApplicationException("Configuration Missing!");
                        var settings = conf.GetSection("BusConfiguration").Get<Configuration.BusConfiguration>();
                        return settings;
                    });
                    services.AddTransient<IOutputFileService, OutputFileService>();
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

            Splash();

            host.Services.UseScheduler(scheduler =>
            {
                //var stageOne = scheduler
                //    .OnWorker("TaskStageOne")
                //    .Schedule<ScheduleStageOne>()
                //    .EveryFifteenSeconds()
                //    .Weekday()
                //    .Zoned(TimeZoneInfo.Local)
                //    .PreventOverlapping("TaskStageOne");

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
            //http://patorjk.com/software/taag/#p=display&f=ANSI%20Shadow&t=Messaging
            Console.WriteLine("███╗   ███╗███████╗███████╗███████╗ █████╗  ██████╗ ██╗███╗   ██╗ ██████╗ ");
            Console.WriteLine("████╗ ████║██╔════╝██╔════╝██╔════╝██╔══██╗██╔════╝ ██║████╗  ██║██╔════╝ ");
            Console.WriteLine("██╔████╔██║█████╗  ███████╗███████╗███████║██║  ███╗██║██╔██╗ ██║██║  ███╗");
            Console.WriteLine("██║╚██╔╝██║██╔══╝  ╚════██║╚════██║██╔══██║██║   ██║██║██║╚██╗██║██║   ██║");
            Console.WriteLine("██║ ╚═╝ ██║███████╗███████║███████║██║  ██║╚██████╔╝██║██║ ╚████║╚██████╔╝");
            Console.WriteLine("╚═╝     ╚═╝╚══════╝╚══════╝╚══════╝╚═╝  ╚═╝ ╚═════╝ ╚═╝╚═╝  ╚═══╝ ╚═════╝ ");
            Console.WriteLine($"Version {Assembly.GetExecutingAssembly().GetName().Version}");                                                                         
        }
    }
}