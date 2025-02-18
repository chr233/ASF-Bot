using System.Reflection;
using System.Runtime.Versioning;

namespace ASF_Bot.Infrastructure;

/// <summary>
/// 编译信息
/// </summary>
public static class BuildInfo
{
    static BuildInfo()
    {
        var assembly = Assembly.GetExecutingAssembly() ?? throw new NullReferenceException("Assembly.GetExecutingAssembly() is null");

        AppPath = assembly.Location ?? AppContext.BaseDirectory;
        AppDir = !string.IsNullOrEmpty(AppPath) ? Directory.GetParent(AppPath)?.FullName ?? "." : AppContext.BaseDirectory;

        AppName = "ASF-Bot";

        Version = assembly.GetName().Version?.ToString()!;
        Company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company!;
        Copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright!;
        Configuration = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration!;
        FrameworkName = assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkDisplayName!;
        Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description!;
    }

    public static string AppDir { get; }
    public static string AppPath { get; }
    public static string AppName { get; }
    public static string Version { get; }
    public static string Company { get; }
    public static string Copyright { get; }
    public static string Configuration { get; }
    public static string FrameworkName { get; }
    public static string Description { get; }

    /// <summary>
    /// 是否为调试模式
    /// </summary>
#if DEBUG
    public const bool IsDebug = true;
#else
    public const bool IsDebug = false;
#endif
}
