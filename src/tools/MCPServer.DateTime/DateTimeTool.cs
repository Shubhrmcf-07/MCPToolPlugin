namespace MCPServer.DateTime;

/// <summary>
/// Tool that provides the current date and time.
/// </summary>
public class DateTimeTool : MCPToolBase
{
    public DateTimeTool(ILogger<DateTimeTool> logger) : base(logger)
    {
    }

    public override ToolMetadata GetMetadata()
    {
        return new ToolMetadata
        {
            ToolId = "date-time",
            Name = "Date Time Tool",
            Version = "1.0.0",
            Author = "MCP Team",
            Description = "Provides the current date and time"
        };
    }

    public override List<ToolFunction> GetFunctions()
    {
        return new List<ToolFunction>
        {
            new ToolFunction
            {
                Name = "GetCurrentDateTime",
                Description = "Returns the current date and time",
                Parameters = new List<ToolParameter>
                {
                    new ToolParameter
                    {
                        Name = "format",
                        Type = "string",
                        Description = "Optional .NET date/time format string",
                        Required = false
                    },
                    new ToolParameter
                    {
                        Name = "utc",
                        Type = "bool",
                        Description = "If true, returns UTC time. Default is local time",
                        Required = false
                    }
                }
            }
        };
    }

    public override Task<ToolResult> ExecuteAsync(string functionName, Dictionary<string, object?> parameters)
    {
        return functionName switch
        {
            "GetCurrentDateTime" => ExecuteGetCurrentDateTime(parameters),
            _ => Task.FromResult(Failure($"Function not found: {functionName}"))
        };
    }

    private Task<ToolResult> ExecuteGetCurrentDateTime(Dictionary<string, object?> parameters)
    {
        var useUtc = TryGetBool(parameters, "utc") ?? false;
        var format = TryGetString(parameters, "format");

        var now = useUtc ? DateTimeOffset.UtcNow : DateTimeOffset.Now;

        var result = new
        {
            iso8601 = now.ToString("O"),
            unixSeconds = now.ToUnixTimeSeconds(),
            unixMilliseconds = now.ToUnixTimeMilliseconds(),
            timeZone = useUtc ? "UTC" : now.Offset.ToString(),
            formatted = string.IsNullOrWhiteSpace(format) ? null : now.ToString(format)
        };

        return Task.FromResult(Success(result));
    }

    private static string? TryGetString(Dictionary<string, object?> parameters, string key)
    {
        return parameters.TryGetValue(key, out var value) ? value?.ToString() : null;
    }

    private static bool? TryGetBool(Dictionary<string, object?> parameters, string key)
    {
        if (!parameters.TryGetValue(key, out var value) || value == null)
        {
            return null;
        }

        return value switch
        {
            bool b => b,
            string s when bool.TryParse(s, out var parsed) => parsed,
            _ => null
        };
    }
}
