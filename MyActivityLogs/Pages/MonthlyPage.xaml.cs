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
            // might be wrong
            activities = MainWindow.AddToListForWeekly(-DateTime.Now.Day + 1, dict, activities);

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
