using Betsson.OnlineWallets.Data.Models;
using Betsson.OnlineWallets.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Betsson.OnlineWallets.Data.IntegrationTests;

public class OnlineWalletRepositoryTests
{
    private OnlineWalletContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OnlineWalletContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OnlineWalletContext(options);
    }

    [Fact]
    public async Task GetLastOnlineWalletEntryAsync_ShouldReturnLatestEntry()
    {
        // Arrange
        using var context = CreateContext();
        var onlineWalletRepository = new OnlineWalletRepository(context);

        var olderEntry = new OnlineWalletEntry { EventTime = DateTimeOffset.UtcNow.AddHours(-5) };
        var newerEntry = new OnlineWalletEntry { EventTime = DateTimeOffset.UtcNow };
        context.Transactions.AddRange(olderEntry, newerEntry);
        await context.SaveChangesAsync();

        // Act
        var lastEntry = await onlineWalletRepository.GetLastOnlineWalletEntryAsync();

        // Assert
        lastEntry.ShouldNotBeNull();
        lastEntry.ShouldBe(newerEntry);
        lastEntry.EventTime.ShouldBeGreaterThan(olderEntry.EventTime);
    }

    [Fact]
    public async Task GetLastOnlineWalletEntryAsync_ShouldReturnSingleEntry_WhenOnlyOneEntryExists()
    {
        // Arrange
        using var context = CreateContext();
        var onlineWalletRepository = new OnlineWalletRepository(context);

        var entry = new OnlineWalletEntry { EventTime = DateTimeOffset.UtcNow };
        context.Transactions.AddRange(entry);
        await context.SaveChangesAsync();

        // Act
        var lastEntry = await onlineWalletRepository.GetLastOnlineWalletEntryAsync();

        // Assert
        lastEntry.ShouldNotBeNull();
        lastEntry.ShouldBe(entry);
        lastEntry.EventTime.ShouldBe(entry.EventTime);
    }

    [Fact]
    public async Task GetLastOnlineWalletEntryAsync_ShouldReturnOneOfEntries_WhenMultipleEntriesHaveSameEventTime()
    {
        // Arrange
        using var context = CreateContext();
        var onlineWalletRepository = new OnlineWalletRepository(context);

        var commonTime = DateTimeOffset.UtcNow;
        var entry1 = new OnlineWalletEntry { EventTime = commonTime };
        var entry2 = new OnlineWalletEntry { EventTime = commonTime };
        context.Transactions.AddRange(entry1, entry2);
        await context.SaveChangesAsync();

        // Act
        var lastEntry = await onlineWalletRepository.GetLastOnlineWalletEntryAsync();

        // Assert
        lastEntry.ShouldNotBeNull();
        lastEntry.EventTime.ShouldBe(commonTime);
        (lastEntry == entry1 || lastEntry == entry2).ShouldBeTrue();
    }

    [Fact]
    public async Task GetLastOnlineWalletEntryAsync_ShouldReturnNull_WhenNoEntiresExist()
    {
        // Arrange
        using var context = CreateContext();
        var onlineWalletRepository = new OnlineWalletRepository(context);

        // Act
        var lastEntry = await onlineWalletRepository.GetLastOnlineWalletEntryAsync();

        // Assert
        lastEntry.ShouldBeNull();
    }

    [Fact]
    public async Task InsertOnlineWalletEntryAsync_ShouldInsertEntry_AndPersistIt()
    {
        // Arrange
        using var context = CreateContext();
        var onlineWalletRepository = new OnlineWalletRepository(context);

        var newEntry = new OnlineWalletEntry { EventTime = DateTimeOffset.UtcNow };

        // Act
        await onlineWalletRepository.InsertOnlineWalletEntryAsync(newEntry);

        // Assert
        var entryInDb = context.Transactions.FirstOrDefault(e => e.EventTime == newEntry.EventTime);
        entryInDb.ShouldNotBeNull();
    }

    [Fact]
    public async Task InsertOnlineWalletEntryAsync_ShouldHandleMultipleSequentialInsertions()
    {
        // Arrange
        using var context = CreateContext();
        var onlineWalletRepository = new OnlineWalletRepository(context);

        var entries = new[]
        {
            new OnlineWalletEntry { EventTime = DateTimeOffset.UtcNow.AddMinutes(-10) },
            new OnlineWalletEntry { EventTime = DateTimeOffset.UtcNow },
            new OnlineWalletEntry { EventTime = DateTimeOffset.UtcNow.AddMinutes(10) }
        };

        // Act
        foreach (var entry in entries)
        {
            await onlineWalletRepository.InsertOnlineWalletEntryAsync(entry);
        }

        // Assert
        var lastEntry = await onlineWalletRepository.GetLastOnlineWalletEntryAsync();
        lastEntry.ShouldNotBeNull();
        lastEntry.ShouldBe(entries.Last());
    }

    [Fact]
    public async Task InsertOnlineWalletEntryAsync_ShouldThrowNullReferenceException_WhenEntryIsNull()
    {
        // Arrange
        using var context = CreateContext();
        var onlineWalletRepository = new OnlineWalletRepository(context);

        // Act & Assert
        await Should.ThrowAsync<NullReferenceException>(
            () => onlineWalletRepository.InsertOnlineWalletEntryAsync(null!));
    }
}
