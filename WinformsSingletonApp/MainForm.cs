
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.CodeDom;

namespace WinformsSingletonApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public void OnPipeMessage(string json)
        {
            int N = 0;
            if(JsonConvert.DeserializeObject<JArray>(json) is { } pm)
            {
                var displayArgs = string.Join(Environment.NewLine, pm.Select(_ => $"[{N++}] {_}"));
                BeginInvoke(() => MessageBox.Show(this, $"Main Form {displayArgs}"));
                BeginInvoke(() => TopMost = true);
                BeginInvoke(() => TopMost = false);
            }
        }
    }
}
