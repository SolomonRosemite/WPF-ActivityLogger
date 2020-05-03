using System.Collections.Generic;
using System.Windows.Controls;
using System;

namespace MyActivityLogs.Pages
{
    public partial class Yesterday : Page
    {
        private List<Activity> activities = new List<Activity>();
        public Yesterday()
        {
            InitializeComponent();

            LoadYesterday(MainWindow.activitiesDict);
        }

        private void LoadYesterday(Dictionary<string, List<Activity>> dict)
        {
            activities = MainWindow.AddToListPerDay(DateTime.Now.AddDays(-1), dict, activities, false);
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
