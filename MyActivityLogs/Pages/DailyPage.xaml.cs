using System.Collections.Generic;
using System.Windows.Controls;
using System;
using System.ComponentModel;
using System.IO.Packaging;
using System.Windows;

namespace MyActivityLogs.Pages
{
    public partial class DailyPage
    {
        private List<Activity> activities = new List<Activity>();

        public DailyPage()
        {
            InitializeComponent();

            LoadDaily(MainWindow.ActivitiesDict);
        }

        private void LoadDaily(Dictionary<string, List<Activity>> dict)
        {
            string date = MainWindow.DateFormat();
            if (!dict.ContainsKey(date)) { return; }

            activities = MainWindow.AddToListPerDay(DateTime.Now, dict, activities, false, false);

            LoadFinish();
        }

        private void LoadFinish()
        {
            NoActivitiesTextBlock.Visibility = activities.Count == 0 ? Visibility.Visible : Visibility.Hidden;

            activities = MainWindow.SortList(activities);
            activities = MainWindow.CalculateSumOfList(activities);
            activities = MainWindow.SetProgressBarColor(activities);

            if (MainWindow.ShowInHours) { MainWindow.ActivitiesToHours(activities); }
            
            ActivitiesItemsControl.ItemsSource = activities;
        }
    }
}
