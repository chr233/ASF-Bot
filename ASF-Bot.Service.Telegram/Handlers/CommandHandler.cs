using ASF_Bot.Infrastructure;
using ASF_Bot.Infrastructure.Extensions;
using ASF_Bot.Service.Handlers;
using ASF_Bot.Service.Services;
using ASF_Bot.Service.Telegram.Database;
using ASF_Bot.Service.Telegram.Extensions;
using ASF_Bot.Service.Telegram.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Frozen;
using System.Data;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ASF_Bot.Service.Telegram.Handlers;

/// <summary>
/// 命令处理器
/// </summary>
/// <param name="_logger"></param>
/// <param name="_botClient"></param>
/// <param name="_options"></param>
/// <param name="_validUserService"></param>
/// <param name="_asfHandler"></param>
/// <param name="_tgChatService"></param>
public class CommandHandler(
    ILogger<CommandHandler> _logger,
    ITelegramBotClient _botClient,
    IOptions<AppSettings> _options,
    ValidUserService _validUserService,
    AsfHandler _asfHandler,
    ChatSettingService _tgChatService)
{
    private User? _me;

    public async Task HandleCommand(Message message, bool isGroup, CancellationToken cancellation)
    {
        if (!_validUserService.ValidUser(message.From))
        {
            return;
        }

        //切分命令参数
        var args = message.Text!.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
        var cmd = args.First()[1..].ToUpperInvariant();

        //判断是不是艾特机器人的命令
        var IsAtMe = false;
        var index = cmd.IndexOf('@');
        if (isGroup && index > -1)
        {
            var botName = cmd[(index + 1)..];

            _me ??= await _botClient.GetMe(cancellation).ConfigureAwait(false);

            if (botName.Equals(_me.Username, StringComparison.OrdinalIgnoreCase))
            {
                cmd = cmd[..index];
                IsAtMe = true;
            }
            else
            {
                return;
            }
        }

        var handler = CommandDispatch(cmd, message, cancellation);

        if (handler != null)
        {
            await handler.ConfigureAwait(false);
        }

        if (handler == null && ((isGroup && IsAtMe) || (!isGroup)))
        {
            _logger.LogWarning("未知的命令 {cmd}", cmd);
            await _botClient.AutoReply("未知的命令", message, cancellationToken: cancellation).ConfigureAwait(false);
        }

    }

    private Task? CommandDispatch(string command, Message message, CancellationToken cancellation)
    {
        return command switch {
#if DEBUG
            "TEST" => Test(message, cancellation),
#endif
            "START" => Start(message, cancellation),
            "VERSION" => Version(message, cancellation),
            "MESSAGE" or "MSG" => Message(message, cancellation),
            "IPC_MENU" or "IPC" => IpcMenu(message, cancellation),
            "NEXT_IPC" or "NEXT" => NextIpc(true, message, cancellation),
            "PREV_IPC" or "PREV" => NextIpc(false, message, cancellation),
            "ALL_IPC" or "ALL" => AllIpc(message, cancellation),
            "ENABLE_STATUS" => EnableStatus(message, cancellation),
            "DISABLE_STATUS" => DisableStatus(message, cancellation),
            "SET_COMMAND" => SetCommand(message, cancellation),
            "CLEAR_COMMAND" => ClearCommand(message, cancellation),
            "SHOW_LOG" => ShowLog(message, cancellation),
            _ => null
        };
    }

    private readonly FrozenDictionary<string, string> _commandDict = new Dictionary<string, string> {
        { "/ipc_menu", "选择激活的 IPC" },
        { "/next_ipc", "激活下一个 IPC" },
        { "/prev_ipc", "激活上一个 IPC" },
        { "/all_ipc", "激活全部 IPC" },
        { "/enable_status", "在当前会话启用 IPC 状态实时更新" },
        { "/disable_status", "在当前会话禁用 IPC 状态实时更新" },
        { "/set_command", "设置命令提示" },
        { "/clear_command", "清除命令提示" },
        { "/show_log", "显示日志" },
    }.ToFrozenDictionary();

#if DEBUG
    /// <summary>
    /// 测试
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task Test(Message message, CancellationToken cancellation)
    {
        await _botClient.LeaveChat(-1002327007359);
    }
#endif

    /// <summary>
    /// 开始
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public Task Start(Message message, CancellationToken cancellation)
    {
        var msg = $"欢迎使用 ASF-Bot 机器人";
        return _botClient.AutoReply(msg, message, cancellationToken: cancellation);
    }

    /// <summary>
    /// 版本
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public Task Version(Message message, CancellationToken cancellation)
    {
        var sb = new StringBuilder();
        sb.AppendFormat("版本: {0}\r\n", BuildInfo.Version);
        sb.AppendFormat("框架: {0}\r\n", BuildInfo.FrameworkName);
        sb.AppendFormat("版权: {0}\r\n", BuildInfo.Copyright);

        return _botClient.AutoReply(sb.ToString(), message, cancellationToken: cancellation);
    }

    public Task Message(Message message, CancellationToken cancellation)
    {
        var sb = new StringBuilder();
        sb.AppendFormat("Chat: {0}\r\n", message.Chat.FullChatID());
        sb.AppendFormat("Thread: {0}\r\n", message.MessageThreadId);
        sb.AppendFormat("From: {0}\r\n", message.From?.FullName());
        sb.AppendFormat("Text: {0}\r\n", message.Text);
        return _botClient.AutoReply(sb.ToString(), message, cancellationToken: cancellation);
    }

    public async Task SetCommand(Message message, CancellationToken cancellation)
    {
        var commands = _commandDict.Select(static x => new BotCommand { Command = x.Key, Description = x.Value });

        await _botClient.SetMyCommands(commands, null, null, cancellation).ConfigureAwait(false);

        await _botClient.AutoReply("设置命令提示成功", message, cancellationToken: cancellation).ConfigureAwait(false);
    }

    public async Task ClearCommand(Message message, CancellationToken cancellation)
    {
        BotCommand[] commands = [];
        await _botClient.SetMyCommands(commands, null, null, cancellation).ConfigureAwait(false);

        await _botClient.AutoReply("清除命令提示成功", message, cancellationToken: cancellation).ConfigureAwait(false);
    }

    public async Task IpcMenu(Message message, CancellationToken cancellation)
    {
        if (_asfHandler.IpcCount == 0)
        {
            await _botClient.AutoReply("IPC 列表为空, 无法操作", message, cancellationToken: cancellation).ConfigureAwait(false);
            return;
        }

        var setting = await _tgChatService.GetOrCreateChatSetting(message.Chat.Id, message.MessageThreadId).ConfigureAwait(false);

        List<InlineKeyboardButton[]> buttons = [
            [ new InlineKeyboardButton {
                Text = "-- 全部 IPC --",
                CallbackData = "SWITCH_IPC"
            }],
        ];

        var max = Math.Max(_asfHandler.IpcCount, 6);

        for (int i = 0; i < max; i++)
        {
            var ipc = _asfHandler.GetSprcifyApiService(i);
            if (ipc == null)
            {
                continue;
            }
            buttons.Add([new InlineKeyboardButton {
                Text = $"[{ipc.Id}] {ipc.Name} {ipc.Status}",
                CallbackData = $"SWITCH_IPC {i}"
            }]);
        }

        buttons.Add([new InlineKeyboardButton { Text = "取消", CallbackData = "CANCEL" }]);

        var ipcId = setting.IpcId == null ? 0 : setting.IpcId.Value + 1;
        buttons[ipcId][0].Text = $"【{buttons[ipcId][0].Text}】";

        await _botClient.AutoReply("请选择激活的 IPC", message, replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: cancellation).ConfigureAwait(false);
    }

    public async Task NextIpc(bool next, Message message, CancellationToken cancellation)
    {
        if (_asfHandler.IpcCount == 0)
        {
            await _botClient.AutoReply("IPC 列表为空, 无法操作", message, cancellationToken: cancellation).ConfigureAwait(false);
            return;
        }

        var setting = await _tgChatService.GetOrCreateChatSetting(message.Chat.Id, message.MessageThreadId).ConfigureAwait(false);

        if (setting == null)
        {
            await _botClient.AutoReply("内部错误", message, cancellationToken: cancellation).ConfigureAwait(false);
            return;
        }

        if (next)
        {
            if (setting.IpcId == null || setting.IpcId + 1 == _asfHandler.IpcCount)
            {
                setting.IpcId = 0;
            }
            else
            {
                setting.IpcId++;
            }
        }
        else
        {
            if (setting.IpcId == null || setting.IpcId == 0)
            {
                setting.IpcId = _asfHandler.IpcCount - 1;
            }
            else
            {
                setting.IpcId--;
            }
        }

        await _tgChatService.UpdateSetting(setting).ConfigureAwait(false);

        var ipc = _asfHandler.GetSprcifyApiService(setting.IpcId.Value);

        if (ipc == null)
        {
            await _botClient.AutoReply("切换失败, IPC 列表可能为空", message, cancellationToken: cancellation).ConfigureAwait(false);
        }
        else
        {
            await _botClient.AutoReply($"已切换至 IPC: {ipc.Name}", message, cancellationToken: cancellation).ConfigureAwait(false);
        }
    }

    public async Task AllIpc(Message message, CancellationToken cancellation)
    {
        if (_asfHandler.IpcCount == 0)
        {
            await _botClient.AutoReply("IPC 列表为空, 无法操作", message, cancellationToken: cancellation).ConfigureAwait(false);
            return;
        }

        var setting = await _tgChatService.GetOrCreateChatSetting(message.Chat.Id, message.MessageThreadId).ConfigureAwait(false);

        if (setting == null)
        {
            await _botClient.AutoReply("内部错误", message, cancellationToken: cancellation).ConfigureAwait(false);
            return;
        }

        setting.IpcId = null;
        await _tgChatService.UpdateSetting(setting).ConfigureAwait(false);

        await _botClient.AutoReply("已切换至 全部 IPC", message, cancellationToken: cancellation).ConfigureAwait(false);
    }

    public async Task EnableStatus(Message message, CancellationToken cancellation)
    {
        if (message.Chat.Type == ChatType.Private)
        {
            await _botClient.AutoReply("此功能只能在群组中使用", message, cancellationToken: cancellation).ConfigureAwait(false);
            return;
        }

        if (_options.Value.System.UpdateStatusInterval == 0)
        {
            await _botClient.AutoReply("IPC 状态实时更新未启用, 请将 System.UpdateStatusInterval 改为非 0 的值", message, cancellationToken: cancellation).ConfigureAwait(false);
            return;
        }

        var chatInfo = await _botClient.GetChat(message.Chat, cancellationToken: cancellation).ConfigureAwait(false);

        bool verify;

        if (chatInfo.IsForum)
        {
            verify = chatInfo.Permissions?.CanManageTopics == true;
        }
        else
        {
            verify = chatInfo.Permissions?.CanChangeInfo == true;
        }

        if (!verify)
        {
            var msg = chatInfo.IsForum ? "机器人没有合适的权限, 请设置为管理员并给予 “管理话题” 权限" : "机器人没有合适的权限, 请设置为管理员并给予 “修改群组信息” 权限";
            await _botClient.AutoReply(msg, message, cancellationToken: cancellation).ConfigureAwait(false);
            return;
        }

        var setting = await _tgChatService.GetOrCreateChatSetting(message.Chat.Id, message.MessageThreadId).ConfigureAwait(false);
        setting.IsUpdateTitle = true;
        setting.IsTopidEnable = chatInfo.IsForum;

        await _tgChatService.UpdateSetting(setting).ConfigureAwait(false);

        await _botClient.AutoReply("IPC 状态实时更新已启用", message, cancellationToken: cancellation).ConfigureAwait(false);
    }

    public async Task DisableStatus(Message message, CancellationToken cancellation)
    {
        if (message.Chat.Type == ChatType.Private)
        {
            await _botClient.AutoReply("此功能只能在群组中使用", message, cancellationToken: cancellation).ConfigureAwait(false);
            return;
        }

        var setting = await _tgChatService.GetOrCreateChatSetting(message.Chat.Id, message.MessageThreadId).ConfigureAwait(false);

        if (setting.IsUpdateTitle)
        {
            setting.IsUpdateTitle = false;
            await _tgChatService.UpdateSetting(setting).ConfigureAwait(false);
        }

        await _botClient.AutoReply("IPC 状态实时更新已关闭", message, cancellationToken: cancellation).ConfigureAwait(false);
        return;
    }

    public async Task ShowLog(Message message, CancellationToken cancellation)
    {
        var setting = await _tgChatService.GetOrCreateChatSetting(message.Chat.Id, message.MessageThreadId).ConfigureAwait(false);

        if (setting.IpcId != null)
        {
            var ipc = _asfHandler.GetSprcifyApiService(setting.IpcId.Value);
            if (ipc == null)
            {
                await _botClient.SendMessage(message.Chat, $"未找到指定的 IPC: [{setting.IpcId}]", parseMode: ParseMode.Html, messageThreadId: message.MessageThreadId, cancellationToken: cancellation).ConfigureAwait(false);
                return;
            }

            await DoFetchLog(ipc, message, false, cancellation).ConfigureAwait(false);
        }
        else
        {
            var ipcs = _asfHandler.GetAllApiService();
            if (ipcs.Length == 0)
            {
                await _botClient.SendMessage(message.Chat, "未设置任何 IPC", parseMode: ParseMode.Html, messageThreadId: message.MessageThreadId, cancellationToken: cancellation).ConfigureAwait(false);
                return;
            }

            var tasks = ipcs.Select(ipc => DoFetchLog(ipc, message, true, cancellation));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }

    private async Task DoFetchLog(IpcService ipcService, Message message, bool withId, CancellationToken cancellation)
    {
        var response = await ipcService.GetAsfLog(20).ConfigureAwait(false);
        string text;

        if (response?.Result?.Content == null)
        {
            text = "网络错误, 请检查 Ipc 设置";
        }
        else
        {
            var sb = new StringBuilder();
            sb.Append("<code>");
            foreach (var line in response.Result.Content)
            {
                sb.AppendLine(line.EscapeHtml().ReEscapeHtml());
            }
            sb.Append("</code>");

            text = sb.ToString();
        }

        if (withId)
        {
            text += $"-------------------\r\n{ipcService.Name}";
        }

        await _botClient.SendMessage(message.Chat, text, parseMode: ParseMode.Html, messageThreadId: message.MessageThreadId, cancellationToken: cancellation).ConfigureAwait(false);
    }
}