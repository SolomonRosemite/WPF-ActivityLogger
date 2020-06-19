using System;
using System.Runtime.InteropServices;
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
    public partial class CustomPage : Page
    {
        private List<Activity> activities = new List<Activity>();

        private DateTime start; 
        private DateTime end;

        public CustomPage()
        {
            InitializeComponent();
            LoadCustom(MainWindow.activitiesDict);
        }

        private void LoadCustom(Dictionary<string, List<Activity>> dict)
        {
            if (end.Year == 0001)
                end = DateTime.Now;

            activities = MainWindow.AddToListForCustom(start, end, dict, activities);

            LoadFinish();
        }

        private void LoadFinish()
        {
            activities = MainWindow.SortList(activities);
            activities = MainWindow.CalculateSumOfList(activities);
            activities = MainWindow.SetProgressBarColor(activities);

            ActivitiesItemsControl.ItemsSource = activities;

            string startingDate = start.Date.ToString();
            string endingDate = end.Date.ToString();
            CustomTitle.Content = $"From {startingDate.Remove(startingDate.Length - 8)} to {endingDate.Remove(endingDate.Length - 8)}";
        }

        public void UpdateDates(DateTime start, DateTime end)
        {
            this.start = start;
            this.end = end;

            activities = new List<Activity>();
            LoadCustom(MainWindow.activitiesDict);
        }
    }
}
