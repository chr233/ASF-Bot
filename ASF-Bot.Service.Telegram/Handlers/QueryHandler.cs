using ASF_Bot.Service.Handlers;
using ASF_Bot.Service.Telegram.Database;
using ASF_Bot.Service.Telegram.Extensions;
using ASF_Bot.Service.Telegram.Service;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ASF_Bot.Service.Telegram.Handlers;

/// <summary>
/// 
/// </summary>
/// <param name="_logger"></param>
/// <param name="_botClient"></param>
/// <param name="_validUserService"></param>
/// <param name="_asfHandler"></param>
/// <param name="_tgChatService"></param>
public class QueryHandler(
    ILogger<QueryHandler> _logger,
    ITelegramBotClient _botClient,
    ValidUserService _validUserService,
    AsfHandler _asfHandler,
    ChatSettingService _tgChatService)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="query"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task HandleCallbackQuery(CallbackQuery query, CancellationToken cancellation)
    {
        if (!_validUserService.ValidUser(query.From))
        {
            return;
        }

        var message = query.Message;
        if (message == null)
        {
            await _botClient.AutoReply("消息不存在", query, true, cancellation).ConfigureAwait(false);
            return;
        }

        if (string.IsNullOrEmpty(query.Data))
        {
            await _botClient.RemoveMessageReplyMarkup(message, cancellationToken: cancellation).ConfigureAwait(false);
            return;
        }

        //切分命令参数
        string[] args = query.Data!.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
        string cmd = args.First().ToUpperInvariant();

        if (cmd == "CMD")
        {
            if (args.Length < 2 || !long.TryParse(args[1], out long userID))
            {
                await _botClient.AutoReply("Payload 非法", query, true, cancellationToken: cancellation).ConfigureAwait(false);
                await _botClient.RemoveMessageReplyMarkup(message, cancellationToken: cancellation).ConfigureAwait(false);
                return;
            }

            args = args[2..];
            cmd = args.First().ToUpperInvariant();
        }

        var handler = CommandDispatch(cmd, args, query, message, cancellation);

        if (handler != null)
        {
            await handler.ConfigureAwait(false);
        }

        if (handler == null)
        {
            await _botClient.AutoReply("未知的命令", query, cancellationToken: cancellation).ConfigureAwait(false);
        }
    }

    private Task? CommandDispatch(string command, string[] args, CallbackQuery query, Message message, CancellationToken cancellation)
    {
        return command switch {
            "CANCEL" or "IPC" => Cancel(query, message, cancellation),
            "SWITCH_IPC" => SwitchIpc(args, query, message, cancellation),
            _ => null
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="query"></param>
    /// <param name="message"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task Cancel(CallbackQuery query, Message message, CancellationToken cancellation)
    {
        await _botClient.EditMessageText(message.Chat, message.MessageId, "操作已取消", cancellationToken: cancellation).ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <param name="query"></param>
    /// <param name="message"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task SwitchIpc(string[] args, CallbackQuery query, Message message, CancellationToken cancellation)
    {
        if (_asfHandler.IpcCount == 0)
        {
            await _botClient.AutoReply("IPC 列表为空, 无法操作", query, cancellationToken: cancellation).ConfigureAwait(false);
            return;
        }

        var setting = await _tgChatService.GetOrCreateChatSetting(message.Chat.Id, message.MessageThreadId).ConfigureAwait(false);

        _logger.LogWarning(setting.ToString());

        if (args.Length == 1 || !int.TryParse(args[1], out var id))
        {
            setting.IpcId = null;

            await _botClient.EditMessageText(message, "已切换至 [全部 IPC]", parseMode: ParseMode.Html, cancellationToken: cancellation).ConfigureAwait(false);
        }
        else
        {
            setting.IpcId = id;

            var ipc = _asfHandler.GetSprcifyApiService(setting.IpcId.Value);

            if (ipc == null)
            {
                await _botClient.AutoReply("切换失败, 选择的 IPC 可能不存在", query, cancellationToken: cancellation).ConfigureAwait(false);
                return;
            }

            await _botClient.EditMessageText(message, $"已切换至 IPC: {ipc.Name}", parseMode: ParseMode.Html, cancellationToken: cancellation).ConfigureAwait(false);
        }

        await _tgChatService.UpdateSetting(setting).ConfigureAwait(false);
        _logger.LogWarning(setting.ToString());
    }
}