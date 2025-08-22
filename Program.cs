using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace BatteryMonitor
{
    internal static class Program
    {
        // Native Win32 call to bring a window to the foreground
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var assemblyName = new System.Reflection.AssemblyName(args.Name).Name + ".dll";
                var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName);
                if (File.Exists(dataPath))
                    return System.Reflection.Assembly.LoadFrom(dataPath);
                return null;
            };

            try
            {

               // AppDomain.CurrentDomain.AssemblyResolve += ResolveFromDataFolder;
                // Ensure the application runs with high DPI awareness
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                bool createdNew;

                // Name must be unique to your app — it prevents multiple instances
                using (Mutex mutex = new Mutex(true, "BatteryMonitorSingleInstanceMutex", out createdNew))
                {
                    if (createdNew)
                    {
                        // Usual WinForms boot
                        ApplicationConfiguration.Initialize();
                        Application.Run(new Monitor());
                    }
                    else
                    {
                        // Find any other instance with same name, different process ID
                        Process current = Process.GetCurrentProcess();
                        foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                        {
                            if (process.Id != current.Id)
                            {
                                IntPtr h = process.MainWindowHandle;
                                if (h != IntPtr.Zero)
                                {
                                    SetForegroundWindow(h);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
