#!/usr/bin/env dotnet-script

// Tool generator script for creating new MCP tools
// Usage: dotnet script create-tool.csx <tool-name> <tool-description>

#r "nuget: Spectre.Console, 0.47.0"

using System;
using System.IO;
using System.Text;
using Spectre.Console;

if (Args.Count < 2)
{
    AnsiConsole.MarkupLine("[red]Usage: dotnet script create-tool.csx <tool-name> <tool-description>[/]");
    return;
}

var toolName = Args[0];
var toolDescription = Args[1];
var toolId = toolName.ToLower().Replace(" ", "-");
var toolNamePascal = string.Concat(toolName.Split(' ').Select(w => char.ToUpper(w[0]) + w.Substring(1)));

AnsiConsole.MarkupLine($"[green]Creating MCP Tool: {toolNamePascal}[/]");

var toolDir = Path.Combine("tools", toolNamePascal);
Directory.CreateDirectory(toolDir);
Directory.CreateDirectory(Path.Combine(toolDir, $"{toolNamePascal}.Tests"));

// Create .csproj
var csprojContent = $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include=""../../src/MCPServer.ToolTemplate/MCPServer.ToolTemplate.csproj"" />
  </ItemGroup>

</Project>";

File.WriteAllText(Path.Combine(toolDir, $"{toolNamePascal}.csproj"), csprojContent);

// Create Tool Class
var toolClassContent = $@"using Microsoft.Extensions.Logging;
using MCPServer.ToolTemplate;

namespace {toolNamePascal};

/// <summary>
/// {toolDescription}
/// </summary>
public class {toolNamePascal}Tool : MCPToolBase
{{
    public {toolNamePascal}Tool(ILogger<{toolNamePascal}Tool> logger) : base(logger)
    {{
    }}

    public override ToolMetadata GetMetadata()
    {{
        return new ToolMetadata
        {{
            ToolId = ""{toolId}"",
            Name = ""{toolNamePascal} Tool"",
            Version = ""1.0.0"",
            Author = ""Your Name"",
            Description = ""{toolDescription}""
        }};
    }}

    public override List<ToolFunction> GetFunctions()
    {{
        return new List<ToolFunction>
        {{
            new ToolFunction
            {{
                Name = ""Process"",
                Description = ""Main processing function"",
                Parameters = new List<ToolParameter>
                {{
                    new ToolParameter
                    {{
                        Name = ""input"",
                        Type = ""string"",
                        Description = ""Input data"",
                        Required = true
                    }}
                }}
            }}
        }};
    }}

    public override async Task<ToolResult> ExecuteAsync(string functionName, Dictionary<string, object?> parameters)
    {{
        return functionName switch
        {{
            ""Process"" => await ExecuteProcess(parameters),
            _ => Failure($""Function not found: {{functionName}}"")
        }};
    }}

    private async Task<ToolResult> ExecuteProcess(Dictionary<string, object?> parameters)
    {{
        if (!ValidateRequiredParameters(parameters, ""input""))
        {{
            return Failure(""Missing required parameter: input"");
        }}

        try
        {{
            var input = parameters[""input""]?.ToString();
            var result = await DoWork(input);
            return Success(new {{ result }});
        }}
        catch (Exception ex)
        {{
            return Failure(ex.Message);
        }}
    }}

    private async Task<string> DoWork(string? input)
    {{
        // TODO: Implement your logic here
        await Task.Delay(10);
        return $""Processed: {{input}}"";
    }}
}}";

File.WriteAllText(Path.Combine(toolDir, $"{toolNamePascal}Tool.cs"), toolClassContent);

// Create README
var readmeContent = $@"# {toolNamePascal} Tool

{toolDescription}

## Functions

### Process
Processes input data.

**Parameters:**
- `input` (string): Input data to process

**Returns:**
```json
{{
  ""success"": true,
  ""data"": {{
    ""result"": ""...""
  }}
}}
```

## Example Usage

```bash
curl -X POST http://localhost:5000/api/tools/{toolId}/execute \\
  -H ""Content-Type: application/json"" \\
  -d '{{
    ""functionName"": ""Process"",
    ""parameters"": {{ ""input"": ""test"" }}
  }}'
```

## Development

Build the tool:
```bash
dotnet build
```

Run tests:
```bash
dotnet test
```

## Author
Your Name (@github-username)
";

File.WriteAllText(Path.Combine(toolDir, "README.md"), readmeContent);

// Create test .csproj
var testCsprojContent = $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsTestProject>true</IsTestProject>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.VisualStudio.TestPlatform.TestFramework"" Version=""17.8.2"" />
    <PackageReference Include=""Moq"" Version=""4.20.70"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""../{toolNamePascal}.csproj"" />
    <ProjectReference Include=""../../../src/MCPServer.ToolTemplate/MCPServer.ToolTemplate.csproj"" />
  </ItemGroup>

</Project>";

File.WriteAllText(Path.Combine(toolDir, $"{toolNamePascal}.Tests", $"{toolNamePascal}ToolTests.csproj"), testCsprojContent);

// Create test class
var testContent = $@"using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Extensions.Logging;

namespace {toolNamePascal}.Tests;

[TestClass]
public class {toolNamePascal}ToolTests
{{
    private Mock<ILogger<{toolNamePascal}Tool>> _mockLogger;
    private {toolNamePascal}Tool _tool;

    [TestInitialize]
    public void Setup()
    {{
        _mockLogger = new Mock<ILogger<{toolNamePascal}Tool>>();
        _tool = new {toolNamePascal}Tool(_mockLogger.Object);
    }}

    [TestMethod]
    public void GetMetadata_ReturnsValidMetadata()
    {{
        // Act
        var metadata = _tool.GetMetadata();

        // Assert
        Assert.IsNotNull(metadata);
        Assert.AreEqual(""{toolId}"", metadata.ToolId);
        Assert.AreEqual(""1.0.0"", metadata.Version);
    }}

    [TestMethod]
    public void GetFunctions_ReturnsProcessFunction()
    {{
        // Act
        var functions = _tool.GetFunctions();

        // Assert
        Assert.IsNotNull(functions);
        Assert.IsTrue(functions.Any(f => f.Name == ""Process""));
    }}

    [TestMethod]
    public async Task ExecuteAsync_WithValidInput_ReturnsSuccess()
    {{
        // Arrange
        var parameters = new Dictionary<string, object?> {{ {{ ""input"", ""test"" }} }};

        // Act
        var result = await _tool.ExecuteAsync(""Process"", parameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
    }}

    [TestMethod]
    public async Task ExecuteAsync_WithMissingParameter_ReturnsFailed()
    {{
        // Arrange
        var parameters = new Dictionary<string, object?>();

        // Act
        var result = await _tool.ExecuteAsync(""Process"", parameters);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
    }}

    [TestMethod]
    public async Task ExecuteAsync_WithUnknownFunction_ReturnsFailed()
    {{
        // Arrange
        var parameters = new Dictionary<string, object?> {{ {{ ""input"", ""test"" }} }};

        // Act
        var result = await _tool.ExecuteAsync(""UnknownFunction"", parameters);

        // Assert
        Assert.IsFalse(result.Success);
    }}

    [TestMethod]
    public async Task ValidateAsync_ReturnsTrue()
    {{
        // Act
        var isValid = await _tool.ValidateAsync();

        // Assert
        Assert.IsTrue(isValid);
    }}
}}";

File.WriteAllText(Path.Combine(toolDir, $"{toolNamePascal}.Tests", $"{toolNamePascal}ToolTests.cs"), testContent);

AnsiConsole.MarkupLine($"[green]✓ Tool created at: {toolDir}[/]");
AnsiConsole.MarkupLine("[yellow]Next steps:[/]");
AnsiConsole.MarkupLine($"1. cd {toolDir}");
AnsiConsole.MarkupLine("2. Implement your tool logic in {toolNamePascal}Tool.cs");
AnsiConsole.MarkupLine("3. Add tests in {toolNamePascal}.Tests/");
AnsiConsole.MarkupLine("4. Update README.md with documentation");
AnsiConsole.MarkupLine("5. Submit a PR!");
