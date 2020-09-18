using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KillUselessBackgroundProcesses
{
    public static class Scanner
    {

        // https://stackoverflow.com/a/14053737/5452781
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        static extern uint GetClassLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        static extern IntPtr GetClassLong64(IntPtr hWnd, int nIndex);

        /// <summary>
        /// 64 bit version maybe loses significant 64-bit specific information
        /// </summary>
        static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
                return new IntPtr((long)GetClassLong32(hWnd, nIndex));
            else
                return GetClassLong64(hWnd, nIndex);
        }

        static readonly uint WM_GETICON = 0x007f;
        static readonly IntPtr ICON_SMALL2 = new IntPtr(2);
        static readonly IntPtr IDI_APPLICATION = new IntPtr(0x7F00);
        static readonly int GCL_HICON = -14;

        public static Image GetSmallWindowIcon(IntPtr hWnd)
        {
            try
            {
                IntPtr hIcon = default(IntPtr);

                hIcon = SendMessage(hWnd, WM_GETICON, ICON_SMALL2, IntPtr.Zero);

                if (hIcon == IntPtr.Zero)
                    hIcon = GetClassLongPtr(hWnd, GCL_HICON);

                if (hIcon == IntPtr.Zero)
                    hIcon = LoadIcon(IntPtr.Zero, (IntPtr)0x7F00/*IDI_APPLICATION*/);

                if (hIcon != IntPtr.Zero)
                    return new Bitmap(Icon.FromHandle(hIcon).ToBitmap(), 16, 16);
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // creates list of unwanted processes (based on scanning list)
        public static List<ProcessData> Scan()
        {
            // Get all processes running on the local computer.
            var localProcesses = Process.GetProcesses();

            Array.Sort<Process>(localProcesses, (a, b) => a.ProcessName.CompareTo(b.ProcessName));

            var res = new List<ProcessData>();

            for (int i = 0; i < localProcesses.Length; i++)
            {
                //Console.WriteLine(localProcesses[i].ProcessName + " " + localProcesses[i].GetMainModuleFileName());

                var p = new ProcessData();
                p.process = localProcesses[i];

                try
                {
                    //Console.WriteLine(p.process);
                    var procname = p.process.GetMainModuleFileName();

                    //Console.WriteLine("name: "+procname);
                    if (procname != null)
                    {
                        p.Icon = ImageSourceFromBitmap(Icon.ExtractAssociatedIcon(procname).ToBitmap());
                    }
                    else
                    {
                        p.Icon = ImageSourceFromBitmap(Bitmap.FromHicon(SystemIcons.Error.Handle));

                    }
                    //p.Icon = ImageSourceFromBitmap(Icon.ExtractAssociatedIcon(localProcesses[i].MainModule.FileName).ToBitmap());
                    //var img = GetSmallWindowIcon(p.process.Handle);//.ToBitmap());
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
                        //throw;
                    }
                }


                p.Name = localProcesses[i].ProcessName;
                p.FileName = localProcesses[i].GetMainModuleFileName();

                res.Add(p);
            }

            return res;
        }

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
