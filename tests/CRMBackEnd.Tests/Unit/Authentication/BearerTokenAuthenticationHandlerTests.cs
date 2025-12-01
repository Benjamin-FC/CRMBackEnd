using CRMBackEnd.API.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace CRMBackEnd.Tests.Unit.Authentication;

public class BearerTokenAuthenticationHandlerTests
{
    private readonly Mock<IOptionsMonitor<AuthenticationSchemeOptions>> _optionsMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ILogger<BearerTokenAuthenticationHandler>> _loggerMock;
    private readonly Mock<UrlEncoder> _encoderMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly DefaultHttpContext _httpContext;

    public BearerTokenAuthenticationHandlerTests()
    {
        _optionsMock = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerMock = new Mock<ILogger<BearerTokenAuthenticationHandler>>();
        _encoderMock = new Mock<UrlEncoder>();
        _configurationMock = new Mock<IConfiguration>();
        _httpContext = new DefaultHttpContext();

        _optionsMock.Setup(x => x.Get(It.IsAny<string>()))
            .Returns(new AuthenticationSchemeOptions());

        _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(_loggerMock.Object);
    }

    private BearerTokenAuthenticationHandler CreateHandler(string validToken = "123")
    {
        _configurationMock.Setup(x => x["Authentication:BearerToken"])
            .Returns(validToken);

        var handler = new BearerTokenAuthenticationHandler(
            _optionsMock.Object,
            _loggerFactoryMock.Object,
            _encoderMock.Object,
            _configurationMock.Object);

        handler.InitializeAsync(
            new AuthenticationScheme("Bearer", null, typeof(BearerTokenAuthenticationHandler)),
            _httpContext).GetAwaiter().GetResult();

        return handler;
    }

    [Fact]
    public async Task HandleAuthenticateAsync_ValidToken_ReturnsSuccess()
    {
        // Arrange
        var handler = CreateHandler("123");
        _httpContext.Request.Headers["Authorization"] = "Bearer 123";

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Principal.Should().NotBeNull();
        result.Principal!.Identity!.IsAuthenticated.Should().BeTrue();
        result.Principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "API User");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_InvalidToken_ReturnsFailure()
    {
        // Arrange
        var handler = CreateHandler("123");
        _httpContext.Request.Headers["Authorization"] = "Bearer invalid_token";

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure!.Message.Should().Be("Invalid token");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_MissingAuthorizationHeader_ReturnsFailure()
    {
        // Arrange
        var handler = CreateHandler("123");
        // No Authorization header set

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure!.Message.Should().Be("Missing Authorization header");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_InvalidHeaderFormat_ReturnsFailure()
    {
        // Arrange
        var handler = CreateHandler("123");
        _httpContext.Request.Headers["Authorization"] = "Basic 123"; // Not Bearer

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure!.Message.Should().Be("Invalid Authorization header format");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_TokenWithWhitespace_TrimsAndValidates()
    {
        // Arrange
        var handler = CreateHandler("123");
        _httpContext.Request.Headers["Authorization"] = "Bearer   123   ";

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Principal.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_EmptyToken_ReturnsFailure()
    {
        // Arrange
        var handler = CreateHandler("123");
        _httpContext.Request.Headers["Authorization"] = "Bearer ";

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure!.Message.Should().Be("Invalid token");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_CaseInsensitiveBearerScheme_ReturnsSuccess()
    {
        // Arrange
        var handler = CreateHandler("123");
        _httpContext.Request.Headers["Authorization"] = "bearer 123"; // lowercase

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_CustomTokenFromConfig_ValidatesCorrectly()
    {
        // Arrange
        var customToken = "custom_secret_token_456";
        var handler = CreateHandler(customToken);
        _httpContext.Request.Headers["Authorization"] = $"Bearer {customToken}";

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Principal.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_NoConfigToken_UsesDefault123()
    {
        // Arrange
        _configurationMock.Setup(x => x["Authentication:BearerToken"])
            .Returns((string)null!); // No config value

        var handler = new BearerTokenAuthenticationHandler(
            _optionsMock.Object,
            _loggerFactoryMock.Object,
            _encoderMock.Object,
            _configurationMock.Object);

        handler.InitializeAsync(
            new AuthenticationScheme("Bearer", null, typeof(BearerTokenAuthenticationHandler)),
            _httpContext).GetAwaiter().GetResult();

        _httpContext.Request.Headers["Authorization"] = "Bearer 123";

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
    }
}
