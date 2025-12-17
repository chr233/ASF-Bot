using ASF_Bot.Infrastructure;
using ASF_Bot.Infrastructure.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace ASF_Bot.Telegram.Extensions;

/// <summary>
/// Telegram扩展
/// </summary>
public static class TelegramExtension
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    /// <summary>
    /// 注册Telegram客户端
    /// </summary>
    /// <param name="services"></param>
    public static void AddTelegramBotClient(this IServiceCollection services)
    {
        services.AddSingleton<ITelegramBotClient>(serviceProvider => {
            var httpHelperService = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpHelperService.CreateClient("Telegram");

            var config = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value?.Telegram;

            _logger.Warn(serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value.ToString());

            var token = config?.BotToken;

            if (string.IsNullOrEmpty(token))
            {
                _logger.Error("Telegram.BotToken 不能为空");
                throw new ConfigNotValidException("Telegram.BotToken 不能为空");
            }

            var baseUrl = config?.BaseUrl;
            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = null;
            }

            try
            {
                var options = new TelegramBotClientOptions(token, baseUrl, false);

                return new TelegramBotClient(options, httpClient);
            }
            catch (Exception ex)
            {
                _logger.Error("Telegram.BotToken 或者 Telegram.BaseUrl 无效");
                _logger.Error(ex);
                throw new ConfigNotValidException("Telegram.BotToken 或者 Telegram.BaseUrl 无效");
            }
        });
    }
}
