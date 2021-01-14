using System.Collections.Generic;
using System.Windows.Controls;
using System;
using System.Windows;

namespace MyActivityLogs.Pages
{
    public partial class MonthlyPage : Page
    {
        private List<Activity> activities = new List<Activity>();

        public MonthlyPage()
        {
            InitializeComponent();

            LoadMonthly(MainWindow.ActivitiesDict);
        }

        private void LoadMonthly(Dictionary<string, List<Activity>> dict)
        {
            activities = MainWindow.AddToListForWeekly(-DateTime.Now.Day + 1, dict, activities);

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
