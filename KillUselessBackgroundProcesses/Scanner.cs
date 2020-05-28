using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillUselessBackgroundProcesses
{
    public static class Scanner
    {
        // creates list of unwanted processes (based on scanning list)
        public static List<ProcessData> Scan()
        {
            // Get all processes running on the local computer.
            var localProcesses = Process.GetProcesses();

            Array.Sort<Process>(localProcesses, (a, b) => a.ProcessName.CompareTo(b.ProcessName));

            var res = new List<ProcessData>();

            for (int i = 0; i < localProcesses.Length; i++)
            {
                Console.WriteLine(localProcesses[i].ProcessName + " " + localProcesses[i].GetMainModuleFileName());

                var p = new ProcessData();
                p.process = localProcesses[i];

                p.Name = localProcesses[i].ProcessName;
                p.FileName = localProcesses[i].GetMainModuleFileName();

                res.Add(p);
            }

            return res;
        }
    }
}
