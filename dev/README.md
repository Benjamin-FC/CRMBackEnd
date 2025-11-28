# Development Scripts

This folder contains scripts for local development and testing.

## Scripts

### start-api.ps1
Runs the API locally in development mode.

**Usage:**
```powershell
.\start-api.ps1
```

The API will start at: http://localhost:5018
- Swagger UI: http://localhost:5018/swagger
- API Endpoint: http://localhost:5018/api/customer/info/{id}

**Authentication:** Bearer token `123`

### test-api.ps1
Tests the locally running API.

**Usage:**
```powershell
.\test-api.ps1
```

Tests the endpoint `http://localhost:5018/api/customer/info/12345` with Bearer authentication.

### test-url.ps1
Tests URL construction for the external CRM API.

**Usage:**
```powershell
.\test-url.ps1
```

Tests calling the external CRM service at `http://localhost/CRMApi/api/v1/ClientData/12345`.

## Quick Start

1. **Start the API locally:**
   ```powershell
   cd dev
   .\start-api.ps1
   ```

2. **In another terminal, test it:**
   ```powershell
   cd dev
   .\test-api.ps1
   ```

## Configuration

The API uses configuration from:
- `src/CRMBackEnd.API/appsettings.json`
- `src/CRMBackEnd.API/appsettings.Development.json`

Key settings:
- **Authentication:BearerToken**: `"123"`
- **ExternalCRMService:BaseUrl**: `"http://localhost/CRMApi/"`
- **ExternalCRMService:BearerToken**: `"123"`
