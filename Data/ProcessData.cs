using Newtonsoft.Json;
using System.Diagnostics;
using System.Windows.Media;

namespace KillUselessBackgroundProcesses
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ProcessData
    {
        public Process process;
        public bool Selected { get; set; }
        public ImageSource Icon { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string FileName { get; set; }
    }
}