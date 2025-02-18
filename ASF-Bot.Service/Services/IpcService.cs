using ASF_Bot.Data.Datas;
using ASF_Bot.Data.Requests;
using ASF_Bot.Data.Responses;
using ASF_Bot.Infrastructure;
using ASF_Bot.Infrastructure.Configs;
using ASF_Bot.Infrastructure.Exceptions;
using ASF_Bot.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace ASF_Bot.Service.Services;

public sealed class IpcService(
    ILogger<IpcService> _logger,
    IHttpClientFactory _httpClientFactory,
    IOptions<AppSettings> _options) : IDisposable
{
    private readonly bool _debug = _options.Value.System.Debug;

    private HttpClient _httpClient = _httpClientFactory.CreateClient("ASF");

    private string _prefix = "/Api";

    private int _id = 0;
    private string? _name = null;

    private IpcStatus? _status;

    private Timer? _statusTimer;

    public string Name => !string.IsNullOrEmpty(_name) ? _name : $"[{_id}]";
    public int Id => _id;

    public IpcStatus? Status => _status;

    public void Setup(int id, IpcConfig config)
    {
        HttpClient client;

        if (!string.IsNullOrEmpty(config.Proxy))
        {
            _logger.LogInformation("已配置 Asf 代理: {proxy}", config.Proxy);
            var proxy = new WebProxy { Address = new Uri(config.Proxy) };

            HttpMessageHandler handler = config.Proxy.StartsWith("socks")
                ? new SocketsHttpHandler { Proxy = proxy, UseProxy = true }
                : new HttpClientHandler { Proxy = proxy, UseProxy = true };

            client = new HttpClient(handler);
        }
        else
        {
            client = _httpClientFactory.CreateClient("ASF");
        }

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(BuildInfo.AppName, BuildInfo.Version));
        client.Timeout = TimeSpan.FromSeconds(config.Timeout);

        if (string.IsNullOrEmpty(config.BaseUrl) || !Uri.TryCreate(config.BaseUrl, UriKind.Absolute, out var uri))
        {
            throw new ConfigNotValidException("BaseUrl 无效");
        }

        client.BaseAddress = uri;

        if (!string.IsNullOrEmpty(config.IpcPassword))
        {
            client.DefaultRequestHeaders.Add("Authentication", config.IpcPassword);
        }

        if (!string.IsNullOrEmpty(config.ApiPrefix))
        {
            _prefix = config.ApiPrefix;

            if (!_prefix.StartsWith('/'))
            {
                _prefix = $"/{_prefix}";
            }

            if (_prefix.EndsWith('/'))
            {
                _prefix = _prefix[..^1];
            }
        }
        else
        {
            _prefix = "/Api";
        }

        _name = config.Name?.EscapeHtml().ReEscapeHtml();
        _id = id;

        _httpClient = client;

        var period = _options.Value.System.UpdateStatusInterval;
        if (period > 0)
        {
            var delay = Random.Shared.Next(10, 10 + period);
            _logger.LogInformation("已配置 Asf 状态更新间隔: {period} 秒, 将于 {delay} 秒后更新", period, delay);
            _statusTimer = new Timer(UpdateBotStatus, null, TimeSpan.FromSeconds(delay), TimeSpan.FromSeconds(period));
        }
    }

    #region base
    private async Task<StreamReader> SendToStream(HttpRequestMessage message)
    {
        if (_debug)
        {
            _logger.LogDebug("SendRequest -> {url}", message.RequestUri);
        }

        var response = await _httpClient.SendAsync(message).ConfigureAwait(false);

        if (_debug)
        {
            _logger.LogDebug("EwcvResponse <- {code} size: {size}", response.StatusCode, response.Content.Headers.ContentLength);
        }

        response.EnsureSuccessStatusCode();

        var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        Stream decompressedStream;

        var encoding = response.Content.Headers.ContentEncoding;
        // 检查并解压缩内容
        if (encoding.Contains("gzip"))
        {
            // 检查并解压缩内容
            decompressedStream = new GZipStream(contentStream, CompressionMode.Decompress);
        }
        else if (encoding.Contains("deflate"))
        {
            // 检查并解压缩内容
            decompressedStream = new DeflateStream(contentStream, CompressionMode.Decompress);
        }
        else if (encoding.Contains("br"))
        {
            // 检查并解压缩内容
            decompressedStream = new BrotliStream(contentStream, CompressionMode.Decompress);
        }
        else
        {
            // 检查并解压缩内容
            decompressedStream = contentStream;
        }

        var reader = new StreamReader(decompressedStream, Encoding.UTF8);
        return reader;
    }

    private async Task<T?> SendToJsonObject<T>(HttpRequestMessage message) where T : notnull
    {
        try
        {
            using var reader = await SendToStream(message).ConfigureAwait(false);
            var json = await reader.ReadToEndAsync().ConfigureAwait(false);
            var obj = json.ToJsonObjct<T>();
            return obj;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "网络请求失败");
            return default;
        }
    }
    #endregion

    private async void UpdateBotStatus(object? _)
    {
        var response = await GetBots("ASF").ConfigureAwait(false);
        var result = response?.Result;

        if (result == null)
        {
            _logger.LogWarning("IPC: {name} 获取 ASF 状态失败, 请检查 IPC 配置", Name);
            return;
        }

        _status = new IpcStatus(response);

        _logger.LogWarning("IPC: {name} 状态 {status}", Name, _status);
    }

    public Task<CommandResponse?> ExecuteCommand(string command)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_prefix}/Command") {
            Content = JsonContent.Create(
            new CommandRequest(command), options: JsonExtensions.DefaultJsonOption)
        };

        return SendToJsonObject<CommandResponse>(request);
    }

    public Task<GetBotResponse?> GetBots(string botNames)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_prefix}/Bot/{botNames}");
        return SendToJsonObject<GetBotResponse>(request);
    }

    public Task<GetLogResponse?> GetAsfLog(int count)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_prefix}/NLog/File?count={count}");
        return SendToJsonObject<GetLogResponse>(request);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _statusTimer?.Dispose();
    }
}