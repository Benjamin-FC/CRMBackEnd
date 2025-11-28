```mermaid
graph TB
    subgraph External["🌐 External Services"]
        CRM["<b>External CRM API</b><br/>http://localhost/CRMApi<br/>📡 REST API"]
    end

    subgraph IIS["🖥️ IIS Deployment"]
        IISAPP["<b>IIS Application</b><br/>http://localhost/CRMBackend<br/>🌍 Production"]
    end

    subgraph API["🎯 API Layer<br/>(CRMBackEnd.API)"]
        CTRL["<b>CustomerController</b><br/>GET /api/customer/info/{id}<br/>🎮 REST Endpoint"]
        AUTH["<b>BearerTokenAuthenticationHandler</b><br/>Token: '123'<br/>🔐 Authentication"]
        MID["<b>ExceptionHandlingMiddleware</b><br/>Global Error Handler<br/>⚠️ Error Handling"]
        SWAGGER["<b>Swagger UI</b><br/>/swagger<br/>📚 API Documentation"]
    end

    subgraph APP["⚙️ Application Layer<br/>(CRMBackEnd.Application)"]
        SVC["<b>CustomerService</b><br/>Business Logic<br/>🔄 Service"]
        DTO["<b>DTOs</b><br/>CustomerInfoResponse<br/>📦 Data Transfer"]
        MAP["<b>AutoMapper</b><br/>MappingProfile<br/>🗺️ Object Mapping"]
    end

    subgraph INFRA["🏗️ Infrastructure Layer<br/>(CRMBackEnd.Infrastructure)"]
        CLIENT["<b>CRMServiceClient</b><br/>HTTP Client Wrapper<br/>🔌 Integration"]
        HANDLER["<b>BearerTokenHandler</b><br/>Token Injection<br/>🎫 DelegatingHandler"]
        CONFIG["<b>Configuration</b><br/>ExternalCRMServiceSettings<br/>⚙️ Settings"]
    end

    subgraph DOMAIN["🎨 Domain Layer<br/>(CRMBackEnd.Domain)"]
        ENT["<b>Customer Entity</b><br/>Domain Model<br/>📋 Core Model"]
        INT["<b>Interfaces</b><br/>ICRMServiceClient<br/>📝 Contracts"]
    end

    subgraph TEST["🧪 Test Layer<br/>(CRMBackEnd.Tests)"]
        UNIT["<b>Unit Tests</b><br/>Services, Mappings<br/>✅ Isolated Testing"]
        INTTEST["<b>Integration Tests</b><br/>Full API Testing<br/>🔗 End-to-End"]
    end

    %% External connections
    IISAPP -->|"HTTP Request<br/>Bearer Token"| CTRL
    CTRL -->|"Validates"| AUTH
    CTRL -->|"Protected by"| MID
    
    %% API to Application
    CTRL -->|"Calls"| SVC
    SVC -->|"Uses"| MAP
    SVC -->|"Returns"| DTO
    
    %% Application to Infrastructure
    SVC -->|"Depends on"| INT
    CLIENT -->|"Implements"| INT
    
    %% Infrastructure to External
    CLIENT -->|"HTTP GET<br/>/api/v1/ClientData/{id}"| CRM
    CLIENT -->|"Uses"| HANDLER
    HANDLER -->|"Injects Token"| CRM
    CLIENT -->|"Configured by"| CONFIG
    
    %% Mapping
    MAP -->|"Maps"| ENT
    MAP -->|"To"| DTO
    
    %% Testing
    UNIT -.->|"Tests"| SVC
    UNIT -.->|"Tests"| MAP
    UNIT -.->|"Tests"| CLIENT
    INTTEST -.->|"Tests"| CTRL

    %% Styling
    classDef apiStyle fill:#4A90E2,stroke:#2E5C8A,stroke-width:3px,color:#fff
    classDef appStyle fill:#7B68EE,stroke:#5B48CE,stroke-width:3px,color:#fff
    classDef infraStyle fill:#50C878,stroke:#30A858,stroke-width:3px,color:#fff
    classDef domainStyle fill:#FF6B6B,stroke:#DF4B4B,stroke-width:3px,color:#fff
    classDef testStyle fill:#FFA500,stroke:#DF8500,stroke-width:3px,color:#fff
    classDef externalStyle fill:#95A5A6,stroke:#75858A,stroke-width:3px,color:#fff
    classDef iisStyle fill:#34495E,stroke:#24394E,stroke-width:3px,color:#fff

    class CTRL,AUTH,MID,SWAGGER apiStyle
    class SVC,DTO,MAP appStyle
    class CLIENT,HANDLER,CONFIG infraStyle
    class ENT,INT domainStyle
    class UNIT,INTTEST testStyle
    class CRM externalStyle
    class IISAPP iisStyle
```

## Architecture Overview

### 🎯 Clean Architecture Layers

**API Layer** (Blue) - Entry point and HTTP concerns
- Controllers handle HTTP requests/responses
- Authentication validates bearer tokens
- Middleware handles global error handling
- Swagger provides API documentation

**Application Layer** (Purple) - Business logic and use cases
- Services orchestrate domain operations
- DTOs define data transfer contracts
- AutoMapper handles entity-to-DTO transformations

**Infrastructure Layer** (Green) - External integrations
- HTTP clients communicate with external services
- Handlers inject authentication tokens
- Configuration manages external service settings

**Domain Layer** (Red) - Core business entities and rules
- Entities represent core domain models
- Interfaces define contracts for implementations

**Test Layer** (Orange) - Quality assurance
- Unit tests verify isolated components
- Integration tests validate end-to-end flows

### 🔄 Request Flow

1. **Client Request** → IIS Application receives HTTP request with Bearer token
2. **Authentication** → Token validated against configured value ("123")
3. **Controller** → Parses customer ID from route
4. **Service Layer** → Orchestrates business logic
5. **Infrastructure** → Makes authenticated HTTP call to external CRM API
6. **Mapping** → Transforms Customer entity to CustomerInfoResponse DTO
7. **Response** → Returns JSON response to client

### 🔐 Authentication Flow

- **API Authentication**: Bearer token "123" validated by `BearerTokenAuthenticationHandler`
- **External API**: `BearerTokenHandler` injects token into outgoing HTTP requests

### 📊 Key Design Patterns

- **Clean Architecture**: Dependency inversion, layer separation
- **Repository Pattern**: `ICRMServiceClient` interface
- **Dependency Injection**: All services registered in DI container
- **DTO Pattern**: Separate domain models from API contracts
- **Decorator Pattern**: `BearerTokenHandler` extends HttpClient behavior
