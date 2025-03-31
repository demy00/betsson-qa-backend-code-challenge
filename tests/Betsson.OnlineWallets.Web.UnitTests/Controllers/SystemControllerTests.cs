using Betsson.OnlineWallets.Exceptions;
using Betsson.OnlineWallets.Web.Controllers;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Betsson.OnlineWallets.Web.UnitTests.Controllers;

public class SystemControllerTests
{
    [Fact]
    public void Error_ShouldReturnProblemDetails_ForInsufficientBalanceException()
    {
        // Arrange
        var fakeException = new InsufficientBalanceException();
        var exceptionFeature = new ExceptionHandlerFeature { Error = fakeException };

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IExceptionHandlerPathFeature>(exceptionFeature);

        var controller = new SystemController();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = controller.Error();

        // Assert
        var objectResult = result as ObjectResult;
        objectResult.ShouldNotBeNull();

        var problemDetails = objectResult.Value as ProblemDetails;
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problemDetails.Title.ShouldBe("Invalid withdrawal amount. There are insufficient funds.");
        problemDetails.Type.ShouldBe(nameof(InsufficientBalanceException));
    }

    [Fact]
    public void Error_ShouldReturnGenericProblemDetails_WhenNoExceptionFeatureIsPresent()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        var controller = new SystemController();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = controller.Error();

        // Assert
        var objectResult = result as ObjectResult;
        objectResult.ShouldNotBeNull();

        var problemDetails = objectResult.Value as ProblemDetails;
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status500InternalServerError);
    }
}
