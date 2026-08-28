using System.Drawing;

namespace Sicavi.WinForms.Forms;

partial class RegistrationForm
{
    private TextBox _fullNameTextBox = null!;
    private TextBox _usernameTextBox = null!;
    private TextBox _passwordTextBox = null!;
    private TextBox _confirmPasswordTextBox = null!;
    private Label _statusLabel = null!;

    private void InitializeComponent()
    {
        var title = new Label();
        var subtitle = new Label();
        var nameLabel = new Label();
        var userLabel = new Label();
        var passwordLabel = new Label();
        var confirmLabel = new Label();
        var saveButton = new Button();
        var cancelButton = new Button();
        _fullNameTextBox = new TextBox();
        _usernameTextBox = new TextBox();
        _passwordTextBox = new TextBox();
        _confirmPasswordTextBox = new TextBox();
        _statusLabel = new Label();
        SuspendLayout();
        title.Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold);
        title.ForeColor = Color.White;
        title.Location = new Point(42, 28);
        title.Size = new Size(430, 48);
        title.Text = "Registro de operador";
        subtitle.ForeColor = Color.FromArgb(157, 172, 189);
        subtitle.Location = new Point(46, 78);
        subtitle.Size = new Size(430, 44);
        subtitle.Text = "Crea una cuenta para acceder a SICAVI. La clave debe tener al menos 8 caracteres.";
        nameLabel.ForeColor = Color.FromArgb(190, 203, 216);
        nameLabel.Location = new Point(46, 132);
        nameLabel.Size = new Size(430, 24);
        nameLabel.Text = "NOMBRE COMPLETO";
        _fullNameTextBox.BackColor = Color.FromArgb(10, 18, 27);
        _fullNameTextBox.BorderStyle = BorderStyle.FixedSingle;
        _fullNameTextBox.Font = new Font("Segoe UI", 12F);
        _fullNameTextBox.ForeColor = Color.White;
        _fullNameTextBox.Location = new Point(46, 158);
        _fullNameTextBox.Size = new Size(430, 34);
        userLabel.ForeColor = Color.FromArgb(190, 203, 216);
        userLabel.Location = new Point(46, 213);
        userLabel.Size = new Size(430, 24);
        userLabel.Text = "USUARIO";
        _usernameTextBox.BackColor = Color.FromArgb(10, 18, 27);
        _usernameTextBox.BorderStyle = BorderStyle.FixedSingle;
        _usernameTextBox.Font = new Font("Segoe UI", 12F);
        _usernameTextBox.ForeColor = Color.White;
        _usernameTextBox.Location = new Point(46, 239);
        _usernameTextBox.Size = new Size(430, 34);
        passwordLabel.ForeColor = Color.FromArgb(190, 203, 216);
        passwordLabel.Location = new Point(46, 294);
        passwordLabel.Size = new Size(430, 24);
        passwordLabel.Text = "CLAVE";
        _passwordTextBox.BackColor = Color.FromArgb(10, 18, 27);
        _passwordTextBox.BorderStyle = BorderStyle.FixedSingle;
        _passwordTextBox.Font = new Font("Segoe UI", 12F);
        _passwordTextBox.ForeColor = Color.White;
        _passwordTextBox.Location = new Point(46, 320);
        _passwordTextBox.Size = new Size(430, 34);
        _passwordTextBox.UseSystemPasswordChar = true;
        confirmLabel.ForeColor = Color.FromArgb(190, 203, 216);
        confirmLabel.Location = new Point(46, 375);
        confirmLabel.Size = new Size(430, 24);
        confirmLabel.Text = "CONFIRMAR CLAVE";
        _confirmPasswordTextBox.BackColor = Color.FromArgb(10, 18, 27);
        _confirmPasswordTextBox.BorderStyle = BorderStyle.FixedSingle;
        _confirmPasswordTextBox.Font = new Font("Segoe UI", 12F);
        _confirmPasswordTextBox.ForeColor = Color.White;
        _confirmPasswordTextBox.Location = new Point(46, 401);
        _confirmPasswordTextBox.Size = new Size(430, 34);
        _confirmPasswordTextBox.UseSystemPasswordChar = true;
        saveButton.BackColor = Color.FromArgb(26, 200, 218);
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.FlatStyle = FlatStyle.Flat;
        saveButton.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        saveButton.ForeColor = Color.FromArgb(8, 35, 43);
        saveButton.Location = new Point(46, 466);
        saveButton.Size = new Size(210, 40);
        saveButton.Text = "REGISTRAR";
        saveButton.Click += SaveButton_Click;
        cancelButton.DialogResult = DialogResult.Cancel;
        cancelButton.FlatStyle = FlatStyle.Flat;
        cancelButton.ForeColor = Color.FromArgb(210, 221, 232);
        cancelButton.Location = new Point(266, 466);
        cancelButton.Size = new Size(210, 40);
        cancelButton.Text = "CANCELAR";
        _statusLabel.ForeColor = Color.FromArgb(255, 105, 105);
        _statusLabel.Location = new Point(46, 516);
        _statusLabel.Size = new Size(430, 42);
        _statusLabel.TextAlign = ContentAlignment.TopCenter;
        AcceptButton = saveButton;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = cancelButton;
        BackColor = Color.FromArgb(24, 34, 47);
        ClientSize = new Size(525, 570);
        Controls.AddRange(new Control[] { title, subtitle, nameLabel, _fullNameTextBox, userLabel,
            _usernameTextBox, passwordLabel, _passwordTextBox, confirmLabel,
            _confirmPasswordTextBox, saveButton, cancelButton, _statusLabel });
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "RegistrationForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "SICAVI · Registro";
        ResumeLayout(false);
    }
}
