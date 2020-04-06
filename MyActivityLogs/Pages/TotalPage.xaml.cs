using System.Collections.Generic;
using System.Windows.Controls;
using System;

namespace MyActivityLogs.Pages
{
    public partial class TotalPage : Page
    {
        private List<Activity> activities = new List<Activity>();

        public TotalPage()
        {
            InitializeComponent();

            LoadTotal(MainWindow.activitiesDict);
        }

        private void LoadTotal(Dictionary<string, List<Activity>> dict)
        {
            foreach (KeyValuePair<string, List<Activity>> entry in dict)
            {
                MainWindow.AddToListPerDay(DateTime.Parse(entry.Key), dict, activities, true, true);
            }

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
