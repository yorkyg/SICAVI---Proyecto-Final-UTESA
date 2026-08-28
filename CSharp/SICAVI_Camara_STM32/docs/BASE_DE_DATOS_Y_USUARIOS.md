# Base de datos y usuarios de SICAVI

## Configuración incluida

SICAVI usa SQL Server con autenticación integrada de Windows. El archivo editable es:

```text
src\Sicavi.WinForms\database-settings.json
```

La configuración inicial apunta a:

```text
Servidor: .\WINCC
Base:     SICAVI
```

En cada inicio, `DatabaseInitializer` ejecuta de forma idempotente `Database\SICAVI.sql`: crea solamente la base, tablas e índices que falten.

## Primer acceso

Si la tabla de usuarios está vacía se crea:

```text
Usuario: admin
Clave:   Sicavi@123
Rol:     Administrador
```

Conviene cambiar esa clave desde **GESTIÓN DE USUARIOS**. Las claves no se guardan como texto: se deriva un hash PBKDF2-SHA256 con sal aleatoria y 210 000 iteraciones.

## Formularios

- **Inicio de sesión**: impide abrir el panel principal sin una cuenta activa.
- **Registro**: se abre con **Registrar nuevo operador** en la pantalla de inicio de sesión y crea una cuenta con rol Operador.
- **Usuarios**: entra como Administrador y pulsa **GESTIÓN DE USUARIOS** dentro de `01 · ACCESOS`, en el panel lateral del formulario principal; crea y edita cuentas, roles, estado y contraseña.
- **Datos de producción**: filtra por fechas y muestra clasificaciones, conteos, horómetro y alarmas.

Todos los formularios tienen archivo `.Designer.cs` y `.resx`. Para editarlos, abre la solución en Visual Studio, haz clic derecho sobre el formulario y selecciona **Ver diseñador**.

La conexión operativa es fija: cámara 0 y COM6. No existe un archivo ni controles de selección de cámara o puerto en el formulario principal.

## Tablas

| Tabla | Contenido |
|---|---|
| `Usuarios` | Cuenta, nombre, rol, estado, hash/sal y últimos accesos. |
| `Inspecciones` | Caja sin ceros iniciales, fecha, color, forma, métricas, decisión en español y operador. |
| `HorometroDiario` | Segundos trabajados y ciclos del expulsor acumulados por día. |
| `EstadoSistema` | Última lectura absoluta del STM32 para calcular incrementos sin duplicarlos. |
| `Alarmas` | Código, detalle, fecha/hora de inicio, cierre y estado. |

## Si SQL Server muestra “Cannot open database SICAVI”

1. Verifica que el servicio de la instancia `WINCC` esté iniciado.
2. Ejecuta SICAVI una vez con un usuario de Windows que pueda crear bases.
3. Revisa que el servidor de `database-settings.json` coincida con tu instancia.
4. En SSMS o la extensión MSSQL, actualiza la lista de bases y conecta a `SICAVI` con autenticación de Windows.

La aplicación muestra un error claro y no abre el panel principal si no puede preparar la base.
