# Private MCP Server Setup (Internal Only)

This document describes how to set up the internal MCP server that consumes published tools from the public repository.

## Architecture

```
Public Repository (GitHub)
├── MCPServer.ToolTemplate
├── MCPServer.Examples
└── CI/CD publishes tools to NuGet
        ↓
        Published Packages on NuGet.org
        ↓
Private MCP Server (Your Infrastructure)
├── MCPServer.Core (not public)
├── Consumes tools via NuGet packages
└── Manages tool execution & routing
```

## Setup

### 1. Create Private MCP Server Project

```bash
dotnet new webapi -n MCPServer.Core
cd MCPServer.Core
dotnet add package Serilog
dotnet add package Serilog.Sinks.File
```

### 2. Configure Custom NuGet Feed

Add your custom feed to your local NuGet config:

```bash
dotnet nuget add source \
  --name YourCustomFeed \
  --username your-username \
  --password your-password \
  https://your-nuget-server.com/v3/index.json
```

Or edit `~/.nuget/NuGet/NuGet.Config`:

```xml
<configuration>
  <packageSources>
    <add key="YourCustomFeed" value="https://your-nuget-server.com/v3/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <YourCustomFeed>
      <add key="Username" value="your-username" />
      <add key="ClearTextPassword" value="your-password" />
    </YourCustomFeed>
  </packageSourceCredentials>
</configuration>
```

### 3. Add Published Tools

```bash
dotnet add package MCPServer.Examples --source YourCustomFeed
dotnet add package MCPServer.YourToolName --source YourCustomFeed
```

Your `Program.cs` should:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("./logs/mcp-server-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// Register tools from NuGet packages
builder.Services.AddSingleton<TextProcessingTool>();
builder.Services.AddSingleton<YourOtherTool>();  // From NuGet

var app = builder.Build();

// Create ToolManager and register tools
var toolManager = app.Services.GetRequiredService<ToolManager>();
app.Services.GetRequiredService<TextProcessingTool>()
    .ForEach(t => toolManager.RegisterTool(t));

// Add API routes...
await app.RunAsync();
```

### 4. Deploy

```bash
dotnet publish -c Release
# Deploy to your infrastructure (Docker, Kubernetes, Azure, etc.)
```

## Workflow: Adding New Tools

1. **Community contributes tool** → Submits PR to public repo
2. **CI validates tool** → Runs tests and checks
3. **Tool published to NuGet** → On merge to main
4. **Update private server** → `dotnet add package MCPServer.NewTool`
5. **Redeploy** → Server now offers new tool

No code changes needed in your server - just add the NuGet package!

## Benefits

✓ **Separation of Concerns**: Public tools, private server
✓ **Community Contributions**: Easy for developers to contribute tools
✓ **Version Control**: Each tool versioned independently on NuGet
✓ **Decoupled**: Tools don't need to know about each other
✓ **Scalable**: Add tools without modifying server code
✓ **Security**: Keep your server infrastructure private

## Key Files

- `MCPServer.ToolTemplate/IMCPTool.cs` - Interface all tools implement
- `MCPServer.Examples/TextProcessingTool.cs` - Reference implementation
- `.github/workflows/build.yml` - Auto-publishes tools to NuGet

## Publishing Your Own Tools

1. Follow the TOOL_TEMPLATE.md guide
2. Submit PR to public repository
3. CI validates and tests your tool
4. On merge, tool automatically published to NuGet
5. Reference in your internal server via `dotnet add package`

## Troubleshooting

**Tool not loading?**
- Verify NuGet package installed: `dotnet list package`
- Check tool registration in Program.cs
- Review tool manifest (IMCPTool implementation)

**Version conflicts?**
- Update via: `dotnet package update MCPServer.ToolName`
- Pin specific version if needed

## GitHub Actions Setup

### Configure Publishing Secrets

In your GitHub repository, add these secrets (Settings → Secrets → Actions):

| Secret | Value | Example |
|--------|-------|---------|
| `NUGET_FEED_URL` | Your custom feed URL | `https://pkgs.dev.azure.com/org/_packaging/MCP/nuget/v3/index.json` |
| `NUGET_FEED_USERNAME` | Feed username or PAT | `user@domain.com` |
| `NUGET_FEED_PASSWORD` | Feed password or PAT token | `*** (your token)` |
| `NUGET_FEED_API_KEY` | Feed API key | `oy2...` (if required) |

### Custom Feed Options

**Azure Artifacts** (Recommended for Azure users)
```
URL: https://pkgs.dev.azure.com/{org}/_packaging/{feed}/nuget/v3/index.json
Auth: Personal Access Token (PAT)
```

**GitHub Packages**
```
URL: https://nuget.pkg.github.com/{owner}/index.json
Auth: GitHub token
```

**On-Premises (ProGet, Artifactory, etc.)**
```
URL: https://your-server.com/nuget/v3/index.json
Auth: API key or credentials
```

**Private MyGet Feed**
```
URL: https://www.myget.org/F/{feed}/auth/{apikey}/api/v3/index.json
```

## Troubleshooting

1. Create private Git repository for MCPServer.Core
2. Implement ToolManager (tool registry & execution)
3. Add security layer (authentication/authorization)
4. Deploy to your infrastructure
