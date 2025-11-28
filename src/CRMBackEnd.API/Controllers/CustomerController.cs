using CRMBackEnd.Application.DTOs;
using CRMBackEnd.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMBackEnd.API.Controllers;

/// <summary>
/// Controller for customer operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(
        ICustomerService customerService,
        ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Get customer information by ID
    /// </summary>
    /// <param name="id">Customer ID (string)</param>
    /// <returns>Customer information</returns>
    /// <response code="200">Returns customer information</response>
    /// <response code="400">Invalid customer ID</response>
    /// <response code="401">Unauthorized - Bearer token required</response>
    /// <response code="404">Customer not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("info/{id}")]
    [ProducesResponseType(typeof(CustomerInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerInfoResponse>> GetCustomerInfo(string id)
    {
        try
        {
            _logger.LogInformation("Getting customer info for ID: {CustomerId}", id);

            var result = await _customerService.GetCustomerInfoAsync(id);

            _logger.LogInformation("Successfully retrieved customer info for ID: {CustomerId}", id);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid customer ID: {CustomerId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer info for ID: {CustomerId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving customer information" });
        }
    }
}
