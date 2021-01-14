using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Windows;

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

            LoadCustom(MainWindow.ActivitiesDict);
        }
        public CustomPage(DateTime start, DateTime end)
        {
            InitializeComponent();

            this.start = start;
            this.end = end;

            LoadCustom(MainWindow.ActivitiesDict);
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
            NoActivitiesTextBlock.Visibility = activities.Count == 0 ? Visibility.Visible : Visibility.Hidden;

            activities = MainWindow.SortList(activities);
            activities = MainWindow.CalculateSumOfList(activities);
            activities = MainWindow.SetProgressBarColor(activities);

            if (MainWindow.ShowInHours) { MainWindow.ActivitiesToHours(activities); }            
            
            ActivitiesItemsControl.ItemsSource = activities;

            string startingDate = start.Date.ToString("dd.MM.yyyy");
            string endingDate = end.Date.ToString("dd.MM.yyyy");
            CustomTitle.Content = $"From {startingDate} to {endingDate}";
        }
    }
}
