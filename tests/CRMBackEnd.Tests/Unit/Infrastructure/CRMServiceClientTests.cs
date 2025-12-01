using System.Net;
using System.Net.Http.Json;
using CRMBackEnd.Domain.Entities;
using CRMBackEnd.Infrastructure.ExternalServices;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace CRMBackEnd.Tests.Unit.Infrastructure;

public class CRMServiceClientTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<ILogger<CRMServiceClient>> _mockLogger;
    private readonly HttpClient _httpClient;
    private readonly CRMServiceClient _sut;

    public CRMServiceClientTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockLogger = new Mock<ILogger<CRMServiceClient>>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/CRMApi/")
        };
        _sut = new CRMServiceClient(_httpClient, _mockLogger.Object);
    }

    [Fact]
    public async Task GetClientDataAsync_WithValidId_ReturnsCustomer()
    {
        // Arrange
        var customerId = 12345;
        var expectedCustomer = new Customer
        {
            ClientId = "12345",
            EditApproval = "Approved",
            Dba = "Test Company",
            ClientLegalName = "Test Company LLC",
            ComplianceHold = "No",
            Level = "Gold",
            PaymentTermID = "NET30",
            PaymentMethod = "Credit Card",
            Status = "Active"
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(expectedCustomer)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"api/v1/ClientData/{customerId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _sut.GetClientDataAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result.ClientId.Should().Be("12345");
        result.EditApproval.Should().Be("Approved");
        result.Dba.Should().Be("Test Company");
    }

    [Fact]
    public async Task GetClientDataAsync_WhenApiReturns404_ThrowsKeyNotFoundException()
    {
        // Arrange
        var customerId = 99999;
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        Func<Task> act = async () => await _sut.GetClientDataAsync(customerId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Customer with ID {customerId} not found in external CRM system");
    }

    [Fact]
    public async Task GetClientDataAsync_WhenApiReturnsNullContent_ThrowsInvalidOperationException()
    {
        // Arrange
        var customerId = 12345;
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create<Customer>(null!)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        Func<Task> act = async () => await _sut.GetClientDataAsync(customerId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"BACKEND: Failed to deserialize customer data for ID: {customerId}");
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CRMServiceClient>>();
        
        // Act
        Action act = () => new CRMServiceClient(null!, mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("httpClient");
    }

    [Fact]
    public async Task GetClientDataAsync_WhenApiReturns401_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var customerId = 12345;
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Unauthorized,
            Content = new StringContent("Unauthorized")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        Func<Task> act = async () => await _sut.GetClientDataAsync(customerId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage($"Authentication failed when accessing external CRM for customer ID {customerId}");
    }

    [Fact]
    public async Task GetClientDataAsync_WhenApiReturns500_ThrowsInvalidOperationException()
    {
        // Arrange
        var customerId = 12345;
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Content = new StringContent("Internal Server Error")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        Func<Task> act = async () => await _sut.GetClientDataAsync(customerId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"External CRM service encountered an error processing customer ID {customerId}");
    }

    [Fact]
    public async Task GetClientDataAsync_WhenApiReturnsBadRequest_ThrowsException()
    {
        // Arrange
        var customerId = 12345;
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent("Bad Request")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        Func<Task> act = async () => await _sut.GetClientDataAsync(customerId);

        // Assert
        var exception = await act.Should().ThrowAsync<Exception>();
        exception.And.InnerException.Should().BeOfType<HttpRequestException>()
            .Which.Message.Should().Contain($"External CRM API returned status code 400 for customer ID {customerId}");
    }
}
