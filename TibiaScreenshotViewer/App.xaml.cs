using System;
using System.Configuration;
using System.Linq;
using System.Windows;
using log4net.Util;
using TibiaScreenshotViewer.Properties;


namespace TibiaScreenshotViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public App()
        {
            Log.Debug("Starting TibiaScreenshotViewer");

            var errors = log4net.LogManager.GetRepository().ConfigurationMessages.Cast<LogLog>();
            foreach (var error in errors)
            {
                Console.WriteLine($@"Error in log4net config: {error}");
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            foreach (SettingsProperty property in Settings.Default.Properties)
            {
                var value = Settings.Default[property.Name];

                Log.Debug($"Setting {property.Name} = {value} (Default: {property.DefaultValue})");
            }
        }

        private static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Fatal($"App Dispatcher Unhandled Exception: {e.Exception.Message}", e.Exception);

            MessageBox.Show($"App Dispatcher Unhandled Exception: {e.Exception.Message}\r\n\r\n{e.Exception.StackTrace}", "App Dispatcher Unhandled Exception");

            e.Handled = true;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;

            Log.Fatal($"CurrentDomain Unhandled Exception: {exception.Message}", exception);

            MessageBox.Show($"CurrentDomain Unhandled Exception: {exception.Message}\r\n\r\n{exception.StackTrace}", "CurrentDomain Unhandled Exception");
        }
    }
}
