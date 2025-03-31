using Microsoft.Extensions.Logging;

namespace Betsson.OnlineWallets.Web.E2ETests;

public static class TestLoggingConfiguration
{
    public static void Configure(ILoggingBuilder builder)
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Debug);
    }

    public static ILogger<T> CreateLogger<T>()
    {
        var factory = LoggerFactory.Create(Configure);
        return factory.CreateLogger<T>();
    }
}
