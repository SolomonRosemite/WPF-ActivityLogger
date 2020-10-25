using System.Collections.Generic;
using System.Windows.Controls;
using System;

namespace MyActivityLogs.Pages
{
    public partial class WeeklyPage : Page
    {
        private List<Activity> activities = new List<Activity>();

        public WeeklyPage()
        {
            InitializeComponent();

            LoadWeekly(MainWindow.activitiesDict);
        }

        private void LoadWeekly(Dictionary<string, List<Activity>> dict)
        {
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    // Only Today
                    activities = MainWindow.AddToListForWeekly(0, dict, activities);
                    break;
                case DayOfWeek.Tuesday:
                    // Today and yesterday and so on...
                    activities = MainWindow.AddToListForWeekly(-1, dict, activities);
                    break;
                case DayOfWeek.Wednesday:
                    activities = MainWindow.AddToListForWeekly(-2, dict, activities);
                    break;
                case DayOfWeek.Thursday:
                    activities = MainWindow.AddToListForWeekly(-3, dict, activities);
                    break;
                case DayOfWeek.Friday:
                    activities = MainWindow.AddToListForWeekly(-4, dict, activities);
                    break;
                case DayOfWeek.Saturday:
                    activities= MainWindow.AddToListForWeekly(-5, dict, activities);
                    break;
                case DayOfWeek.Sunday:
                    activities = MainWindow.AddToListForWeekly(-6, dict, activities);
                    break;
            }

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
