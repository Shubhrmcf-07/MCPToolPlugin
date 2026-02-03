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

### 2. Add Published Tools

```bash
dotnet add package MCPServer.Examples
dotnet add package MCPServer.YourToolName  # Any published tools
```

### 3. Implement Tool Loading

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

## Next Steps

1. Create private Git repository for MCPServer.Core
2. Implement ToolManager (tool registry & execution)
3. Add security layer (authentication/authorization)
4. Deploy to your infrastructure
