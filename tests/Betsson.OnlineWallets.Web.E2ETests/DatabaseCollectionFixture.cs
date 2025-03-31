namespace Betsson.OnlineWallets.Web.E2ETests;

[CollectionDefinition("Database", DisableParallelization = true)]
public class DatabaseCollectionFixture : ICollectionFixture<PostgreSqlTestFixture>
{
    // Just a marker for the collection
}
