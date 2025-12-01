using CRMBackEnd.API.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace CRMBackEnd.Tests.Unit.Authentication;

public class CrmTokenServiceTests
{
    private readonly Mock<ILogger<CrmTokenService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;

    public CrmTokenServiceTests()
    {
        _mockLogger = new Mock<ILogger<CrmTokenService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        // Setup default configuration values
        _mockConfiguration.Setup(x => x["CrmAuthentication:TokenUrl"]).Returns("https://test.com/token");
        _mockConfiguration.Setup(x => x["CrmAuthentication:ClientId"]).Returns("test-client");
        _mockConfiguration.Setup(x => x["CrmAuthentication:ClientSecret"]).Returns("test-secret");
        _mockConfiguration.Setup(x => x["CrmAuthentication:Scope"]).Returns("test-scope");
    }

    [Fact]
    public async Task GetTokenAsync_WithValidCredentials_ReturnsAccessToken()
    {
        // Arrange
        Environment.SetEnvironmentVariable("CRM_USERNAME", "test-user");
        Environment.SetEnvironmentVariable("CRM_PASSWORD", "test-password");

        var tokenResponse = new
        {
            access_token = "test-access-token-12345",
            expires_in = 3600,
            token_type = "Bearer"
        };

        var responseContent = new StringContent(JsonSerializer.Serialize(tokenResponse));
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        var sut = new CrmTokenService(_httpClient, _mockLogger.Object, _mockConfiguration.Object);

        // Act
        var result = await sut.GetTokenAsync();

        // Assert
        result.Should().Be("test-access-token-12345");

        // Cleanup
        Environment.SetEnvironmentVariable("CRM_USERNAME", null);
        Environment.SetEnvironmentVariable("CRM_PASSWORD", null);
    }

    [Fact]
    public async Task GetTokenAsync_CalledMultipleTimes_ReturnsCachedToken()
    {
        // Arrange
        Environment.SetEnvironmentVariable("CRM_USERNAME", "test-user");
        Environment.SetEnvironmentVariable("CRM_PASSWORD", "test-password");

        var tokenResponse = new
        {
            access_token = "cached-token",
            expires_in = 3600,
            token_type = "Bearer"
        };

        var responseContent = new StringContent(JsonSerializer.Serialize(tokenResponse));
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        var sut = new CrmTokenService(_httpClient, _mockLogger.Object, _mockConfiguration.Object);

        // Act
        var firstResult = await sut.GetTokenAsync();
        var secondResult = await sut.GetTokenAsync();
        var thirdResult = await sut.GetTokenAsync();

        // Assert
        firstResult.Should().Be("cached-token");
        secondResult.Should().Be("cached-token");
        thirdResult.Should().Be("cached-token");

        // Verify HTTP call was made only once
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );

        // Cleanup
        Environment.SetEnvironmentVariable("CRM_USERNAME", null);
        Environment.SetEnvironmentVariable("CRM_PASSWORD", null);
    }

    [Fact]
    public async Task GetTokenAsync_WithMissingUsername_ThrowsInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("CRM_USERNAME", null);
        Environment.SetEnvironmentVariable("CRM_PASSWORD", "test-password");

        var sut = new CrmTokenService(_httpClient, _mockLogger.Object, _mockConfiguration.Object);

        // Act
        Func<Task> act = async () => await sut.GetTokenAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CRM_USERNAME*not set*");

        // Cleanup
        Environment.SetEnvironmentVariable("CRM_PASSWORD", null);
    }

    [Fact]
    public async Task GetTokenAsync_WithMissingPassword_ThrowsInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("CRM_USERNAME", "test-user");
        Environment.SetEnvironmentVariable("CRM_PASSWORD", null);

        var sut = new CrmTokenService(_httpClient, _mockLogger.Object, _mockConfiguration.Object);

        // Act
        Func<Task> act = async () => await sut.GetTokenAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CRM_PASSWORD*not set*");

        // Cleanup
        Environment.SetEnvironmentVariable("CRM_USERNAME", null);
    }

    [Fact]
    public async Task GetTokenAsync_WhenHttpRequestFails_ThrowsException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("CRM_USERNAME", "test-user");
        Environment.SetEnvironmentVariable("CRM_PASSWORD", "test-password");

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var sut = new CrmTokenService(_httpClient, _mockLogger.Object, _mockConfiguration.Object);

        // Act
        Func<Task> act = async () => await sut.GetTokenAsync();

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*Connection failed*");

        // Cleanup
        Environment.SetEnvironmentVariable("CRM_USERNAME", null);
        Environment.SetEnvironmentVariable("CRM_PASSWORD", null);
    }

    [Fact]
    public async Task GetTokenAsync_WhenResponseHasNoAccessToken_ThrowsInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("CRM_USERNAME", "test-user");
        Environment.SetEnvironmentVariable("CRM_PASSWORD", "test-password");

        var tokenResponse = new
        {
            expires_in = 3600,
            token_type = "Bearer"
            // access_token is missing
        };

        var responseContent = new StringContent(JsonSerializer.Serialize(tokenResponse));
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        var sut = new CrmTokenService(_httpClient, _mockLogger.Object, _mockConfiguration.Object);

        // Act
        Func<Task> act = async () => await sut.GetTokenAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to retrieve access token*");

        // Cleanup
        Environment.SetEnvironmentVariable("CRM_USERNAME", null);
        Environment.SetEnvironmentVariable("CRM_PASSWORD", null);
    }

    [Fact]
    public async Task GetTokenAsync_WithUnauthorizedResponse_ThrowsHttpRequestException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("CRM_USERNAME", "test-user");
        Environment.SetEnvironmentVariable("CRM_PASSWORD", "wrong-password");

        var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("{\"error\":\"invalid_grant\"}")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        var sut = new CrmTokenService(_httpClient, _mockLogger.Object, _mockConfiguration.Object);

        // Act
        Func<Task> act = async () => await sut.GetTokenAsync();

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();

        // Cleanup
        Environment.SetEnvironmentVariable("CRM_USERNAME", null);
        Environment.SetEnvironmentVariable("CRM_PASSWORD", null);
    }

    [Fact]
    public async Task GetTokenAsync_UsesDefaultValuesWhenConfigurationMissing()
    {
        // Arrange
        Environment.SetEnvironmentVariable("CRM_USERNAME", "test-user");
        Environment.SetEnvironmentVariable("CRM_PASSWORD", "test-password");

        // Setup configuration to return null (use defaults)
        _mockConfiguration.Setup(x => x["CrmAuthentication:TokenUrl"]).Returns((string?)null);
        _mockConfiguration.Setup(x => x["CrmAuthentication:ClientId"]).Returns((string?)null);
        _mockConfiguration.Setup(x => x["CrmAuthentication:ClientSecret"]).Returns((string?)null);
        _mockConfiguration.Setup(x => x["CrmAuthentication:Scope"]).Returns((string?)null);

        var tokenResponse = new
        {
            access_token = "default-config-token",
            expires_in = 3600,
            token_type = "Bearer"
        };

        var responseContent = new StringContent(JsonSerializer.Serialize(tokenResponse));
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString() == "https://fcapppcliamapidev01.azurewebsites.net/connect/token"),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        var sut = new CrmTokenService(_httpClient, _mockLogger.Object, _mockConfiguration.Object);

        // Act
        var result = await sut.GetTokenAsync();

        // Assert
        result.Should().Be("default-config-token");

        // Cleanup
        Environment.SetEnvironmentVariable("CRM_USERNAME", null);
        Environment.SetEnvironmentVariable("CRM_PASSWORD", null);
    }
}
