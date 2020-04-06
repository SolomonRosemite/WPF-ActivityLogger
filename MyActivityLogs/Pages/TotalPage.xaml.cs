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
