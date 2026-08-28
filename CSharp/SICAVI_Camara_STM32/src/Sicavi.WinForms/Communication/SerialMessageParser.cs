using System.Text.RegularExpressions;

namespace Sicavi.WinForms.Communication;

public static partial class SerialMessageParser
{
    [GeneratedRegex(
        "^(?<category>EVT|CMD|TEL|ACK|ERR)=(?<name>[A-Z_]+)(?<fields>(?:;[A-Z_]+=[^;\\r\\n]+)*)$",
        RegexOptions.CultureInvariant)]
    private static partial Regex MessagePattern();

    public static bool TryParse(string input, out SerialMessage? message)
    {
        message = null;
        Match match = MessagePattern().Match(input.Trim());

        if (!match.Success)
        {
            return false;
        }

        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string fieldsText = match.Groups["fields"].Value;

        foreach (string item in fieldsText.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] pair = item.Split('=', 2);
            if (pair.Length != 2 || !fields.TryAdd(pair[0], pair[1]))
            {
                return false;
            }
        }

        message = new SerialMessage(
            match.Groups["category"].Value,
            match.Groups["name"].Value,
            fields);

        return true;
    }
}
