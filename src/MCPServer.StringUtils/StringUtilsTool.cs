namespace MCPServer.StringUtils;

/// <summary>
/// String manipulation and analysis tool
/// </summary>
public class StringUtilsTool : IMCPTool
{
    public string ToolId => "string-utils";

    public ToolMetadata GetMetadata()
    {
        return new ToolMetadata
        {
            ToolId = ToolId,
            Name = "String Utils",
            Version = "1.0.0",
            Author = "MCP Team",
            Description = "String manipulation and analysis utilities"
        };
    }

    public List<ToolFunction> GetFunctions()
    {
        return new()
        {
            new ToolFunction
            {
                Name = "reverse",
                Description = "Reverse a string",
                Parameters = new()
                {
                    new() { Name = "text", Type = "string", Description = "Text to reverse", Required = true }
                }
            },
            new ToolFunction
            {
                Name = "repeat",
                Description = "Repeat a string N times",
                Parameters = new()
                {
                    new() { Name = "text", Type = "string", Description = "Text to repeat", Required = true },
                    new() { Name = "count", Type = "number", Description = "Number of times to repeat", Required = true }
                }
            },
            new ToolFunction
            {
                Name = "pad",
                Description = "Pad string to length with character",
                Parameters = new()
                {
                    new() { Name = "text", Type = "string", Description = "Text to pad", Required = true },
                    new() { Name = "length", Type = "number", Description = "Target length", Required = true },
                    new() { Name = "char", Type = "string", Description = "Padding character", Required = true }
                }
            }
        };
    }

    public async Task<FunctionResult> ExecuteAsync(string functionName, Dictionary<string, object?> parameters)
    {
        try
        {
            return functionName.ToLower() switch
            {
                "reverse" => await ExecuteReverse(parameters),
                "repeat" => await ExecuteRepeat(parameters),
                "pad" => await ExecutePad(parameters),
                _ => FunctionResult.Error($"Unknown function: {functionName}")
            };
        }
        catch (Exception ex)
        {
            return FunctionResult.Error($"Error: {ex.Message}");
        }
    }

    private Task<FunctionResult> ExecuteReverse(Dictionary<string, object?> parameters)
    {
        var text = parameters["text"]?.ToString() ?? "";
        var reversed = new string(text.Reverse().ToArray());
        return Task.FromResult(FunctionResult.Success(new { original = text, reversed }));
    }

    private Task<FunctionResult> ExecuteRepeat(Dictionary<string, object?> parameters)
    {
        var text = parameters["text"]?.ToString() ?? "";
        var count = Convert.ToInt32(parameters["count"] ?? 1);
        var repeated = string.Concat(Enumerable.Repeat(text, count));
        return Task.FromResult(FunctionResult.Success(new { text, count, result = repeated }));
    }

    private Task<FunctionResult> ExecutePad(Dictionary<string, object?> parameters)
    {
        var text = parameters["text"]?.ToString() ?? "";
        var length = Convert.ToInt32(parameters["length"] ?? 0);
        var @char = parameters["char"]?.ToString()?[0] ?? ' ';
        var padded = text.PadRight(length, @char);
        return Task.FromResult(FunctionResult.Success(new { text, length, character = @char.ToString(), result = padded }));
    }
}
