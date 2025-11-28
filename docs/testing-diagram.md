# Testing Architecture

```mermaid
graph TB
    subgraph TESTS["🧪 Test Project<br/>(CRMBackEnd.Tests)"]
        subgraph UNIT["✅ Unit Tests"]
            SVCTEST["<b>CustomerServiceTests</b><br/>5 tests<br/>🔄 Service Logic"]
            CLIENTTEST["<b>CRMServiceClientTests</b><br/>4 tests<br/>🔌 HTTP Client"]
            MAPTEST["<b>MappingProfileTests</b><br/>3 tests<br/>🗺️ AutoMapper"]
        end
        
        subgraph INTEGRATION["🔗 Integration Tests"]
            INTTEST["<b>CustomerControllerIntegrationTests</b><br/>5 tests<br/>🎯 End-to-End API"]
        end
    end

    subgraph SUT["System Under Test"]
        subgraph API["🎯 API Layer"]
            CTRL["<b>CustomerController</b><br/>REST Endpoint"]
        end
        
        subgraph APP["⚙️ Application Layer"]
            SVC["<b>CustomerService</b><br/>Business Logic"]
            MAP["<b>MappingProfile</b><br/>Object Mapping"]
        end
        
        subgraph INFRA["🏗️ Infrastructure Layer"]
            CLIENT["<b>CRMServiceClient</b><br/>HTTP Client"]
        end
    end

    subgraph MOCKS["🎭 Test Doubles"]
        MOCKCLIENT["<b>Mock ICRMServiceClient</b><br/>Moq"]
        MOCKMAPPER["<b>Mock IMapper</b><br/>Moq"]
        MOCKHTTP["<b>Mock HttpMessageHandler</b><br/>Moq"]
        WEBFACTORY["<b>WebApplicationFactory</b><br/>Test Server"]
    end

    %% Unit Test Relationships
    SVCTEST -.->|"Tests with mocks"| SVC
    SVCTEST -.->|"Uses"| MOCKCLIENT
    SVCTEST -.->|"Uses"| MOCKMAPPER
    
    CLIENTTEST -.->|"Tests with mock HTTP"| CLIENT
    CLIENTTEST -.->|"Uses"| MOCKHTTP
    
    MAPTEST -.->|"Tests real mapping"| MAP

    %% Integration Test Relationships
    INTTEST -.->|"Tests full stack"| CTRL
    INTTEST -.->|"Uses"| WEBFACTORY
    WEBFACTORY -.->|"Hosts"| CTRL
    WEBFACTORY -.->|"Configures"| SVC
    WEBFACTORY -.->|"Configures"| CLIENT

    %% Styling
    classDef testStyle fill:#FFA500,stroke:#DF8500,stroke-width:3px,color:#fff
    classDef unitStyle fill:#FFB84D,stroke:#DF9830,stroke-width:2px,color:#fff
    classDef integrationStyle fill:#FF8C00,stroke:#DF6C00,stroke-width:2px,color:#fff
    classDef sutStyle fill:#4A90E2,stroke:#2E5C8A,stroke-width:2px,color:#fff
    classDef mockStyle fill:#9B59B6,stroke:#7B39A6,stroke-width:2px,color:#fff

    class TESTS testStyle
    class SVCTEST,CLIENTTEST,MAPTEST unitStyle
    class INTTEST integrationStyle
    class CTRL,SVC,MAP,CLIENT sutStyle
    class MOCKCLIENT,MOCKMAPPER,MOCKHTTP,WEBFACTORY mockStyle
```

## Testing Strategy

### ✅ Unit Tests (12 tests total)

**CustomerServiceTests** (5 tests)
- ✓ Valid integer ID returns customer info
- ✓ Invalid ID format throws ArgumentException
- ✓ Multiple invalid formats tested (Theory)
- ✓ CRM client exceptions are propagated
- **Mocks**: ICRMServiceClient, IMapper

**CRMServiceClientTests** (4 tests)
- ✓ Successful API call returns customer
- ✓ HTTP errors throw HttpRequestException
- ✓ Bearer token injection verified
- ✓ URL construction validated
- **Mocks**: HttpMessageHandler

**MappingProfileTests** (3 tests)
- ✓ Configuration validation
- ✓ Customer to CustomerInfoResponse mapping
- ✓ All properties mapped correctly
- **Uses**: Real AutoMapper instance

### 🔗 Integration Tests (5 tests)

**CustomerControllerIntegrationTests** (5 tests)
- ✓ Valid request returns 200 OK with customer data
- ✓ Invalid ID format returns 400 Bad Request
- ✓ Missing authentication returns 401 Unauthorized
- ✓ Invalid token returns 401 Unauthorized
- ✓ Full request/response validation
- **Uses**: WebApplicationFactory (in-memory test server)

### 🎯 Test Coverage

| Layer | Coverage | Tests |
|-------|----------|-------|
| **Application** | High | 5 unit + 5 integration |
| **Infrastructure** | High | 4 unit |
| **API** | High | 5 integration |
| **Domain** | Implicit | Via mapping tests |

### 🛠️ Testing Tools

- **xUnit 3.1.4**: Test framework
- **Moq 4.20.72**: Mocking library
- **FluentAssertions 8.8.0**: Assertion library
- **Microsoft.AspNetCore.Mvc.Testing 10.0.0**: Integration testing

### 📊 Test Results

```
Test summary: total: 19, failed: 0, succeeded: 19, skipped: 0
Duration: ~4s
```

### 🔍 Testing Patterns

**Arrange-Act-Assert (AAA)**
```csharp
// Arrange - Set up test data and mocks
var customerId = "12345";
_mockCrmClient.Setup(x => x.GetClientDataAsync(12345)).ReturnsAsync(customer);

// Act - Execute the method under test
var result = await _sut.GetCustomerInfoAsync(customerId);

// Assert - Verify the results
result.Should().NotBeNull();
result.ClientId.Should().Be(customerId);
```

**Theory Tests** - Data-driven tests
```csharp
[Theory]
[InlineData("")]
[InlineData("abc")]
[InlineData("12.34")]
public async Task MultipleInputs_ThrowsException(string invalidId)
```

**Integration Tests** - Full HTTP pipeline
```csharp
var response = await _client.GetAsync("/api/customer/info/12345");
response.StatusCode.Should().Be(HttpStatusCode.OK);
```
