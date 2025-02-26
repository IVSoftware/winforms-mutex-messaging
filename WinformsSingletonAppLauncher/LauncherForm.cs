using System.Diagnostics;

namespace WinformsSingletonAppLauncher
{
    public partial class LauncherForm : Form
    {
        int _autoIncrement = 0;
        // Project-relative path.
        const string APP_PATH = @"..\..\..\..\WinformsSingletonApp\bin\Debug\net8.0-windows\WinformsSingletonApp.exe";

        public LauncherForm()
        {
            InitializeComponent();
            buttonLaunch.Click += (sender, e) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = APP_PATH,
                    Arguments = $"{DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:ssK")} {_autoIncrement++}"
                });
            };
        }
    }
}
