using Microsoft.Win32;

namespace TibiaScreenshotViewer
{
    internal static class TibiaUtils
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string KeyBase = @"Software\Microsoft\Windows\CurrentVersion\App Paths";

        public static string GetPathForExe(string fileName)
        {
            var key = $@"{KeyBase}\{fileName}";

            Log.Info($"Opening registry subkey {key}");
            var fileKey = Registry.CurrentUser.OpenSubKey(key);
            
            if (fileKey == null)
            {
                Log.Warn($"Registry subkey {key} not found");
                return null;
            }

            var result = (string) fileKey.GetValue(string.Empty);
            fileKey.Close();


            return result;
        }

        public static string GetTibiaPath()
        {
            return GetPathForExe("Tibia.exe").Replace(@"\Tibia.exe", "");
        }
            
        public static string GetScreenshotsPath(string tibiaPath)
        {
            return $@"{tibiaPath}\packages\Tibia\screenshots";
        }
    }
}
