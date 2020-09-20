using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                // access denied
                //ret = QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ? fileNameBuilder.ToString() : null;
                //ret = QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ? Path.GetFileName(fileNameBuilder.ToString()) : null;
                // argument exception
                return GetExecutablePath(process);
            }
            catch (Exception)
            {
                // system process?
                ret = "(System)";
            }

            return ret;
        }


        private static string GetExecutablePath(Process Process)
        {
            //If running on Vista or later use the new function
            if (Environment.OSVersion.Version.Major >= 6)
            {
                return GetExecutablePathAboveVista(Process.Id);
            }

            return Process.MainModule.FileName;
        }

        // https://stackoverflow.com/a/3654195/5452781 and https://www.giorgi.dev/net-framework/how-to-get-elevated-process-path-in-net/
        private static string GetExecutablePathAboveVista(int ProcessId)
        {
            var buffer = new StringBuilder(1024);
            IntPtr hprocess = OpenProcess(ProcessAccessFlags.PROCESS_QUERY_LIMITED_INFORMATION, false, ProcessId);

            if (hprocess != IntPtr.Zero)
            {
                try
                {
                    int size = buffer.Capacity;
                    if (QueryFullProcessImageName(hprocess, 0, buffer, out size))
                    {
                        //Console.WriteLine(buffer.ToString());
                        return buffer.ToString();
                    }
                }
                finally
                {
                    CloseHandle(hprocess);
                }
            }
            //throw new Win32Exception(Marshal.GetLastWin32Error());
            return null;
        }

        [DllImport("kernel32.dll")]
        private static extern bool QueryFullProcessImageName(IntPtr hprocess, int dwFlags,
                       StringBuilder lpExeName, out int size);
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess,
                       bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hHandle);

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            /// <summary>
            /// Required to terminate a process using TerminateProcess.
            /// </summary>
            Terminate = 1 << 0,

            /// <summary>
            /// Required to create a thread.
            /// </summary>
            CreateThread = 1 << 1,

            /// <summary>
            /// Required to perform an operation on the address space of a process.
            /// </summary>
            VirtualMemoryOperation = 1 << 3,

            /// <summary>
            /// Required to read memory in a process using ReadProcessMemory.
            /// </summary>
            VirtualMemoryRead = 1 << 4,

            /// <summary>
            /// Required to write to memory in a process using WriteProcessMemory.
            /// </summary>
            VirtualMemoryWrite = 1 << 5,

            /// <summary>
            /// Required to duplicate a handle using DuplicateHandle.
            /// </summary>
            DuplicateHandle = 1 << 6,

            /// <summary>
            /// Required to create a process.
            /// </summary>
            CreateProcess = 1 << 7,

            /// <summary>
            /// Required to set memory limits using SetProcessWorkingSetSize.
            /// </summary>
            SetQuota = 1 << 8,

            /// <summary>
            /// Required to set certain information about a process, such as its priority class.
            /// </summary>
            SetInformation = 1 << 9,

            /// <summary>
            /// Required to retrieve certain information about a process, such as its token, exit code, and priority class.
            /// </summary>
            QueryInformation = 1 << 10,

            /// <summary>
            /// Required to suspend or resume a process.
            /// </summary>
            SuspendResume = 1 << 11,

            /// <summary>
            /// Required to retrieve certain information about a process.
            /// </summary>
            QueryLimitedInformation = 1 << 12,

            /// <summary>
            /// Required to wait for the process to terminate using the wait functions.
            /// </summary>
            Synchronize = 1 << 20,

            PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,

            /// <summary>
            /// All possible access rights for a process object.
            /// </summary>
            All = 0x001F0FFF
        }

    }


}
