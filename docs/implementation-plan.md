# CRM Backend API - Implementation Plan

## Project Overview
Create a .NET 10.0 CRM backend API solution following Clean Architecture principles with a wrapper around an external CRM service at `http://localhost/CRMApi`.

### ✅ Confirmed Requirements
- **.NET Version**: .NET 10.0
- **Database**: Not needed (no persistence layer)
- **Scope**: Single Customer endpoint only
- **Method**: `CustomerInfo` - takes string ID parameter
- **External API**: Wraps `GET /api/v1/ClientData/{id}` endpoint
- **Authentication**: Bearer token "123" for both our API and external service

---

## External CRM API Details

### Endpoint Information
**Base URL**: `http://localhost/CRMApi`  
**Endpoint**: `GET /api/v1/ClientData/{id}`  
**Authentication**: Bearer token "123"

### Parameters
- **id**: integer (int32), required, path parameter

### Response Model (200 Success)
```json
{
  "clientId": "string",
  "editApproval": "string",
  "dba": "string",
  "clientLegalName": "string",
  "complianceHold": "string",
  "level": "string",
  "paymentTermID": "string",
  "paymentMethod": "string",
  "status": "string"
}
```

---

## Architecture Overview

### Simplified Clean Architecture Structure
```
CRMBackEnd/
├── src/
│   ├── CRMBackEnd.Domain/          # Business entities and interfaces
│   ├── CRMBackEnd.Application/     # Business logic and DTOs
│   ├── CRMBackEnd.Infrastructure/  # External CRM API wrapper
│   └── CRMBackEnd.API/            # Web API presentation layer
└── docs/
    └── implementation-plan.md
```

**Note**: No test projects needed for initial implementation.

---

## 1. Solution Structure

### 1.1 Domain Project (`CRMBackEnd.Domain`)
**Purpose**: Core business entities and domain logic (no dependencies on other layers)

**Contents**:
- **Entities**: 
  - `Customer` - Core customer entity
  
- **Value Objects** (Optional for now):
  - Can be added later if needed
  
- **Interfaces**: 
  - `ICRMServiceClient` - Interface for external CRM service wrapper

**Key Entity**:
```csharp
public class Customer
{
    public string ClientId { get; set; }
    public string EditApproval { get; set; }
    public string Dba { get; set; }
    public string ClientLegalName { get; set; }
    public string ComplianceHold { get; set; }
    public string Level { get; set; }
    public string PaymentTermID { get; set; }
    public string PaymentMethod { get; set; }
    public string Status { get; set; }
}
```

**Dependencies**: None (pure .NET)


---

### 1.2 Application Project (`CRMBackEnd.Application`)
**Purpose**: Application business rules, use cases, and orchestration

**Contents**:
- **Interfaces**:
  - `ICustomerService` - Service interface for customer operations
  
- **DTOs** (Data Transfer Objects):
  - `CustomerInfoRequest` - Request model (takes string ID)
  - `CustomerInfoResponse` - Response model
  - Mapping profiles (AutoMapper)
  
- **Services**:
  - `CustomerService` - Implements `ICustomerService`
  - Contains `GetCustomerInfo(string id)` method
  
- **Validators** (Optional): 
  - FluentValidation for input validation

**Key Service Interface**:
```csharp
public interface ICustomerService
{
    Task<CustomerInfoResponse> GetCustomerInfoAsync(string id);
}
```

**Dependencies**:
- Domain project
- NuGet: AutoMapper, FluentValidation (optional)


---

### 1.3 Infrastructure Project (`CRMBackEnd.Infrastructure`)
**Purpose**: Implementation of external CRM API wrapper

**Contents**:
- **ExternalServices**:
  - `CRMServiceClient` - Implements `ICRMServiceClient`
  - HTTP client configuration
  - Bearer token authentication handler (`BearerTokenHandler`)
  
- **Configuration**:
  - Dependency injection setup
  - HTTP client factory configuration
  
- **Logging**: Request/response logging

**Dependencies**:
- Domain project
- Application project
- NuGet: 
  - Microsoft.Extensions.Http
  - Microsoft.Extensions.Options
  - Serilog (optional, for logging)

**External CRM Service Integration**:
- Base URL: `http://localhost/CRMApi`
- Endpoint: `GET /api/v1/ClientData/{id}`
- Authentication: Bearer token "123"
- Implementation: HttpClient with DelegatingHandler for token injection

**Key Implementation**:
```csharp
public class CRMServiceClient : ICRMServiceClient
{
    private readonly HttpClient _httpClient;
    
    public CRMServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<Customer> GetClientDataAsync(int id)
    {
        var response = await _httpClient.GetAsync($"/api/v1/ClientData/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Customer>();
    }
}
```


---

### 1.4 API Project (`CRMBackEnd.API`)
**Purpose**: RESTful Web API presentation layer

**Contents**:
- **Controllers**:
  - `CustomerController` - Single controller with `CustomerInfo` endpoint
  
- **Middleware**:
  - Global exception handling
  - Request/Response logging
  - Bearer token authentication
  
- **Configuration**:
  - Swagger/OpenAPI setup
  - CORS configuration (optional)
  - Dependency injection registration
  
- **Authentication**:
  - Bearer token "123" validation

**Dependencies**:
- Application project
- Infrastructure project
- NuGet:
  - Swashbuckle.AspNetCore (Swagger)
  - Microsoft.AspNetCore.Authentication.JwtBearer

**Key Controller**:
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires Bearer token "123"
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    
    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }
    
    [HttpGet("info/{id}")]
    public async Task<ActionResult<CustomerInfoResponse>> GetCustomerInfo(string id)
    {
        var result = await _customerService.GetCustomerInfoAsync(id);
        return Ok(result);
    }
}
```


---

## 2. Technology Stack

### Core Framework
- **.NET 10.0**
- **ASP.NET Core Web API**
- **C# 13**

### Libraries & Packages
- **AutoMapper** - Object-to-object mapping
- **Swashbuckle** - API documentation (Swagger)
- **Serilog** (Optional) - Structured logging
- **Polly** (Optional) - Resilience and retry policies for external API calls

### Authentication
- **Bearer Token Authentication** - Simple token validation for "123"


---

## 3. External CRM Service Wrapper Design

### 3.1 Interface Definition
```csharp
public interface ICRMServiceClient
{
    Task<Customer> GetClientDataAsync(int id);
}
```

### 3.2 Implementation Strategy
- **HttpClient Factory**: Use IHttpClientFactory for efficient HTTP client management
- **Authentication Handler**: Custom DelegatingHandler to inject Bearer token "123"
- **Error Handling**: Proper exception mapping from HTTP responses
- **Logging** (Optional): Request/response logging for debugging

### 3.3 Bearer Token Handler
```csharp
public class BearerTokenHandler : DelegatingHandler
{
    private readonly string _token;
    
    public BearerTokenHandler(string token)
    {
        _token = token;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        request.Headers.Authorization = 
            new AuthenticationHeaderValue("Bearer", _token);
        return await base.SendAsync(request, cancellationToken);
    }
}
```

### 3.4 Configuration
```json
{
  "ExternalCRMService": {
    "BaseUrl": "http://localhost/CRMApi",
    "BearerToken": "123",
    "TimeoutSeconds": 30
  }
}
```


---

## 4. API Endpoints Design

### Base URL
`/api/customer`

### Endpoint

#### Customer Info
- `GET /api/customer/info/{id}` - Get customer information by ID
  - **Parameter**: `id` (string) - Customer ID
  - **Authentication**: Bearer token "123" required
  - **Response**: CustomerInfoResponse object

**Example Request**:
```
GET /api/customer/info/12345
Authorization: Bearer 123
```

**Example Response** (200 OK):
```json
{
  "clientId": "12345",
  "editApproval": "Approved",
  "dba": "ABC Company",
  "clientLegalName": "ABC Company LLC",
  "complianceHold": "No",
  "level": "Gold",
  "paymentTermID": "NET30",
  "paymentMethod": "Credit Card",
  "status": "Active"
}
```


---

## 5. Implementation Phases

### Phase 1: Project Setup
1. Create solution structure with 4 projects
2. Configure project dependencies
3. Install required NuGet packages
4. Set up basic folder structure in each project

### Phase 2: Domain Layer
1. Define Customer entity
2. Define ICRMServiceClient interface

### Phase 3: Application Layer
1. Create DTOs (CustomerInfoRequest, CustomerInfoResponse)
2. Create mapping profiles (AutoMapper)
3. Define ICustomerService interface
4. Implement CustomerService

### Phase 4: Infrastructure Layer
1. Create BearerTokenHandler for external API authentication
2. Implement CRMServiceClient wrapper
3. Configure HttpClient factory
4. Set up dependency injection

### Phase 5: API Layer
1. Create CustomerController with CustomerInfo endpoint
2. Configure Swagger/OpenAPI
3. Implement Bearer token authentication ("123")
4. Implement global exception handling middleware
5. Set up dependency injection
6. Configure logging (optional)

### Phase 6: Testing & Documentation
1. Test endpoint with Swagger UI
2. Verify external CRM service integration
3. Create README with setup instructions

---



## 6. Configuration & Settings

### appsettings.json Structure
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ExternalCRMService": {
    "BaseUrl": "http://localhost/CRMApi",
    "BearerToken": "123",
    "TimeoutSeconds": 30
  },
  "Authentication": {
    "BearerToken": "123"
  },
  "AllowedHosts": "*"
}
```

---

## 7. Security Considerations

1. **API Authentication**: Bearer token "123" validation
2. **Input Validation**: Basic validation on ID parameter
3. **HTTPS**: Enforce HTTPS in production
4. **Error Handling**: Don't expose sensitive information in error messages
5. **Secrets Management**: Use User Secrets for development

---

## 8. Error Handling Strategy

### Global Exception Handler
- Catch all unhandled exceptions
- Return consistent error response format
- Log all errors with context

### Error Response Format
```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": [
    {
      "field": "Email",
      "message": "Email is required"
    }
  ],
  "traceId": "0HMVFE0A2SSST:00000001"
}
```

---

## 9. Logging Strategy (Optional)

### Structured Logging with Serilog
- **Information**: API requests/responses
- **Warning**: Validation failures, business rule violations
- **Error**: Exceptions, external service failures
- **Debug**: Detailed diagnostic information

### Log Sinks
- Console (Development)
- File (Development/Production)
- Application Insights (Production - optional)

---

## 10. Performance Considerations

1. **Async/Await**: Use async operations throughout
2. **HTTP Client Reuse**: Use IHttpClientFactory for external API calls
3. **Error Handling**: Implement proper timeout and retry logic

---

## 11. Development Workflow

### Prerequisites
- .NET 10.0 SDK installed
- Visual Studio 2022 or VS Code
- External CRM service running at `http://localhost/CRMApi`
- Postman or similar API testing tool (optional, Swagger UI provided)

### Getting Started
1. Clone repository
2. Restore NuGet packages: `dotnet restore`
3. Update appsettings.json if needed (Bearer tokens, URLs)
4. Run application: `dotnet run --project src/CRMBackEnd.API`
5. Access Swagger: `https://localhost:7xxx/swagger`
6. Test endpoint: `GET /api/customer/info/{id}` with Bearer token "123"

---



## 12. Success Criteria

The implementation will be considered complete when:

✅ Solution structure follows Clean Architecture with 4 projects  
✅ External CRM service wrapper is functional with Bearer token "123"  
✅ CustomerInfo endpoint is implemented: `GET /api/customer/info/{id}`  
✅ Our API requires Bearer token "123" for authentication  
✅ Swagger UI is configured and functional  
✅ Global exception handling is in place  
✅ API successfully calls external CRM service at `http://localhost/CRMApi/api/v1/ClientData/{id}`  
✅ Response mapping from external API to our DTOs works correctly  
✅ Code follows Clean Architecture and SOLID principles  

---

## 13. Next Steps

**Ready to proceed with implementation!** 🚀

I will:
1. Create the solution structure with 4 projects (Domain, Application, Infrastructure, API)
2. Set up project dependencies and install required NuGet packages
3. Implement Customer entity and DTOs
4. Create external CRM service wrapper with Bearer token "123"
5. Implement CustomerService and CustomerController
6. Configure Bearer token authentication for our API
7. Set up Swagger UI
8. Test the complete flow

**The plan is ready for your approval. Should I proceed with the implementation?**
