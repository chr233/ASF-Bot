using ASF_Bot.Service.Handlers;
using ASF_Bot.Service.Services;
using ASF_Bot.Service.Telegram.Database;
using ASF_Bot.Service.Telegram.Handler;
using ASF_Bot.Service.Telegram.Handlers;
using ASF_Bot.Service.Telegram.Service;
using ASF_Bot.Service.Telegram.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace ASF_Bot.Telegram.Extensions;

/// <summary>
/// 动态注册服务扩展
/// </summary>
public static class AppServiceExtensions
{
    /// <summary>
    /// 注册引用程序域中所有有AppService标记的类的服务
    /// </summary>
    /// <param name="services"></param>
    public static void AddAppService(this IServiceCollection services)
    {
        services.AddSingleton<DbInitializationService>();

        services.AddSingleton<PollingService>();
        services.AddSingleton<UpdateService>();
        services.AddSingleton<ValidUserService>();
        services.AddSingleton<DispatchService>();
        services.AddSingleton<StatusService>();

        services.AddSingleton<AsfHandler>();
        services.AddSingleton<MessageHandler>();
        services.AddSingleton<CommandHandler>();
        services.AddSingleton<QueryHandler>();

        services.AddTransient<IpcService>();

        services.AddTransient<ChatSettingService>();
        services.AddTransient<ChatMessageService>();
    }
}