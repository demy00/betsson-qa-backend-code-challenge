using Betsson.OnlineWallets.Data.Models;
using Betsson.OnlineWallets.Data.Repositories;
using Betsson.OnlineWallets.Exceptions;
using Betsson.OnlineWallets.Models;
using Betsson.OnlineWallets.Services;
using Moq;
using Shouldly;

namespace Betsson.OnlineWallets.UnitTests;

public class OnlineWalletServiceTests
{
    [Fact]
    public async Task GetBalanceAsync_ShouldReturnCorrectBalance_WhenTransactionExists()
    {
        // Arrange
        var existingEntry = new OnlineWalletEntry
        {
            Amount = 50,
            BalanceBefore = 100
        };

        var onlineWalletRepoMock = new Mock<IOnlineWalletRepository>();
        onlineWalletRepoMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
            .ReturnsAsync(existingEntry);

        var service = new OnlineWalletService(onlineWalletRepoMock.Object);

        // Act
        var balance = await service.GetBalanceAsync();

        // Assert
        balance.ShouldNotBeNull();
        balance.Amount.ShouldBe(150);
    }

    [Fact]
    public async Task GetBalanceAsync_ShouldReturnZeroBalance_WhenNoTransactionExists()
    {
        // Arrange
        var onlineWalletRepoMock = new Mock<IOnlineWalletRepository>();
        onlineWalletRepoMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
            .ReturnsAsync((OnlineWalletEntry?)null);

        var service = new OnlineWalletService(onlineWalletRepoMock.Object);

        // Act
        var balance = await service.GetBalanceAsync();

        // Assert
        balance.ShouldNotBeNull();
        balance.Amount.ShouldBe(0);
        onlineWalletRepoMock.Verify(r => r.GetLastOnlineWalletEntryAsync(), Times.Once);
    }

    [Fact]
    public async Task GetBalanceAsync_ShouldThrowException_WhenRepositoryThrows()
    {
        // Arrange
        var onlineWalletRepoMock = new Mock<IOnlineWalletRepository>();
        onlineWalletRepoMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
            .ThrowsAsync(new Exception("Test exception"));

        var service = new OnlineWalletService(onlineWalletRepoMock.Object);

        // Act & Assert
        await Should.ThrowAsync<Exception>(() => service.GetBalanceAsync());
    }

    [Fact]
    public async Task DepositFundsAsync_ShouldIncreaseBalanceByDepositAmount()
    {
        // Arrange
        var existingEntry = new OnlineWalletEntry
        {
            Amount = 50,
            BalanceBefore = 100
        };

        var onlineWalletRepoMock = new Mock<IOnlineWalletRepository>();
        onlineWalletRepoMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
            .ReturnsAsync(existingEntry);

        var service = new OnlineWalletService(onlineWalletRepoMock.Object);
        var deposit = new Deposit { Amount = 75 };

        // Act
        var newBalance = await service.DepositFundsAsync(deposit);

        // Assert
        newBalance.ShouldNotBeNull();
        newBalance.Amount.ShouldBe(225);
        onlineWalletRepoMock.Verify(r => r.InsertOnlineWalletEntryAsync(It.Is<OnlineWalletEntry>(
            entry => entry.Amount == 75 && entry.BalanceBefore == 150)), Times.Once);
    }

    [Fact]
    public async Task DepositFundsAsync_ShouldReturnDepositAmount_WhenNoTransactionExists()
    {
        // Arrange
        var onlineWalletRepoMock = new Mock<IOnlineWalletRepository>();
        onlineWalletRepoMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
            .ReturnsAsync((OnlineWalletEntry?)null);

        var service = new OnlineWalletService(onlineWalletRepoMock.Object);
        var deposit = new Deposit { Amount = 100 };

        // Act
        var newBalance = await service.DepositFundsAsync(deposit);

        // Assert
        newBalance.ShouldNotBeNull();
        newBalance.Amount.ShouldBe(100);
        onlineWalletRepoMock.Verify(r => r.InsertOnlineWalletEntryAsync(It.Is<OnlineWalletEntry>(
            entry => entry.Amount == 100 && entry.BalanceBefore == 0)), Times.Once);
    }

    [Fact]
    public async Task DepositFundsAsync_ShouldReturnSameBalance_WhenDepositIsZero()
    {
        // Arrange
        var existingEntry = new OnlineWalletEntry
        {
            Amount = 50,
            BalanceBefore = 100
        };

        var onlineWalletRepoMock = new Mock<IOnlineWalletRepository>();
        onlineWalletRepoMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
            .ReturnsAsync(existingEntry);

        var service = new OnlineWalletService(onlineWalletRepoMock.Object);
        var deposit = new Deposit { Amount = 0 };

        // Act
        var newBalance = await service.DepositFundsAsync(deposit);

        // Assert
        newBalance.ShouldNotBeNull();
        newBalance.Amount.ShouldBe(150);
        onlineWalletRepoMock.Verify(r => r.InsertOnlineWalletEntryAsync(It.Is<OnlineWalletEntry>(
            entry => entry.Amount == 0 && entry.BalanceBefore == 150)), Times.Once);
    }

    [Fact]
    public async Task DepositFundsAsync_ShouldThrowException_WhenRepositoryThrowsOnInsert()
    {
        // Arrange
        var existingEntry = new OnlineWalletEntry
        {
            Amount = 50,
            BalanceBefore = 100
        };

        var onlineWalletRepoMock = new Mock<IOnlineWalletRepository>();
        onlineWalletRepoMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
            .ReturnsAsync(existingEntry);
        onlineWalletRepoMock.Setup(r => r.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>()))
            .ThrowsAsync(new Exception("Deposit insert exception"));

        var service = new OnlineWalletService(onlineWalletRepoMock.Object);
        var deposit = new Deposit { Amount = 25 };

        // Act & Assert
        await Should.ThrowAsync<Exception>(() => service.DepositFundsAsync(deposit));
    }

    [Fact]
    public async Task DepositFundsAsync_ShouldSetEventTimeWithinRecentRange()
    {
        // Arrange
        var existingEntry = new OnlineWalletEntry
        {
            Amount = 50,
            BalanceBefore = 100
        };

        var onlineWalletRepoMock = new Mock<IOnlineWalletRepository>();
        onlineWalletRepoMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
            .ReturnsAsync(existingEntry);

        OnlineWalletEntry? capcuredEntity = null;
        onlineWalletRepoMock.Setup(r => r.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>()))
            .Callback<OnlineWalletEntry>(entry => capcuredEntity = entry)
            .Returns(Task.CompletedTask);

        var service = new OnlineWalletService(onlineWalletRepoMock.Object);
        var deposit = new Deposit { Amount = 25 };

        // Act
        var newBalance = await service.DepositFundsAsync(deposit);

        // Assert
        capcuredEntity.ShouldNotBeNull();
        capcuredEntity.EventTime.ShouldBeInRange(DateTimeOffset.UtcNow.AddSeconds(-2), DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task WithdrawFundsAsync_ShouldDecreaseBalance_WhenBalanceIsSufficient()
    {
        // Arrange
        var existingEntry = new OnlineWalletEntry
        {
            Amount = 0,
            BalanceBefore = 100
        };

        var onlineWalletRepoMock = new Mock<IOnlineWalletRepository>();
        onlineWalletRepoMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
            .ReturnsAsync(existingEntry);

        var service = new OnlineWalletService(onlineWalletRepoMock.Object);
        var withdrawal = new Withdrawal { Amount = 25 };

        // Act
        var newBalance = await service.WithdrawFundsAsync(withdrawal);

        // Assert
        newBalance.ShouldNotBeNull();
        newBalance.Amount.ShouldBe(75);
        onlineWalletRepoMock.Verify(r => r.InsertOnlineWalletEntryAsync(It.Is<OnlineWalletEntry>(
            entry => entry.Amount == -25 && entry.BalanceBefore == 100)), Times.Once);
    }

    [Fact]
    public async Task WithdrawFundsAsync_ShouldReturnZeroBalance_WhenWithdrawalEqualsCurrentBalance()
    {
        // Arrange
        var existingEntry = new OnlineWalletEntry
        {
            Amount = 0,
            BalanceBefore = 100
        };

        var onlineWalletRepoMock = new Mock<IOnlineWalletRepository>();
        onlineWalletRepoMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
            .ReturnsAsync(existingEntry);

        var service = new OnlineWalletService(onlineWalletRepoMock.Object);
        var withdrawal = new Withdrawal { Amount = 100 };

        // Act
        var newBalance = await service.WithdrawFundsAsync(withdrawal);

        // Assert
        newBalance.ShouldNotBeNull();
        newBalance.Amount.ShouldBe(0);
        onlineWalletRepoMock.Verify(r => r.InsertOnlineWalletEntryAsync(It.Is<OnlineWalletEntry>(
            entry => entry.Amount == -100 && entry.BalanceBefore == 100)), Times.Once);
    }

    [Fact]
    public async Task WithdrawFundsAsync_ShouldReturnSameBalance_WhenWithdrawalIsZero()
    {
        // Arrange
        var existingEntry = new OnlineWalletEntry
        {
            Amount = 0,
            BalanceBefore = 100
        };

        var onlineWalletRepoMock = new Mock<IOnlineWalletRepository>();
        onlineWalletRepoMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
            .ReturnsAsync(existingEntry);

        var service = new OnlineWalletService(onlineWalletRepoMock.Object);
        var withdrawal = new Withdrawal { Amount = 0 };

        // Act
        var newBalance = await service.WithdrawFundsAsync(withdrawal);

        // Assert
        newBalance.ShouldNotBeNull();
        newBalance.Amount.ShouldBe(100);
        onlineWalletRepoMock.Verify(r => r.InsertOnlineWalletEntryAsync(It.Is<OnlineWalletEntry>(
            entry => entry.Amount == 0 && entry.BalanceBefore == 100)), Times.Once);
    }

    [Fact]
    public async Task WithdrawFundsAsync_ShouldThrowInsufficientBalanceException_WhenBalanceIsInsufficient()
    {
        // Arrange
        var existingEntry = new OnlineWalletEntry
        {
            Amount = 0,
            BalanceBefore = 100
        };

        var onlineWalletRepoMock = new Mock<IOnlineWalletRepository>();
        onlineWalletRepoMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
            .ReturnsAsync(existingEntry);

        var service = new OnlineWalletService(onlineWalletRepoMock.Object);
        var withdrawal = new Withdrawal { Amount = 125 };

        // Act & Assert
        await Should.ThrowAsync<InsufficientBalanceException>(
            () => service.WithdrawFundsAsync(withdrawal));
    }

    [Fact]
    public async Task WithdrawFundsAsync_ShouldThrowInsufficientBalanceException_WhenNoTransactionExists()
    {
        // Arrange
        var onlineWalletRepoMock = new Mock<IOnlineWalletRepository>();
        onlineWalletRepoMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
            .ReturnsAsync((OnlineWalletEntry?)null);

        var service = new OnlineWalletService(onlineWalletRepoMock.Object);
        var withdrawal = new Withdrawal { Amount = 10 };

        // Act & Assert
        await Should.ThrowAsync<InsufficientBalanceException>(
            () => service.WithdrawFundsAsync(withdrawal));
    }

    [Fact]
    public async Task WithdrawFundsAsync_ShouldThrowException_WhenRepositoryThrowsOnInsert()
    {
        // Arrange
        var existingEntry = new OnlineWalletEntry
        {
            Amount = 0,
            BalanceBefore = 100
        };

        var onlineWalletRepoMock = new Mock<IOnlineWalletRepository>();
        onlineWalletRepoMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
            .ReturnsAsync(existingEntry);
        onlineWalletRepoMock.Setup(r => r.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>()))
            .ThrowsAsync(new Exception("Withdrawal insert exception"));

        var service = new OnlineWalletService(onlineWalletRepoMock.Object);
        var withdrawal = new Withdrawal { Amount = 25 };

        // Act & Assert
        await Should.ThrowAsync<Exception>(() => service.WithdrawFundsAsync(withdrawal));
    }

    [Fact]
    public async Task WithdrawFundsAsync_ShouldSetEventTimeWithinRecentRange()
    {
        // Arrange
        var existingEntry = new OnlineWalletEntry
        {
            Amount = 50,
            BalanceBefore = 100
        };

        var onlineWalletRepoMock = new Mock<IOnlineWalletRepository>();
        onlineWalletRepoMock.Setup(r => r.GetLastOnlineWalletEntryAsync())
            .ReturnsAsync(existingEntry);

        OnlineWalletEntry? capcuredEntity = null;
        onlineWalletRepoMock.Setup(r => r.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>()))
            .Callback<OnlineWalletEntry>(entry => capcuredEntity = entry)
            .Returns(Task.CompletedTask);

        var service = new OnlineWalletService(onlineWalletRepoMock.Object);
        var deposit = new Withdrawal { Amount = 25 };

        // Act
        var newBalance = await service.WithdrawFundsAsync(deposit);

        // Assert
        capcuredEntity.ShouldNotBeNull();
        capcuredEntity.EventTime.ShouldBeInRange(DateTimeOffset.UtcNow.AddSeconds(-2), DateTimeOffset.UtcNow);
    }
}
