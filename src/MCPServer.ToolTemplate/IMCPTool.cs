namespace MCPServer.ToolTemplate;

/// <summary>
/// Base interface for all MCP tools.
/// Implement this interface to create a new tool that can be loaded by the MCP server.
/// </summary>
public interface IMCPTool
{
    /// <summary>
    /// Gets the metadata about this tool.
    /// </summary>
    ToolMetadata GetMetadata();

    /// <summary>
    /// Gets the list of functions exposed by this tool.
    /// </summary>
    List<ToolFunction> GetFunctions();

    /// <summary>
    /// Executes a function in this tool.
    /// </summary>
    /// <param name="functionName">Name of the function to execute</param>
    /// <param name="parameters">Dictionary of parameters for the function</param>
    /// <returns>Result of the function execution</returns>
    Task<ToolResult> ExecuteAsync(string functionName, Dictionary<string, object?> parameters);

    /// <summary>
    /// Validates if the tool is properly configured and ready to use.
    /// </summary>
    /// <returns>true if valid, false otherwise</returns>
    Task<bool> ValidateAsync();
}

/// <summary>
/// Abstract base class for implementing MCP tools.
/// Provides common functionality for tool implementation.
/// </summary>
public abstract class MCPToolBase : IMCPTool
{
    private readonly ILogger<MCPToolBase> _logger;

    protected MCPToolBase(ILogger<MCPToolBase> logger)
    {
        _logger = logger;
    }

    public abstract ToolMetadata GetMetadata();

    public abstract List<ToolFunction> GetFunctions();

    public abstract Task<ToolResult> ExecuteAsync(string functionName, Dictionary<string, object?> parameters);

    public virtual async Task<bool> ValidateAsync()
    {
        _logger.LogInformation("Validating tool: {ToolName}", GetMetadata().Name);
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Helper method to create a successful result.
    /// </summary>
    protected ToolResult Success(object? data, long executionTimeMs = 0)
    {
        return new ToolResult
        {
            Success = true,
            Data = data,
            ExecutionTimeMs = executionTimeMs
        };
    }

    /// <summary>
    /// Helper method to create a failed result.
    /// </summary>
    protected ToolResult Failure(string error, long executionTimeMs = 0)
    {
        return new ToolResult
        {
            Success = false,
            Error = error,
            ExecutionTimeMs = executionTimeMs
        };
    }

    /// <summary>
    /// Helper method to validate required parameters.
    /// </summary>
    protected bool ValidateRequiredParameters(Dictionary<string, object?> parameters, params string[] requiredParams)
    {
        foreach (var param in requiredParams)
        {
            if (!parameters.ContainsKey(param) || parameters[param] == null)
            {
                _logger.LogWarning("Missing required parameter: {Parameter}", param);
                return false;
            }
        }
        return true;
    }
}
