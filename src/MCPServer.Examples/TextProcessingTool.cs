using Microsoft.Extensions.Logging;
using MCPServer.ToolTemplate;

namespace MCPServer.Examples;

/// <summary>
/// Example tool demonstrating text processing capabilities.
/// </summary>
public class TextProcessingTool : MCPToolBase
{
    public TextProcessingTool(ILogger<TextProcessingTool> logger) : base(logger)
    {
    }

    public override ToolMetadata GetMetadata()
    {
        return new ToolMetadata
        {
            ToolId = "text-processing",
            Name = "Text Processing Tool",
            Version = "1.0.0",
            Author = "MCP Team",
            Description = "Provides text processing capabilities including case conversion, word counting, and text analysis"
        };
    }

    public override List<ToolFunction> GetFunctions()
    {
        return new List<ToolFunction>
        {
            new ToolFunction
            {
                Name = "ToUpperCase",
                Description = "Converts text to uppercase",
                Parameters = new List<ToolParameter>
                {
                    new ToolParameter
                    {
                        Name = "text",
                        Type = "string",
                        Description = "The text to convert",
                        Required = true
                    }
                }
            },
            new ToolFunction
            {
                Name = "CountWords",
                Description = "Counts the number of words in text",
                Parameters = new List<ToolParameter>
                {
                    new ToolParameter
                    {
                        Name = "text",
                        Type = "string",
                        Description = "The text to analyze",
                        Required = true
                    }
                }
            },
            new ToolFunction
            {
                Name = "Reverse",
                Description = "Reverses the input text",
                Parameters = new List<ToolParameter>
                {
                    new ToolParameter
                    {
                        Name = "text",
                        Type = "string",
                        Description = "The text to reverse",
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
            "ToUpperCase" => await ExecuteToUpperCase(parameters),
            "CountWords" => await ExecuteCountWords(parameters),
            "Reverse" => await ExecuteReverse(parameters),
            _ => Failure($"Function not found: {functionName}")
        };
    }

    private Task<ToolResult> ExecuteToUpperCase(Dictionary<string, object?> parameters)
    {
        if (!ValidateRequiredParameters(parameters, "text"))
        {
            return Task.FromResult(Failure("Missing required parameter: text"));
        }

        var text = parameters["text"]?.ToString();
        var result = text?.ToUpperInvariant();

        return Task.FromResult(Success(new { result, originalLength = text?.Length }));
    }

    private Task<ToolResult> ExecuteCountWords(Dictionary<string, object?> parameters)
    {
        if (!ValidateRequiredParameters(parameters, "text"))
        {
            return Task.FromResult(Failure("Missing required parameter: text"));
        }

        var text = parameters["text"]?.ToString();
        var wordCount = text?.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length ?? 0;

        return Task.FromResult(Success(new { wordCount, text }));
    }

    private Task<ToolResult> ExecuteReverse(Dictionary<string, object?> parameters)
    {
        if (!ValidateRequiredParameters(parameters, "text"))
        {
            return Task.FromResult(Failure("Missing required parameter: text"));
        }

        var text = parameters["text"]?.ToString();
        var reversed = new string((text ?? "").Reverse().ToArray());

        return Task.FromResult(Success(new { reversed, originalLength = text?.Length }));
    }
}
