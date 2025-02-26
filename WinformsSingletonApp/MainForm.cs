
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
                MessageBox.Show(string.Join(Environment.NewLine, pm.Select(_ => $"[{N++}] {_}")));
            }
        }
    }
}
