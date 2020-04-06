using System.Collections.Generic;
using System.Windows.Controls;
using System;

namespace MyActivityLogs.Pages
{
    /// <summary>
    /// Interaction logic for DailyPage.xaml
    /// </summary>
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
            if (!dict.ContainsKey(MainWindow.DateFormat()))
            {
                //ErrorMessage("IDK no logs for today yet");
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
