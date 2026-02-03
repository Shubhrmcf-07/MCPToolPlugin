# Contributing Guidelines

We love your input! We want to make contributing to the MCP Server as easy and transparent as possible.

## Development Process

### 1. Fork and Clone
```bash
git clone https://github.com/yourname/mcp-server.git
cd mcp-server
```

### 2. Create a Feature Branch
```bash
git checkout -b feature/my-amazing-tool
```

### 3. Create Your Tool

Create a new directory under `tools/`:
```bash
tools/
└── my-amazing-tool/
    ├── MyAmazingTool.csproj
    ├── MyAmazingTool.cs          # Main tool implementation
    ├── README.md                 # Tool documentation
    └── MyAmazingTool.Tests/
        └── MyAmazingToolTests.cs  # Unit tests
```

### 4. Implement the Tool Template

See [TOOL_TEMPLATE.md](TOOL_TEMPLATE.md) for detailed instructions.

### 5. Add Unit Tests

```csharp
[TestClass]
public class MyAmazingToolTests
{
    private MyAmazingTool _tool;

    [TestInitialize]
    public void Setup()
    {
        var logger = new Mock<ILogger<MyAmazingTool>>();
        _tool = new MyAmazingTool(logger.Object);
    }

    [TestMethod]
    public async Task ExecuteFunction_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        var parameters = new Dictionary<string, object?> { { "input", "test" } };

        // Act
        var result = await _tool.ExecuteAsync("MyFunction", parameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
    }
}
```

### 6. Document Your Tool

Create a `README.md` in your tool directory:

```markdown
# My Amazing Tool

Brief description of what your tool does.

## Functions

### MyFunction
Description of what it does.

**Parameters:**
- `input` (string): Description of the input

**Returns:**
```json
{
  "result": "..."
}
```

## Examples

```bash
curl -X POST http://localhost:5000/api/tools/my-amazing-tool/execute \
  -H "Content-Type: application/json" \
  -d '{
    "functionName": "MyFunction",
    "parameters": { "input": "test" }
  }'
```

## Author
Your Name (@github-username)
```

### 7. Commit Your Changes

```bash
git add .
git commit -m "Add my amazing tool

- Implements MyFunction for data processing
- Includes comprehensive unit tests
- 85% code coverage"
```

### 8. Push and Create Pull Request

```bash
git push origin feature/my-amazing-tool
```

Then create a PR on GitHub with:
- Clear title: "Add My Amazing Tool"
- Description of what it does
- Reference to any related issues
- Link to your tool's README

## Pull Request Checklist

- [ ] Tool implements `IMCPTool` interface
- [ ] Tool inherits from `MCPToolBase`
- [ ] All functions are async
- [ ] Unit tests included (min 80% coverage)
- [ ] README.md with documentation
- [ ] No external dependencies unless necessary
- [ ] Code follows C# naming conventions
- [ ] No hardcoded credentials or secrets
- [ ] Meaningful commit messages

## Code Standards

### Naming Conventions
- Classes: PascalCase (`TextProcessingTool`)
- Methods: PascalCase (`ExecuteAsync`)
- Properties: PascalCase (`ToolId`)
- Parameters: camelCase (`inputText`)
- Private fields: _camelCase (`_logger`)

### Comments and Documentation
```csharp
/// <summary>
/// Processes text according to specified rules.
/// </summary>
/// <param name="text">The text to process</param>
/// <returns>The processed result</returns>
public async Task<string> ProcessTextAsync(string text)
{
    // Implementation here
}
```

### Error Handling
Always validate parameters and return meaningful errors:

```csharp
private Task<ToolResult> ExecuteFunction(Dictionary<string, object?> parameters)
{
    if (!ValidateRequiredParameters(parameters, "param1", "param2"))
    {
        return Task.FromResult(Failure("Missing required parameters"));
    }

    try
    {
        var result = DoWork(parameters);
        return Task.FromResult(Success(result));
    }
    catch (Exception ex)
    {
        return Task.FromResult(Failure(ex.Message));
    }
}
```

## Testing Requirements

### Minimum Coverage
- 80% code coverage required
- All public methods must be tested
- Error cases must be tested

### Test Structure
```csharp
[TestClass]
public class ToolTests
{
    private Mock<ILogger<MyTool>> _mockLogger;
    private MyTool _tool;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<MyTool>>();
        _tool = new MyTool(_mockLogger.Object);
    }

    [TestMethod]
    [DataRow("input1", "expected1")]
    [DataRow("input2", "expected2")]
    public async Task ExecuteFunction_VariousInputs_ReturnsExpected(string input, string expected)
    {
        // Test implementation
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task ExecuteFunction_InvalidInput_ThrowsException()
    {
        // Test implementation
    }
}
```

## Review Process

1. **Automated Checks**: CI/CD pipeline validates:
   - Code compiles
   - Tests pass
   - Code coverage meets minimum
   - No security issues

2. **Code Review**: Maintainers review:
   - Code quality
   - Documentation clarity
   - Alignment with guidelines
   - Functionality

3. **Approval**: Once approved, changes are merged to main

## Tool Lifecycle

### New Tool
- PR submitted with tool implementation
- Automated validation runs
- Code review completed
- Merged to main branch

### Publishing
- Tool is packaged as NuGet package
- Published to NuGet.org
- Added to tool registry
- Available for download

### Updates
- Submit PR with changes
- Same validation and review process
- Version number updated (semantic versioning)
- New package published

## Reporting Issues

Found a bug? Please report it:

1. Check [existing issues](https://github.com/yourorg/mcp-server/issues) first
2. Create a detailed issue with:
   - Reproduction steps
   - Expected behavior
   - Actual behavior
   - Environment details

## Suggestions and Features

Have an idea? We'd love to hear it:

1. [Create a discussion](https://github.com/yourorg/mcp-server/discussions)
2. Describe your idea and use case
3. Community discusses and provides feedback

## Questions?

- 📖 Check our [documentation](.)
- 💬 Start a [discussion](https://github.com/yourorg/mcp-server/discussions)
- 📧 Email: maintainers@example.com

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

## Code of Conduct

Please note that this project is released with a [Contributor Code of Conduct](CODE_OF_CONDUCT.md). By participating in this project you agree to abide by its terms.

Thank you for contributing to MCP Server! 🎉
