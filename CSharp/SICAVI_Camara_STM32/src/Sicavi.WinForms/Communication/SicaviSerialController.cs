namespace Sicavi.WinForms.Communication;

public sealed class SicaviSerialController : IDisposable
{
    private readonly SerialConnectionService _connection = new();
    private readonly object _pendingCommandSync = new();
    private System.Threading.Timer? _heartbeatTimer;
    private TaskCompletionSource<bool>? _pendingReset;
    private TaskCompletionSource<bool>? _pendingRun;
    private int _commandsAwaitingAcknowledgement;

    private static readonly TimeSpan CommandRetryInterval = TimeSpan.FromMilliseconds(400);

    public SicaviSerialController()
    {
        _connection.LineReceived += Connection_LineReceived;
        _connection.ConnectionChanged += Connection_ConnectionChanged;
        _connection.CommunicationError += (_, message) => CommunicationError?.Invoke(this, message);
    }

    public event EventHandler<bool>? ConnectionChanged;
    public event EventHandler<string>? CommunicationError;
    public event EventHandler<string>? RawLineReceived;
    public event EventHandler<SicaviTelemetry>? TelemetryReceived;
    public event EventHandler<BoxDetectedEventArgs>? BoxDetected;
    public event EventHandler<InspectionFinishedEventArgs>? InspectionFinished;
    public event EventHandler<AlarmEventArgs>? AlarmRaised;
    public event EventHandler<CommandRejectedEventArgs>? CommandRejected;
    public event EventHandler? ResetAcknowledged;
    public event EventHandler<string>? StateChanged;

    public bool IsConnected => _connection.IsConnected;

    public void Connect(string portName)
    {
        _connection.Connect(portName, 115200);
        _heartbeatTimer = new System.Threading.Timer(
            _ => SendHeartbeat(),
            null,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1));
    }

    public void Disconnect()
    {
        StopHeartbeat();
        _connection.Disconnect();
    }
    public void Run() => _connection.SendLine("CMD=RUN");
    public void Stop() => _connection.SendLine("CMD=STOP");
    public void Reset() => _connection.SendLine("CMD=RESET");

    public Task RunAsync(TimeSpan timeout, CancellationToken cancellationToken = default) =>
        SendAcknowledgedCommandAsync("RUN", timeout, cancellationToken);

    public Task ResetAsync(TimeSpan timeout, CancellationToken cancellationToken = default) =>
        SendAcknowledgedCommandAsync("RESET", timeout, cancellationToken);

    public void SendVisionResult(string id, string result)
    {
        string normalized = result.ToUpperInvariant();
        if (normalized is not ("GOOD" or "REJECT" or "UNKNOWN"))
        {
            throw new ArgumentOutOfRangeException(nameof(result), "Resultado no reconocido.");
        }

        _connection.SendLine($"CMD={normalized};ID={id}");
    }

    private void Connection_LineReceived(object? sender, string line)
    {
        RawLineReceived?.Invoke(this, line);

        if (!SerialMessageParser.TryParse(line, out SerialMessage? message) || message is null)
        {
            return;
        }

        if (message.Category == "TEL" && message.Name == "STATUS")
        {
            SicaviTelemetry telemetry = CreateTelemetry(message.Fields);
            TelemetryReceived?.Invoke(this, telemetry);
            StateChanged?.Invoke(this, telemetry.State);
            return;
        }

        if (message.Category == "ACK" && message.Name == "RESET")
        {
            CompletePendingCommand("RESET");
            ResetAcknowledged?.Invoke(this, EventArgs.Empty);
            StateChanged?.Invoke(this, Get(message.Fields, "STATE", "STOPPED"));
            return;
        }

        if (message.Category == "ACK" && message.Name == "RUN")
        {
            CompletePendingCommand("RUN");
            StateChanged?.Invoke(this, Get(message.Fields, "STATE", "TRANSPORTING"));
            return;
        }

        if (message.Category == "ERR")
        {
            string code = Get(message.Fields, "CODE", "UNKNOWN");
            var exception = new SicaviCommandException(message.Name, code);
            FailPendingCommand(message.Name, exception);
            CommandRejected?.Invoke(this, new CommandRejectedEventArgs(message.Name, code));
            return;
        }

        if (message.Category == "EVT")
        {
            switch (message.Name)
            {
                case "DETECTED":
                    BoxDetected?.Invoke(this, new BoxDetectedEventArgs(
                        Get(message.Fields, "ID", "0"),
                        GetInt(message.Fields, "LIGHT"),
                        GetInt(message.Fields, "ADC")));
                    break;

                case "FINISHED":
                    InspectionFinished?.Invoke(this, new InspectionFinishedEventArgs(
                        Get(message.Fields, "ID", "0"),
                        Get(message.Fields, "RESULT", "UNKNOWN")));
                    break;

                case "ALARM":
                    AlarmRaised?.Invoke(this, new AlarmEventArgs(Get(message.Fields, "CODE", "UNKNOWN")));
                    StateChanged?.Invoke(this, "ALARM");
                    break;

                case "RUNNING":
                    StateChanged?.Invoke(this, "TRANSPORTING");
                    break;

                case "STOPPED":
                    StateChanged?.Invoke(this, "STOPPED");
                    break;
            }
        }
    }

    private static SicaviTelemetry CreateTelemetry(IReadOnlyDictionary<string, string> fields) => new(
        Get(fields, "FW", "DESCONOCIDO"),
        Get(fields, "STATE", "UNKNOWN"),
        GetInt(fields, "LIGHT"),
        GetInt(fields, "ADC"),
        GetInt(fields, "MV"),
        TimeSpan.FromSeconds(GetHourmeterSeconds(fields)),
        (uint)Math.Max(0, GetLong(fields, "CYCLES")),
        Get(fields, "ALARM", "NONE"));

    private static long GetHourmeterSeconds(IReadOnlyDictionary<string, string> fields) =>
        fields.ContainsKey("HOURMETER_S")
            ? GetLong(fields, "HOURMETER_S")
            : GetLong(fields, "MOTOR_S");

    private static string Get(IReadOnlyDictionary<string, string> fields, string key, string fallback) =>
        fields.TryGetValue(key, out string? value) ? value : fallback;

    private static int GetInt(IReadOnlyDictionary<string, string> fields, string key) =>
        fields.TryGetValue(key, out string? value) && int.TryParse(value, out int parsed) ? parsed : 0;

    private static long GetLong(IReadOnlyDictionary<string, string> fields, string key) =>
        fields.TryGetValue(key, out string? value) && long.TryParse(value, out long parsed) ? parsed : 0L;

    private void SendHeartbeat()
    {
        if (!_connection.IsConnected || Volatile.Read(ref _commandsAwaitingAcknowledgement) != 0)
        {
            return;
        }

        try
        {
            _connection.SendLine("CMD=PING");
        }
        catch (Exception exception)
        {
            CommunicationError?.Invoke(this, $"Latido UART: {exception.Message}");
        }
    }

    private async Task SendAcknowledgedCommandAsync(
        string command,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var completion = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_pendingCommandSync)
        {
            if (GetPending(command) is not null)
            {
                throw new InvalidOperationException($"Ya existe un comando {command} pendiente.");
            }

            SetPending(command, completion);
        }

        Interlocked.Increment(ref _commandsAwaitingAcknowledgement);
        DateTime deadline = DateTime.UtcNow + timeout;

        try
        {
            while (true)
            {
                if (completion.Task.IsCompleted)
                {
                    await completion.Task.ConfigureAwait(false);
                    return;
                }

                _connection.SendLine($"CMD={command}");

                TimeSpan remaining = deadline - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero)
                {
                    throw new TimeoutException();
                }

                TimeSpan acknowledgementWindow = remaining < CommandRetryInterval
                    ? remaining
                    : CommandRetryInterval;

                try
                {
                    await completion.Task
                        .WaitAsync(acknowledgementWindow, cancellationToken)
                        .ConfigureAwait(false);
                    return;
                }
                catch (TimeoutException) when (DateTime.UtcNow < deadline)
                {
                    /* El comando idempotente se reenvia hasta recibir su ACK. */
                }
            }
        }
        finally
        {
            Interlocked.Decrement(ref _commandsAwaitingAcknowledgement);
            lock (_pendingCommandSync)
            {
                if (ReferenceEquals(GetPending(command), completion))
                {
                    SetPending(command, null);
                }
            }
        }
    }

    private TaskCompletionSource<bool>? GetPending(string command)
    {
        if (command.Equals("RESET", StringComparison.OrdinalIgnoreCase)) return _pendingReset;
        if (command.Equals("RUN", StringComparison.OrdinalIgnoreCase)) return _pendingRun;
        return null;
    }

    private void SetPending(string command, TaskCompletionSource<bool>? completion)
    {
        if (command.Equals("RESET", StringComparison.OrdinalIgnoreCase)) _pendingReset = completion;
        else if (command.Equals("RUN", StringComparison.OrdinalIgnoreCase)) _pendingRun = completion;
    }

    private void CompletePendingCommand(string command)
    {
        TaskCompletionSource<bool>? completion;
        lock (_pendingCommandSync)
        {
            completion = GetPending(command);
            SetPending(command, null);
        }
        completion?.TrySetResult(true);
    }

    private void FailPendingCommand(string command, Exception exception)
    {
        TaskCompletionSource<bool>? completion;
        lock (_pendingCommandSync)
        {
            completion = GetPending(command);
            SetPending(command, null);
        }
        completion?.TrySetException(exception);
    }

    private void Connection_ConnectionChanged(object? sender, bool connected)
    {
        if (!connected)
        {
            var exception = new IOException("La conexión con el STM32 se cerró.");
            FailPendingCommand("RESET", exception);
            FailPendingCommand("RUN", exception);
        }
        ConnectionChanged?.Invoke(this, connected);
    }

    private void StopHeartbeat()
    {
        System.Threading.Timer? timer = Interlocked.Exchange(ref _heartbeatTimer, null);
        timer?.Dispose();
    }

    public void Dispose()
    {
        StopHeartbeat();
        _connection.LineReceived -= Connection_LineReceived;
        _connection.ConnectionChanged -= Connection_ConnectionChanged;
        _connection.Dispose();
    }
}
