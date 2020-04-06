using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyActivityLogs;

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
