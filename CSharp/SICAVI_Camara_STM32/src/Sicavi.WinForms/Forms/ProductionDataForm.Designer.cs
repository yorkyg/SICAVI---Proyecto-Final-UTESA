#nullable enable

namespace Sicavi.WinForms.Forms;

partial class ProductionDataForm
{
    private System.ComponentModel.IContainer? components = null;
    private DateTimePicker _fromPicker = null!;
    private DateTimePicker _toPicker = null!;
    private Button _refreshButton = null!;
    private Label _lastUpdateLabel = null!;
    private Label _goodValue = null!;
    private Label _rejectValue = null!;
    private Label _unknownValue = null!;
    private Label _totalValue = null!;
    private Label _totalHourmeterValue = null!;
    private Label _totalCyclesValue = null!;
    private DataGridView _inspectionsGrid = null!;
    private DataGridView _hourmeterGrid = null!;
    private DataGridView _alarmsGrid = null!;

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
        var filterPanel = new Panel();
        var fromLabel = new Label();
        _fromPicker = new DateTimePicker();
        var toLabel = new Label();
        _toPicker = new DateTimePicker();
        _refreshButton = new Button();
        _lastUpdateLabel = new Label();
        var cards = new TableLayoutPanel();
        var goodCard = new Panel();
        var goodTitle = new Label();
        _goodValue = new Label();
        var rejectCard = new Panel();
        var rejectTitle = new Label();
        _rejectValue = new Label();
        var unknownCard = new Panel();
        var unknownTitle = new Label();
        _unknownValue = new Label();
        var totalCard = new Panel();
        var totalTitle = new Label();
        _totalValue = new Label();
        var tabs = new TabControl();
        var inspectionsTab = new TabPage();
        _inspectionsGrid = new DataGridView();
        var boxColumn = new DataGridViewTextBoxColumn();
        var inspectionDateColumn = new DataGridViewTextBoxColumn();
        var colorColumn = new DataGridViewTextBoxColumn();
        var shapeColumn = new DataGridViewTextBoxColumn();
        var greenColumn = new DataGridViewTextBoxColumn();
        var redColumn = new DataGridViewTextBoxColumn();
        var verticesColumn = new DataGridViewTextBoxColumn();
        var circularityColumn = new DataGridViewTextBoxColumn();
        var resultColumn = new DataGridViewTextBoxColumn();
        var reasonColumn = new DataGridViewTextBoxColumn();
        var operatorColumn = new DataGridViewTextBoxColumn();
        var hourmeterTab = new TabPage();
        var hourmeterSummary = new Panel();
        var totalHourmeterTitle = new Label();
        _totalHourmeterValue = new Label();
        var totalCyclesTitle = new Label();
        _totalCyclesValue = new Label();
        _hourmeterGrid = new DataGridView();
        var hourDateColumn = new DataGridViewTextBoxColumn();
        var hoursColumn = new DataGridViewTextBoxColumn();
        var cyclesColumn = new DataGridViewTextBoxColumn();
        var updatedColumn = new DataGridViewTextBoxColumn();
        var alarmsTab = new TabPage();
        _alarmsGrid = new DataGridView();
        var alarmColumn = new DataGridViewTextBoxColumn();
        var alarmStartColumn = new DataGridViewTextBoxColumn();
        var alarmEndColumn = new DataGridViewTextBoxColumn();
        var alarmStatusColumn = new DataGridViewTextBoxColumn();
        var alarmDetailColumn = new DataGridViewTextBoxColumn();
        root.SuspendLayout();
        header.SuspendLayout();
        filterPanel.SuspendLayout();
        cards.SuspendLayout();
        goodCard.SuspendLayout();
        rejectCard.SuspendLayout();
        unknownCard.SuspendLayout();
        totalCard.SuspendLayout();
        tabs.SuspendLayout();
        inspectionsTab.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_inspectionsGrid).BeginInit();
        hourmeterTab.SuspendLayout();
        hourmeterSummary.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_hourmeterGrid).BeginInit();
        alarmsTab.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_alarmsGrid).BeginInit();
        SuspendLayout();
        // root
        root.BackColor = Color.FromArgb(16, 23, 32);
        root.ColumnCount = 1;
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.Controls.Add(header, 0, 0);
        root.Controls.Add(filterPanel, 0, 1);
        root.Controls.Add(cards, 0, 2);
        root.Controls.Add(tabs, 0, 3);
        root.Dock = DockStyle.Fill;
        root.RowCount = 4;
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 126F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        // header
        header.BackColor = Color.FromArgb(24, 35, 48);
        header.Controls.Add(title);
        header.Controls.Add(subtitle);
        header.Dock = DockStyle.Fill;
        title.AutoSize = true; title.Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold); title.ForeColor = Color.FromArgb(39, 203, 221); title.Location = new Point(24, 12); title.Text = "DATOS DE PRODUCCIÓN";
        subtitle.AutoSize = true; subtitle.ForeColor = Color.FromArgb(168, 184, 201); subtitle.Location = new Point(27, 58); subtitle.Text = "Clasificaciones, conteos, horómetro e historial de alarmas";
        // filters
        filterPanel.BackColor = Color.FromArgb(20, 29, 40);
        filterPanel.Controls.Add(fromLabel); filterPanel.Controls.Add(_fromPicker); filterPanel.Controls.Add(toLabel); filterPanel.Controls.Add(_toPicker); filterPanel.Controls.Add(_refreshButton); filterPanel.Controls.Add(_lastUpdateLabel);
        filterPanel.Dock = DockStyle.Fill;
        fromLabel.AutoSize = true; fromLabel.ForeColor = Color.White; fromLabel.Location = new Point(24, 25); fromLabel.Text = "Desde";
        _fromPicker.CalendarMonthBackground = Color.FromArgb(24, 35, 48); _fromPicker.Format = DateTimePickerFormat.Short; _fromPicker.Location = new Point(77, 20); _fromPicker.Size = new Size(130, 28);
        toLabel.AutoSize = true; toLabel.ForeColor = Color.White; toLabel.Location = new Point(232, 25); toLabel.Text = "Hasta";
        _toPicker.Format = DateTimePickerFormat.Short; _toPicker.Location = new Point(282, 20); _toPicker.Size = new Size(130, 28);
        _refreshButton.BackColor = Color.FromArgb(15, 151, 166); _refreshButton.FlatAppearance.BorderSize = 0; _refreshButton.FlatStyle = FlatStyle.Flat; _refreshButton.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold); _refreshButton.ForeColor = Color.White; _refreshButton.Location = new Point(438, 14); _refreshButton.Size = new Size(142, 40); _refreshButton.Text = "ACTUALIZAR"; _refreshButton.UseVisualStyleBackColor = false; _refreshButton.Click += RefreshButton_Click;
        _lastUpdateLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right; _lastUpdateLabel.ForeColor = Color.FromArgb(148, 164, 181); _lastUpdateLabel.Location = new Point(800, 23); _lastUpdateLabel.Size = new Size(370, 22); _lastUpdateLabel.Text = "ACTUALIZADO: --"; _lastUpdateLabel.TextAlign = ContentAlignment.MiddleRight;
        // cards
        cards.BackColor = Color.FromArgb(16, 23, 32); cards.ColumnCount = 4; cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F)); cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F)); cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F)); cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F)); cards.Controls.Add(goodCard, 0, 0); cards.Controls.Add(rejectCard, 1, 0); cards.Controls.Add(unknownCard, 2, 0); cards.Controls.Add(totalCard, 3, 0); cards.Dock = DockStyle.Fill; cards.Padding = new Padding(14, 12, 14, 10); cards.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        goodCard.BackColor = Color.FromArgb(23, 66, 56); goodCard.Controls.Add(goodTitle); goodCard.Controls.Add(_goodValue); goodCard.Dock = DockStyle.Fill; goodCard.Margin = new Padding(8, 0, 8, 0);
        goodTitle.AutoSize = true; goodTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold); goodTitle.ForeColor = Color.FromArgb(150, 235, 191); goodTitle.Location = new Point(18, 16); goodTitle.Text = "ACEPTADAS";
        _goodValue.AutoSize = true; _goodValue.Font = new Font("Segoe UI Semibold", 26F, FontStyle.Bold); _goodValue.ForeColor = Color.White; _goodValue.Location = new Point(15, 39); _goodValue.Text = "0";
        rejectCard.BackColor = Color.FromArgb(79, 39, 44); rejectCard.Controls.Add(rejectTitle); rejectCard.Controls.Add(_rejectValue); rejectCard.Dock = DockStyle.Fill; rejectCard.Margin = new Padding(8, 0, 8, 0);
        rejectTitle.AutoSize = true; rejectTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold); rejectTitle.ForeColor = Color.FromArgb(255, 166, 173); rejectTitle.Location = new Point(18, 16); rejectTitle.Text = "RECHAZADAS";
        _rejectValue.AutoSize = true; _rejectValue.Font = new Font("Segoe UI Semibold", 26F, FontStyle.Bold); _rejectValue.ForeColor = Color.White; _rejectValue.Location = new Point(15, 39); _rejectValue.Text = "0";
        unknownCard.BackColor = Color.FromArgb(76, 64, 38); unknownCard.Controls.Add(unknownTitle); unknownCard.Controls.Add(_unknownValue); unknownCard.Dock = DockStyle.Fill; unknownCard.Margin = new Padding(8, 0, 8, 0);
        unknownTitle.AutoSize = true; unknownTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold); unknownTitle.ForeColor = Color.FromArgb(246, 214, 139); unknownTitle.Location = new Point(18, 16); unknownTitle.Text = "INDETERMINADAS";
        _unknownValue.AutoSize = true; _unknownValue.Font = new Font("Segoe UI Semibold", 26F, FontStyle.Bold); _unknownValue.ForeColor = Color.White; _unknownValue.Location = new Point(15, 39); _unknownValue.Text = "0";
        totalCard.BackColor = Color.FromArgb(29, 67, 88); totalCard.Controls.Add(totalTitle); totalCard.Controls.Add(_totalValue); totalCard.Dock = DockStyle.Fill; totalCard.Margin = new Padding(8, 0, 8, 0);
        totalTitle.AutoSize = true; totalTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold); totalTitle.ForeColor = Color.FromArgb(143, 213, 244); totalTitle.Location = new Point(18, 16); totalTitle.Text = "TOTAL";
        _totalValue.AutoSize = true; _totalValue.Font = new Font("Segoe UI Semibold", 26F, FontStyle.Bold); _totalValue.ForeColor = Color.White; _totalValue.Location = new Point(15, 39); _totalValue.Text = "0";
        // tabs
        tabs.Controls.Add(inspectionsTab); tabs.Controls.Add(hourmeterTab); tabs.Controls.Add(alarmsTab); tabs.Dock = DockStyle.Fill; tabs.Font = new Font("Segoe UI Semibold", 10F); tabs.Location = new Point(18, 289); tabs.Margin = new Padding(18, 3, 18, 18); tabs.SelectedIndex = 0;
        inspectionsTab.BackColor = Color.FromArgb(20, 29, 40); inspectionsTab.Controls.Add(_inspectionsGrid); inspectionsTab.Padding = new Padding(10); inspectionsTab.Text = "  CLASIFICACIONES  ";
        _inspectionsGrid.AutoGenerateColumns = false; _inspectionsGrid.Columns.AddRange(new DataGridViewColumn[] { boxColumn, inspectionDateColumn, colorColumn, shapeColumn, greenColumn, redColumn, verticesColumn, circularityColumn, resultColumn, reasonColumn, operatorColumn }); _inspectionsGrid.Dock = DockStyle.Fill;
        hourmeterTab.BackColor = Color.FromArgb(20, 29, 40); hourmeterTab.Controls.Add(_hourmeterGrid); hourmeterTab.Controls.Add(hourmeterSummary); hourmeterTab.Padding = new Padding(10); hourmeterTab.Text = "  HORÓMETRO  ";
        hourmeterSummary.BackColor = Color.FromArgb(27, 39, 52); hourmeterSummary.Controls.Add(totalHourmeterTitle); hourmeterSummary.Controls.Add(_totalHourmeterValue); hourmeterSummary.Controls.Add(totalCyclesTitle); hourmeterSummary.Controls.Add(_totalCyclesValue); hourmeterSummary.Dock = DockStyle.Top; hourmeterSummary.Height = 76;
        totalHourmeterTitle.AutoSize = true; totalHourmeterTitle.ForeColor = Color.FromArgb(160, 176, 193); totalHourmeterTitle.Location = new Point(22, 12); totalHourmeterTitle.Text = "HORÓMETRO TOTAL";
        _totalHourmeterValue.AutoSize = true; _totalHourmeterValue.Font = new Font("Consolas", 17F, FontStyle.Bold); _totalHourmeterValue.ForeColor = Color.FromArgb(39, 203, 221); _totalHourmeterValue.Location = new Point(20, 35); _totalHourmeterValue.Text = "0:00:00";
        totalCyclesTitle.AutoSize = true; totalCyclesTitle.ForeColor = Color.FromArgb(160, 176, 193); totalCyclesTitle.Location = new Point(300, 12); totalCyclesTitle.Text = "CICLOS TOTALES";
        _totalCyclesValue.AutoSize = true; _totalCyclesValue.Font = new Font("Consolas", 17F, FontStyle.Bold); _totalCyclesValue.ForeColor = Color.FromArgb(39, 203, 221); _totalCyclesValue.Location = new Point(298, 35); _totalCyclesValue.Text = "0";
        _hourmeterGrid.AutoGenerateColumns = false; _hourmeterGrid.Columns.AddRange(new DataGridViewColumn[] { hourDateColumn, hoursColumn, cyclesColumn, updatedColumn }); _hourmeterGrid.Dock = DockStyle.Fill;
        alarmsTab.BackColor = Color.FromArgb(20, 29, 40); alarmsTab.Controls.Add(_alarmsGrid); alarmsTab.Padding = new Padding(10); alarmsTab.Text = "  ALARMAS  ";
        _alarmsGrid.AutoGenerateColumns = false; _alarmsGrid.Columns.AddRange(new DataGridViewColumn[] { alarmColumn, alarmStartColumn, alarmEndColumn, alarmStatusColumn, alarmDetailColumn }); _alarmsGrid.Dock = DockStyle.Fill;
        // inspections grid
        _inspectionsGrid.AllowUserToAddRows = false;
        _inspectionsGrid.AllowUserToDeleteRows = false;
        _inspectionsGrid.AllowUserToResizeRows = false;
        _inspectionsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _inspectionsGrid.BackgroundColor = Color.FromArgb(13, 20, 28);
        _inspectionsGrid.BorderStyle = BorderStyle.None;
        _inspectionsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 53, 70);
        _inspectionsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _inspectionsGrid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        _inspectionsGrid.ColumnHeadersHeight = 40;
        _inspectionsGrid.DefaultCellStyle.BackColor = Color.FromArgb(24, 35, 48);
        _inspectionsGrid.DefaultCellStyle.ForeColor = Color.FromArgb(232, 239, 246);
        _inspectionsGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(27, 124, 147);
        _inspectionsGrid.DefaultCellStyle.SelectionForeColor = Color.White;
        _inspectionsGrid.EnableHeadersVisualStyles = false;
        _inspectionsGrid.GridColor = Color.FromArgb(52, 68, 84);
        _inspectionsGrid.ReadOnly = true;
        _inspectionsGrid.RowHeadersVisible = false;
        _inspectionsGrid.RowTemplate.Height = 34;
        _inspectionsGrid.ScrollBars = ScrollBars.Both;
        _inspectionsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        // hourmeter grid
        _hourmeterGrid.AllowUserToAddRows = false;
        _hourmeterGrid.AllowUserToDeleteRows = false;
        _hourmeterGrid.AllowUserToResizeRows = false;
        _hourmeterGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _hourmeterGrid.BackgroundColor = Color.FromArgb(13, 20, 28);
        _hourmeterGrid.BorderStyle = BorderStyle.None;
        _hourmeterGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 53, 70);
        _hourmeterGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _hourmeterGrid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        _hourmeterGrid.ColumnHeadersHeight = 40;
        _hourmeterGrid.DefaultCellStyle.BackColor = Color.FromArgb(24, 35, 48);
        _hourmeterGrid.DefaultCellStyle.ForeColor = Color.FromArgb(232, 239, 246);
        _hourmeterGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(27, 124, 147);
        _hourmeterGrid.DefaultCellStyle.SelectionForeColor = Color.White;
        _hourmeterGrid.EnableHeadersVisualStyles = false;
        _hourmeterGrid.GridColor = Color.FromArgb(52, 68, 84);
        _hourmeterGrid.ReadOnly = true;
        _hourmeterGrid.RowHeadersVisible = false;
        _hourmeterGrid.RowTemplate.Height = 34;
        _hourmeterGrid.ScrollBars = ScrollBars.Both;
        _hourmeterGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        // alarms grid
        _alarmsGrid.AllowUserToAddRows = false;
        _alarmsGrid.AllowUserToDeleteRows = false;
        _alarmsGrid.AllowUserToResizeRows = false;
        _alarmsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _alarmsGrid.BackgroundColor = Color.FromArgb(13, 20, 28);
        _alarmsGrid.BorderStyle = BorderStyle.None;
        _alarmsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 53, 70);
        _alarmsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _alarmsGrid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        _alarmsGrid.ColumnHeadersHeight = 40;
        _alarmsGrid.DefaultCellStyle.BackColor = Color.FromArgb(24, 35, 48);
        _alarmsGrid.DefaultCellStyle.ForeColor = Color.FromArgb(232, 239, 246);
        _alarmsGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(27, 124, 147);
        _alarmsGrid.DefaultCellStyle.SelectionForeColor = Color.White;
        _alarmsGrid.EnableHeadersVisualStyles = false;
        _alarmsGrid.GridColor = Color.FromArgb(52, 68, 84);
        _alarmsGrid.ReadOnly = true;
        _alarmsGrid.RowHeadersVisible = false;
        _alarmsGrid.RowTemplate.Height = 34;
        _alarmsGrid.ScrollBars = ScrollBars.Both;
        _alarmsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        boxColumn.DataPropertyName = "Caja"; boxColumn.HeaderText = "Caja"; boxColumn.MinimumWidth = 110; boxColumn.Name = "Caja"; boxColumn.Width = 110;
        inspectionDateColumn.DataPropertyName = "Fecha"; inspectionDateColumn.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss"; inspectionDateColumn.HeaderText = "Fecha y hora"; inspectionDateColumn.MinimumWidth = 165; inspectionDateColumn.Name = "Fecha"; inspectionDateColumn.Width = 165;
        colorColumn.DataPropertyName = "Color"; colorColumn.HeaderText = "Color"; colorColumn.MinimumWidth = 90; colorColumn.Name = "Color"; colorColumn.Width = 90;
        shapeColumn.DataPropertyName = "Forma"; shapeColumn.HeaderText = "Forma"; shapeColumn.MinimumWidth = 100; shapeColumn.Name = "Forma"; shapeColumn.Width = 100;
        greenColumn.DataPropertyName = "Verde"; greenColumn.DefaultCellStyle.Format = "N2"; greenColumn.HeaderText = "% verde"; greenColumn.MinimumWidth = 90; greenColumn.Name = "Verde"; greenColumn.Width = 90;
        redColumn.DataPropertyName = "Rojo"; redColumn.DefaultCellStyle.Format = "N2"; redColumn.HeaderText = "% rojo"; redColumn.MinimumWidth = 90; redColumn.Name = "Rojo"; redColumn.Width = 90;
        verticesColumn.DataPropertyName = "Vértices"; verticesColumn.HeaderText = "Vértices"; verticesColumn.MinimumWidth = 80; verticesColumn.Name = "Vértices"; verticesColumn.Width = 80;
        circularityColumn.DataPropertyName = "Circularidad"; circularityColumn.DefaultCellStyle.Format = "N4"; circularityColumn.HeaderText = "Circularidad"; circularityColumn.MinimumWidth = 105; circularityColumn.Name = "Circularidad"; circularityColumn.Width = 105;
        resultColumn.DataPropertyName = "Resultado"; resultColumn.HeaderText = "Resultado"; resultColumn.MinimumWidth = 105; resultColumn.Name = "Resultado"; resultColumn.Width = 105;
        reasonColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; reasonColumn.DataPropertyName = "Motivo"; reasonColumn.FillWeight = 180F; reasonColumn.HeaderText = "Motivo"; reasonColumn.MinimumWidth = 300; reasonColumn.Name = "Motivo"; reasonColumn.Width = 300;
        operatorColumn.DataPropertyName = "Operador"; operatorColumn.HeaderText = "Operador"; operatorColumn.MinimumWidth = 170; operatorColumn.Name = "Operador"; operatorColumn.Width = 170;
        hourDateColumn.DataPropertyName = "Fecha"; hourDateColumn.HeaderText = "Fecha"; hourDateColumn.MinimumWidth = 150; hourDateColumn.Name = "FechaHorometro"; hourDateColumn.Width = 150;
        hoursColumn.DataPropertyName = "Horas"; hoursColumn.HeaderText = "Horas trabajadas"; hoursColumn.MinimumWidth = 180; hoursColumn.Name = "Horas"; hoursColumn.Width = 180;
        cyclesColumn.DataPropertyName = "Ciclos"; cyclesColumn.HeaderText = "Ciclos expulsor"; cyclesColumn.MinimumWidth = 170; cyclesColumn.Name = "Ciclos"; cyclesColumn.Width = 170;
        updatedColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; updatedColumn.DataPropertyName = "Actualizado"; updatedColumn.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss"; updatedColumn.HeaderText = "Última actualización"; updatedColumn.MinimumWidth = 220; updatedColumn.Name = "Actualizado"; updatedColumn.Width = 220;
        alarmColumn.DataPropertyName = "Alarma"; alarmColumn.HeaderText = "Alarma"; alarmColumn.MinimumWidth = 180; alarmColumn.Name = "Alarma"; alarmColumn.Width = 180;
        alarmStartColumn.DataPropertyName = "Inicio"; alarmStartColumn.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss"; alarmStartColumn.HeaderText = "Inicio"; alarmStartColumn.MinimumWidth = 180; alarmStartColumn.Name = "Inicio"; alarmStartColumn.Width = 180;
        alarmEndColumn.DataPropertyName = "Cierre"; alarmEndColumn.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss"; alarmEndColumn.HeaderText = "Cierre"; alarmEndColumn.MinimumWidth = 180; alarmEndColumn.Name = "Cierre"; alarmEndColumn.Width = 180;
        alarmStatusColumn.DataPropertyName = "Estado"; alarmStatusColumn.HeaderText = "Estado"; alarmStatusColumn.MinimumWidth = 130; alarmStatusColumn.Name = "Estado"; alarmStatusColumn.Width = 130;
        alarmDetailColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; alarmDetailColumn.DataPropertyName = "Detalle"; alarmDetailColumn.HeaderText = "Detalle"; alarmDetailColumn.MinimumWidth = 300; alarmDetailColumn.Name = "Detalle"; alarmDetailColumn.Width = 300;
        // form
        AutoScaleDimensions = new SizeF(7F, 15F); AutoScaleMode = AutoScaleMode.Font; BackColor = Color.FromArgb(16, 23, 32); ClientSize = new Size(1440, 820); Controls.Add(root); Font = new Font("Segoe UI", 9F); MinimumSize = new Size(1200, 720); StartPosition = FormStartPosition.CenterParent; Text = "SICAVI - Datos de producción"; WindowState = FormWindowState.Maximized;
        root.ResumeLayout(false); header.ResumeLayout(false); header.PerformLayout(); filterPanel.ResumeLayout(false); filterPanel.PerformLayout(); cards.ResumeLayout(false); goodCard.ResumeLayout(false); goodCard.PerformLayout(); rejectCard.ResumeLayout(false); rejectCard.PerformLayout(); unknownCard.ResumeLayout(false); unknownCard.PerformLayout(); totalCard.ResumeLayout(false); totalCard.PerformLayout(); tabs.ResumeLayout(false); inspectionsTab.ResumeLayout(false); ((System.ComponentModel.ISupportInitialize)_inspectionsGrid).EndInit(); hourmeterTab.ResumeLayout(false); hourmeterSummary.ResumeLayout(false); hourmeterSummary.PerformLayout(); ((System.ComponentModel.ISupportInitialize)_hourmeterGrid).EndInit(); alarmsTab.ResumeLayout(false); ((System.ComponentModel.ISupportInitialize)_alarmsGrid).EndInit(); ResumeLayout(false);
    }
}
