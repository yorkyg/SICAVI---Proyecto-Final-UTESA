using Sicavi.WinForms.Data;
using Sicavi.WinForms.Models;

namespace Sicavi.WinForms.Forms;

public sealed partial class LoginForm : Form
{
    private readonly UserRepository? _users;

    public LoginForm()
    {
        InitializeComponent();
    }

    public LoginForm(UserRepository users) : this()
    {
        _users = users;
    }

    public AppUser? AuthenticatedUser { get; private set; }

    private void LoginButton_Click(object? sender, EventArgs e)
    {
        if (_users is null)
        {
            return;
        }

        try
        {
            AppUser? user = _users.Authenticate(_usernameTextBox.Text, _passwordTextBox.Text);
            if (user is null)
            {
                ShowStatus("Usuario, clave o estado de cuenta no válidos.", true);
                _passwordTextBox.SelectAll();
                _passwordTextBox.Focus();
                return;
            }

            AuthenticatedUser = user;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception exception)
        {
            ShowStatus($"No se pudo iniciar sesión: {exception.Message}", true);
        }
    }

    private void RegisterButton_Click(object? sender, EventArgs e)
    {
        if (_users is null)
        {
            return;
        }

        using var form = new RegistrationForm(_users);
        if (form.ShowDialog(this) == DialogResult.OK && form.CreatedUsername is not null)
        {
            _usernameTextBox.Text = form.CreatedUsername;
            _passwordTextBox.Clear();
            _passwordTextBox.Focus();
            ShowStatus("Usuario registrado. Ya puedes iniciar sesión.", false);
        }
    }

    private void ShowPasswordCheckBox_CheckedChanged(object? sender, EventArgs e) =>
        _passwordTextBox.UseSystemPasswordChar = !_showPasswordCheckBox.Checked;

    private void ShowStatus(string message, bool error)
    {
        _statusLabel.Text = message;
        _statusLabel.ForeColor = error
            ? Color.FromArgb(255, 105, 105)
            : Color.FromArgb(78, 214, 145);
    }
}
