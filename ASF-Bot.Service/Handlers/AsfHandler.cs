using ASF_Bot.Infrastructure;
using ASF_Bot.Service.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;


namespace ASF_Bot.Service.Handlers;
public sealed class AsfHandler
{
    private readonly ILogger<AsfHandler> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ImmutableArray<IpcService> _services;

    public AsfHandler(
        ILogger<AsfHandler> logger,
        IServiceProvider serviceProvider,
        IOptions<AppSettings> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var configSet = options.Value.AsfIpcs;

        if (configSet.Count == 0)
        {
            _logger.LogWarning("未配置任何 ASF IPC");
            return;
        }

        List<IpcService> services = [];
        int id = 1;

        foreach (var config in configSet)
        {
            var service = _serviceProvider.GetRequiredService<IpcService>();
            try
            {
                service.Setup(id++, config);
                services.Add(service);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ipc 配置 {name} 无效: {msg}", id, ex.Message);
            }
        }

        _services = [.. services];
    }

    public IpcService? GetSprcifyApiService(int id)
    {
        return id < 0 || id >= _services.Length ? null : _services[id];
    }

    public ImmutableArray<IpcService> GetAllApiService()
    {
        return _services;
    }

    public int IpcCount => _services.Length;
}
