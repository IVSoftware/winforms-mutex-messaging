using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;
using System.Windows.Forms;

namespace WinformsSingletonApp
{
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
}
