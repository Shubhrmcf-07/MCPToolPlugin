using System.Text.Json.Serialization;

namespace MCPServer.ToolTemplate;

/// <summary>
/// Metadata describing an MCP tool.
/// </summary>
public class ToolMetadata
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("version")]
    public required string Version { get; set; }

    [JsonPropertyName("author")]
    public required string Author { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("toolId")]
    public required string ToolId { get; set; }
}

/// <summary>
/// Represents a parameter for a tool function.
/// </summary>
public class ToolParameter
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("required")]
    public bool Required { get; set; } = false;
}

/// <summary>
/// Represents a function/capability exposed by a tool.
/// </summary>
public class ToolFunction
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("parameters")]
    public List<ToolParameter> Parameters { get; set; } = new();
}

/// <summary>
/// Result returned from executing a tool function.
/// </summary>
public class ToolResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public object? Data { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("executionTimeMs")]
    public long ExecutionTimeMs { get; set; }
}
