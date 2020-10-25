using System.Collections.Generic;
using System.Windows.Controls;
using System;

namespace MyActivityLogs.Pages
{
    public partial class MonthlyPage : Page
    {
        private List<Activity> activities = new List<Activity>();

        public MonthlyPage()
        {
            InitializeComponent();

            LoadMonthly(MainWindow.activitiesDict);
        }

        private void LoadMonthly(Dictionary<string, List<Activity>> dict)
        {
            activities = MainWindow.AddToListForWeekly(-DateTime.Now.Day + 1, dict, activities);

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
