using System.Collections.Generic;
using System.Windows.Controls;
using System;
using System.Windows;

namespace MyActivityLogs.Pages
{
    public partial class Yesterday : Page
    {
        private List<Activity> activities = new List<Activity>();
        public Yesterday()
        {
            InitializeComponent();

            LoadYesterday(MainWindow.ActivitiesDict);
        }

        private void LoadYesterday(Dictionary<string, List<Activity>> dict)
        {
            activities = MainWindow.AddToListPerDay(DateTime.Now.AddDays(-1), dict, activities, false);
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
