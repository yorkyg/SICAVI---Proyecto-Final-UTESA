using Sicavi.WinForms.Data;
using Sicavi.WinForms.Models;

namespace Sicavi.WinForms.Forms;

public sealed partial class UserManagementForm : Form
{
    private UserRepository? _repository;
    private AppUser _currentUser = AppUser.DesignUser;
    private int? _selectedUserId;

    public UserManagementForm()
    {
        InitializeComponent();
    }

    public UserManagementForm(UserRepository repository, AppUser currentUser) : this()
    {
        _repository = repository;
        _currentUser = currentUser;
        LoadUsers();
    }

    private void LoadUsers()
    {
        if (_repository is null)
        {
            return;
        }

        try
        {
            _usersGrid.DataSource = _repository.GetAll().ToList();
            ConfigureGridColumns();
            _usersGrid.ClearSelection();
            NewUser();
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ConfigureGridColumns()
    {
        if (_usersGrid.Columns[nameof(AppUser.Id)] is { } idColumn)
        {
            idColumn.Visible = false;
        }
        if (_usersGrid.Columns[nameof(AppUser.IsAdministrator)] is { } adminColumn)
        {
            adminColumn.Visible = false;
        }

        SetHeader(nameof(AppUser.Username), "Usuario");
        SetHeader(nameof(AppUser.FullName), "Nombre completo");
        SetHeader(nameof(AppUser.Role), "Rol");
        SetHeader(nameof(AppUser.IsActive), "Activo");
        SetHeader(nameof(AppUser.CreatedAt), "Fecha de creación");
        SetHeader(nameof(AppUser.LastAccess), "Último acceso");
    }

    private void SetHeader(string columnName, string text)
    {
        if (_usersGrid.Columns[columnName] is { } column)
        {
            column.HeaderText = text;
        }
    }

    private void UsersGrid_SelectionChanged(object? sender, EventArgs e)
    {
        if (_usersGrid.CurrentRow?.DataBoundItem is not AppUser user)
        {
            return;
        }

        _selectedUserId = user.Id;
        _usernameTextBox.Text = user.Username;
        _usernameTextBox.ReadOnly = true;
        _fullNameTextBox.Text = user.FullName;
        _roleComboBox.SelectedItem = user.Role;
        _activeCheckBox.Checked = user.IsActive;
        _passwordTextBox.Clear();
        _confirmPasswordTextBox.Clear();
        _editorTitleLabel.Text = $"EDITAR USUARIO · {user.Username}";
    }

    private void NewButton_Click(object? sender, EventArgs e) => NewUser();

    private void NewUser()
    {
        _selectedUserId = null;
        _usernameTextBox.ReadOnly = false;
        _usernameTextBox.Clear();
        _fullNameTextBox.Clear();
        _roleComboBox.SelectedItem = "Operador";
        _activeCheckBox.Checked = true;
        _passwordTextBox.Clear();
        _confirmPasswordTextBox.Clear();
        _editorTitleLabel.Text = "NUEVO USUARIO";
        _usernameTextBox.Focus();
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (_repository is null)
        {
            return;
        }

        string role = _roleComboBox.SelectedItem?.ToString() ?? "Operador";
        string password = _passwordTextBox.Text;
        try
        {
            if (_selectedUserId is null)
            {
                ValidatePasswordConfirmation(required: true);
                _repository.Create(
                    _usernameTextBox.Text,
                    _fullNameTextBox.Text,
                    password,
                    role,
                    _activeCheckBox.Checked);
            }
            else
            {
                if (_selectedUserId == _currentUser.Id && (!_activeCheckBox.Checked || role != "Administrador"))
                {
                    throw new InvalidOperationException("No puedes desactivar ni retirar tu propio rol de administrador durante la sesión.");
                }

                _repository.Update(_selectedUserId.Value, _fullNameTextBox.Text, role, _activeCheckBox.Checked);
                if (!string.IsNullOrWhiteSpace(password))
                {
                    ValidatePasswordConfirmation(required: false);
                    _repository.ChangePassword(_selectedUserId.Value, password);
                }
            }

            MessageBox.Show(this, "Usuario guardado correctamente.", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadUsers();
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "No se pudo guardar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ValidatePasswordConfirmation(bool required)
    {
        if (required && string.IsNullOrWhiteSpace(_passwordTextBox.Text))
        {
            throw new ArgumentException("Escribe una contraseña para el nuevo usuario.");
        }

        if (_passwordTextBox.Text != _confirmPasswordTextBox.Text)
        {
            throw new ArgumentException("Las contraseñas no coinciden.");
        }
    }
}
