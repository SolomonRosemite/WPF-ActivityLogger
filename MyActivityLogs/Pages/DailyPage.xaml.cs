using System.Collections.Generic;
using System.Windows.Controls;
using System;
using System.IO.Packaging;

namespace MyActivityLogs.Pages
{
    public partial class DailyPage : Page
    {
        private List<Activity> activities = new List<Activity>();

        public DailyPage()
        {
            InitializeComponent();

            LoadDaily(MainWindow.activitiesDict);
        }

        private void LoadDaily(Dictionary<string, List<Activity>> dict)
        {
            string date = MainWindow.DateFormat();
            if (!dict.ContainsKey(date))
            {
                return;
            }

            activities = MainWindow.AddToListPerDay(DateTime.Now, dict, activities, false);

            LoadFinish();
        }

        private void LoadFinish()
        {
            activities = MainWindow.SortList(activities);
            activities = MainWindow.CalculateSumOfList(activities);
            activities = MainWindow.SetProgressBarColor(activities);


            ActivitiesItemsControl.ItemsSource = activities;
        }
    }
}
