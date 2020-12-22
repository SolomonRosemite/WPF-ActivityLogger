using System.Windows.Controls;
using System.Diagnostics;
using System.Windows;
using System.IO;

using System;
using Newtonsoft.Json;

namespace MyActivityLogs.Pages
{
    public partial class SettingsPage : Page
    {
        private DateTime start;
        private DateTime end;

        public SettingsPage() => InitializeComponent();

        public SettingsPage(DateTime start, DateTime end)
        {
            InitializeComponent();

            this.start = start;
            this.end = end;

            MyDatePickerStart.SelectedDate = start;
            MyDatePickerEnd.SelectedDate = end;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(MainWindow.ActivityLoggerPath + @"\SavedActivities.json"))
            {
                OpenWithDefaultProgram(MainWindow.ActivityLoggerPath);
                return;
            }

            OpenWithDefaultProgram(MainWindow.ActivityLoggerPath + @"\SavedActivities.json");
        }

        private static void OpenWithDefaultProgram(string path)
        {
            Process p = new Process { StartInfo = { FileName = "explorer", Arguments = "\"" + path + "\"" } };
            p.Start();
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            MainWindow.Load();

            MainWindow.mainWindow.DailyButton();
        }

        private void ConvertTime(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowInHours = !MainWindow.ShowInHours;

            if (MainWindow.ShowInHours)
            {
                convertTimeButton.Content = "Convert to Minutes";
            }
            else
            {
                convertTimeButton.Content = "Convert to Hours";
            }

            MainWindow.Load(true);

            MainWindow.mainWindow.DailyButton();
        }

        private void UpdateDates(object sender, RoutedEventArgs e)
        {
            if (start.Date <= end.Date)
            {
                const string dateFormat = "dd.MM.yyyy";

                // Save Updates dates
                string json = JsonConvert.SerializeObject(new[] { start.ToString(dateFormat), end.ToString(dateFormat) }, Formatting.Indented);
                File.WriteAllText(MainWindow.ActivityLoggerPath + @"\MyActivityLogs\Dates.json", json);

                Popup popup = new Popup("Dates have been Updated.");
                popup.Show();

                MainWindow.Load();
            }
            else
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
