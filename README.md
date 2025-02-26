To handle the second scenario, one simple approach is to use a named pipe. 

```
internal static class Program
{
    .
    .
    .
    const string PIPE = "{5F563B29-172F-4385-B813-21062155F22E}-PIPE";
    private static void SendMessageToPipe(Array args)
    {
        try
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", PIPE, PipeDirection.Out))
            {
                pipeClient.Connect(timeout: TimeSpan.FromSeconds(5)); 
                using (StreamWriter writer = new StreamWriter(pipeClient) { AutoFlush = true })
                {
                    writer.WriteLine(JsonConvert.SerializeObject(args, Formatting.Indented)); 
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to send arguments to the main instance: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private static async void ListenForMessages()
    {
        await Task.Run(() =>
        {
            while (true)
            {
                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(PIPE, PipeDirection.In))
                {
                    pipeServer.WaitForConnection(); 
                    using (StreamReader reader = new StreamReader(pipeServer))
                    {
                        if(reader.ReadToEnd() is { } json)
                        {
                            mainForm?.OnPipeMessage(json);
                        }
                    }
                }
            }
        });
    }
}
```

___

For demo purposes, when the singleton app receives a message, it pops up a message box showing the arguments.

```
namespace WinformsSingletonApp
{
    public partial class MainForm : Form
    {
        public MainForm() => InitializeComponent();

        public void OnPipeMessage(string json)
        {
            int N = 0;
            if(JsonConvert.DeserializeObject<JArray>(json) is { } pm)
            {
                MessageBox.Show(
                    string.Join(
                        Environment.NewLine, 
                        pm.Select(_ => $"[{N++}] {_}")));
            }
        }
    }
}
```
___

**Mutex**

Keep handling the Mutex the same way, only now push a message into the pipe when the mutex can't be created.

```
internal static class Program
{
    private static Mutex? mutex;
    private static MainForm? mainForm;
    const string MUTEX = "{5F563B29-172F-4385-B813-21062155F22E}-MUTEX";

    [STAThread]
    static void Main()
    { 
        bool createdNew;
        using (mutex = new Mutex(true, MUTEX, out createdNew))
        {
            if (createdNew)
            {
                try
                {
                    ListenForMessages();
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    mainForm = new MainForm();
                    Application.Run(mainForm);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            else
            {
                var args = Environment.GetCommandLineArgs();
                args[0] = Path.GetFileName(args[0]);
                SendMessageToPipe(args);
            }
        }
    }
    .
    .
    .
}
```
___

**Testing**

What I've done for testing purposes is make a second winforms app that invokes the `.exe` of the first.

```
public partial class LauncherForm : Form
{
    int _autoIncrement = 0;
    const string APP_PATH = @"D:\Github\stackoverflow\WinForms\winforms-mutex-messaging\WinformsSingletonApp\bin\Debug\net8.0-windows\WinformsSingletonApp.exe";
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
```

[![demo][1]][1]


  [1]: https://i.sstatic.net/KnHqSiDG.png