namespace MCPServer.JsonValidator;

public class JsonValidatorTool : MCPToolBase
{
    public JsonValidatorTool(ILogger<JsonValidatorTool> logger) : base(logger)
    {
    }

    public override ToolMetadata GetMetadata()
    {
        return new ToolMetadata
        {
            ToolId = "json-validator",
            Name = "JSON Validator",
            Version = "1.0.0",
            Author = "Community Contributor",
            Description = "Validates JSON syntax and formats JSON documents"
        };
    }

    public override List<ToolFunction> GetFunctions()
    {
        return new List<ToolFunction>
        {
            new ToolFunction
            {
                Name = "ValidateJson",
                Description = "Validates if a JSON string is valid",
                Parameters = new List<ToolParameter>
                {
                    new ToolParameter
                    {
                        Name = "jsonContent",
                        Type = "string",
                        Description = "The JSON content to validate",
                        Required = true
                    }
                }
            },
            new ToolFunction
            {
                Name = "FormatJson",
                Description = "Formats a JSON string with proper indentation",
                Parameters = new List<ToolParameter>
                {
                    new ToolParameter
                    {
                        Name = "jsonContent",
                        Type = "string",
                        Description = "The JSON content to format",
                        Required = true
                    },
                    new ToolParameter
                    {
                        Name = "indentSize",
                        Type = "integer",
                        Description = "Number of spaces for indentation (default: 2)",
                        Required = false
                    }
                }
            },
            new ToolFunction
            {
                Name = "MinifyJson",
                Description = "Removes whitespace and compresses JSON",
                Parameters = new List<ToolParameter>
                {
                    new ToolParameter
                    {
                        Name = "jsonContent",
                        Type = "string",
                        Description = "The JSON content to minify",
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
            "ValidateJson" => await ValidateJsonAsync(parameters),
            "FormatJson" => await FormatJsonAsync(parameters),
            "MinifyJson" => await MinifyJsonAsync(parameters),
            _ => Failure($"Function not found: {functionName}")
        };
    }

    private async Task<ToolResult> ValidateJsonAsync(Dictionary<string, object?> parameters)
    {
        if (!ValidateRequiredParameters(parameters, "jsonContent"))
        {
            return Failure("Missing required parameter: jsonContent");
        }

        try
        {
            var jsonContent = parameters["jsonContent"]?.ToString();
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return Failure("JSON content cannot be empty");
            }

            JsonDocument.Parse(jsonContent);
            await Task.CompletedTask;
            return Success(new { isValid = true, message = "JSON is valid" });
        }
        catch (JsonException ex)
        {
            return Failure($"Invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Failure($"Error: {ex.Message}");
        }
    }

    private async Task<ToolResult> FormatJsonAsync(Dictionary<string, object?> parameters)
    {
        if (!ValidateRequiredParameters(parameters, "jsonContent"))
        {
            return Failure("Missing required parameter: jsonContent");
        }

        try
        {
            var jsonContent = parameters["jsonContent"]?.ToString();
            var indentSize = int.TryParse(parameters.GetValueOrDefault("indentSize")?.ToString(), out var size) ? size : 2;

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return Failure("JSON content cannot be empty");
            }

            var jsonDoc = JsonDocument.Parse(jsonContent);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var formatted = JsonSerializer.Serialize(jsonDoc.RootElement, options);
            await Task.CompletedTask;
            return Success(new { formatted });
        }
        catch (JsonException ex)
        {
            return Failure($"Invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Failure($"Error: {ex.Message}");
        }
    }

    private async Task<ToolResult> MinifyJsonAsync(Dictionary<string, object?> parameters)
    {
        if (!ValidateRequiredParameters(parameters, "jsonContent"))
        {
            return Failure("Missing required parameter: jsonContent");
        }

        try
        {
            var jsonContent = parameters["jsonContent"]?.ToString();

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return Failure("JSON content cannot be empty");
            }

            var jsonDoc = JsonDocument.Parse(jsonContent);
            var options = new JsonSerializerOptions
            {
                WriteIndented = false
            };

            var minified = JsonSerializer.Serialize(jsonDoc.RootElement, options);
            await Task.CompletedTask;
            return Success(new { minified });
        }
        catch (JsonException ex)
        {
            return Failure($"Invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Failure($"Error: {ex.Message}");
        }
    }
}
