using Sicavi.WinForms.Data;

namespace Sicavi.WinForms.Forms;

public sealed partial class ProductionDataForm : Form
{
    private ProductionRepository? _repository;

    public ProductionDataForm()
    {
        InitializeComponent();
        _fromPicker.Value = DateTime.Today.AddDays(-7);
        _toPicker.Value = DateTime.Today;
        ApplyPersistentColumnLayout();
        Shown += (_, _) => ApplyPersistentColumnLayout();
    }

    public ProductionDataForm(ProductionRepository repository) : this()
    {
        _repository = repository;
        RefreshData();
    }

    private void RefreshButton_Click(object? sender, EventArgs e) => RefreshData();

    private void RefreshData()
    {
        if (_repository is null)
        {
            return;
        }

        if (_fromPicker.Value.Date > _toPicker.Value.Date)
        {
            MessageBox.Show(this, "La fecha inicial no puede ser mayor que la fecha final.", "Rango de fechas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            DateTime from = _fromPicker.Value.Date;
            DateTime to = _toPicker.Value.Date;
            var summary = _repository.GetSummary(from, to);
            _goodValue.Text = summary.Good.ToString("N0");
            _rejectValue.Text = summary.Reject.ToString("N0");
            _unknownValue.Text = summary.Unknown.ToString("N0");
            _totalValue.Text = summary.Total.ToString("N0");

            _inspectionsGrid.DataSource = _repository.GetInspections(from, to)
                .Select(item => new
                {
                    item.Id,
                    Caja = item.BoxCode,
                    Fecha = item.Timestamp,
                    Color = item.DetectedColor,
                    Forma = item.DetectedShape,
                    Verde = item.GreenPercentage,
                    Rojo = item.RedPercentage,
                    Vértices = item.Vertices,
                    Circularidad = item.Circularity,
                    Resultado = item.Result,
                    Motivo = item.Reason,
                    Operador = item.OperatorName
                })
                .ToList();

            _hourmeterGrid.DataSource = _repository.GetHourmeter(from, to)
                .Select(item => new
                {
                    Fecha = item.Date.ToString("dd/MM/yyyy"),
                    Horas = FormatDuration(item.WorkSeconds),
                    Ciclos = item.EjectorCycles,
                    Actualizado = item.LastUpdate
                })
                .ToList();

            _alarmsGrid.DataSource = _repository.GetAlarms(from, to)
                .Select(item => new
                {
                    item.Id,
                    Alarma = item.Code,
                    Inicio = item.StartedAt,
                    Cierre = item.ClosedAt,
                    Estado = item.Status,
                    Detalle = item.Detail
                })
                .ToList();

            (long workSeconds, long ejectorCycles) = _repository.GetHourmeterTotals();
            _totalHourmeterValue.Text = FormatDuration(workSeconds);
            _totalCyclesValue.Text = ejectorCycles.ToString("N0");
            _lastUpdateLabel.Text = $"ACTUALIZADO: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            ApplyPersistentColumnLayout();
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "No se pudieron consultar los datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ApplyPersistentColumnLayout()
    {
        _inspectionsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        SetFixedWidth(_inspectionsGrid, "Caja", 110);
        SetFixedWidth(_inspectionsGrid, "Fecha", 165);
        SetFixedWidth(_inspectionsGrid, "Color", 90);
        SetFixedWidth(_inspectionsGrid, "Forma", 100);
        SetFixedWidth(_inspectionsGrid, "Verde", 90);
        SetFixedWidth(_inspectionsGrid, "Rojo", 90);
        SetFixedWidth(_inspectionsGrid, "Vértices", 80);
        SetFixedWidth(_inspectionsGrid, "Circularidad", 105);
        SetFixedWidth(_inspectionsGrid, "Resultado", 105);
        SetFillWidth(_inspectionsGrid, "Motivo", 300, 180F);
        SetFixedWidth(_inspectionsGrid, "Operador", 170);

        _hourmeterGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        SetFixedWidth(_hourmeterGrid, "FechaHorometro", 150);
        SetFixedWidth(_hourmeterGrid, "Horas", 180);
        SetFixedWidth(_hourmeterGrid, "Ciclos", 170);
        SetFillWidth(_hourmeterGrid, "Actualizado", 220, 100F);

        _alarmsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        SetFixedWidth(_alarmsGrid, "Alarma", 180);
        SetFixedWidth(_alarmsGrid, "Inicio", 180);
        SetFixedWidth(_alarmsGrid, "Cierre", 180);
        SetFixedWidth(_alarmsGrid, "Estado", 130);
        SetFillWidth(_alarmsGrid, "Detalle", 300, 140F);
    }

    private static void SetFixedWidth(DataGridView grid, string columnName, int width)
    {
        if (grid.Columns[columnName] is not { } column)
        {
            return;
        }

        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        column.MinimumWidth = width;
        column.Width = width;
    }

    private static void SetFillWidth(DataGridView grid, string columnName, int minimumWidth, float fillWeight)
    {
        if (grid.Columns[columnName] is not { } column)
        {
            return;
        }

        column.MinimumWidth = minimumWidth;
        column.FillWeight = fillWeight;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
    }

    private static string FormatDuration(long totalSeconds)
    {
        long hours = Math.Max(0, totalSeconds) / 3600;
        long minutes = Math.Max(0, totalSeconds) % 3600 / 60;
        long seconds = Math.Max(0, totalSeconds) % 60;
        return $"{hours:N0}:{minutes:00}:{seconds:00}";
    }
}
