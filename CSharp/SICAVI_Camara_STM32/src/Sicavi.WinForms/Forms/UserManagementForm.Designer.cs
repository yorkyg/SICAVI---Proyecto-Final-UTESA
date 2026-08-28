#nullable enable

namespace Sicavi.WinForms.Forms;

partial class UserManagementForm
{
    private System.ComponentModel.IContainer? components = null;
    private DataGridView _usersGrid = null!;
    private Label _editorTitleLabel = null!;
    private TextBox _usernameTextBox = null!;
    private TextBox _fullNameTextBox = null!;
    private ComboBox _roleComboBox = null!;
    private CheckBox _activeCheckBox = null!;
    private TextBox _passwordTextBox = null!;
    private TextBox _confirmPasswordTextBox = null!;
    private Button _newButton = null!;
    private Button _saveButton = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        var root = new TableLayoutPanel();
        var header = new Panel();
        var title = new Label();
        var subtitle = new Label();
        var content = new SplitContainer();
        var listPanel = new Panel();
        var listTitle = new Label();
        _usersGrid = new DataGridView();
        var userColumn = new DataGridViewTextBoxColumn();
        var fullNameColumn = new DataGridViewTextBoxColumn();
        var roleColumn = new DataGridViewTextBoxColumn();
        var activeColumn = new DataGridViewCheckBoxColumn();
        var createdColumn = new DataGridViewTextBoxColumn();
        var lastAccessColumn = new DataGridViewTextBoxColumn();
        var editorPanel = new Panel();
        _editorTitleLabel = new Label();
        var usernameLabel = new Label();
        _usernameTextBox = new TextBox();
        var fullNameLabel = new Label();
        _fullNameTextBox = new TextBox();
        var roleLabel = new Label();
        _roleComboBox = new ComboBox();
        _activeCheckBox = new CheckBox();
        var passwordLabel = new Label();
        _passwordTextBox = new TextBox();
        var confirmLabel = new Label();
        _confirmPasswordTextBox = new TextBox();
        _newButton = new Button();
        _saveButton = new Button();
        root.SuspendLayout();
        header.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)content).BeginInit();
        content.Panel1.SuspendLayout();
        content.Panel2.SuspendLayout();
        content.SuspendLayout();
        listPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_usersGrid).BeginInit();
        editorPanel.SuspendLayout();
        SuspendLayout();
        // root
        root.BackColor = Color.FromArgb(16, 23, 32);
        root.ColumnCount = 1;
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.Controls.Add(header, 0, 0);
        root.Controls.Add(content, 0, 1);
        root.Dock = DockStyle.Fill;
        root.RowCount = 2;
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        // header
        header.BackColor = Color.FromArgb(24, 35, 48);
        header.Controls.Add(title);
        header.Controls.Add(subtitle);
        header.Dock = DockStyle.Fill;
        // title
        title.AutoSize = true;
        title.Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold);
        title.ForeColor = Color.FromArgb(39, 203, 221);
        title.Location = new Point(24, 14);
        title.Text = "GESTIÓN DE USUARIOS";
        // subtitle
        subtitle.AutoSize = true;
        subtitle.ForeColor = Color.FromArgb(168, 184, 201);
        subtitle.Location = new Point(27, 60);
        subtitle.Text = "Cuentas, permisos y acceso al sistema SICAVI";
        // content
        content.BackColor = Color.FromArgb(16, 23, 32);
        content.Dock = DockStyle.Fill;
        content.FixedPanel = FixedPanel.Panel2;
        content.IsSplitterFixed = false;
        content.Location = new Point(0, 96);
        content.Name = "content";
        content.Panel1.Controls.Add(listPanel);
        content.Panel1MinSize = 500;
        content.Panel2.Controls.Add(editorPanel);
        content.Panel2MinSize = 360;
        content.Size = new Size(1080, 624);
        content.SplitterDistance = 690;
        content.SplitterWidth = 6;
        // listPanel
        listPanel.BackColor = Color.FromArgb(20, 29, 40);
        listPanel.Controls.Add(_usersGrid);
        listPanel.Controls.Add(listTitle);
        listPanel.Dock = DockStyle.Fill;
        listPanel.Padding = new Padding(20);
        // listTitle
        listTitle.AutoSize = true;
        listTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
        listTitle.ForeColor = Color.White;
        listTitle.Location = new Point(20, 18);
        listTitle.Text = "USUARIOS REGISTRADOS";
        // usersGrid
        _usersGrid.AllowUserToAddRows = false;
        _usersGrid.AllowUserToDeleteRows = false;
        _usersGrid.AllowUserToResizeRows = false;
        _usersGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _usersGrid.AutoGenerateColumns = false;
        _usersGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _usersGrid.BackgroundColor = Color.FromArgb(13, 20, 28);
        _usersGrid.BorderStyle = BorderStyle.None;
        _usersGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 53, 70);
        _usersGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _usersGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(35, 53, 70);
        _usersGrid.ColumnHeadersHeight = 38;
        _usersGrid.Columns.AddRange(new DataGridViewColumn[] { userColumn, fullNameColumn, roleColumn, activeColumn, createdColumn, lastAccessColumn });
        _usersGrid.DefaultCellStyle.BackColor = Color.FromArgb(24, 35, 48);
        _usersGrid.DefaultCellStyle.ForeColor = Color.FromArgb(232, 239, 246);
        _usersGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(27, 124, 147);
        _usersGrid.DefaultCellStyle.SelectionForeColor = Color.White;
        _usersGrid.EnableHeadersVisualStyles = false;
        _usersGrid.GridColor = Color.FromArgb(52, 68, 84);
        _usersGrid.Location = new Point(20, 55);
        _usersGrid.MultiSelect = false;
        _usersGrid.ReadOnly = true;
        _usersGrid.RowHeadersVisible = false;
        _usersGrid.RowTemplate.Height = 34;
        _usersGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _usersGrid.Size = new Size(650, 555);
        _usersGrid.SelectionChanged += UsersGrid_SelectionChanged;
        userColumn.DataPropertyName = "Username"; userColumn.FillWeight = 80F; userColumn.HeaderText = "Usuario"; userColumn.Name = "Username"; userColumn.ReadOnly = true;
        fullNameColumn.DataPropertyName = "FullName"; fullNameColumn.FillWeight = 130F; fullNameColumn.HeaderText = "Nombre completo"; fullNameColumn.Name = "FullName"; fullNameColumn.ReadOnly = true;
        roleColumn.DataPropertyName = "Role"; roleColumn.FillWeight = 80F; roleColumn.HeaderText = "Rol"; roleColumn.Name = "Role"; roleColumn.ReadOnly = true;
        activeColumn.DataPropertyName = "IsActive"; activeColumn.FillWeight = 42F; activeColumn.HeaderText = "Activo"; activeColumn.Name = "IsActive"; activeColumn.ReadOnly = true;
        createdColumn.DataPropertyName = "CreatedAt"; createdColumn.FillWeight = 92F; createdColumn.HeaderText = "Creación"; createdColumn.Name = "CreatedAt"; createdColumn.ReadOnly = true;
        lastAccessColumn.DataPropertyName = "LastAccess"; lastAccessColumn.FillWeight = 92F; lastAccessColumn.HeaderText = "Último acceso"; lastAccessColumn.Name = "LastAccess"; lastAccessColumn.ReadOnly = true;
        // editorPanel
        editorPanel.AutoScroll = true;
        editorPanel.BackColor = Color.FromArgb(27, 39, 52);
        editorPanel.Controls.Add(_editorTitleLabel);
        editorPanel.Controls.Add(usernameLabel);
        editorPanel.Controls.Add(_usernameTextBox);
        editorPanel.Controls.Add(fullNameLabel);
        editorPanel.Controls.Add(_fullNameTextBox);
        editorPanel.Controls.Add(roleLabel);
        editorPanel.Controls.Add(_roleComboBox);
        editorPanel.Controls.Add(_activeCheckBox);
        editorPanel.Controls.Add(passwordLabel);
        editorPanel.Controls.Add(_passwordTextBox);
        editorPanel.Controls.Add(confirmLabel);
        editorPanel.Controls.Add(_confirmPasswordTextBox);
        editorPanel.Controls.Add(_newButton);
        editorPanel.Controls.Add(_saveButton);
        editorPanel.Dock = DockStyle.Fill;
        // editor fields
        _editorTitleLabel.AutoSize = true;
        _editorTitleLabel.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
        _editorTitleLabel.ForeColor = Color.FromArgb(39, 203, 221);
        _editorTitleLabel.Location = new Point(26, 24);
        _editorTitleLabel.Text = "NUEVO USUARIO";
        usernameLabel.AutoSize = true; usernameLabel.ForeColor = Color.White; usernameLabel.Location = new Point(28, 78); usernameLabel.Text = "Usuario";
        _usernameTextBox.BackColor = Color.FromArgb(13, 20, 28); _usernameTextBox.BorderStyle = BorderStyle.FixedSingle; _usernameTextBox.ForeColor = Color.White; _usernameTextBox.Location = new Point(28, 101); _usernameTextBox.Size = new Size(306, 28);
        fullNameLabel.AutoSize = true; fullNameLabel.ForeColor = Color.White; fullNameLabel.Location = new Point(28, 147); fullNameLabel.Text = "Nombre completo";
        _fullNameTextBox.BackColor = Color.FromArgb(13, 20, 28); _fullNameTextBox.BorderStyle = BorderStyle.FixedSingle; _fullNameTextBox.ForeColor = Color.White; _fullNameTextBox.Location = new Point(28, 170); _fullNameTextBox.Size = new Size(306, 28);
        roleLabel.AutoSize = true; roleLabel.ForeColor = Color.White; roleLabel.Location = new Point(28, 216); roleLabel.Text = "Rol";
        _roleComboBox.BackColor = Color.FromArgb(13, 20, 28); _roleComboBox.DropDownStyle = ComboBoxStyle.DropDownList; _roleComboBox.FlatStyle = FlatStyle.Flat; _roleComboBox.ForeColor = Color.White; _roleComboBox.Items.AddRange(new object[] { "Administrador", "Operador" }); _roleComboBox.Location = new Point(28, 239); _roleComboBox.Size = new Size(306, 28);
        _activeCheckBox.AutoSize = true; _activeCheckBox.Checked = true; _activeCheckBox.CheckState = CheckState.Checked; _activeCheckBox.ForeColor = Color.White; _activeCheckBox.Location = new Point(28, 286); _activeCheckBox.Text = "Cuenta activa";
        passwordLabel.AutoSize = true; passwordLabel.ForeColor = Color.White; passwordLabel.Location = new Point(28, 330); passwordLabel.Text = "Contraseña (vacía conserva la actual)";
        _passwordTextBox.BackColor = Color.FromArgb(13, 20, 28); _passwordTextBox.BorderStyle = BorderStyle.FixedSingle; _passwordTextBox.ForeColor = Color.White; _passwordTextBox.Location = new Point(28, 353); _passwordTextBox.Size = new Size(306, 28); _passwordTextBox.UseSystemPasswordChar = true;
        confirmLabel.AutoSize = true; confirmLabel.ForeColor = Color.White; confirmLabel.Location = new Point(28, 399); confirmLabel.Text = "Confirmar contraseña";
        _confirmPasswordTextBox.BackColor = Color.FromArgb(13, 20, 28); _confirmPasswordTextBox.BorderStyle = BorderStyle.FixedSingle; _confirmPasswordTextBox.ForeColor = Color.White; _confirmPasswordTextBox.Location = new Point(28, 422); _confirmPasswordTextBox.Size = new Size(306, 28); _confirmPasswordTextBox.UseSystemPasswordChar = true;
        // buttons
        _newButton.BackColor = Color.FromArgb(55, 70, 86); _newButton.FlatAppearance.BorderSize = 0; _newButton.FlatStyle = FlatStyle.Flat; _newButton.ForeColor = Color.White; _newButton.Location = new Point(28, 486); _newButton.Size = new Size(144, 42); _newButton.Text = "NUEVO"; _newButton.UseVisualStyleBackColor = false; _newButton.Click += NewButton_Click;
        _saveButton.BackColor = Color.FromArgb(15, 151, 166); _saveButton.FlatAppearance.BorderSize = 0; _saveButton.FlatStyle = FlatStyle.Flat; _saveButton.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold); _saveButton.ForeColor = Color.White; _saveButton.Location = new Point(190, 486); _saveButton.Size = new Size(144, 42); _saveButton.Text = "GUARDAR"; _saveButton.UseVisualStyleBackColor = false; _saveButton.Click += SaveButton_Click;
        // form
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(16, 23, 32);
        ClientSize = new Size(1080, 720);
        Controls.Add(root);
        Font = new Font("Segoe UI", 9F);
        MinimumSize = new Size(980, 650);
        StartPosition = FormStartPosition.CenterParent;
        Text = "SICAVI - Gestión de usuarios";
        root.ResumeLayout(false);
        header.ResumeLayout(false);
        header.PerformLayout();
        content.Panel1.ResumeLayout(false);
        content.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)content).EndInit();
        content.ResumeLayout(false);
        listPanel.ResumeLayout(false);
        listPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_usersGrid).EndInit();
        editorPanel.ResumeLayout(false);
        editorPanel.PerformLayout();
        ResumeLayout(false);
    }
}
