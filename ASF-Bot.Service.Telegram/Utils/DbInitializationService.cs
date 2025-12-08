using ASF_Bot.Infrastructure;
using ASF_Bot.Model.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace ASF_Bot.Service.Telegram.Utils;

/// <summary>
/// 消息接收服务
/// </summary>
/// <remarks>
/// 消息接收服务
/// </remarks>
/// <param name="_logger"></param>
/// <param name="_options"></param>
/// <param name="_dbClient"></param>
public class DbInitializationService(
    ILogger<DbInitializationService> _logger,
    ISqlSugarClient _dbClient,
    IOptions<AppSettings> _options)
{
    /// <summary>
    /// 
    /// </summary>
    public void InitDatabase()
    {
        if (!_options.Value.Database.Generate)
        {
            return;
        }

        _logger.LogInformation("开始生成数据库结构");
        //创建数据库
        try
        {
            _dbClient.DbMaintenance.CreateDatabase(_options.Value.Database.DbName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "创建数据库失败, 可能没有权限");
        }

        //创建数据表
        _dbClient.CodeFirst.InitTables<ChatSettings>();
        _dbClient.CodeFirst.InitTables<ChatMessages>();

        _logger.LogWarning("数据库结构生成完毕, 建议禁用 Database.Generate 来加快启动速度");
    }
}