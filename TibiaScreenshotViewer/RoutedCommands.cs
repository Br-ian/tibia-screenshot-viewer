using System.Windows.Input;

namespace TibiaScreenshotViewer
{
    static class RoutedCommands
    {
        public static RoutedCommand Exit = new RoutedCommand("Exit", typeof(MainWindow), new InputGestureCollection { new KeyGesture(Key.Q, ModifierKeys.Control) });
    }
}
