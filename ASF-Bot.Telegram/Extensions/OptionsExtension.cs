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
        var relativePath = Path.Combine("config", "config.json");

        // 优先使用 BuildInfo.AppDir（如果可用），否则使用运行时目录
        var baseDir = !string.IsNullOrWhiteSpace(BuildInfo.AppDir) ? BuildInfo.AppDir : AppContext.BaseDirectory;

        var config = new ConfigurationBuilder()
            .SetBasePath(baseDir)
            .AddJsonFile(relativePath, optional: true, reloadOnChange: false)
            .AddUserSecrets<Program>()
            .Build();

        // 注册 IConfiguration 与绑定 AppSettings
        builder.AddSingleton<IConfiguration>(config);
        builder.Configure<AppSettings>(config);
    }
}
