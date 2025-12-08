using ASF_Bot.Service.Telegram.Extensions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ASF_Bot.Service.Telegram.Service;

/// <summary>
/// 
/// </summary>
/// <param name="_logger"></param>
/// <param name="_dispatchService"></param>
public sealed class UpdateService(
    ILogger<UpdateService> _logger,
    DispatchService _dispatchService)
{
    /// <summary>
    /// 处理Update
    /// </summary>
    /// <param name="_"></param>
    /// <param name="update"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellation)
    {
        _logger.LogUpdate(update);

        var handler = update.Type switch {
            UpdateType.Message => _dispatchService.OnMessageReceived(update.Message!, cancellation),
            UpdateType.CallbackQuery => _dispatchService.OnCallbackQueryReceived(update.CallbackQuery!, cancellation),
            _ => _dispatchService.OnOtherUpdateReceived(update, cancellation)
        };

        if (handler != null)
        {
            try
            {
                await handler.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理轮询出错 {update}", update);
            }
        }
    }

    /// <summary>
    /// 处理Error
    /// </summary>
    /// <param name="_"></param>
    /// <param name="exception"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task HandlePollingErrorAsync(ITelegramBotClient _, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "处理轮询出错");

        if (exception is RequestException)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
        }
    }
}
