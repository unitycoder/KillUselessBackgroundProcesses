using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KillUselessBackgroundProcesses
{

    public class ProcessData
    {
        public Process process;
        public ImageSource Icon { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
    }
}