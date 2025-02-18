using ASF_Bot.Infrastructure;
using ASF_Bot.Infrastructure.Localization;
using ASF_Bot.Service.Telegram.Service;
using ASF_Bot.Service.Telegram.Utils;
using ASF_Bot.Telegram.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SteamToolProfileSpider.Server.Extensions;
using System.Text;

const string banner = @"
 █████╗ ███████╗███████╗    ██████╗  ██████╗ ████████╗
██╔══██╗██╔════╝██╔════╝    ██╔══██╗██╔═══██╗╚══██╔══╝
███████║███████╗█████╗█████╗██████╔╝██║   ██║   ██║
██╔══██║╚════██║██╔══╝╚════╝██╔══██╗██║   ██║   ██║
██║  ██║███████║██║         ██████╔╝╚██████╔╝   ██║
╚═╝  ╚═╝╚══════╝╚═╝         ╚═════╝  ╚═════╝    ╚═╝.Telegram
";

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine(Langs.MotdLine);
Console.WriteLine(banner);
Console.WriteLine(Langs.MotdLine);
Console.WriteLine(Langs.MotdFramework, BuildInfo.FrameworkName);
Console.WriteLine(Langs.MotdVersion, BuildInfo.Version, BuildInfo.Configuration);
Console.WriteLine(Langs.MotdCompany, BuildInfo.Company);
Console.WriteLine(Langs.MotdCopyright, BuildInfo.Copyright);
Console.WriteLine(Langs.MotdLine);

#if !DEBUG
Thread.Sleep(2000);
#endif

var cts = new CancellationTokenSource();

var services = new ServiceCollection();

// NLog
services.AddNlog();

// 配置文件
services.AddCustomOptions();

// AppService
services.AddAppService();

// HttpClient
services.AddHttpClients();

// Telegram
services.AddTelegramBotClient();

// SqlSugar
services.AddSqlSugarSetup();

var serviceProvider = services.BuildServiceProvider();

var dbUtils = serviceProvider.GetRequiredService<DbInitializationService>();
dbUtils.InitDatabase();

var statusService = serviceProvider.GetRequiredService<StatusService>();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

Console.CancelKeyPress += (sender, e) => {
    e.Cancel = true;
    cts.Cancel();
    logger.LogInformation(Langs.ProgramIsStoping);
};

var pollingService = serviceProvider.GetRequiredService<PollingService>();

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        await pollingService.StartAsync(cts.Token).ConfigureAwait(false);
    }
}
catch (OperationCanceledException)
{
    logger.LogInformation("Operation canceled.");
}
finally
{
    logger.LogInformation("Application stopped.");
}