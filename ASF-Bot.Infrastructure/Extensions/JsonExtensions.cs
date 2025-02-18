using NLog;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ASF_Bot.Infrastructure.Extensions;
public static class JsonExtensions
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static readonly JsonSerializerOptions DefaultJsonOption = new JsonSerializerOptions {
        AllowTrailingCommas = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = true,
    };

    public static T? ToJsonObjct<T>(this string text) where T : notnull
    {
        return text.ToJsonObjct<T>(DefaultJsonOption);
    }

    public static T? ToJsonObjct<T>(this string text, JsonSerializerOptions jsonOption) where T : notnull
    {
        try
        {
            var result = JsonSerializer.Deserialize<T>(text, jsonOption);
            return result;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "反序列化失败");
            Logger.Debug(text);
            return default;
        }
    }

    public static string ToJsonString<T>(this T obj) where T : notnull
    {
        return obj.ToJsonString(DefaultJsonOption);
    }

    public static string ToJsonString<T>(this T obj, JsonSerializerOptions jsonOption) where T : notnull
    {
        try
        {
            return JsonSerializer.Serialize(obj, jsonOption);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "序列化失败");
            Logger.Debug(obj);
            return "";
        }
    }
}
