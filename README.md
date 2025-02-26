To have your singleton app process new command line arguments, just invoke it exactly the same way (e.g. on the command line or by using `ProcessStart`) without regard for whether the app is running or not. This way you _"handle the second scenario"_ by capturing the "new" command line arguments _before denying the new instance_" and then send those new arguments to the running instance via a named pipe.
___
**Here's what I mean:** If no instance is running we take the command line args and display them in the title bar so we have some way of observing what's happening. This is the result of the `if` block having successfully executed in `Main()`.

`PS >> .\WinformsSingletonApp.exe orig cmd line args`

[![first instance][1]][1]

```
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
                ListenForMessages(); // Running instance is now listening to pipe.
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                int N = 0;
                var displayArgs = string.Join(
                    " ", 
                    Environment.GetCommandLineArgs().Skip(1).Select(_ => $"[{N++}] {_}"));

                mainForm = new MainForm
                {
                    Text = $"Main Form {displayArgs}"
                };
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
```

___

**Subsequent command line invocations**

`PS .\WinformsSingletonApp.exe new args orig instance`

The app is already running, so **this obviously doesn't actually start a new instance**. However, the "new" command line arguments are transmitted to the running instance via a named pipe. This occurs in the `else` block in `Main()`.

```
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
                var displayArgs = string.Join(Environment.NewLine, pm.Select(_ => $"[{N++}] {_}"));
                BeginInvoke(() => MessageBox.Show(this, $"Main Form {displayArgs}"));
                // This helps pop it up when running from CLI
                BeginInvoke(() => TopMost = true);
                BeginInvoke(() => TopMost = false);
            }
        }
    }
}
```
___

[![new args received][2]][2]


  [1]: https://i.sstatic.net/kEJ7KoEb.png
  [2]: https://i.sstatic.net/6HGrxLcB.png