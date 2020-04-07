using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Windows;
using System.Linq;
using System.IO;
using System;

using MyActivityLogs.Pages;

namespace MyActivityLogs
{
    public partial class MainWindow : Window
    {
        public static Dictionary<string, List<Activity>> activitiesDict = new Dictionary<string, List<Activity>>();
        public static readonly string ActivityLoggerPath = GetDirectory() + @"\TMRosemite\ActivityLogger";

        private static dynamic[] pages = new dynamic[5];
        private DoubleAnimation animation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(700)));

        private enum CurrentPage
        {
            Daily,
            Weekly,
            Monthly,
            Total,

            Settings
        }

        public MainWindow()
        {
            InitializeComponent();

            Load();

            SetPage(CurrentPage.Daily);
            MyFrame.Content = pages[0];

            AnimationRectangle.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(1400))));
        }

        public static void Load()
        {
            var output = ReadJson();

            // Checking if json could be read
            if (output is int)
            {
                if (output == 0)
                {
                    ShowPopUp("Json could not be found.\nTry Starting the ActivityLogger first.\nTip: Keep in mind the Program" +
                    "\nwill only show programs\n" +
                    "with more than 5 min of use.", GetDirectory() + @"\TMRosemite\ActivityLogger");
                    return;
                }
                else if (output == 1)
                {
                    ShowPopUp("The Json seems to be corrupted.\nYou can Remove the current Json the Solve this issue", GetDirectory() + @"\TMRosemite\ActivityLogger");
                    return;
                }
            }

            if (output.Count == 0)
            {
                ShowPopUp("no logs found.\n\nTip: Keep in mind the Program\nwill only show programs\n" +
                    "with more than 5 min of use.");
                return;
            }

            activitiesDict = output;

            pages[0] = new DailyPage();
            pages[1] = new WeeklyPage();
            pages[2] = new MonthlyPage();
            pages[3] = new TotalPage();
            pages[4] = new SettingsPage();
        }

        private static void ShowPopUp(string message, string path = "")
        {
            Popup popup = new Popup(message, path);
            popup.Show();
        }

        private static dynamic ReadJson()
        {
            if (!File.Exists(ActivityLoggerPath + @"\SavedActivities.json"))
            {
                return 0;
            }

            string jsonFromFile;
            using (var reader = new StreamReader(ActivityLoggerPath + @"\SavedActivities.json"))
            {
                jsonFromFile = reader.ReadToEnd();
                if (!jsonFromFile.Contains("{"))
                {
                    return 1;
                }
            }

            try { return JsonConvert.DeserializeObject<Dictionary<string, List<Activity>>>(jsonFromFile); }
            catch { return 1; }
        }
        private void SetPage(CurrentPage currentPage)
        {
            switch (currentPage)
            {
                case CurrentPage.Daily:
                    DailySelected.Visibility = Visibility.Visible;
                    WeeklySelected.Visibility = Visibility.Hidden;
                    MonthlySelected.Visibility = Visibility.Hidden;
                    TotalSelected.Visibility = Visibility.Hidden;
                    SettingsSelected.Visibility = Visibility.Hidden;
                    break;
                case CurrentPage.Weekly:
                    DailySelected.Visibility = Visibility.Hidden;
                    WeeklySelected.Visibility = Visibility.Visible;
                    MonthlySelected.Visibility = Visibility.Hidden;
                    TotalSelected.Visibility = Visibility.Hidden;
                    SettingsSelected.Visibility = Visibility.Hidden;
                    break;
                case CurrentPage.Monthly:
                    DailySelected.Visibility = Visibility.Hidden;
                    WeeklySelected.Visibility = Visibility.Hidden;
                    MonthlySelected.Visibility = Visibility.Visible;
                    TotalSelected.Visibility = Visibility.Hidden;
                    SettingsSelected.Visibility = Visibility.Hidden;
                    break;
                case CurrentPage.Total:
                    DailySelected.Visibility = Visibility.Hidden;
                    WeeklySelected.Visibility = Visibility.Hidden;
                    MonthlySelected.Visibility = Visibility.Hidden;
                    TotalSelected.Visibility = Visibility.Visible;
                    SettingsSelected.Visibility = Visibility.Hidden;
                    break;
                case CurrentPage.Settings:
                    DailySelected.Visibility = Visibility.Hidden;
                    WeeklySelected.Visibility = Visibility.Hidden;
                    MonthlySelected.Visibility = Visibility.Hidden;
                    TotalSelected.Visibility = Visibility.Hidden;
                    SettingsSelected.Visibility = Visibility.Visible;
                    break;
            }
        }
        private void PlayAnimation()
        {
            AnimationRectangle.BeginAnimation(OpacityProperty, animation);
        }

        public static List<Activity> AddToListForWeekly(int daysBehind, Dictionary<string, List<Activity>> dict, List<Activity> list)
        {
            DateTime date = DateTime.Now;

            // If Today would be monday there would be no daysBehind because the week just started
            if (daysBehind == 0)
            {
                return AddToListPerDay(date, dict, list, false);
            }

            while (daysBehind != 0)
            {
                // If on that date nothing was logged we skip that date
                if (!dict.ContainsKey(DateFormat(date.AddDays(daysBehind))))
                {
                    daysBehind++;
                    continue;
                }

                list = AddToListPerDay(date.AddDays(daysBehind), dict, list, true);
                daysBehind++;
            }

            return AddToListPerDay(date.AddDays(daysBehind), dict, list, true);
        }
        public static List<Activity> AddToListPerDay(DateTime date, Dictionary<string, List<Activity>> dict, List<Activity> list, bool checkIfEntryAlreadyExists)
        {
            if (checkIfEntryAlreadyExists == false)
            {
                foreach (Activity item in dict[DateFormat(date)])
                {
                    if (int.Parse(item.TimeSpent.Remove(item.TimeSpent.Length - 7)) < 5)
                    {
                        continue;
                    }

                    list.Add(new Activity()
                    {
                        ActivityName = item.ActivityName,
                        TimeSpent = item.TimeSpent,
                        TimeSpentint = int.Parse(item.TimeSpent.Remove(item.TimeSpent.Length - 7))
                    });
                }
                return list;
            }

            List<Activity> myActivities = dict[DateFormat(date)];
            bool skipped = false;

            for (int i = 0; i < myActivities.Count; i++)
            {
                int temp;
                if ((temp = int.Parse(myActivities[i].TimeSpent.Remove(myActivities[i].TimeSpent.Length - 7))) < 4)
                {
                    continue;
                }

                // If that ActivityName already exists we dont add and just add the minutes
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].ActivityName == myActivities[i].ActivityName)
                    {

                        list[j].TimeSpent = (list[j].TimeSpentint + temp).ToString() + " Minutes";
                        list[j].TimeSpentint += temp;
                        skipped = true;
                        break;
                    }
                }

                if (skipped == true)
                {
                    skipped = false;
                    continue;
                }

                list.Add(new Activity()
                {
                    ActivityName = myActivities[i].ActivityName,
                    TimeSpent = myActivities[i].TimeSpent,
                    TimeSpentint = int.Parse(myActivities[i].TimeSpent.Remove(myActivities[i].TimeSpent.Length - 7))
                });
            }
            return list;
        }
        public static List<Activity> SortList(List<Activity> list)
        {
            return list.OrderBy(o => o.TimeSpentint).ToList();
        }
        public static List<Activity> CalculateSumOfList(List<Activity> list)
        {
            int sum = 0;
            int biggestnumber = 0;
            foreach (Activity item in list)
            {
                sum += item.TimeSpentint;

                if (item.TimeSpentint > biggestnumber)
                {
                    biggestnumber = item.TimeSpentint;
                }
            }

            if (biggestnumber > sum / 2)
            {
                foreach (Activity item in list)
                {
                    item.ProgressBarMaxValue = biggestnumber + biggestnumber / 10;
                }
                return list;
            }

            foreach (Activity item in list)
            {
                item.ProgressBarMaxValue = sum / 2;
            }
            return list;
        }
        public static List<Activity> SetProgressBarColor(List<Activity> list)
        {
            float baseindex = list.Count * 10;
            float indexItem = baseindex;
            float subtractValue = baseindex / 5f;

            list.Reverse();
            foreach (Activity item in list)
            {
                if (indexItem > subtractValue * 4 & subtractValue * 5 >= indexItem)
                {
                    item.Color = "Green";
                }
                else if (indexItem > subtractValue * 3 & subtractValue * 4 >= indexItem)
                {
                    item.Color = "LimeGreen";
                }
                else if (indexItem > subtractValue * 2 & subtractValue * 3 >= indexItem)
                {
                    item.Color = "SeaGreen";
                }
                else if (indexItem > subtractValue * 1 & subtractValue * 2 >= indexItem)
                {
                    item.Color = "DarkSeaGreen";
                }
                else
                {
                    item.Color = "LightGreen";
                }
                indexItem -= subtractValue;
            }
            return list;
        }
       
        public static string DateFormat([Optional] DateTime date)
        {
            if (date.ToString() != "01.01.0001 00:00:00")
            {
                return date.ToString("dd.MM.yyyy");
            }
            return DateTime.Now.ToString("dd.MM.yyyy");
        }
        private static string GetDirectory()
        {
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            if (Environment.OSVersion.Version.Major >= 6)
            {
                return Directory.GetParent(path).ToString();
            }
            return "";
        }

        private void DailyButton(object sender, RoutedEventArgs e)
        {
            PlayAnimation();
            SetPage(CurrentPage.Daily);
            MyFrame.Content = pages[0];
        }
        private void WeeklyButton(object sender, RoutedEventArgs e)
        {
            PlayAnimation();
            SetPage(CurrentPage.Weekly);
            MyFrame.Content = pages[1];
        }
        private void MonthlyButton(object sender, RoutedEventArgs e)
        {
            PlayAnimation();
            SetPage(CurrentPage.Monthly);
            MyFrame.Content = pages[2];
        }
        private void TotalButton(object sender, RoutedEventArgs e)
        {
            PlayAnimation();
            SetPage(CurrentPage.Total);
            MyFrame.Content = pages[3];
        }
        private void SettingsButton(object sender, RoutedEventArgs e)
        {
            PlayAnimation();
            SetPage(CurrentPage.Settings);
            MyFrame.Content = pages[4];
        }
    }

    public class Activity
    {
        public string ActivityName { get; set; }
        public string TimeSpent { get; set; }
        public int TimeSpentint { get; set; }
        public string Color { get; set; }
        public float ProgressBarMaxValue { get; set; }
    }
}
