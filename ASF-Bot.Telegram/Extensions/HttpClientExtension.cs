using ASF_Bot.Infrastructure;
using ASF_Bot.Service.Telegram.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;

namespace ASF_Bot.Telegram.Extensions;

/// <summary>
/// HttpClient扩展
/// </summary>
public static class HttpClientExtension
{
    /// <summary>
    /// 注册HttpClient
    /// </summary>
    /// <param name="services"></param>
    public static void AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient("Telegram", (serviceProvider, httpClient) => {
            var config = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value.Telegram;
            var baseUrl = config?.BaseUrl;
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(BuildInfo.AppName, BuildInfo.Version));
        }).ConfigurePrimaryHttpMessageHandler(serviceProvider => {
            var config = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value.Telegram;
            var proxy = config?.Proxy;

            if (!string.IsNullOrEmpty(proxy))
            {
                var logger = serviceProvider.GetRequiredService<ILogger<DispatchService>>();
                logger.LogInformation("已配置 Telegram 代理: {proxy}", proxy);

                var webProxy = new WebProxy { Address = new Uri(proxy) };

                return proxy.StartsWith("socks")
                    ? new SocketsHttpHandler { Proxy = webProxy, UseProxy = true, }
                    : new HttpClientHandler { Proxy = webProxy, UseProxy = true, };
            }
            else
            {
                return new HttpClientHandler();
            }
        }).SetHandlerLifetime(Timeout.InfiniteTimeSpan).RemoveAllLoggers();

        services.AddHttpClient("ASF", (serviceProvider, httpClient) => {
            var config = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(BuildInfo.AppName, BuildInfo.Version));
            httpClient.Timeout = TimeSpan.FromSeconds(10);
        }).SetHandlerLifetime(Timeout.InfiniteTimeSpan).RemoveAllLoggers();

        services.AddHttpClient("Statistic", (serviceProvider, httpClient) => {
            var config = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
            httpClient.BaseAddress = new Uri("https://asfe.chrxw.com/");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(BuildInfo.AppName, BuildInfo.Version));
            httpClient.Timeout = TimeSpan.FromSeconds(10);
        }).SetHandlerLifetime(Timeout.InfiniteTimeSpan).RemoveAllLoggers();
    }
}
