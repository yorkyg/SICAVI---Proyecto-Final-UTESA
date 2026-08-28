using Sicavi.WinForms.Configuration;
using Sicavi.WinForms.Data;
using Sicavi.WinForms.Forms;

namespace Sicavi.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            string settingsPath = Path.Combine(AppContext.BaseDirectory, "database-settings.json");
            string scriptPath = Path.Combine(AppContext.BaseDirectory, "Database", "SICAVI.sql");
            DatabaseSettings settings = DatabaseSettings.LoadOrCreate(settingsPath);
            var connections = new SqlConnectionFactory(settings);
            new DatabaseInitializer(connections).Initialize(scriptPath);

            var users = new UserRepository(connections);
            var production = new ProductionRepository(connections);

            while (true)
            {
                Models.AppUser authenticatedUser;
                using (var login = new LoginForm(users))
                {
                    if (login.ShowDialog() != DialogResult.OK || login.AuthenticatedUser is null)
                    {
                        return;
                    }

                    authenticatedUser = login.AuthenticatedUser;
                }

                using var mainForm = new MainForm(authenticatedUser, users, production);
                Application.Run(mainForm);

                if (!mainForm.LogoutRequested)
                {
                    return;
                }
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"No se pudo iniciar SICAVI ni preparar su base de datos.\n\n{exception.Message}\n\n" +
                "Verifica que SQL Server esté iniciado y que database-settings.json apunte a la instancia correcta.",
                "SICAVI - Error de base de datos",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
