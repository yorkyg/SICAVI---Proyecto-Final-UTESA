namespace Sicavi.WinForms.Communication;

public sealed record SerialMessage(
    string Category,
    string Name,
    IReadOnlyDictionary<string, string> Fields);
