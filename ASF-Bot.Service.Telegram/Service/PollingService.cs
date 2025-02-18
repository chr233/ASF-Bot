using ASF_Bot.Infrastructure;
using ASF_Bot.Infrastructure.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;

namespace ASF_Bot.Service.Telegram.Service;

/// <summary>
/// 消息接收服务
/// </summary>
public sealed class PollingService(
    IServiceProvider _serviceProvider,
    ILogger<PollingService> _logger,
    IOptions<AppSettings> _options,
    ITelegramBotClient _botClient)
{

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        var config = _options.Value.Telegram ?? throw new ConfigNotValidException("Telegram 配置无效, 无法监听消息");

        try
        {
            var me = await _botClient.GetMe(stoppingToken).ConfigureAwait(false);
            _logger.LogInformation("Bot 信息: {nick} @{name} #{id}", me.FirstName, me.Username, me.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "无法获取 Bot 信息, 请检查 Telegram.BotToken 以及 Telegram.Proxy 是否正确");
            throw new ConfigNotValidException("无法获取 Bot 信息, 请检查 Telegram.BotToken 以及 Telegram.Proxy 是否正确");
        }

        using var scope = _serviceProvider.CreateScope();
        var updateService = scope.ServiceProvider.GetRequiredService<UpdateService>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var receiverOptions = new ReceiverOptions {
                    AllowedUpdates = [],
                    DropPendingUpdates = config.DropPendingUpdates,
                    Limit = 100,
                };

                _logger.LogInformation("接收服务运行中...");

                await _botClient.ReceiveAsync(
                    updateHandler: updateService.HandleUpdateAsync,
                    errorHandler: updateService.HandlePollingErrorAsync,
                    receiverOptions: receiverOptions,
                    cancellationToken: stoppingToken).ConfigureAwait(false);
            }
            catch (ApiRequestException ex)
            {
                _logger.LogError(ex, "Telegram API 调用出错");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "接收服务运行出错");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
            }
        }
    }
}
