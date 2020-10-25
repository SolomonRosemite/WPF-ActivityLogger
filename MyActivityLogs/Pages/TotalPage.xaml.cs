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
                MainWindow.AddToListPerDay(DateTime.ParseExact(entry.Key, "dd.MM.yyyy", null), dict, activities, true);

            LoadFinish();
        }

        private void LoadFinish()
        {
            activities = MainWindow.SortList(activities);
            activities = MainWindow.CalculateSumOfList(activities);
            activities = MainWindow.SetProgressBarColor(activities);

            if (MainWindow.ShowInHours) { MainWindow.ActivitiesToHours(activities); }
            
            ActivitiesItemsControl.ItemsSource = activities;
        }
    }
}
