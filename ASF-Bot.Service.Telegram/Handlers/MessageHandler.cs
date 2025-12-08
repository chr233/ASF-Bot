using ASF_Bot.Infrastructure.Extensions;
using ASF_Bot.Service.Handlers;
using ASF_Bot.Service.Services;
using ASF_Bot.Service.Telegram.Database;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ASF_Bot.Service.Telegram.Handler;
/// <summary>
/// 
/// </summary>
public class MessageHandler
{
    private readonly ILogger<MessageHandler> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly AsfHandler _asfHandler;
    private readonly ChatSettingService _chatSettingService;
    private readonly ChatMessageService _chatMessageService;
    private readonly SemaphoreSlim _semaphore;
    private readonly Timer _timer;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="botClient"></param>
    /// <param name="asfHandler"></param>
    /// <param name="chatSettingService"></param>
    /// <param name="chatMessageService"></param>
    public MessageHandler(
        ILogger<MessageHandler> logger,
        ITelegramBotClient botClient,
        AsfHandler asfHandler,
        ChatSettingService chatSettingService,
        ChatMessageService chatMessageService)
    {
        _logger = logger;
        _botClient = botClient;
        _asfHandler = asfHandler;
        _chatSettingService = chatSettingService;
        _chatMessageService = chatMessageService;
        _semaphore = new SemaphoreSlim(1);
        _timer = new Timer(UpdateTitle, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(15));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="isGroup"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task HandlerTextMessage(Message message, bool isGroup, CancellationToken cancellation)
    {
        if (string.IsNullOrEmpty(message.Text))
        {
            return;
        }

        var setting = await _chatSettingService.GetOrCreateChatSetting(message.Chat.Id, message.MessageThreadId).ConfigureAwait(false);

        if (setting.IpcId != null)
        {
            var ipc = _asfHandler.GetSprcifyApiService(setting.IpcId.Value);
            if (ipc == null)
            {
                await _botClient.SendMessage(message.Chat, $"未找到指定的 IPC: [{setting.IpcId}]", parseMode: ParseMode.Html, messageThreadId: message.MessageThreadId, cancellationToken: cancellation).ConfigureAwait(false);
                return;
            }

            await DoExecuteCommand(ipc, message, false, cancellation).ConfigureAwait(false);
        }
        else
        {
            var ipcs = _asfHandler.GetAllApiService();
            if (ipcs.Length == 0)
            {
                await _botClient.SendMessage(message.Chat, "未设置任何 IPC", parseMode: ParseMode.Html, messageThreadId: message.MessageThreadId, cancellationToken: cancellation).ConfigureAwait(false);
                return;
            }

            var tasks = ipcs.Select(ipc => DoExecuteCommand(ipc, message, true, cancellation));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }

    private async Task DoExecuteCommand(IpcService ipcService, Message message, bool withId, CancellationToken cancellation)
    {
        var response = await ipcService.ExecuteCommand(message.Text!).ConfigureAwait(false);
        var text = response?.Result ?? "网络错误, 请检查 Ipc 设置";
        if (withId)
        {
            text = text + $"\r\n-------------------\r\n{ipcService.Name}";
        }

        text = text.EscapeHtml().ReEscapeHtml();
        var reply = await _botClient.SendMessage(message.Chat, text, parseMode: ParseMode.Html, messageThreadId: message.MessageThreadId, cancellationToken: cancellation).ConfigureAwait(false);

        var cmd = message.Text!.Trim().ToUpperInvariant();
        if (cmd == "2FA" || cmd.StartsWith("2FA "))
        {
            await _chatMessageService.CreateMessage(reply.Chat.Id, reply.MessageId).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_"></param>
    public async void UpdateTitle(object? _)
    {
        if (_semaphore.CurrentCount == 0)
        {
            return;
        }

        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            var messages = await _chatMessageService.GetExpiredMessages().ConfigureAwait(false);
            foreach (var message in messages)
            {
                await _botClient.EditMessageText(message.ChatId, message.MessageId, "<i>消息已删除</i>", ParseMode.Html).ConfigureAwait(false);
                await _chatMessageService.DeletMessage(message).ConfigureAwait(false);

                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
