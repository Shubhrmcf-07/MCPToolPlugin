# MCP Server Tool Template

This template helps developers create new tools for the MCP (Model Context Protocol) Server. Tools are modular components that expose functionality through well-defined functions.

## Quick Start

### 1. Create a new tool class library project

```bash
dotnet new classlib -n "MyCustomTool"
cd MyCustomTool
dotnet add reference ../MCPServer.ToolTemplate/MCPServer.ToolTemplate.csproj
```

### 2. Implement the `IMCPTool` interface

```csharp
using MCPServer.ToolTemplate;
using Microsoft.Extensions.Logging;

public class MyCustomTool : MCPToolBase
{
    public MyCustomTool(ILogger<MyCustomTool> logger) : base(logger)
    {
    }

    public override ToolMetadata GetMetadata()
    {
        return new ToolMetadata
        {
            ToolId = "my-custom-tool",
            Name = "My Custom Tool",
            Version = "1.0.0",
            Author = "Your Name",
            Description = "Description of what your tool does"
        };
    }

    public override List<ToolFunction> GetFunctions()
    {
        return new List<ToolFunction>
        {
            new ToolFunction
            {
                Name = "MyFunction",
                Description = "What this function does",
                Parameters = new List<ToolParameter>
                {
                    new ToolParameter
                    {
                        Name = "inputParam",
                        Type = "string",
                        Description = "Parameter description",
                        Required = true
                    }
                }
            }
        };
    }

    public override async Task<ToolResult> ExecuteAsync(string functionName, Dictionary<string, object?> parameters)
    {
        return functionName switch
        {
            "MyFunction" => await ExecuteMyFunction(parameters),
            _ => Failure($"Function not found: {functionName}")
        };
    }

    private async Task<ToolResult> ExecuteMyFunction(Dictionary<string, object?> parameters)
    {
        if (!ValidateRequiredParameters(parameters, "inputParam"))
        {
            return Failure("Missing required parameters");
        }

        try
        {
            var input = parameters["inputParam"]?.ToString();
            var result = await DoSomeWork(input);
            return Success(new { result });
        }
        catch (Exception ex)
        {
            return Failure(ex.Message);
        }
    }

    private async Task<string> DoSomeWork(string? input)
    {
        // Implement your logic here
        await Task.Delay(10);
        return $"Processed: {input}";
    }
}
```

## Tool Structure

### ToolMetadata
Describes your tool with:
- **ToolId**: Unique identifier (lowercase, hyphenated)
- **Name**: Display name
- **Version**: Semantic versioning
- **Author**: Creator information
- **Description**: What your tool does

### ToolFunction
Each function your tool exposes must have:
- **Name**: Function identifier
- **Description**: What it does
- **Parameters**: List of `ToolParameter` objects

### ToolParameter
Describes function parameters:
- **Name**: Parameter name
- **Type**: Data type (string, int, bool, etc.)
- **Description**: Usage information
- **Required**: Whether it's mandatory

### ToolResult
Return value from function execution:
- **Success**: Boolean indicating success/failure
- **Data**: Result payload
- **Error**: Error message if failed
- **ExecutionTimeMs**: Execution duration

## Publishing Your Tool

### 1. Build your tool
```bash
dotnet build -c Release
```

### 2. Create a NuGet package (optional)
```bash
dotnet pack -c Release
```

### 3. Add to plugin directory
Copy your compiled DLL to the MCP server's `./plugins` directory:
```bash
cp bin/Release/net8.0/MyCustomTool.dll ../MCPServer.Core/plugins/
```

### 4. Restart the MCP server
The server will automatically discover and load your tool on startup.

## Helper Methods

The `MCPToolBase` class provides:

- **Success(data, executionTimeMs)**: Create successful result
- **Failure(error, executionTimeMs)**: Create failure result
- **ValidateRequiredParameters(parameters, ...paramNames)**: Validate required params

## Testing Your Tool

### Register in MCP Server
```csharp
var toolManager = app.Services.GetRequiredService<ToolManager>();
var myTool = new MyCustomTool(logger);
toolManager.RegisterTool(myTool);
```

### Call via HTTP API
```bash
curl -X POST http://localhost:5000/api/tools/my-custom-tool/execute \
  -H "Content-Type: application/json" \
  -d '{
    "functionName": "MyFunction",
    "parameters": { "inputParam": "test" }
  }'
```

## Best Practices

1. **Async All The Way**: Use async/await for I/O operations
2. **Validate Input**: Always validate required parameters
3. **Handle Exceptions**: Catch and return meaningful error messages
4. **Log Appropriately**: Use ILogger for debugging
5. **Document**: Include XML comments for public members
6. **Version**: Use semantic versioning for your tool
7. **Test**: Write unit tests for your functions

## Deployment

When submitting a tool for integration:

1. Create a PR with your tool source code
2. Include README documenting the tool
3. Include unit tests (minimum 80% coverage)
4. Pass all CI/CD checks
5. Get code review approval
6. Tool will be built and added to plugin repository

## CI/CD Integration

New tools are validated through:

1. **Build verification**: Ensures compilation
2. **Unit tests**: Runs all tests
3. **Code analysis**: Static analysis checks
4. **Package creation**: Creates NuGet package
5. **Integration test**: Loads tool in MCP server
6. **Publish**: Publishes to plugin repository

See `.github/workflows/tool-validation.yml` for the full pipeline.
