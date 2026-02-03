using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MCPServer.ToolTemplate;

namespace MCPServer.Core;

/// <summary>
/// Manages loading and executing MCP tools dynamically.
/// </summary>
public class ToolManager
{
    private readonly Dictionary<string, IMCPTool> _tools = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ToolManager> _logger;

    public ToolManager(IServiceProvider serviceProvider, ILogger<ToolManager> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Registers a tool instance directly.
    /// </summary>
    public void RegisterTool(IMCPTool tool)
    {
        var metadata = tool.GetMetadata();
        _tools[metadata.ToolId] = tool;
        _logger.LogInformation("Registered tool: {ToolName} (v{Version})", metadata.Name, metadata.Version);
    }

    /// <summary>
    /// Loads tools from a directory by discovering and instantiating IMCPTool implementations.
    /// </summary>
    public async Task<int> LoadToolsFromDirectoryAsync(string directory)
    {
        if (!Directory.Exists(directory))
        {
            _logger.LogWarning("Tool directory does not exist: {Directory}", directory);
            return 0;
        }

        var dllFiles = Directory.GetFiles(directory, "*.dll");
        int loadedCount = 0;

        foreach (var dllFile in dllFiles)
        {
            try
            {
                var assembly = Assembly.LoadFrom(dllFile);
                var toolTypes = assembly.GetTypes()
                    .Where(t => typeof(IMCPTool).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var toolType in toolTypes)
                {
                    try
                    {
                        var tool = ActivateToolInstance(toolType);
                        if (tool != null)
                        {
                            if (await tool.ValidateAsync())
                            {
                                RegisterTool(tool);
                                loadedCount++;
                            }
                            else
                            {
                                _logger.LogWarning("Tool validation failed: {ToolType}", toolType.Name);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to instantiate tool: {ToolType}", toolType.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load assembly: {AssemblyPath}", dllFile);
            }
        }

        _logger.LogInformation("Loaded {LoadedCount} tools from {Directory}", loadedCount, directory);
        return loadedCount;
    }

    /// <summary>
    /// Attempts to instantiate a tool using dependency injection.
    /// </summary>
    private IMCPTool? ActivateToolInstance(Type toolType)
    {
        try
        {
            // Try to use DI container first
            var instance = _serviceProvider.GetService(toolType);
            if (instance is IMCPTool tool)
            {
                return tool;
            }

            // Fallback to direct instantiation
            return (IMCPTool?)Activator.CreateInstance(toolType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating tool instance: {ToolType}", toolType.Name);
            return null;
        }
    }

    /// <summary>
    /// Executes a function on a specific tool.
    /// </summary>
    public async Task<ToolResult> ExecuteToolFunctionAsync(string toolId, string functionName, Dictionary<string, object?> parameters)
    {
        var stopwatch = Stopwatch.StartNew();

        if (!_tools.TryGetValue(toolId, out var tool))
        {
            stopwatch.Stop();
            return new ToolResult
            {
                Success = false,
                Error = $"Tool not found: {toolId}",
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        try
        {
            _logger.LogInformation("Executing {ToolId}.{FunctionName}", toolId, functionName);
            var result = await tool.ExecuteAsync(functionName, parameters);
            stopwatch.Stop();
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error executing tool function: {ToolId}.{FunctionName}", toolId, functionName);
            return new ToolResult
            {
                Success = false,
                Error = ex.Message,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    /// <summary>
    /// Gets all registered tools.
    /// </summary>
    public IEnumerable<(string ToolId, ToolMetadata Metadata)> GetAllTools()
    {
        return _tools.Select(kvp => (kvp.Key, kvp.Value.GetMetadata()));
    }

    /// <summary>
    /// Gets functions for a specific tool.
    /// </summary>
    public List<ToolFunction>? GetToolFunctions(string toolId)
    {
        return _tools.TryGetValue(toolId, out var tool) ? tool.GetFunctions() : null;
    }

    /// <summary>
    /// Gets a tool by ID.
    /// </summary>
    public IMCPTool? GetTool(string toolId)
    {
        _tools.TryGetValue(toolId, out var tool);
        return tool;
    }
}
