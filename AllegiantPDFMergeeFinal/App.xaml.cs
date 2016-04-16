using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace AllegiantPDFMerger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Get Reference to the current Process
            Process thisProc = Process.GetCurrentProcess();
            // Check how many total processes have the same name as the current one
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                // If ther is more than one, than it is already running.
                MessageBox.Show("Application is already running.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            //RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            //if (rkApp.GetValue("Allegiant PDF Merger") == null)
            //{
            //    rkApp.SetValue("Allegiant PDF Merger", Application);
            //}

            //Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            //if (key.GetValue("Allegiant PDF Merger") == null)
            //{
            //    Assembly curAssembly = Assembly.GetExecutingAssembly();
            //    key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            //}

            //AllegiantPDFMerger.MainWindow window = new MainWindow();
            //window.Show();

            base.OnStartup(e);
        }
    }
}
