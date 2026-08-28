using Sicavi.WinForms.Data;

namespace Sicavi.WinForms.Forms;

public sealed partial class RegistrationForm : Form
{
    private readonly UserRepository? _users;

    public RegistrationForm()
    {
        InitializeComponent();
    }

    public RegistrationForm(UserRepository users) : this()
    {
        _users = users;
    }

    public string? CreatedUsername { get; private set; }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (_users is null)
        {
            return;
        }

        if (_passwordTextBox.Text != _confirmPasswordTextBox.Text)
        {
            ShowError("Las claves no coinciden.");
            return;
        }

        try
        {
            var user = _users.Create(
                _usernameTextBox.Text,
                _fullNameTextBox.Text,
                _passwordTextBox.Text,
                "Operador");
            CreatedUsername = user.Username;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private void ShowError(string message)
    {
        _statusLabel.Text = message;
        _statusLabel.ForeColor = Color.FromArgb(255, 105, 105);
    }
}
