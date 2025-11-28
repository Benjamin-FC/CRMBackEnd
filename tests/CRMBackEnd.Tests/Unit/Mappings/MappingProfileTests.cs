using AutoMapper;
using CRMBackEnd.Application.DTOs;
using CRMBackEnd.Application.Mappings;
using CRMBackEnd.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CRMBackEnd.Tests.Unit.Mappings;

public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void MappingProfile_Configuration_IsValid()
    {
        // Assert
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_Customer_To_CustomerInfoResponse_MapsAllProperties()
    {
        // Arrange
        var customer = new Customer
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

        // Act
        var result = _mapper.Map<CustomerInfoResponse>(customer);

        // Assert
        result.Should().NotBeNull();
        result.ClientId.Should().Be(customer.ClientId);
        result.EditApproval.Should().Be(customer.EditApproval);
        result.Dba.Should().Be(customer.Dba);
        result.ClientLegalName.Should().Be(customer.ClientLegalName);
        result.ComplianceHold.Should().Be(customer.ComplianceHold);
        result.Level.Should().Be(customer.Level);
        result.PaymentTermID.Should().Be(customer.PaymentTermID);
        result.PaymentMethod.Should().Be(customer.PaymentMethod);
        result.Status.Should().Be(customer.Status);
    }

    [Fact]
    public void Map_Customer_WithEmptyValues_MapsCorrectly()
    {
        // Arrange
        var customer = new Customer
        {
            ClientId = "",
            EditApproval = "",
            Dba = "",
            ClientLegalName = "",
            ComplianceHold = "",
            Level = "",
            PaymentTermID = "",
            PaymentMethod = "",
            Status = ""
        };

        // Act
        var result = _mapper.Map<CustomerInfoResponse>(customer);

        // Assert
        result.Should().NotBeNull();
        result.ClientId.Should().BeEmpty();
        result.EditApproval.Should().BeEmpty();
    }
}
