namespace Sicavi.WinForms.Communication;

public sealed record SicaviTelemetry(
    string FirmwareVersion,
    string State,
    int LightPercentage,
    int Adc,
    int Millivolts,
    TimeSpan WorkTime,
    uint EjectorCycles,
    string Alarm);

public sealed record BoxDetectedEventArgs(string Id, int LightPercentage, int Adc);
public sealed record InspectionFinishedEventArgs(string Id, string Result);
public sealed record AlarmEventArgs(string Code);
public sealed record CommandRejectedEventArgs(string Command, string Code);

public sealed class SicaviCommandException : Exception
{
    public SicaviCommandException(string command, string code)
        : base($"El STM32 rechazó {command}: {code}.")
    {
        Command = command;
        Code = code;
    }

    public string Command { get; }
    public string Code { get; }
}
