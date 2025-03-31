using Betsson.OnlineWallets.Exceptions;
using Betsson.OnlineWallets.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Betsson.OnlineWallets.Web.E2ETests;

[Collection("Database")]
public class OnlineWalletControllerTests : IAsyncLifetime, IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly PostgreSqlTestFixture _fixture;
    private readonly HttpClient _client;

    public OnlineWalletControllerTests(PostgreSqlTestFixture fixture, CustomWebApplicationFactory<Program> factory)
    {
        _fixture = fixture;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Balance_ShouldReturnOk_AndZeroAmount_WhenNoTransactionExists()
    {
        // Act
        var response = await _client.GetAsync("onlinewallet/balance");

        // Assert
        response.EnsureSuccessStatusCode();

        var balance = await response.Content.ReadFromJsonAsync<BalanceResponse>();
        balance.ShouldNotBeNull();
        balance.Amount.ShouldBe(0);
    }

    [Fact]
    public async Task Balance_ShouldReturnOk_AndCorrectAmount_WhenTransactionExists()
    {
        // Arrange
        var entry = await new OnlineWalletEntryTestDataBuilder()
            .WithAmount(50)
            .WithBalanceBefore(100)
            .BuildAsync(_fixture.Context);

        // Act
        var response = await _client.GetAsync("onlinewallet/balance");

        // Assert
        response.EnsureSuccessStatusCode();

        var balance = await response.Content.ReadFromJsonAsync<BalanceResponse>();
        balance.ShouldNotBeNull();
        balance.Amount.ShouldBe(150);
    }

    [Fact]
    public async Task Deposit_ShouldReturnOk_AndIncreaseBalance_WhenDepositRequestIsValid()
    {
        // Arrange
        var depositRequest = new DepositRequest { Amount = 100 };

        // Act
        var response = await _client.PostAsJsonAsync("/onlinewallet/deposit", depositRequest);

        // Assert
        response.EnsureSuccessStatusCode();

        var balance = await response.Content.ReadFromJsonAsync<BalanceResponse>();
        balance.ShouldNotBeNull();
        balance.Amount.ShouldBe(100);
    }

    [Fact]
    public async Task Deposit_ShouldReturnBadRequest_AndThrowException_WhenDepositRequestIsNegativeAmount()
    {
        // Arrange
        var depositRequest = new DepositRequest { Amount = -100 };

        // Act
        var response = await _client.PostAsJsonAsync("/onlinewallet/deposit", depositRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.ShouldNotBeNull();
        problemDetails.Title.ShouldBe("One or more validation errors occurred.");

        var errorsElement = (JsonElement)problemDetails.Extensions["errors"]!;
        errorsElement.TryGetProperty("Amount", out var amountErrors).ShouldBeTrue();

        amountErrors.ValueKind.ShouldBe(JsonValueKind.Array);
        amountErrors.GetArrayLength().ShouldBeGreaterThan(0);

        var firstError = amountErrors[0].GetString();
        firstError.ShouldBe("'Amount' must be greater than or equal to '0'.");
    }

    [Fact]
    public async Task Withdraw_ShouldDecreaseBalance_WhenBalanceIsSufficient()
    {
        // Arrange
        var entry = await new OnlineWalletEntryTestDataBuilder()
            .WithAmount(50)
            .WithBalanceBefore(100)
            .BuildAsync(_fixture.Context);

        var withdrawRequest = new WithdrawalRequest { Amount = 75 };

        // Act
        var response = await _client.PostAsJsonAsync("/onlinewallet/withdraw", withdrawRequest);

        // Assert
        response.EnsureSuccessStatusCode();

        var balance = await response.Content.ReadFromJsonAsync<BalanceResponse>();
        balance.ShouldNotBeNull();
        balance.Amount.ShouldBe(75);
    }

    [Fact]
    public async Task Withdraw_ShouldReturnBadRequest_AndThrowException_WhenWithdrawalRequestIsNegativeAmount()
    {
        // Arrange
        var entry = await new OnlineWalletEntryTestDataBuilder()
            .WithAmount(50)
            .WithBalanceBefore(100)
            .BuildAsync(_fixture.Context);

        var withdrawRequest = new WithdrawalRequest { Amount = -75 };

        // Act
        var response = await _client.PostAsJsonAsync("/onlinewallet/withdraw", withdrawRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.ShouldNotBeNull();
        problemDetails.Title.ShouldBe("One or more validation errors occurred.");

        var errorsElement = (JsonElement)problemDetails.Extensions["errors"]!;
        errorsElement.TryGetProperty("Amount", out var amountErrors).ShouldBeTrue();

        amountErrors.ValueKind.ShouldBe(JsonValueKind.Array);
        amountErrors.GetArrayLength().ShouldBeGreaterThan(0);

        var firstError = amountErrors[0].GetString();
        firstError.ShouldBe("'Amount' must be greater than or equal to '0'.");
    }

    [Fact]
    public async Task Withdraw_ShouldThrowInsufficientBalanceException_WhenBalanceIsInsufficient()
    {
        // Arrange
        var withdrawRequest = new WithdrawalRequest { Amount = 100 };

        // Act
        var response = await _client.PostAsJsonAsync("/onlinewallet/withdraw", withdrawRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.ShouldNotBeNull();
        problemDetails.Type.ShouldBe(nameof(InsufficientBalanceException));
        problemDetails.Title.ShouldBe("Invalid withdrawal amount. There are insufficient funds.");
    }
}
