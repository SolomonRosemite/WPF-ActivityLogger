using System.Windows.Controls;
using System.Diagnostics;
using System.Windows;
using System.IO;
using System;
using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;

namespace MyActivityLogs.Pages
{
    public partial class SettingsPage : Page
    {
        DateTime start;
        DateTime end;
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

            MainWindow.mainWindow.DailyButton();
        }

        public void UpdateDates(object sender, RoutedEventArgs e)
        {
            if (start.Date <= end.Date)
            {
                MainWindow.mainWindow.UpdateCustomDates(start, end);
                Popup popup = new Popup("Dates have been Updated.");
                popup.Show();
            } else
            {
                Popup popup = new Popup("The Starting date can't be after the Ending date.");
                popup.Show();
            }
        }

        private void MyDatePickerStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            start = (DateTime)e.AddedItems[0];
        }

        private void MyDatePickerEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            end = (DateTime)e.AddedItems[0];
        }
    }
}
