using System.Windows.Controls;
using System.Diagnostics;
using System.Windows;
using System.IO;

namespace MyActivityLogs.Pages
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(MainWindow.ActivityLoggerPath + @"\SavedActivities.json"))
            {
                Process.Start("explorer.exe", MainWindow.ActivityLoggerPath);
                return;
            }
            Process.Start("notepad.exe", MainWindow.ActivityLoggerPath + @"\SavedActivities.json");
        }

        public void Refresh(object sender, RoutedEventArgs e)
        {
            MainWindow.Load();

            Dispatcher.Invoke(() =>
            {
                MainWindow.mainWindow.DailyButton();
            });
        }
    }
}
