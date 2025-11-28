# CRM Backend API - Deployment Guide

## Prerequisites
- Windows with IIS installed
- .NET 8.0 Runtime installed
- ASP.NET Core Hosting Bundle installed
- Administrator privileges

## Quick Deployment

### Option 1: Automated Deployment (Recommended)
Run as Administrator:
```powershell
.\scripts\deploy.ps1
```

This will:
1. Build the application in Release mode
2. Publish to the `publish` folder
3. Deploy to IIS at `http://localhost/CRMBackend`

### Option 2: Manual Steps

#### Step 1: Build
```powershell
.\scripts\build.ps1
```

#### Step 2: Deploy to IIS (Run as Administrator)
```powershell
.\scripts\deploy-iis.ps1
```

## Configuration

### Default Settings
- **Parent Site**: Default Web Site
- **Virtual Directory**: CRMBackend
- **URL**: http://localhost/CRMBackend
- **Physical Path**: C:\inetpub\wwwroot\CRMBackend
- **App Pool**: DefaultAppPool (from Default Web Site)

### Custom Settings
```powershell
.\scripts\deploy.ps1 -VirtualDirName "MyAPI" -SiteName "Default Web Site"
```

## Testing the Deployment

Once deployed, access:
- **API**: http://localhost/CRMBackend/api/customer/info/12345
- **Swagger**: http://localhost/CRMBackend/swagger

Test with:
```powershell
Invoke-RestMethod -Uri "http://localhost/CRMBackend/api/customer/info/12345" -Headers @{"Authorization"="Bearer 123"}
```

## Undeployment

To remove application from IIS:
```powershell
.\scripts\undeploy.ps1
```

To remove application and files:
```powershell
.\scripts\undeploy.ps1 -RemoveFiles
```

To remove from a different parent site:
```powershell
.\scripts\undeploy.ps1 -VirtualDirName "CRMBackend" -SiteName "Default Web Site" -RemoveFiles
```

## Troubleshooting

### Issue: Site won't start
- Check Event Viewer > Application logs
- Verify .NET 8.0 Runtime is installed
- Check app pool identity has proper permissions

### Issue: 500 errors
- Check logs in C:\inetpub\wwwroot\CRMBackend\logs
- Verify appsettings.json configuration
- Ensure external CRM API is accessible

### Issue: 502 Bad Gateway
- Restart the application pool
- Check if another process is using the port
- Verify web.config settings

## Configuration Files

### appsettings.json
Located in the deployment directory. Update:
```json
{
  "ExternalCRMService": {
    "BaseUrl": "http://localhost/CRMApi/",
    "BearerToken": "123"
  },
  "Authentication": {
    "BearerToken": "123"
  }
}
```

Then restart the app pool:
```powershell
Restart-WebAppPool -Name "DefaultAppPool"
```
