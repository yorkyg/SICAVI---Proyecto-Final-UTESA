using System.Text;

namespace Sicavi.WinForms.Utilities;

public enum AppLogLevel
{
    Information,
    Warning,
    Error
}

public sealed record LogEntry(DateTime Timestamp, AppLogLevel Level, string Message)
{
    public override string ToString() => $"{Timestamp:HH:mm:ss} [{Level}] {Message}";
}

public sealed class AppLogger
{
    private readonly object _sync = new();
    private readonly StringBuilder _history = new();

    public event EventHandler<LogEntry>? EntryAdded;

    public void Info(string message) => Write(AppLogLevel.Information, message);
    public void Warning(string message) => Write(AppLogLevel.Warning, message);
    public void Error(string message) => Write(AppLogLevel.Error, message);

    public string GetHistory()
    {
        lock (_sync)
        {
            return _history.ToString();
        }
    }

    private void Write(AppLogLevel level, string message)
    {
        var entry = new LogEntry(DateTime.Now, level, message);

        lock (_sync)
        {
            _history.AppendLine(entry.ToString());

            const int maximumCharacters = 30_000;
            if (_history.Length > maximumCharacters)
            {
                _history.Remove(0, _history.Length - maximumCharacters);
            }
        }

        EntryAdded?.Invoke(this, entry);
    }
}
