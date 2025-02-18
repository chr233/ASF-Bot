using ASF_Bot.Infrastructure;
using ASF_Bot.Service.Handlers;
using ASF_Bot.Service.Telegram.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace ASF_Bot.Service.Telegram.Service;

public sealed class StatusService
{
    private readonly ILogger<StatusService> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly AsfHandler _asfHandler;
    private readonly ChatSettingService _tgChatService;
    private readonly SemaphoreSlim _semaphore;
    private readonly Timer? _timer;

    public StatusService(
        ILogger<StatusService> logger,
        ITelegramBotClient botClient,
        IOptions<AppSettings> options,
        AsfHandler asfHandler,
        ChatSettingService tgChatService)
    {
        _logger = logger;
        _botClient = botClient;
        _asfHandler = asfHandler;
        _tgChatService = tgChatService;

        _semaphore = new SemaphoreSlim(1);

        if (options.Value.System.UpdateTitleInterval > 0)
        {
            _timer = new Timer(UpdateTitle, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(options.Value.System.UpdateTitleInterval));
        }
    }

    public async void UpdateTitle(object? _)
    {
        if (_semaphore.CurrentCount == 0)
        {
            return;
        }

        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            var settings = await _tgChatService.GetNeedUpdateSettings().ConfigureAwait(false);
            foreach (var setting in settings)
            {
                string title;

                if (setting.IpcId == null)
                {
                    var ipcs = _asfHandler.GetAllApiService();
                    var onlines = ipcs.Count(ipc => ipc.Status?.IsConnected == true);

                    title = $"在线 IPC: {onlines}/{ipcs.Length}";
                }
                else
                {
                    var ipc = _asfHandler.GetSprcifyApiService(setting.IpcId.Value);

                    if (ipc == null)
                    {
                        title = "未找到 IPC";
                    }
                    else
                    {
                        var status = ipc.Status?.ToString() ?? "暂无数据";
                        title = string.Format("[{0}] {1}", ipc.Name, status);
                    }
                }

                if (setting.Title == title)
                {
                    continue;
                }

                if (setting.ChatId != null)
                {
                    try
                    {
                        if (setting.IsTopidEnable)
                        {
                            if (setting.ThreadId == null)
                            {
                                await _botClient.EditGeneralForumTopic(setting.ChatId, title).ConfigureAwait(false);
                            }
                            else
                            {
                                await _botClient.EditForumTopic(setting.ChatId, setting.ThreadId.Value, title).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            await _botClient.SetChatTitle(setting.ChatId, title).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "更新群标题失败");

                        if (ex.Message.Contains("TOPIC_ID_INVALID"))
                        {
                            await _tgChatService.DeleteSetting(setting).ConfigureAwait(false);
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        setting.Title = title;
                        await _tgChatService.UpdateSetting(setting).ConfigureAwait(false);
                    }
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
