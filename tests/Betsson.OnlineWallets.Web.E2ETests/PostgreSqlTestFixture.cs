using Betsson.OnlineWallets.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace Betsson.OnlineWallets.Web.E2ETests;

public class PostgreSqlTestFixture : IAsyncLifetime
{
    internal OnlineWalletContext Context { get; private set; } = null!;
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private readonly ILogger<PostgreSqlTestFixture> _logger;

    public PostgreSqlTestFixture()
    {
        _logger = TestLoggingConfiguration.CreateLogger<PostgreSqlTestFixture>();

        _logger.LogInformation("Init PostgreSQL Testcontainer...");

        var postgresConfiguration = new PostgreSqlConfiguration("testdb", "postgres", "postgres");

        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithDatabase(postgresConfiguration.Database)
            .WithImage("postgres:16-alpine")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Starting PostgreSQL Testcontainer...");
            await _postgreSqlContainer.StartAsync();

            var connectionString = _postgreSqlContainer.GetConnectionString();
            _logger.LogInformation($"Testcontainer started. ConnectionString: {connectionString}");

            Environment.SetEnvironmentVariable("CONNECTION_STRING", connectionString);

            var options = new DbContextOptionsBuilder<OnlineWalletContext>()
                .UseNpgsql(connectionString)
                .Options;

            Context = new OnlineWalletContext(options);

            await RecreateDatabaseAsync();
            _logger.LogInformation("Database schema created.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during {nameof(InitializeAsync)}");
            throw;
        }
    }

    public async Task ResetDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Resetting database to a clean state...");
            await RecreateDatabaseAsync();
            _logger.LogInformation("Database reset complete.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during {nameof(ResetDatabaseAsync)}");
            throw;
        }
    }

    private async Task RecreateDatabaseAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        try
        {
            _logger.LogInformation("Stopping and disposing PostgreSQL Testcontainer...");
            await _postgreSqlContainer.StopAsync();
            await _postgreSqlContainer.DisposeAsync();
            Context?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PostgreSQL Testcontainer disposal.");
            throw;
        }
    }
}
