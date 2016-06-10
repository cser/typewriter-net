using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using SingleInstance;

public class Program
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
       /*bool createdNew = true;
       using (Mutex mutex = new Mutex(true, "3549b015-0564-4e97-b519-2a911a927b45", out createdNew))
       {
          if (createdNew)
          {
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
                   SetForegroundWindow(process.MainWindowHandle);
                   break;
                }
             }
          }
       }*/
        if (SingleInstanceApplication.AlreadyExists)
        {
            if (!SingleInstanceApplication.NotifyExistingInstance(args))
            {
                Run(args);
            }
        }
        else
        {
            Run(args);
        }
    }

    static void Run(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        MainForm mainForm = new MainForm(args);
        SingleInstanceApplication.NewInstanceMessage += delegate(object sender, object message)
        {
            mainForm.ProcessParameters(message as string[]);
        };
        try
        {
            SingleInstanceApplication.Initialize();
            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Running error");
        }
        finally
        {
            MessageBox.Show("Close");
            SingleInstanceApplication.Close();
        }
    }
}
