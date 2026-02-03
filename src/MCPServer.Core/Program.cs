using Microsoft.Extensions.DependencyInjection;
using Serilog;
using MCPServer.Core;
using MCPServer.Examples;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("./logs/mcp-server-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// Add services
builder.Services.AddSingleton<ToolManager>();
builder.Services.AddSingleton<TextProcessingTool>();
builder.Services.AddSingleton<PluginWatcher>();

var app = builder.Build();

var toolManager = app.Services.GetRequiredService<ToolManager>();
var textTool = app.Services.GetRequiredService<TextProcessingTool>();
toolManager.RegisterTool(textTool);
await toolManager.LoadToolsFromDirectoryAsync("./plugins");

// API Routes
app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).WithName("Health");
app.MapGet("/api/tools", () => {
    var tools = toolManager.GetAllTools().Select(t => new { t.ToolId, Name = t.Metadata.Name, Version = t.Metadata.Version, Author = t.Metadata.Author, Description = t.Metadata.Description });
    return Results.Ok(new { tools = tools.ToList() });
}).WithName("GetAllTools");

app.MapGet("/api/tools/{toolId}", (string toolId) => {
    var tool = toolManager.GetTool(toolId);
    if (tool == null) return Results.NotFound();
    var m = tool.GetMetadata();
    return Results.Ok(new { m.ToolId, m.Name, m.Version, m.Author, m.Description, Functions = tool.GetFunctions() });
}).WithName("GetToolDetails");

app.MapGet("/api/tools/{toolId}/functions", (string toolId) => {
    var funcs = toolManager.GetToolFunctions(toolId);
    return funcs == null ? Results.NotFound() : Results.Ok(new { functions = funcs });
}).WithName("GetToolFunctions");

app.MapPost("/api/tools/{toolId}/execute", async (string toolId, ExecutionRequest request) => {
    if (string.IsNullOrEmpty(request.FunctionName)) return Results.BadRequest();
    var result = await toolManager.ExecuteToolFunctionAsync(toolId, request.FunctionName, request.Parameters ?? new());
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
}).WithName("ExecuteToolFunction");

_ = app.Services.GetRequiredService<PluginWatcher>();

var runTask = app.RunAsync();
await runTask;

public class ExecutionRequest {
    public required string FunctionName { get; set; }
    public Dictionary<string, object?>? Parameters { get; set; }
}
