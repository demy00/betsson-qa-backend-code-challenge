using AutoMapper;
using Betsson.OnlineWallets.Models;
using Betsson.OnlineWallets.Web.Mappers;
using Betsson.OnlineWallets.Web.Models;
using Shouldly;

namespace Betsson.OnlineWallets.Web.UnitTests.Mappers;

public class OnlineWalletMappingProfileTests
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _config;

    public OnlineWalletMappingProfileTests()
    {
        _config = new MapperConfiguration(cfg => cfg.AddProfile<OnlineWalletMappingProfile>());

        _mapper = _config.CreateMapper();
    }

    [Fact]
    public void MappingProfile_Configuration_IsValid()
    {
        // Fails if mapping is misconfigured
        _config.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_BalanceToBalanceResponse_ShouldMapAmount_WhenMappingIsDone()
    {
        // Arrange
        var balance = new Balance { Amount = 100 };

        // Act
        var balanceResponse = _mapper.Map<BalanceResponse>(balance);

        // Assert
        balanceResponse.Amount.ShouldBe(balance.Amount);
    }

    [Fact]
    public void Map_DepositRequestToDeposit_ShouldMapAmount_WhenMappingIsDone()
    {
        // Arrange
        var depositRequest = new DepositRequest { Amount = 100 };

        // Act
        var deposit = _mapper.Map<Deposit>(depositRequest);

        // Assert
        deposit.Amount.ShouldBe(depositRequest.Amount);
    }

    [Fact]
    public void Map_WithdrawalRequestToWithdrawal_ShouldMapAmount_WhenMappingIsDone()
    {
        // Arrange
        var withdrawalRequest = new WithdrawalRequest { Amount = 100 };

        // Act
        var withdrawal = _mapper.Map<Withdrawal>(withdrawalRequest);

        // Assert
        withdrawal.Amount.ShouldBe(withdrawalRequest.Amount);
    }
}
