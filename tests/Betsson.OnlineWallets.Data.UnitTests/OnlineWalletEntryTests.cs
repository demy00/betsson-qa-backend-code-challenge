using Betsson.OnlineWallets.Data.Models;
using Shouldly;

namespace Betsson.OnlineWallets.Data.UnitTests;

public class OnlineWalletEntryTests
{
    [Fact]
    public void Constructor_ShouldSetId_AndSetEventTime()
    {
        // Act
        var entity = new OnlineWalletEntry();

        // Assert
        entity.Id.ShouldNotBeNullOrEmpty();
        entity.EventTime.ShouldNotBe(default(DateTimeOffset));
    }
}