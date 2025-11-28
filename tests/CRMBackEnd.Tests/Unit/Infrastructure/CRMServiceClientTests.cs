using System.Net;
using System.Net.Http.Json;
using CRMBackEnd.Domain.Entities;
using CRMBackEnd.Infrastructure.ExternalServices;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace CRMBackEnd.Tests.Unit.Infrastructure;

public class CRMServiceClientTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly CRMServiceClient _sut;

    public CRMServiceClientTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/CRMApi/")
        };
        _sut = new CRMServiceClient(_httpClient);
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
    public async Task GetClientDataAsync_WhenApiReturns404_ThrowsException()
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
        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Error calling external CRM service for customer ID {customerId}*");
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
            .WithMessage($"Failed to deserialize customer data for ID: {customerId}");
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new CRMServiceClient(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("httpClient");
    }
}
