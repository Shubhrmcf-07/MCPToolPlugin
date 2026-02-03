# Architecture Overview

## System Design

The MCP Server is built on a plugin-based architecture that allows for dynamic tool loading and execution.

```
┌─────────────────────────────────────────────────────────┐
│                  HTTP API Layer                         │
│  (ASP.NET Core endpoints for tool discovery/execution)  │
└──────────────────┬──────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│            MCPServerApplication                         │
│  (Route handlers, request/response management)          │
└──────────────────┬──────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│              ToolManager                                │
│  (Loads, registers, and routes calls to tools)          │
└──────────────┬────────────────────────┬─────────────────┘
               │                        │
        ┌──────▼──────┐        ┌───────▼────────┐
        │   Registered │        │  Plugin Assembly │
        │    Tools     │        │    Discovery    │
        │  (in-memory) │        │   & Loading     │
        └─────────────┘        └────────────────┘
               │                        │
        ┌──────▼──────────────────────▼─┐
        │     Tool Instances             │
        │  (Implementing IMCPTool)       │
        └────────────────────────────────┘
```

## Component Details

### MCPServerApplication
- ASP.NET Core hosted application
- Configures HTTP routes
- Manages request/response handling
- Integrates with logging via Serilog

### ToolManager
- Maintains dictionary of loaded tools
- Provides tool discovery
- Routes function calls to appropriate tool
- Handles exceptions and execution timing
- Supports both registered and plugin-based tools

### IMCPTool Interface
Contract that all tools must implement:
- `GetMetadata()`: Returns tool information
- `GetFunctions()`: Lists available functions
- `ExecuteAsync()`: Executes a function
- `ValidateAsync()`: Validates tool is ready

### MCPToolBase
Abstract base class providing:
- Common logger injection
- Helper methods for result creation
- Parameter validation
- Error handling patterns

## Tool Loading Process

```
1. Application Startup
   ↓
2. Service Container Initialization
   ↓
3. Manual Tool Registration (if any)
   ↓
4. Plugin Directory Scan
   ├─ Load assemblies
   ├─ Discover IMCPTool implementations
   ├─ Instantiate via DI or reflection
   └─ Validate and register
   ↓
5. Server Ready to Accept Requests
```

## API Flow

```
HTTP Request
    ↓
MCPServerApplication Route Handler
    ↓
Extract toolId and functionName
    ↓
ToolManager.ExecuteToolFunctionAsync()
    ↓
Retrieve tool from registry
    ↓
Call tool.ExecuteAsync()
    ↓
Build response (with timing)
    ↓
Return to client
```

## Plugin Discovery Process

1. Scan configured plugin directory
2. Load each .dll file as assembly
3. Reflect over assembly types
4. Find all types implementing IMCPTool
5. Instantiate each tool:
   - First try: ServiceProvider.GetService()
   - Fallback: Activator.CreateInstance()
6. Call ValidateAsync() on each instance
7. Register validated tools in ToolManager

## Dependency Injection

The server uses Microsoft.Extensions.DependencyInjection:

```
ServiceProvider
├─ ILogger<T> (Serilog)
├─ ToolManager (Singleton)
├─ User-registered services
└─ Plugin-provided services (if any)
```

Tools can inject dependencies via constructor:
```csharp
public MyTool(ILogger<MyTool> logger, IMyService service) : base(logger)
{
    _service = service;
}
```

## Data Models

### ToolMetadata
```json
{
  "toolId": "unique-identifier",
  "name": "Display Name",
  "version": "1.0.0",
  "author": "Author Name",
  "description": "What it does"
}
```

### ToolFunction
```json
{
  "name": "FunctionName",
  "description": "What it does",
  "parameters": [
    {
      "name": "paramName",
      "type": "string",
      "description": "Parameter description",
      "required": true
    }
  ]
}
```

### ToolResult
```json
{
  "success": true,
  "data": { /* result payload */ },
  "error": null,
  "executionTimeMs": 125
}
```

## Scalability Considerations

### Horizontal Scaling
- Stateless design allows multiple instances
- Tools can be shared across instances
- Use reverse proxy (nginx, etc.) for load balancing

### Vertical Scaling
- Async/await supports high concurrency
- Can handle many concurrent requests
- Monitor memory usage with many plugins

### Tool Isolation
- Each tool instance is independent
- No shared state between tools
- Exceptions isolated to tool execution

## Security Model

### Input Validation
- Tools must validate all parameters
- Framework provides `ValidateRequiredParameters()`
- Type conversion left to implementation

### Tool Isolation
- Tools run in same process but conceptually isolated
- No cross-tool data sharing mechanism
- Each tool responsible for its own security

### API Security (Production)
- Implement authentication/authorization middleware
- Use API keys or JWT tokens
- Rate limiting on tool execution
- Input size limits

## Extensibility Points

### 1. Custom Tool Implementation
```csharp
public class CustomTool : MCPToolBase { }
```

### 2. Middleware Extension
Add custom middleware in `MCPServerApplication.ConfigureMiddleware()`

### 3. Service Registration
Register dependencies in builder.Services

### 4. Plugin Discovery
Extend ToolManager to support additional loading strategies

## Performance Characteristics

### Tool Registration
- O(1) lookup time
- Dictionary-based storage

### Function Execution
- Minimal overhead beyond tool implementation
- Timing includes serialization/deserialization
- Async patterns prevent blocking

### Memory
- Tools loaded once at startup
- Metadata cached after loading
- Results created on-demand

## Testing Strategy

### Unit Testing
- Test tool functions in isolation
- Mock dependencies
- Test error conditions

### Integration Testing
- Load tools in test server
- Call via HTTP API
- Verify end-to-end behavior

### Performance Testing
- Benchmark tool execution
- Load test concurrent calls
- Profile memory usage
