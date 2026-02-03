using System.Collections.Concurrent;

namespace MCPServer.Core;

/// <summary>
/// Watches the plugins directory for new/updated tools and reloads them automatically.
/// </summary>
public class PluginWatcher : IDisposable
{
    private readonly ToolManager _toolManager;
    private readonly ILogger<PluginWatcher> _logger;
    private readonly FileSystemWatcher? _watcher;
    private readonly string _pluginDirectory;
    private readonly ConcurrentDictionary<string, DateTime> _recentChanges;

    public PluginWatcher(ToolManager toolManager, ILogger<PluginWatcher> logger, string pluginDirectory = "./plugins")
    {
        _toolManager = toolManager;
        _logger = logger;
        _pluginDirectory = Path.GetFullPath(pluginDirectory);
        _recentChanges = new ConcurrentDictionary<string, DateTime>();

        // Create plugins directory if it doesn't exist
        if (!Directory.Exists(_pluginDirectory))
        {
            Directory.CreateDirectory(_pluginDirectory);
            _logger.LogInformation("Created plugins directory: {PluginDirectory}", _pluginDirectory);
        }

        // Only set up watcher if directory exists
        if (Directory.Exists(_pluginDirectory))
        {
            _watcher = new FileSystemWatcher(_pluginDirectory, "*.dll")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnDllChanged;
            _watcher.Changed += OnDllChanged;
            _watcher.Renamed += OnDllRenamed;

            _logger.LogInformation("PluginWatcher initialized for: {PluginDirectory}", _pluginDirectory);
        }
        else
        {
            _logger.LogWarning("Plugins directory does not exist: {PluginDirectory}", _pluginDirectory);
        }
    }

    private void OnDllChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce rapid file changes
        var fileName = Path.GetFileName(e.FullPath);
        var now = DateTime.UtcNow;

        if (_recentChanges.TryGetValue(fileName, out var lastChange))
        {
            if ((now - lastChange).TotalSeconds < 2)
            {
                return; // Ignore rapid changes
            }
        }

        _recentChanges[fileName] = now;

        _logger.LogInformation("Detected plugin change: {FileName}", fileName);

        // Wait a bit for file to be fully written
        Task.Delay(1500).ContinueWith(async _ =>
        {
            try
            {
                _logger.LogInformation("Reloading plugins from directory: {PluginDirectory}", _pluginDirectory);
                var loadedCount = await _toolManager.LoadToolsFromDirectoryAsync(_pluginDirectory);
                _logger.LogInformation("Reloaded {LoadedCount} plugins after file change", loadedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading plugins after file change");
            }
        });
    }

    private void OnDllRenamed(object sender, RenamedEventArgs e)
    {
        _logger.LogInformation("Plugin renamed: {OldFileName} -> {NewFileName}",
            Path.GetFileName(e.OldFullPath),
            Path.GetFileName(e.FullPath));

        OnDllChanged(sender, e);
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _recentChanges.Clear();
    }
}
