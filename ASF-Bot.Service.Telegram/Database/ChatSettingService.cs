using ASF_Bot.Model;
using ASF_Bot.Model.Models;
using SqlSugar;

namespace ASF_Bot.Service.Telegram.Database;

#pragma warning disable CS8619 // 值中的引用类型的为 Null 性与目标类型不匹配。

public sealed class ChatSettingService(
    ISqlSugarClient context) : BaseRepository<ChatSettings>(context)
{
    public async Task<ChatSettings> GetOrCreateChatSetting(long? chatId, int? threadId)
    {
        var tgChat = await Queryable().FirstAsync(x => x.ChatId == chatId && x.ThreadId == threadId).ConfigureAwait(false);
        if (tgChat != null)
        {
            return tgChat;
        }

        tgChat = new ChatSettings { ChatId = chatId, ThreadId = threadId, CreateAt = DateTime.Now };
        tgChat = await Insertable(tgChat).ExecuteReturnEntityAsync().ConfigureAwait(false);
        return tgChat;
    }

    public async Task<ChatSettings?> GetChatSetting(long? chatId, int? threadId)
    {
        return await Queryable().FirstAsync(x => x.ChatId == chatId && x.ThreadId == threadId).ConfigureAwait(false);
    }

    public Task<int> UpdateSetting(ChatSettings tgChat)
    {
        tgChat.ModifyAt = DateTime.Now;
        return Updateable(tgChat).ExecuteCommandAsync();
    }

    public Task<List<ChatSettings>> GetNeedUpdateSettings()
    {
        return Queryable().Where(x => x.IsUpdateTitle).ToListAsync();
    }

    public Task<int> DeleteSetting(ChatSettings tgChat)
    {
        return Deleteable().Where(x => x.Id == tgChat.Id).ExecuteCommandAsync();
    }
}
