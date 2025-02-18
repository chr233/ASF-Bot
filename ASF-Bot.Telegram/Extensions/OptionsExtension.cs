using ASF_Bot.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ASF_Bot.Telegram.Extensions;

/// <summary>
/// 配置文件扩展
/// </summary>
public static class OptionsExtension
{
    /// <summary>
    /// 添加自定义配置文件
    /// </summary>
    /// <param name="builder"></param>
    public static void AddCustomOptions(this IServiceCollection builder)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json", optional: true, reloadOnChange: false)
            .AddUserSecrets<Program>()
            .Build();

        builder.AddSingleton<IConfiguration>(config);
        builder.Configure<AppSettings>(config);
    }
}
