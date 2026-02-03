# MCP Tools - Open Contribution Platform

Create and share tools for the Model Context Protocol (MCP). This repository contains the template and examples for building tools that integrate with MCP servers.

## Quick Start

### Build Tools
```bash
dotnet build
```

### Create Your Tool

See [TOOL_TEMPLATE.md](docs/TOOL_TEMPLATE.md) for complete guide.

Basic example:
```csharp
public class MyTool : IMCPTool {
    public string ToolId => "my-tool";
    public ToolMetadata GetMetadata() => new() { Name = "My Tool", Version = "1.0.0" };
    public List<ToolFunction> GetFunctions() => new() { /* functions */ };
    public Task<FunctionResult> ExecuteAsync(string function, Dictionary<string, object?> parameters) => /* ... */;
}
```

## Project Structure

```
├── src/
│   ├── MCPServer.ToolTemplate/  # Tool interfaces & base classes
│   └── MCPServer.Examples/      # Example tool implementations
├── docs/                         # Documentation
└── .github/workflows/            # CI/CD - builds and publishes tools
```

## Workflow: Contribute a Tool

1. **Fork** this repository
2. **Create** a new tool in `src/MCPServer.YourToolName/`
3. **Follow** the [TOOL_TEMPLATE.md](docs/TOOL_TEMPLATE.md) guide
4. **Submit** a PR with your tool
5. **CI validates** the tool (builds, tests, structure)
6. **Merge** → Tool published to your custom feed
7. **MCP servers** install your tool via NuGet

## Example: TextProcessingTool

Located in `src/MCPServer.Examples/`, implements:
- Count words and characters
- Convert case (upper/lower)
- Reverse text
- Remove whitespace

## Documentation

- [API Documentation](docs/API.md)
- [Tool Template Guide](docs/TOOL_TEMPLATE.md)  
- [Tool Architecture](docs/ARCHITECTURE.md)
- [Contributing Guidelines](docs/CONTRIBUTING.md)

## How Tools Get Published

1. **Build & Test** - GitHub Actions validates all PRs (public)
2. **Auto-Pack** - NuGet packages created and uploaded as artifacts
3. **Download locally** - Get packages from GitHub Actions
4. **Publish privately** - Push to your custom feed with credentials (locally)

All credentials stay **private**. Never stored in GitHub.

See [PUBLISH-LOCALLY.md](docs/PUBLISH-LOCALLY.md) for publishing steps.

## Get Tool Details
```bash
GET /api/tools/{toolId}
```

### Get Tool Functions
```bash
GET /api/tools/{toolId}/functions
```

### Execute Tool Function
```bash
POST /api/tools/{toolId}/execute
Content-Type: application/json

{
  "functionName": "FunctionName",
  "parameters": {
    "param1": "value1",
    "param2": "value2"
  }
}
```

## Creating Your First Tool

See [TOOL_TEMPLATE.md](docs/TOOL_TEMPLATE.md) for detailed instructions on creating and publishing tools.

### Example: Text Processing Tool
```csharp
public class MyTool : MCPToolBase
{
    public MyTool(ILogger<MyTool> logger) : base(logger) { }

    public override ToolMetadata GetMetadata() => new()
    {
        ToolId = "my-tool",
        Name = "My Tool",
        Version = "1.0.0",
        Author = "Your Name",
        Description = "What it does"
    };

    public override List<ToolFunction> GetFunctions() => new()
    {
        new ToolFunction 
        { 
            Name = "Process",
            Description = "Process some data",
            Parameters = new() 
            {
                new ToolParameter { Name = "input", Type = "string", Required = true }
            }
        }
    };

    public override async Task<ToolResult> ExecuteAsync(string functionName, Dictionary<string, object?> parameters)
    {
        return functionName switch
        {
            "Process" => Success(new { result = "processed" }),
            _ => Failure("Unknown function")
        };
    }
}
```

## Tool Management

### Tool Discovery
The server automatically discovers and loads tools from:
1. The `.NET ServiceProvider` (registered tools)
2. The `./plugins` directory (plugin assemblies)

### Plugin Directory Structure
```
./plugins/
├── MyTool.dll
├── AnotherTool.dll
└── ThirdTool.dll
```

## Project Structure

```
├── src/
│   ├── MCPServer.Core/          # Main server application
│   ├── MCPServer.ToolTemplate/  # Tool template and interfaces
│   └── MCPServer.Examples/      # Example tool implementations
├── tools/                        # Community contributed tools
├── docs/                         # Documentation
├── .github/workflows/            # CI/CD pipelines
└── MCPServer.sln
```

## CI/CD Integration

### Tool Validation Workflow
When you submit a PR with a new tool, the CI/CD pipeline automatically:

1. **Validates structure** - Ensures required files and interfaces
2. **Builds** - Compiles the tool
3. **Tests** - Runs unit tests
4. **Analyzes** - Static code analysis
5. **Integrates** - Tests loading in the MCP server
6. **Packages** - Creates NuGet package
7. **Publishes** - Publishes to NuGet registry (on main branch)

See [.github/workflows/tool-validation.yml](.github/workflows/tool-validation.yml)

## Contributing

We welcome community contributions! To add a new tool:

1. Create a new directory under `tools/` with your tool name
2. Implement the `IMCPTool` interface (use `MCPToolBase` as a base)
3. Add unit tests in a `YourTool.Tests` directory
4. Include a README.md with documentation
5. Submit a PR

See [CONTRIBUTING.md](docs/CONTRIBUTING.md) for detailed guidelines.

## Architecture

### Core Components

**ToolManager**
- Manages tool lifecycle
- Handles tool discovery and loading
- Routes function calls to appropriate tools
- Tracks execution metrics

**MCPServerApplication**
- HTTP API host
- REST endpoint definitions
- Request/response handling

**IMCPTool Interface**
- Base contract for all tools
- Defines metadata, functions, and execution

**MCPToolBase**
- Abstract base class
- Helper methods for result creation
- Parameter validation

## Configuration

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Development or Production
- `ASPNETCORE_URLS`: Server URL (default: http://localhost:5000)

### Application Settings
Create `appsettings.json` for configuration:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "PluginDirectory": "./plugins"
}
```

## Development

### Debug the Server
```bash
dotnet run --project src/MCPServer.Core --configuration Debug
```

### Run Tests
```bash
dotnet test MCPServer.sln
```

### Package Tools
```bash
dotnet pack src/MCPServer.ToolTemplate -c Release
```

## Deployment

### Docker
```bash
docker build -t mcp-server:latest .
docker run -p 5000:5000 mcp-server:latest
```

### Kubernetes
See `k8s/` directory for deployment manifests.

### Azure
Deploy using Azure Container Instances or App Service.

## Documentation

- [Tool Template Guide](docs/TOOL_TEMPLATE.md) - How to create tools
- [Contributing Guide](docs/CONTRIBUTING.md) - Contribution guidelines
- [API Reference](docs/API.md) - Detailed API documentation
- [Architecture Guide](docs/ARCHITECTURE.md) - System design and components
- [Complete Setup Guide](docs/COMPLETE_SETUP.md) - Step-by-step testing & deployment
- [Deployment Guide](docs/DEPLOYMENT.md) - Production deployment options
- [Quick Reference](docs/QUICKSTART.md) - Quick commands for common tasks

## Performance

- **Async I/O**: All operations use async/await
- **Dependency Injection**: Efficient service resolution
- **Plugin Loading**: Lazy loading of tool assemblies
- **Caching**: Tool metadata cached after loading

## Security Considerations

- Validate all input parameters
- Use managed dependencies with known security patches
- Implement rate limiting for production deployments
- Require authentication for sensitive tools

## Troubleshooting

### Tool Not Loading
1. Check plugin directory: `./plugins/`
2. Verify DLL exists and targets net8.0
3. Check logs for detailed error messages
4. Ensure tool implements `IMCPTool`

### Function Execution Failing
1. Verify parameter names and types
2. Check error message in response
3. Review tool logs
4. Test with valid parameter values

## Support

- 📖 [Documentation](docs/)
- 🐛 [Report Issues](https://github.com/yourorg/mcp-server/issues)
- 💬 [Discussions](https://github.com/yourorg/mcp-server/discussions)

## License

This project is licensed under the MIT License - see LICENSE file for details.

## Acknowledgments

Built with:
- .NET 8.0 
- ASP.NET Core
- Serilog
- Microsoft.Extensions.DependencyInjection
