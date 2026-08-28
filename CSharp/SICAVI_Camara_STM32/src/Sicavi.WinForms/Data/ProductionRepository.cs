using System.Data;
using Microsoft.Data.SqlClient;
using Sicavi.WinForms.Models;

namespace Sicavi.WinForms.Data;

public sealed class ProductionRepository
{
    private readonly SqlConnectionFactory _connections;
    private readonly object _hourmeterSync = new();

    public ProductionRepository(SqlConnectionFactory connections)
    {
        _connections = connections;
    }

    public void AddInspection(VisionSummary summary, int? userId)
    {
        if (string.IsNullOrWhiteSpace(summary.InspectionId))
        {
            return;
        }

        string result = summary.Decision switch
        {
            InspectionDecision.Good => "ACEPTADA",
            InspectionDecision.Reject => "RECHAZADA",
            _ => "INDETERMINADA"
        };

        using SqlConnection connection = _connections.Create();
        connection.Open();
        using var command = new SqlCommand(
            """
            INSERT INTO dbo.Inspecciones
                (CodigoCaja, ColorDetectado, FormaDetectada, PorcentajeVerde,
                 PorcentajeRojo, Vertices, Circularidad, Resultado, Motivo, UsuarioId)
            VALUES
                (@caja, @color, @forma, @verde, @rojo, @vertices,
                 @circularidad, @resultado, @motivo, @usuarioId);
            """,
            connection);
        command.Parameters.Add("@caja", SqlDbType.NVarChar, 50).Value = NormalizeBoxCode(summary.InspectionId);
        command.Parameters.Add("@color", SqlDbType.NVarChar, 20).Value = TranslateColor(summary.Color);
        command.Parameters.Add("@forma", SqlDbType.NVarChar, 20).Value = TranslateShape(summary.Shape);
        SqlParameter greenParameter = command.Parameters.Add("@verde", SqlDbType.Decimal);
        greenParameter.Precision = 6;
        greenParameter.Scale = 2;
        greenParameter.Value = Math.Round(summary.GreenPercentage, 2);
        SqlParameter redParameter = command.Parameters.Add("@rojo", SqlDbType.Decimal);
        redParameter.Precision = 6;
        redParameter.Scale = 2;
        redParameter.Value = Math.Round(summary.RedPercentage, 2);
        command.Parameters.Add("@vertices", SqlDbType.Int).Value = summary.Vertices;
        SqlParameter circularityParameter = command.Parameters.Add("@circularidad", SqlDbType.Decimal);
        circularityParameter.Precision = 8;
        circularityParameter.Scale = 4;
        circularityParameter.Value = Math.Round(summary.Circularity, 4);
        command.Parameters.Add("@resultado", SqlDbType.NVarChar, 20).Value = result;
        command.Parameters.Add("@motivo", SqlDbType.NVarChar, 300).Value = summary.Reason;
        command.Parameters.Add("@usuarioId", SqlDbType.Int).Value = userId is null ? DBNull.Value : userId.Value;
        command.ExecuteNonQuery();
    }

    public void CompleteInspection(string boxCode, string physicalResult)
    {
        using SqlConnection connection = _connections.Create();
        connection.Open();
        using var command = new SqlCommand(
            """
            UPDATE dbo.Inspecciones
            SET ResultadoFisico = @resultado, FechaFinalizacion = SYSDATETIME()
            WHERE Id = (
                SELECT TOP (1) Id FROM dbo.Inspecciones
                WHERE CodigoCaja = @caja AND FechaFinalizacion IS NULL
                ORDER BY Id DESC
            );
            """,
            connection);
        command.Parameters.Add("@resultado", SqlDbType.NVarChar, 20).Value = TranslateResult(physicalResult);
        command.Parameters.Add("@caja", SqlDbType.NVarChar, 50).Value = NormalizeBoxCode(boxCode);
        command.ExecuteNonQuery();
    }

    public void RecordTelemetry(TimeSpan workTime, uint ejectorCycles)
    {
        lock (_hourmeterSync)
        {
            using SqlConnection connection = _connections.Create();
            connection.Open();
            using SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            long currentSeconds = Math.Max(0L, (long)workTime.TotalSeconds);
            long currentCycles = ejectorCycles;
            long previousSeconds = 0;
            long previousCycles = 0;
            bool hasPrevious;

            using (var select = new SqlCommand(
                "SELECT UltimoHorometroSTM, UltimosCiclosSTM FROM dbo.EstadoSistema WITH (UPDLOCK, HOLDLOCK) WHERE Id = 1;",
                connection,
                transaction))
            using (SqlDataReader reader = select.ExecuteReader())
            {
                hasPrevious = reader.Read();
                if (hasPrevious)
                {
                    previousSeconds = reader.GetInt64(0);
                    previousCycles = reader.GetInt64(1);
                }
            }

            long deltaSeconds = hasPrevious
                ? currentSeconds >= previousSeconds ? currentSeconds - previousSeconds : currentSeconds
                : currentSeconds;
            long deltaCycles = hasPrevious
                ? currentCycles >= previousCycles ? currentCycles - previousCycles : currentCycles
                : currentCycles;

            using (var daily = new SqlCommand(
                """
                MERGE dbo.HorometroDiario AS destino
                USING (SELECT CAST(SYSDATETIME() AS date) AS Fecha) AS origen
                ON destino.Fecha = origen.Fecha
                WHEN MATCHED THEN UPDATE SET
                    SegundosTrabajo = destino.SegundosTrabajo + @segundos,
                    CiclosExpulsor = destino.CiclosExpulsor + @ciclos,
                    FechaActualizacion = SYSDATETIME()
                WHEN NOT MATCHED THEN INSERT
                    (Fecha, SegundosTrabajo, CiclosExpulsor, FechaActualizacion)
                    VALUES (origen.Fecha, @segundos, @ciclos, SYSDATETIME());
                """,
                connection,
                transaction))
            {
                daily.Parameters.Add("@segundos", SqlDbType.BigInt).Value = deltaSeconds;
                daily.Parameters.Add("@ciclos", SqlDbType.BigInt).Value = deltaCycles;
                daily.ExecuteNonQuery();
            }

            using (var state = new SqlCommand(
                """
                MERGE dbo.EstadoSistema AS destino
                USING (SELECT CAST(1 AS tinyint) AS Id) AS origen
                ON destino.Id = origen.Id
                WHEN MATCHED THEN UPDATE SET
                    UltimoHorometroSTM = @segundos, UltimosCiclosSTM = @ciclos, FechaLectura = SYSDATETIME()
                WHEN NOT MATCHED THEN INSERT
                    (Id, UltimoHorometroSTM, UltimosCiclosSTM, FechaLectura)
                    VALUES (1, @segundos, @ciclos, SYSDATETIME());
                """,
                connection,
                transaction))
            {
                state.Parameters.Add("@segundos", SqlDbType.BigInt).Value = currentSeconds;
                state.Parameters.Add("@ciclos", SqlDbType.BigInt).Value = currentCycles;
                state.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }

    public void OpenAlarm(string code, string? detail = null)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Equals("NONE", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        using SqlConnection connection = _connections.Create();
        connection.Open();
        using var command = new SqlCommand(
            """
            IF NOT EXISTS (SELECT 1 FROM dbo.Alarmas WHERE Codigo = @codigo AND Estado = N'ABIERTA')
                INSERT INTO dbo.Alarmas (Codigo, Detalle) VALUES (@codigo, @detalle);
            """,
            connection);
        command.Parameters.Add("@codigo", SqlDbType.NVarChar, 80).Value = TranslateAlarmCode(code);
        command.Parameters.Add("@detalle", SqlDbType.NVarChar, 300).Value =
            string.IsNullOrWhiteSpace(detail) ? DBNull.Value : detail;
        command.ExecuteNonQuery();
    }

    public void CloseOpenAlarms()
    {
        using SqlConnection connection = _connections.Create();
        connection.Open();
        using var command = new SqlCommand(
            """
            UPDATE dbo.Alarmas
            SET Estado = N'CERRADA', FechaHoraCierre = SYSDATETIME()
            WHERE Estado = N'ABIERTA';
            """,
            connection);
        command.ExecuteNonQuery();
    }

    public DashboardSummary GetSummary(DateTime from, DateTime to)
    {
        using SqlConnection connection = _connections.Create();
        connection.Open();
        using SqlCommand command = DateRangeCommand(
            connection,
            """
            SELECT
                SUM(CASE WHEN Resultado IN (N'ACEPTADA', N'BUENA', N'GOOD') THEN 1 ELSE 0 END) AS Buenas,
                SUM(CASE WHEN Resultado IN (N'RECHAZADA', N'MALA', N'REJECT') THEN 1 ELSE 0 END) AS Malas,
                SUM(CASE WHEN Resultado IN (N'INDETERMINADA', N'UNKNOWN') THEN 1 ELSE 0 END) AS Indeterminadas,
                COUNT(*) AS Total
            FROM dbo.Inspecciones
            WHERE FechaHora >= @desde AND FechaHora < @hasta;
            """,
            from,
            to);
        using SqlDataReader reader = command.ExecuteReader();
        reader.Read();
        return new DashboardSummary(
            reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader[0]),
            reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader[1]),
            reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader[2]),
            Convert.ToInt32(reader[3]));
    }

    public IReadOnlyList<InspectionRecord> GetInspections(DateTime from, DateTime to)
    {
        var records = new List<InspectionRecord>();
        using SqlConnection connection = _connections.Create();
        connection.Open();
        using SqlCommand command = DateRangeCommand(
            connection,
            """
            SELECT i.Id, i.CodigoCaja, i.FechaHora, i.ColorDetectado, i.FormaDetectada,
                   i.PorcentajeVerde, i.PorcentajeRojo, i.Vertices, i.Circularidad,
                   i.Resultado, i.Motivo, COALESCE(u.NombreCompleto, N'Sistema') AS Operador
            FROM dbo.Inspecciones i
            LEFT JOIN dbo.Usuarios u ON u.Id = i.UsuarioId
            WHERE i.FechaHora >= @desde AND i.FechaHora < @hasta
            ORDER BY i.FechaHora DESC;
            """,
            from,
            to);
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            records.Add(new InspectionRecord(
                reader.GetInt64(0), NormalizeBoxCode(reader.GetString(1)), reader.GetDateTime(2),
                TranslateStoredColor(reader.GetString(3)), TranslateStoredShape(reader.GetString(4)), Convert.ToDouble(reader[5]),
                Convert.ToDouble(reader[6]), reader.GetInt32(7), Convert.ToDouble(reader[8]),
                TranslateResult(reader.GetString(9)), reader.GetString(10), reader.GetString(11)));
        }
        return records;
    }

    public IReadOnlyList<HourmeterDailyRecord> GetHourmeter(DateTime from, DateTime to)
    {
        var records = new List<HourmeterDailyRecord>();
        using SqlConnection connection = _connections.Create();
        connection.Open();
        using var command = new SqlCommand(
            """
            SELECT Fecha, SegundosTrabajo, CiclosExpulsor, FechaActualizacion
            FROM dbo.HorometroDiario
            WHERE Fecha >= @desde AND Fecha <= @hasta
            ORDER BY Fecha DESC;
            """,
            connection);
        command.Parameters.Add("@desde", SqlDbType.Date).Value = from.Date;
        command.Parameters.Add("@hasta", SqlDbType.Date).Value = to.Date;
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            records.Add(new HourmeterDailyRecord(
                reader.GetDateTime(0), reader.GetInt64(1), reader.GetInt64(2), reader.GetDateTime(3)));
        }
        return records;
    }

    public IReadOnlyList<AlarmHistoryRecord> GetAlarms(DateTime from, DateTime to)
    {
        var records = new List<AlarmHistoryRecord>();
        using SqlConnection connection = _connections.Create();
        connection.Open();
        using SqlCommand command = DateRangeCommand(
            connection,
            """
            SELECT Id, Codigo, FechaHoraInicio, FechaHoraCierre, Estado, Detalle
            FROM dbo.Alarmas
            WHERE FechaHoraInicio >= @desde AND FechaHoraInicio < @hasta
            ORDER BY FechaHoraInicio DESC;
            """,
            from,
            to);
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            records.Add(new AlarmHistoryRecord(
                reader.GetInt64(0), TranslateAlarmCode(reader.GetString(1)), reader.GetDateTime(2),
                reader.IsDBNull(3) ? null : reader.GetDateTime(3), reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5)));
        }
        return records;
    }

    public (long WorkSeconds, long EjectorCycles) GetHourmeterTotals()
    {
        using SqlConnection connection = _connections.Create();
        connection.Open();
        using var command = new SqlCommand(
            "SELECT COALESCE(SUM(SegundosTrabajo), 0), COALESCE(SUM(CiclosExpulsor), 0) FROM dbo.HorometroDiario;",
            connection);
        using SqlDataReader reader = command.ExecuteReader();
        reader.Read();
        return (reader.GetInt64(0), reader.GetInt64(1));
    }

    private static SqlCommand DateRangeCommand(
        SqlConnection connection,
        string sql,
        DateTime from,
        DateTime to)
    {
        var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@desde", SqlDbType.DateTime2).Value = from.Date;
        command.Parameters.Add("@hasta", SqlDbType.DateTime2).Value = to.Date.AddDays(1);
        return command;
    }

    private static string NormalizeBoxCode(string boxCode)
    {
        string trimmed = boxCode.Trim();
        string withoutLeadingZeroes = trimmed.TrimStart('0');
        return withoutLeadingZeroes.Length == 0 ? "0" : withoutLeadingZeroes;
    }

    private static string TranslateColor(DetectedColor color) => color switch
    {
        DetectedColor.Green => "VERDE",
        DetectedColor.Red => "ROJO",
        _ => "DESCONOCIDO"
    };

    private static string TranslateShape(DetectedShape shape) => shape switch
    {
        DetectedShape.Circle => "CÍRCULO",
        DetectedShape.Square => "CUADRADO",
        DetectedShape.Triangle => "TRIÁNGULO",
        _ => "DESCONOCIDA"
    };

    private static string TranslateStoredColor(string color) => color.ToUpperInvariant() switch
    {
        "GREEN" => "VERDE",
        "RED" => "ROJO",
        "UNKNOWN" => "DESCONOCIDO",
        _ => color.ToUpperInvariant()
    };

    private static string TranslateStoredShape(string shape) => shape.ToUpperInvariant() switch
    {
        "CIRCLE" => "CÍRCULO",
        "SQUARE" => "CUADRADO",
        "TRIANGLE" => "TRIÁNGULO",
        "UNKNOWN" => "DESCONOCIDA",
        _ => shape.ToUpperInvariant()
    };

    private static string TranslateResult(string result) => result.ToUpperInvariant() switch
    {
        "GOOD" or "BUENA" or "ACEPTADA" => "ACEPTADA",
        "REJECT" or "MALA" or "RECHAZADA" => "RECHAZADA",
        _ => "INDETERMINADA"
    };

    private static string TranslateAlarmCode(string code) => code.ToUpperInvariant() switch
    {
        "UART_ERROR" => "ERROR DE COMUNICACIÓN UART",
        "ADC_ERROR" => "ERROR DEL SENSOR DE LUZ",
        "LIGHT_OUT_OF_RANGE" => "ILUMINACIÓN FUERA DE RANGO",
        "VISION_TIMEOUT" => "TIEMPO DE VISIÓN AGOTADO",
        "COMM_TIMEOUT" => "COMUNICACIÓN CON PC PERDIDA",
        "VISION_UNKNOWN" => "VISIÓN INDETERMINADA",
        "UNKNOWN" => "ALARMA DESCONOCIDA",
        _ => code.Replace('_', ' ').ToUpperInvariant()
    };
}
