using ASF_Bot.Service.Telegram.Extensions;
using ASF_Bot.Service.Telegram.Handler;
using ASF_Bot.Service.Telegram.Handlers;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ASF_Bot.Service.Telegram.Service;

/// <summary>
/// 
/// </summary>
/// <param name="_logger"></param>
/// <param name="_botClient"></param>
/// <param name="_validUserService"></param>
/// <param name="_commandHandler"></param>
/// <param name="_queryHandler"></param>
/// <param name="_telegramHandler"></param>
public sealed class DispatchService(
    ILogger<DispatchService> _logger,
    ITelegramBotClient _botClient,
    ValidUserService _validUserService,
    CommandHandler _commandHandler,
    QueryHandler _queryHandler,
    MessageHandler _telegramHandler)
{
    private User? _me;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task OnMessageReceived(Message message, CancellationToken cancellation)
    {
        if (message.Type == MessageType.NewChatTitle || message.Type == MessageType.ForumTopicEdited)
        {
            _me = await _botClient.GetMe(cancellation).ConfigureAwait(false);
            if (_me.Id == message.From?.Id)
            {
                await _botClient.DeleteMessage(message.Chat, message.MessageId, cancellation).ConfigureAwait(false);
            }
        }
        //else if (message.Type == MessageType.ForumTopicClosed)
        //{
        //    var setting = await _tgChatSettingService.GetChatSetting(message.Chat.Id, message.MessageThreadId).ConfigureAwait(false);
        //    if (setting != null)
        //    {
        //        await _tgChatSettingService.DeleteSetting(setting).ConfigureAwait(false);
        //    }
        //}

        if (!_validUserService.ValidUser(message.From))
        {
            return;
        }

        var text = message.Text;
        if (message.Type != MessageType.Text || string.IsNullOrEmpty(text))
        {
            return;
        }

        var isGroup = message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup;

        if (text.StartsWith('/'))
        {
            await _commandHandler.HandleCommand(message, isGroup, cancellation).ConfigureAwait(false);
        }
        else
        {
            await _telegramHandler.HandlerTextMessage(message, isGroup, cancellation).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="query"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public Task OnCallbackQueryReceived(CallbackQuery query, CancellationToken cancellation)
    {
        if (!_validUserService.ValidUser(query.From))
        {
            return Task.CompletedTask;
        }

        var data = query.Data;
        if (string.IsNullOrEmpty(data) || query.Message == null)
        {
            return Task.CompletedTask;
        }

        return _queryHandler.HandleCallbackQuery(query, cancellation);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="update"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public Task OnOtherUpdateReceived(Update update, CancellationToken cancellation)
    {
        _logger.LogUpdate(update);
        return Task.CompletedTask;
    }
}
