using AutoMapper;
using CRMBackEnd.Application.DTOs;
using CRMBackEnd.Application.Services;
using CRMBackEnd.Domain.Entities;
using CRMBackEnd.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRMBackEnd.Tests.Unit.Services;

public class CustomerServiceTests
{
    private readonly Mock<ICRMServiceClient> _mockCrmClient;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CustomerService>> _mockLogger;
    private readonly CustomerService _sut;

    public CustomerServiceTests()
    {
        _mockCrmClient = new Mock<ICRMServiceClient>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CustomerService>>();
        _sut = new CustomerService(_mockCrmClient.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetCustomerInfoAsync_WithValidIntegerId_ReturnsCustomerInfo()
    {
        // Arrange
        var customerId = "12345";
        var customer = new Customer
        {
            ClientId = customerId,
            EditApproval = "Approved",
            Dba = "Test Company",
            ClientLegalName = "Test Company LLC",
            ComplianceHold = "No",
            Level = "Gold",
            PaymentTermID = "NET30",
            PaymentMethod = "Credit Card",
            Status = "Active"
        };

        var expectedResponse = new CustomerInfoResponse
        {
            ClientId = customerId,
            EditApproval = "Approved",
            Dba = "Test Company",
            ClientLegalName = "Test Company LLC",
            ComplianceHold = "No",
            Level = "Gold",
            PaymentTermID = "NET30",
            PaymentMethod = "Credit Card",
            Status = "Active"
        };

        _mockCrmClient
            .Setup(x => x.GetClientDataAsync(12345))
            .ReturnsAsync(customer);

        _mockMapper
            .Setup(x => x.Map<CustomerInfoResponse>(customer))
            .Returns(expectedResponse);

        // Act
        var result = await _sut.GetCustomerInfoAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result.ClientId.Should().Be(customerId);
        result.EditApproval.Should().Be("Approved");
        result.Dba.Should().Be("Test Company");
        
        _mockCrmClient.Verify(x => x.GetClientDataAsync(12345), Times.Once);
        _mockMapper.Verify(x => x.Map<CustomerInfoResponse>(customer), Times.Once);
    }

    [Fact]
    public async Task GetCustomerInfoAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange
        var invalidId = "invalid";

        // Act
        Func<Task> act = async () => await _sut.GetCustomerInfoAsync(invalidId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid customer ID*");
        
        _mockCrmClient.Verify(x => x.GetClientDataAsync(It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("12.34")]
    [InlineData("12345abc")]
    public async Task GetCustomerInfoAsync_WithInvalidFormats_ThrowsArgumentException(string invalidId)
    {
        // Act
        Func<Task> act = async () => await _sut.GetCustomerInfoAsync(invalidId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetCustomerInfoAsync_WhenCrmClientThrowsException_PropagatesException()
    {
        // Arrange
        var customerId = "12345";
        _mockCrmClient
            .Setup(x => x.GetClientDataAsync(12345))
            .ThrowsAsync(new Exception("External service error"));

        // Act
        Func<Task> act = async () => await _sut.GetCustomerInfoAsync(customerId);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("External service error");
    }
}
