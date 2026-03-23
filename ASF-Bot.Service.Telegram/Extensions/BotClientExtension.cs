using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ASF_Bot.Service.Telegram.Extensions;

/// <summary>
/// BotClient扩展
/// </summary>
public static class BotClientExtension
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 发送回复
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="text"></param>
    /// <param name="message"></param>
    /// <param name="parsemode"></param>
    /// <param name="replyMarkup"></param>
    /// <param name="linkPreviewOptions"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Message> AutoReply(
        this ITelegramBotClient botClient,
        string text,
        Message message,
        ParseMode parsemode = ParseMode.Markdown,
        ReplyMarkup? replyMarkup = null,
        LinkPreviewOptions? linkPreviewOptions = null,
        CancellationToken cancellationToken = default)
    {
        return await botClient.SendMessage(message.Chat, text, parseMode: parsemode, replyParameters: new ReplyParameters { MessageId = message.MessageId, ChatId = message.Chat, AllowSendingWithoutReply = true }, replyMarkup, linkPreviewOptions, message.MessageThreadId, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 发送回复
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="text"></param>
    /// <param name="query"></param>
    /// <param name="showAlert"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task AutoReply(
        this ITelegramBotClient botClient,
        string text,
        CallbackQuery query,
        bool showAlert = false,
        CancellationToken cancellationToken = default)
    {
        await botClient.AnswerCallbackQuery(query.Id, text, showAlert: showAlert, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 编辑消息Markup
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="message"></param>
    /// <param name="replyMarkup"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Message> EditMessageReplyMarkup(
        this ITelegramBotClient botClient,
        Message message,
        InlineKeyboardMarkup? replyMarkup = default,
        CancellationToken cancellationToken = default)
    {
        return await botClient.EditMessageReplyMarkup(message.Chat, message.MessageId, replyMarkup, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 删除消息Markup
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Message> RemoveMessageReplyMarkup(
        this ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken = default)
    {
        return await botClient.EditMessageReplyMarkup(message.Chat, message.MessageId, null, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 编辑消息
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="message"></param>
    /// <param name="text"></param>
    /// <param name="replyMarkup"></param>
    /// <param name="parseMode"></param>
    /// <param name="linkPreviewOptions"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Message> EditMessageText(
        this ITelegramBotClient botClient,
        Message message,
        string text,
        ParseMode parseMode = ParseMode.Markdown,
        LinkPreviewOptions? linkPreviewOptions = null,
        InlineKeyboardMarkup? replyMarkup = default,

        CancellationToken cancellationToken = default)
    {
        return await botClient.EditMessageText(message.Chat, message.MessageId, text, parseMode: parseMode, linkPreviewOptions: linkPreviewOptions, replyMarkup: replyMarkup, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 发送会话状态
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="message"></param>
    /// <param name="chatAction"></param>
    /// <param name="businessConnectionId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task SendChatAction(
        this ITelegramBotClient botClient,
        Message message,
        ChatAction chatAction,
        string? businessConnectionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await botClient.SendChatAction(message.Chat, chatAction, message.MessageThreadId, businessConnectionId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "SendChatAction出错");
        }
    }
}
