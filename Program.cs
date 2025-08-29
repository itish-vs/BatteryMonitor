using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace BatteryMonitor
{
    internal static class Program
    {
        #region Native Methods

        /// <summary>
        /// Native Win32 call to bring a window to the foreground.
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion

        #region Main Entry Point

        /// <summary>
        /// The main entry point for the application.
        /// Handles single instance, DPI awareness, and assembly resolution.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Assembly resolve for loading dependencies from 'data' folder
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var assemblyName = new AssemblyName(args.Name).Name + ".dll";
                var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName);
                if (File.Exists(dataPath))
                    return Assembly.LoadFrom(dataPath);
                return null;
            };

            try
            {
                // Enable high DPI and compatible text rendering
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                bool createdNew;

                // Ensure single instance using a named mutex
                using (Mutex mutex = new Mutex(true, "BatteryMonitorSingleInstanceMutex", out createdNew))
                {
                    if (createdNew)
                    {
                        // Start the main application window
                        ApplicationConfiguration.Initialize();
                        Application.Run(new Monitor());
                    }
                    else
                    {
                        // Bring existing instance to foreground
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
                // Swallow all exceptions to prevent crash on startup
                return;
            }
        }

        #endregion
    }
}