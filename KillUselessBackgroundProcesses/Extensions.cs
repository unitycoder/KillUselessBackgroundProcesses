using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KillUselessBackgroundProcesses
{
    internal static class Extensions
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        public static string GetMainModuleFileName(this Process process, int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;

            string ret = null;

            try
            {
                //ret = QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ? fileNameBuilder.ToString() : null;
                ret = QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ? Path.GetFileName(fileNameBuilder.ToString()) : null;
            }
            catch (Exception)
            {
                // system process?
                ret = "(System)";
            }

            return ret;
        }
    }
}
