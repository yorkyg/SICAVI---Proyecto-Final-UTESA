USE master;
GO

IF DB_ID(N'SICAVI') IS NULL
BEGIN
    CREATE DATABASE SICAVI;
END;
GO

USE SICAVI;
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios
    (
        Id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
        NombreUsuario       NVARCHAR(50) NOT NULL,
        NombreCompleto      NVARCHAR(120) NOT NULL,
        ClaveHash           VARBINARY(64) NOT NULL,
        ClaveSalt           VARBINARY(32) NOT NULL,
        Rol                 NVARCHAR(20) NOT NULL,
        Activo              BIT NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT (1),
        FechaCreacion       DATETIME2(0) NOT NULL CONSTRAINT DF_Usuarios_FechaCreacion DEFAULT (SYSDATETIME()),
        UltimoAcceso        DATETIME2(0) NULL,
        CONSTRAINT UQ_Usuarios_NombreUsuario UNIQUE (NombreUsuario),
        CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN (N'Administrador', N'Operador'))
    );
END;
GO

IF OBJECT_ID(N'dbo.Inspecciones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Inspecciones
    (
        Id                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inspecciones PRIMARY KEY,
        CodigoCaja          NVARCHAR(50) NOT NULL,
        FechaHora           DATETIME2(3) NOT NULL CONSTRAINT DF_Inspecciones_FechaHora DEFAULT (SYSDATETIME()),
        ColorDetectado      NVARCHAR(20) NOT NULL,
        FormaDetectada      NVARCHAR(20) NOT NULL,
        PorcentajeVerde     DECIMAL(6,2) NOT NULL,
        PorcentajeRojo      DECIMAL(6,2) NOT NULL,
        Vertices            INT NOT NULL,
        Circularidad        DECIMAL(8,4) NOT NULL,
        Resultado           NVARCHAR(20) NOT NULL,
        Motivo              NVARCHAR(300) NOT NULL,
        UsuarioId           INT NULL,
        ResultadoFisico     NVARCHAR(20) NULL,
        FechaFinalizacion   DATETIME2(3) NULL,
        CONSTRAINT FK_Inspecciones_Usuarios FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios(Id),
        CONSTRAINT CK_Inspecciones_Resultado CHECK (Resultado IN (N'ACEPTADA', N'RECHAZADA', N'INDETERMINADA'))
    );

    CREATE INDEX IX_Inspecciones_FechaHora ON dbo.Inspecciones(FechaHora DESC);
    CREATE INDEX IX_Inspecciones_Resultado ON dbo.Inspecciones(Resultado, FechaHora DESC);
    CREATE INDEX IX_Inspecciones_CodigoCaja ON dbo.Inspecciones(CodigoCaja, FechaHora DESC);
END;
GO

/* Normaliza registros anteriores al vocabulario operativo y elimina ceros iniciales. */
IF OBJECT_ID(N'dbo.Inspecciones', N'U') IS NOT NULL
BEGIN
    IF OBJECT_ID(N'dbo.CK_Inspecciones_Resultado', N'C') IS NOT NULL
        ALTER TABLE dbo.Inspecciones DROP CONSTRAINT CK_Inspecciones_Resultado;

    UPDATE dbo.Inspecciones
    SET CodigoCaja = CONVERT(NVARCHAR(50), TRY_CONVERT(BIGINT, CodigoCaja))
    WHERE TRY_CONVERT(BIGINT, CodigoCaja) IS NOT NULL;

    UPDATE dbo.Inspecciones
    SET ColorDetectado = CASE UPPER(ColorDetectado)
            WHEN N'GREEN' THEN N'VERDE'
            WHEN N'RED' THEN N'ROJO'
            WHEN N'UNKNOWN' THEN N'DESCONOCIDO'
            ELSE UPPER(ColorDetectado)
        END,
        FormaDetectada = CASE
            WHEN UPPER(FormaDetectada) = N'CIRCLE' THEN N'CÍRCULO'
            WHEN UPPER(FormaDetectada) = N'SQUARE' THEN N'CUADRADO'
            WHEN UPPER(FormaDetectada) = N'TRIANGLE' THEN N'TRIÁNGULO'
            WHEN UPPER(FormaDetectada) = N'UNKNOWN' THEN N'DESCONOCIDA'
            WHEN UPPER(FormaDetectada) LIKE N'C%RCULO' THEN N'CÍRCULO'
            WHEN UPPER(FormaDetectada) LIKE N'TRI%NGULO' THEN N'TRIÁNGULO'
            ELSE UPPER(FormaDetectada)
        END,
        Resultado = CASE UPPER(Resultado)
            WHEN N'GOOD' THEN N'ACEPTADA'
            WHEN N'BUENA' THEN N'ACEPTADA'
            WHEN N'ACEPTADA' THEN N'ACEPTADA'
            WHEN N'REJECT' THEN N'RECHAZADA'
            WHEN N'MALA' THEN N'RECHAZADA'
            WHEN N'RECHAZADA' THEN N'RECHAZADA'
            WHEN N'UNKNOWN' THEN N'INDETERMINADA'
            ELSE UPPER(Resultado)
        END,
        ResultadoFisico = CASE UPPER(COALESCE(ResultadoFisico, N''))
            WHEN N'GOOD' THEN N'ACEPTADA'
            WHEN N'BUENA' THEN N'ACEPTADA'
            WHEN N'ACEPTADA' THEN N'ACEPTADA'
            WHEN N'REJECT' THEN N'RECHAZADA'
            WHEN N'MALA' THEN N'RECHAZADA'
            WHEN N'RECHAZADA' THEN N'RECHAZADA'
            WHEN N'UNKNOWN' THEN N'INDETERMINADA'
            WHEN N'' THEN NULL
            ELSE UPPER(ResultadoFisico)
        END;

    ALTER TABLE dbo.Inspecciones WITH CHECK ADD CONSTRAINT CK_Inspecciones_Resultado
        CHECK (Resultado IN (N'ACEPTADA', N'RECHAZADA', N'INDETERMINADA'));
END;
GO

IF OBJECT_ID(N'dbo.HorometroDiario', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HorometroDiario
    (
        Fecha               DATE NOT NULL CONSTRAINT PK_HorometroDiario PRIMARY KEY,
        SegundosTrabajo     BIGINT NOT NULL CONSTRAINT DF_Horometro_Segundos DEFAULT (0),
        CiclosExpulsor      BIGINT NOT NULL CONSTRAINT DF_Horometro_Ciclos DEFAULT (0),
        FechaActualizacion  DATETIME2(0) NOT NULL CONSTRAINT DF_Horometro_Actualizacion DEFAULT (SYSDATETIME()),
        CONSTRAINT CK_Horometro_Segundos CHECK (SegundosTrabajo >= 0),
        CONSTRAINT CK_Horometro_Ciclos CHECK (CiclosExpulsor >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.EstadoSistema', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EstadoSistema
    (
        Id                  TINYINT NOT NULL CONSTRAINT PK_EstadoSistema PRIMARY KEY,
        UltimoHorometroSTM  BIGINT NOT NULL,
        UltimosCiclosSTM    BIGINT NOT NULL,
        FechaLectura        DATETIME2(0) NOT NULL,
        CONSTRAINT CK_EstadoSistema_Id CHECK (Id = 1)
    );
END;
GO

IF OBJECT_ID(N'dbo.Alarmas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Alarmas
    (
        Id                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Alarmas PRIMARY KEY,
        Codigo              NVARCHAR(80) NOT NULL,
        FechaHoraInicio     DATETIME2(3) NOT NULL CONSTRAINT DF_Alarmas_Inicio DEFAULT (SYSDATETIME()),
        FechaHoraCierre     DATETIME2(3) NULL,
        Estado              NVARCHAR(15) NOT NULL CONSTRAINT DF_Alarmas_Estado DEFAULT (N'ABIERTA'),
        Detalle             NVARCHAR(300) NULL,
        CONSTRAINT CK_Alarmas_Estado CHECK (Estado IN (N'ABIERTA', N'CERRADA'))
    );

    CREATE INDEX IX_Alarmas_Inicio ON dbo.Alarmas(FechaHoraInicio DESC);
    CREATE INDEX IX_Alarmas_Estado ON dbo.Alarmas(Estado, FechaHoraInicio DESC);
END;
GO

IF OBJECT_ID(N'dbo.Alarmas', N'U') IS NOT NULL
BEGIN
    UPDATE dbo.Alarmas
    SET Codigo = CASE UPPER(Codigo)
        WHEN N'UART_ERROR' THEN N'ERROR DE COMUNICACIÓN UART'
        WHEN N'ADC_ERROR' THEN N'ERROR DEL SENSOR DE LUZ'
        WHEN N'LIGHT_OUT_OF_RANGE' THEN N'ILUMINACIÓN FUERA DE RANGO'
        WHEN N'VISION_TIMEOUT' THEN N'TIEMPO DE VISIÓN AGOTADO'
        WHEN N'COMM_TIMEOUT' THEN N'COMUNICACIÓN CON PC PERDIDA'
        WHEN N'VISION_UNKNOWN' THEN N'VISIÓN INDETERMINADA'
        WHEN N'UNKNOWN' THEN N'ALARMA DESCONOCIDA'
        ELSE UPPER(REPLACE(Codigo, N'_', N' '))
    END;
END;
GO
