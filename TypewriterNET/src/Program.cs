using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

public class Program
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [STAThread]
    static void Main(string[] args)
    {
        bool isNewInstance;
        using (Mutex singleMutex = new Mutex(true, "3549b015-0564-4e97-b519-2a911a927b45", out isNewInstance))
        {
            List<string> files = new List<string>();
            foreach (string arg in args)
            {
                if (!arg.StartsWith("-"))
                {
                    files.Add(arg);
                }
            }
            if (files.Count < args.Length)
            {
                files.Clear();
            }
            if (isNewInstance || files.Count == 0)
            {
                // Start the form with the file name as a parameter
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(args));
            }
            else
            {
                Process current = Process.GetCurrentProcess();
                foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                {
                    if (process.Id != current.Id)
                    {
                        IntPtr ptrWnd = process.MainWindowHandle;
                        SetForegroundWindow(ptrWnd);
                        IntPtr ptrCopyData = IntPtr.Zero;
                        try
                        {
                            string text = string.Join("+", files.ToArray());
                            // Create the data structure and fill with data
                            NativeMethods.COPYDATASTRUCT copyData = new NativeMethods.COPYDATASTRUCT();
                            copyData.dwData = new IntPtr(2);    // Just a number to identify the data type
                            copyData.cbData = text.Length + 1;  // One extra byte for the \0 character
                            copyData.lpData = Marshal.StringToHGlobalAnsi(text);

                            // Allocate memory for the data and copy
                            ptrCopyData = Marshal.AllocCoTaskMem(Marshal.SizeOf(copyData));
                            Marshal.StructureToPtr(copyData, ptrCopyData, false);

                            // Send the message
                            NativeMethods.SendMessage(ptrWnd, NativeMethods.WM_COPYDATA, IntPtr.Zero, ptrCopyData);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString(), "Typewriter.NET", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            // Free the allocated memory after the contol has been returned
                            if (ptrCopyData != IntPtr.Zero)
                                Marshal.FreeCoTaskMem(ptrCopyData);
                        }
                        break;
                    }
                }
            }
        }
    }
}
