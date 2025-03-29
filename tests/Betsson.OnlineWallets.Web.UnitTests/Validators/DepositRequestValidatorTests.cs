using Betsson.OnlineWallets.Web.Models;
using Betsson.OnlineWallets.Web.Validators;
using FluentValidation.TestHelper;

namespace Betsson.OnlineWallets.Web.UnitTests.Validators;

public class DepositRequestValidatorTests
{
    private readonly DepositRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Amount_Is_Negative()
    {
        // Arrange
        var model = new DepositRequest { Amount = -10 };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Amount_Is_Positive()
    {
        // Arrange
        var model = new DepositRequest { Amount = 10 };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }
}
