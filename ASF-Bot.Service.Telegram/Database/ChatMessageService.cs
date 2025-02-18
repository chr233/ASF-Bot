using ASF_Bot.Model;
using ASF_Bot.Model.Models;
using SqlSugar;

namespace ASF_Bot.Service.Telegram.Database;

#pragma warning disable CS8619 // 值中的引用类型的为 Null 性与目标类型不匹配。

public sealed class ChatMessageService(
    ISqlSugarClient context) : BaseRepository<ChatMessages>(context)
{
    public async Task<ChatMessages> CreateMessage(long chatId, int messasgeId)
    {
        var msg = new ChatMessages { ChatId = chatId, MessageId = messasgeId, ExpiredAt = DateTime.Now.AddSeconds(30) };
        msg = await Insertable(msg).ExecuteReturnEntityAsync().ConfigureAwait(false);
        return msg;
    }

    public Task<List<ChatMessages>> GetExpiredMessages()
    {
        return Queryable().Where(x => x.ExpiredAt < DateTime.Now).ToListAsync();
    }

    public Task<int> DeletMessage(ChatMessages msg)
    {
        return Deleteable().Where(x => x.ChatId == msg.ChatId && x.MessageId == msg.MessageId).ExecuteCommandAsync();
    }
}
