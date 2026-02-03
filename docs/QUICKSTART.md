# Testing & Deployment Quick Reference

Quick commands for testing, deploying, and monitoring your MCP Server.

## Local Testing

### Start the Server
```bash
dotnet run --project src/MCPServer.Core
```

### Test Endpoints (New Terminal)
```bash
# Health check
curl http://localhost:5000/health

# List tools
curl http://localhost:5000/api/tools

# Execute a function
curl -X POST http://localhost:5000/api/tools/text-processing/execute \
  -H "Content-Type: application/json" \
  -d '{"functionName":"ToUpperCase","parameters":{"text":"hello"}}'
```

### Create and Test a Tool
```bash
# Generate tool
dotnet script create-tool.csx "MyTool" "Description"

# Build tool
cd tools/MyTool && dotnet build && cd ../..

# Copy to plugins (Windows PowerShell)
Copy-Item "tools/MyTool/bin/Debug/net8.0/MyTool.dll" "src/MCPServer.Core/plugins/"

# Tool auto-loads via PluginWatcher when you paste the DLL!
# Check logs for: "Reloaded 1 plugins after file change"

# Test new tool
curl http://localhost:5000/api/tools
```

## GitHub Setup

### Initial Push
```bash
# Initialize and push to GitHub
git init
git add .
git commit -m "Initial MCP Server"
git remote add origin https://github.com/YOUR_USERNAME/mcp-server.git
git push -u origin main
```

### Configure GitHub Secrets
Go to: Settings → Secrets and variables → Actions

**Required for Docker Hub:**
- `DOCKER_USERNAME` - Your Docker Hub username
- `DOCKER_PASSWORD` - Personal access token from Docker Hub

**Required for Azure:**
- `AZURE_CLIENT_ID` - From service principal
- `AZURE_CLIENT_SECRET` - From service principal
- `AZURE_TENANT_ID` - Your Azure tenant ID
- `AZURE_RESOURCE_GROUP` - Your resource group name
- `AZURE_REGISTRY_USERNAME` - Container registry username
- `AZURE_REGISTRY_PASSWORD` - Container registry password

## Deployment (Choose One)

### Docker Compose (Local/Simple)
```bash
docker-compose up
# Server at: http://localhost:5000
```

### Docker (Manual)
```bash
# Build
docker build -t mcp-server:latest .

# Run
docker run -p 5000:5000 \
  -v ${PWD}/plugins:/app/plugins \
  mcp-server:latest
```

### Azure Container Instance (Cloud)
```bash
# Login
az login

# Create resource group
az group create --name mcp-rg --location eastus

# Create container
az container create \
  --resource-group mcp-rg \
  --name mcp-server \
  --image mcp-server:latest \
  --ports 5000 \
  --dns-name-label mcp-server

# Get URL
az container show \
  --resource-group mcp-rg \
  --name mcp-server \
  --query ipAddress.fqdn
```

### Azure App Service
```bash
# Create plan
az appservice plan create \
  --name mcp-plan \
  --resource-group mcp-rg \
  --sku B1 \
  --is-linux

# Create app
az webapp create \
  --resource-group mcp-rg \
  --plan mcp-plan \
  --name mcp-server-app \
  --deployment-container-image-name mcp-server:latest
```

## Tool Contribution Workflow

### For Tool Developers

```bash
# 1. Fork repository
# 2. Clone and create branch
git clone https://github.com/YOUR_FORK/mcp-server.git
git checkout -b feature/my-tool

# 3. Generate tool
dotnet script create-tool.csx "MyTool" "Description"

# 4. Implement and test
cd tools/MyTool
dotnet build
dotnet test

# 5. Commit and push
git add .
git commit -m "Add MyTool"
git push origin feature/my-tool

# 6. Create PR on GitHub
# - CI/CD validates automatically
# - Wait for review
# - Merge when approved
```

## Monitoring

### Local Server Logs
```bash
# Logs saved to: ./logs/mcp-server-{date}.txt
tail -f logs/mcp-server-*.txt
```

### Docker Logs
```bash
docker logs mcp-server
docker logs mcp-server -f  # Follow logs
```

### Azure Container Logs
```bash
az container logs \
  --resource-group mcp-rg \
  --name mcp-server \
  -f  # Follow logs
```

### App Service Logs
```bash
az webapp log tail \
  --resource-group mcp-rg \
  --name mcp-server-app \
  -f  # Follow logs
```

## Verify Deployment

### Check Server Status
```bash
curl http://YOUR_SERVER_URL/health
```

### List Tools
```bash
curl http://YOUR_SERVER_URL/api/tools
```

### Test Tool
```bash
curl -X POST http://YOUR_SERVER_URL/api/tools/text-processing/execute \
  -H "Content-Type: application/json" \
  -d '{"functionName":"ToUpperCase","parameters":{"text":"hello"}}'
```

## Rebuild After Changes

```bash
# Clean and rebuild
dotnet clean
dotnet build

# Or run tests
dotnet test

# Or rebuild Docker
docker build -t mcp-server:latest .
docker push mcp-server:latest  # If using registry
```

## Troubleshooting

### Tool Not Loading
```bash
# Check if plugins directory exists
ls src/MCPServer.Core/plugins/

# Check if DLL is there
ls src/MCPServer.Core/plugins/*.dll

# Check logs for errors
tail -f logs/mcp-server-*.txt | grep -i error
```

### Server Won't Start
```bash
# Port already in use?
netstat -ano | findstr :5000  # Windows
lsof -i :5000  # Mac/Linux

# Try different port
dotnet run --project src/MCPServer.Core -- --urls http://localhost:5001
```

### Docker Build Fails
```bash
# Check Dockerfile
docker build --no-cache -t mcp-server:latest .

# See detailed error
docker build -v -t mcp-server:latest .
```

### Tests Fail
```bash
# Run specific test project
dotnet test src/MCPServer.Examples.Tests -v

# See detailed output
dotnet test --logger "console;verbosity=detailed"
```

## Performance Tips

1. **Use Release Build for production**
   ```bash
   dotnet build -c Release
   docker build --build-arg CONFIGURATION=Release .
   ```

2. **Monitor tool execution times**
   - Check `executionTimeMs` in responses
   - Optimize slow tools

3. **Scale horizontally**
   - Run multiple server instances
   - Use load balancer (nginx, Azure Load Balancer)

4. **Cache tool metadata**
   - Don't call `/api/tools/{id}` for every request
   - Cache locally for 5-10 minutes

5. **Batch operations**
   - Don't call one function per request
   - Batch similar operations

## Complete End-to-End (First Time)

```bash
# 1. Local test (5 min)
dotnet run --project src/MCPServer.Core &
curl http://localhost:5000/health
pkill -f "dotnet run"

# 2. Push to GitHub (5 min)
git init && git add . && git commit -m "Initial"
git remote add origin https://github.com/YOU/mcp-server.git
git push -u origin main

# 3. Configure secrets (5 min)
# Go to GitHub repo → Settings → Secrets → Add DOCKER_USERNAME, etc.

# 4. Deploy (10 min)
docker build -t mcp-server:latest .
docker run -p 5000:5000 mcp-server:latest

# 5. Test deployed (2 min)
curl http://localhost:5000/api/tools

# Done! Now:
# - Developers can fork and create PR with new tools
# - CI/CD automatically validates
# - You merge to auto-deploy
# - PluginWatcher reloads tools live
```

## References

- Full deployment guide: [docs/DEPLOYMENT.md](DEPLOYMENT.md)
- Architecture: [docs/ARCHITECTURE.md](ARCHITECTURE.md)
- Tool template: [docs/TOOL_TEMPLATE.md](TOOL_TEMPLATE.md)
- Contributing: [docs/CONTRIBUTING.md](CONTRIBUTING.md)
- API reference: [docs/API.md](API.md)
