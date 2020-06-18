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
    public partial class CustomPage : Page
    {
        private List<Activity> activities = new List<Activity>();

        private DateTime start = DateTime.Parse("01.02.2020"); // TODO
        private DateTime end = DateTime.Parse("19.06.2020");

        public CustomPage()
        {
            InitializeComponent();

            LoadCustom(MainWindow.activitiesDict, start, end);
        }

        private void LoadCustom(Dictionary<string, List<Activity>> dict, DateTime start, DateTime end)
        {
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
    }
}
