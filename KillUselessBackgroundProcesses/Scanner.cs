using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

                try
                {
                    p.Icon = ImageSourceFromBitmap(Icon.ExtractAssociatedIcon(localProcesses[i].MainModule.FileName).ToBitmap());


                }
                catch (Exception ex)
                {
                    // expected errors if there is no icon or the process is 64-bit
                    if (ex is ArgumentException || ex is Win32Exception)
                    {
                        p.Icon = ImageSourceFromBitmap(Bitmap.FromHicon(SystemIcons.Application.Handle));
                    }
                    else
                    {
                        p.Icon = ImageSourceFromBitmap(Bitmap.FromHicon(SystemIcons.Error.Handle));
                        throw;
                    }
                }

                p.Name = localProcesses[i].ProcessName;
                p.FileName = localProcesses[i].GetMainModuleFileName();

                res.Add(p);
            }

            return res;
        }

        //If you get 'dllimport unknown'-, then add 'using System.Runtime.InteropServices;'
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

    }
}
