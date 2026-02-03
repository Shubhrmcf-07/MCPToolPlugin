# Complete Testing & Deployment Workflow

This guide walks you through testing the MCP server locally, pushing to GitHub, and setting up automatic tool discovery on a deployed instance.

## Phase 1: Local Testing (Your Machine)

### Step 1.1: Build and Run the Server

```bash
# Navigate to project
cd c:\biztalk-migrator-server

# Build the solution
dotnet build

# Run the server
dotnet run --project src/MCPServer.Core
```

Expected output:
```
info: MCPServer.Core.MCPServerApplication[0]
      Registered tool: Text Processing Tool (v1.0.0)
Application started. Press Ctrl+C to exit.
```

Server runs on: `http://localhost:5000`

### Step 1.2: Test the API

In a new terminal:

```bash
# Check server health
curl http://localhost:5000/health

# Get all tools
curl http://localhost:5000/api/tools

# Get tool details
curl http://localhost:5000/api/tools/text-processing

# Test tool function
curl -X POST http://localhost:5000/api/tools/text-processing/execute \
  -H "Content-Type: application/json" \
  -d '{
    "functionName": "ToUpperCase",
    "parameters": { "text": "hello world" }
  }'
```

Expected response:
```json
{
  "success": true,
  "data": {
    "result": "HELLO WORLD",
    "originalLength": 11
  },
  "error": null,
  "executionTimeMs": 12
}
```

### Step 1.3: Create Your First Tool

```bash
# Generate a new tool
dotnet script create-tool.csx "JsonTransform" "Transforms JSON data"

# Navigate to tool directory
cd tools/JsonTransform

# Build the tool
dotnet build

# The DLL will be at: JsonTransform/bin/Debug/net8.0/JsonTransform.dll
```

### Step 1.4: Test Plugin Loading

Copy your tool DLL to the plugins directory:

```bash
# Copy from your tool
cp tools/JsonTransform/bin/Debug/net8.0/JsonTransform.dll src/MCPServer.Core/plugins/

# OR if using PowerShell on Windows
Copy-Item -Path "tools/JsonTransform/bin/Debug/net8.0/JsonTransform.dll" -Destination "src/MCPServer.Core/plugins/"
```

Restart the server:
```bash
# Press Ctrl+C to stop, then:
dotnet run --project src/MCPServer.Core
```

The server should now log:
```
info: MCPServer.Core.ToolManager[0]
      Loaded 1 tools from ./plugins
```

Test your new tool:
```bash
curl http://localhost:5000/api/tools

# Should now include both:
# - text-processing (built-in example)
# - json-transform (your new tool)
```

---

## Phase 2: Push to GitHub

### Step 2.1: Initialize Git Repository

```bash
cd c:\biztalk-migrator-server

# Initialize git
git init

# Add all files
git add .

# Create initial commit
git commit -m "Initial MCP Server setup with example tools"
```

### Step 2.2: Create GitHub Repository

1. Go to [GitHub.com](https://github.com)
2. Click "+" → "New repository"
3. Name: `mcp-server` (or your preferred name)
4. Description: "Model Context Protocol Server with plugin architecture"
5. Choose Public (for open source)
6. Click "Create repository"

### Step 2.3: Push to GitHub

```bash
# Add remote
git remote add origin https://github.com/YOUR_USERNAME/mcp-server.git

# Rename branch if needed
git branch -M main

# Push to GitHub
git push -u origin main
```

### Step 2.4: Set Up GitHub Secrets

Go to your GitHub repository:
1. Settings → Secrets and variables → Actions
2. Create `NUGET_API_KEY` (if publishing NuGet packages)
   - Get from [nuget.org](https://www.nuget.org/) → Account settings
   - Paste your API key

---

## Phase 3: Deploy MCP Server

### Option A: Docker (Easiest for Local Testing)

```bash
# Build Docker image
docker build -t mcp-server:latest .

# Run in Docker
docker run -p 5000:5000 \
  -v ${PWD}/plugins:/app/plugins \
  -v ${PWD}/logs:/app/logs \
  mcp-server:latest
```

### Option B: Azure Container Instance (Cloud Deployment)

#### Prerequisites
- Azure account
- Azure CLI installed

#### Steps

```bash
# Login to Azure
az login

# Create resource group
az group create \
  --name mcp-server-rg \
  --location eastus

# Push image to Azure Container Registry
az acr create \
  --resource-group mcp-server-rg \
  --name mcpserverregistry \
  --sku Basic

# Build and push image
az acr build \
  --registry mcpserverregistry \
  --image mcp-server:latest .

# Create container instance
az container create \
  --resource-group mcp-server-rg \
  --name mcp-server \
  --image mcpserverregistry.azurecr.io/mcp-server:latest \
  --cpu 1 \
  --memory 1 \
  --ports 5000 \
  --dns-name-label mcp-server \
  --restart-policy OnFailure
```

Get the public URL:
```bash
az container show \
  --resource-group mcp-server-rg \
  --name mcp-server \
  --query ipAddress.fqdn
```

Your server is now at: `http://mcp-server.eastus.azurecontainer.io:5000`

### Option C: Docker Hub

```bash
# Tag image
docker tag mcp-server:latest YOUR_USERNAME/mcp-server:latest

# Login to Docker Hub
docker login

# Push image
docker push YOUR_USERNAME/mcp-server:latest

# Pull and run anywhere
docker run -p 5000:5000 \
  -v ${PWD}/plugins:/app/plugins \
  YOUR_USERNAME/mcp-server:latest
```

### Option D: Azure App Service

```bash
# Create App Service Plan
az appservice plan create \
  --name mcp-server-plan \
  --resource-group mcp-server-rg \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --resource-group mcp-server-rg \
  --plan mcp-server-plan \
  --name mcp-server-app \
  --deployment-container-image-name mcpserverregistry.azurecr.io/mcp-server:latest

# Set continuous deployment
az webapp deployment container config \
  --name mcp-server-app \
  --resource-group mcp-server-rg \
  --enable-cd true
```

---

## Phase 4: Automatic Tool Discovery & Loading

### Step 4.1: Update ToolManager for Remote Plugin Loading

The ToolManager already supports loading from the `./plugins` directory. For the MCP server running in production, we need to:

1. **Deploy plugins directory with server**
2. **Monitor directory for new DLLs**
3. **Reload tools on new deployments**

Create a new file: `src/MCPServer.Core/PluginWatcher.cs`

```csharp
using System.IO;
using System.Reflection;

namespace MCPServer.Core;

/// <summary>
/// Watches the plugins directory for new/updated tools.
/// </summary>
public class PluginWatcher : IDisposable
{
    private readonly ToolManager _toolManager;
    private readonly ILogger<PluginWatcher> _logger;
    private readonly FileSystemWatcher _watcher;
    private readonly string _pluginDirectory;

    public PluginWatcher(ToolManager toolManager, ILogger<PluginWatcher> logger, string pluginDirectory = "./plugins")
    {
        _toolManager = toolManager;
        _logger = logger;
        _pluginDirectory = pluginDirectory;

        _watcher = new FileSystemWatcher(Path.GetFullPath(pluginDirectory), "*.dll")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };

        _watcher.Created += OnDllCreatedOrChanged;
        _watcher.Changed += OnDllCreatedOrChanged;
    }

    private void OnDllCreatedOrChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce: Wait a bit for file to be fully written
        Thread.Sleep(1000);

        _logger.LogInformation("Detected new/updated plugin: {FileName}", Path.GetFileName(e.FullPath));
        
        try
        {
            // Reload tools from plugins directory
            _ = _toolManager.LoadToolsFromDirectoryAsync(_pluginDirectory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading plugin from file change");
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}
```

Update `Program.cs` to use the watcher:

```csharp
var builder = MCPServerExtensions.CreateMCPServerBuilder(args);

// Register example tools
builder.Services.AddScoped<TextProcessingTool>();
builder.Services.AddSingleton<PluginWatcher>();

var app = builder.Build();

// Initialize MCP server
var toolManager = app.Services.GetRequiredService<ToolManager>();
var mcpServer = new MCPServerApplication(app, toolManager);

// Register example tool
var textTool = app.Services.GetRequiredService<TextProcessingTool>();
toolManager.RegisterTool(textTool);

// Load tools from plugins directory
await toolManager.LoadToolsFromDirectoryAsync("./plugins");

// Start watching for new plugins
_ = app.Services.GetRequiredService<PluginWatcher>();

mcpServer.Initialize();

// Run on port 5000
var port = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://localhost:5000";
await mcpServer.RunAsync(port);
```

### Step 4.2: Update GitHub Workflow for Automatic Deployment

Update `.github/workflows/build.yml` to deploy after successful merge:

```yaml
name: Build, Test, and Deploy

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Run tests
      run: dotnet test --configuration Release --no-build --verbosity normal
    
    - name: Build Docker image
      if: github.ref == 'refs/heads/main'
      run: docker build -t mcp-server:${{ github.sha }} -t mcp-server:latest .
    
    - name: Push to registry
      if: github.ref == 'refs/heads/main'
      run: |
        echo "${{ secrets.DOCKER_PASSWORD }}" | docker login -u "${{ secrets.DOCKER_USERNAME }}" --password-stdin
        docker push mcp-server:${{ github.sha }}
        docker push mcp-server:latest
  
  deploy:
    needs: build
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    
    steps:
    - name: Deploy to Azure Container Instance
      uses: azure/CLI@v1
      with:
        azcliversion: 2.30.0
        inlineScript: |
          az login --service-principal \
            -u ${{ secrets.AZURE_CLIENT_ID }} \
            -p ${{ secrets.AZURE_CLIENT_SECRET }} \
            --tenant ${{ secrets.AZURE_TENANT_ID }}
          
          az container create \
            --resource-group ${{ secrets.AZURE_RESOURCE_GROUP }} \
            --name mcp-server \
            --image mcp-server:latest \
            --registry-login-server mcpserverregistry.azurecr.io \
            --registry-username ${{ secrets.AZURE_REGISTRY_USERNAME }} \
            --registry-password ${{ secrets.AZURE_REGISTRY_PASSWORD }} \
            --overwrite
```

### Step 4.3: Set Up GitHub Secrets for Azure Deployment

In GitHub:
1. Settings → Secrets and variables → Actions
2. Add secrets:
   - `DOCKER_USERNAME` - Docker Hub username
   - `DOCKER_PASSWORD` - Docker Hub personal access token
   - `AZURE_CLIENT_ID` - Azure service principal
   - `AZURE_CLIENT_SECRET` - Azure service principal password
   - `AZURE_TENANT_ID` - Azure tenant ID
   - `AZURE_RESOURCE_GROUP` - Azure resource group name
   - `AZURE_REGISTRY_USERNAME` - Container registry username
   - `AZURE_REGISTRY_PASSWORD` - Container registry password

---

## Phase 5: Tool Submission Workflow

### Step 5.1: Developer Creates a Tool

```bash
# In their fork
git clone https://github.com/THEIR_USERNAME/mcp-server.git
cd mcp-server
git checkout -b feature/add-json-transform-tool

# Create tool
dotnet script create-tool.csx "JsonTransform" "Transform JSON data"

# Build and test
cd tools/JsonTransform
dotnet build
dotnet test

# Commit and push
git add tools/JsonTransform/
git commit -m "Add JsonTransform tool

- Implements JSON parsing and transformation
- Includes 3 functions: Parse, Transform, Validate
- 85% test coverage"

git push origin feature/add-json-transform-tool
```

### Step 5.2: Create Pull Request

On GitHub, create PR with:
- Title: "Add JsonTransform Tool"
- Description: What the tool does, examples
- Reference any related issues

### Step 5.3: Automated Tool Validation

CI/CD automatically runs ([.github/workflows/tool-validation.yml](.github/workflows/tool-validation.yml)):

✓ Structure validation  
✓ Build verification  
✓ Unit tests (80%+ coverage required)  
✓ Code analysis  
✓ Integration testing  
✓ NuGet packaging  

### Step 5.4: Code Review

Maintainers review:
- Code quality
- Documentation
- Test coverage
- Security considerations

### Step 5.5: Merge and Auto-Deploy

After approval and merge to main:

1. GitHub Actions **build.yml** runs
2. Tests pass ✓
3. Docker image built
4. Image pushed to registry
5. **Auto-deploy triggers**
6. Live server updated with new tool

New tool is now live on production!

---

## Phase 6: Verify Tool on Live Server

### Step 6.1: Test New Tool on Production

```bash
# Get your server URL (from Azure/Docker output)
SERVER_URL="http://mcp-server.eastus.azurecontainer.io:5000"

# List all tools
curl $SERVER_URL/api/tools

# Should now include the new JsonTransform tool

# Test the new tool
curl -X POST $SERVER_URL/api/tools/json-transform/execute \
  -H "Content-Type: application/json" \
  -d '{
    "functionName": "Parse",
    "parameters": { "json": "{\"name\": \"John\"}" }
  }'
```

### Step 6.2: Monitor the Server

Check logs:
```bash
# If using Docker
docker logs mcp-server

# If using Azure
az container logs \
  --resource-group mcp-server-rg \
  --name mcp-server
```

Look for:
```
info: MCPServer.Core.ToolManager[0]
      Loaded 1 tools from ./plugins
```

---

## Complete End-to-End Example

Here's a complete flow from start to finish:

### Developer A: Creates a Tool

```bash
# 1. Create fork and branch
git clone https://github.com/YOUR_USERNAME/mcp-server.git
git checkout -b feature/image-resize-tool

# 2. Generate tool
dotnet script create-tool.csx "ImageResize" "Resize images"

# 3. Implement functions in tools/ImageResize/ImageResizeTool.cs
# ... (implementation details)

# 4. Add tests in tools/ImageResize.Tests/
# ... (test implementation)

# 5. Build and test locally
cd tools/ImageResize
dotnet build
dotnet test

# 6. Commit and push
git add tools/ImageResize/
git commit -m "Add ImageResize tool with width/height/quality params"
git push origin feature/image-resize-tool

# 7. Create PR on GitHub
# ... (fill in title, description, examples)
```

### CI/CD: Automatic Validation

```
PR Created
    ↓
tool-validation.yml runs
    ├─ Validate structure ✓
    ├─ Build tool ✓
    ├─ Run tests ✓
    ├─ Code analysis ✓
    └─ Comment results on PR ✓
    ↓
Maintainer reviews code
    ├─ Code quality ✓
    ├─ Documentation ✓
    └─ Approve PR ✓
    ↓
Merge to main
    ↓
build.yml triggers
    ├─ Build server ✓
    ├─ Run all tests ✓
    ├─ Build Docker image ✓
    ├─ Push to registry ✓
    └─ Deploy to production ✓
    ↓
Production Server Updated
    ├─ PluginWatcher detects new DLL
    ├─ Loads ImageResize tool
    └─ Available at /api/tools ✓
    ↓
Tool is LIVE
```

### User: Uses New Tool

```bash
curl -X POST http://mcp-server.live.com/api/tools/image-resize/execute \
  -H "Content-Type: application/json" \
  -d '{
    "functionName": "Resize",
    "parameters": {
      "imageUrl": "https://example.com/image.jpg",
      "width": 800,
      "height": 600,
      "quality": 90
    }
  }'

# Response:
{
  "success": true,
  "data": {
    "resizedImageUrl": "https://storage.example.com/resized-image.jpg",
    "originalSize": "2.5 MB",
    "newSize": "0.3 MB",
    "dimensions": "800x600"
  },
  "error": null,
  "executionTimeMs": 245
}
```

---

## Troubleshooting

### Tool Not Appearing on Live Server

```bash
# Check Azure logs
az container logs --resource-group mcp-server-rg --name mcp-server

# Look for plugin loading errors
# Common issues:
# - DLL targets wrong framework (must be net8.0)
# - Missing dependencies
# - Doesn't implement IMCPTool
```

### Deployment Failed

```bash
# Check GitHub Actions log
# Go to repository → Actions → Latest run → See detailed logs

# Common issues:
# - Docker build failed (missing dependencies)
# - Tests failed (implement missing tests)
# - Azure credentials incorrect (check secrets)
```

### Manual Restart

```bash
# Restart Azure container
az container restart \
  --resource-group mcp-server-rg \
  --name mcp-server

# Restart Docker container
docker restart mcp-server
```

---

## Summary

| Phase | What Happens | Time |
|-------|--------------|------|
| **1** | Local testing & development | 30 min |
| **2** | Push to GitHub | 5 min |
| **3** | Deploy server to cloud | 10 min |
| **4** | Set up auto-discovery | 15 min |
| **5** | Developer submits tool PR | Variable |
| **6** | CI/CD validates & deploys | 5-10 min |
| **Total** | End-to-end workflow | ~75 min (one-time setup) |

Once set up, each new tool takes only 5-10 minutes to go from PR to production!
