using System.Windows.Controls.Primitives;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Windows;
using System.Timers;
using System.Linq;
using System.IO;
using System;

using MyActivityLogs.Pages;
using MaterialDesignThemes.Wpf;
using System.Globalization;

namespace MyActivityLogs
{
    public partial class MainWindow : Window
    {
        public static MainWindow mainWindow;

        public static Dictionary<string, List<Activity>> ActivitiesDict = new Dictionary<string, List<Activity>>();
        public static readonly string ActivityLoggerPath = GetDirectory() + @"\TMRosemite\ActivityLogger";

        private static readonly dynamic[] Pages = new dynamic[7];
        private static readonly DoubleAnimation Animation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(700)));

        public static bool ShowInHours = false;

        // private static DateTime[] savedDates = new DateTime[2] {new DateTime(), new DateTime()};
        private static DateTime[] savedDates = new DateTime[2];

        private enum CurrentPage
        {
            Daily,
            Yesterday,
            Weekly,
            Monthly,
            Total,
            Custom,

            Settings
        }

        public MainWindow()
        {
            mainWindow = this;

            InitializeComponent();

            Load();

            SetPage(CurrentPage.Daily);
            MyFrame.Content = Pages[0];

            AnimationRectangle.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(1400))));

            RefreshTimer();
        }

        public static void Load(bool isConvertingTime = false)
        {
            if (isConvertingTime && savedDates != null)
            {
                Pages[0] = new DailyPage();
                Pages[1] = new WeeklyPage();
                Pages[2] = new MonthlyPage();
                Pages[3] = new TotalPage();

                Pages[5] = new Yesterday();
                Pages[6] = savedDates == null ? new CustomPage() : new CustomPage(savedDates[0], savedDates[1]);
                return;
            }

            var output = ReadJson();
            DateTime[] dates = GetSavedDates();

            savedDates = dates;

            // Checking if json could be read
            if (output is int)
            {
                if (output == 0)
                {
                    // ShowPopUp("Json could not be found.\nTry Starting the ActivityLogger first.\nTip: Keep in mind the Program" +
                    // "\nwill only show programs\n" +
                    // "with more than 5 min of use.", GetDirectory() + @"\TMRosemite\ActivityLogger");

                    Assign(dates, new Dictionary<string, List<Activity>>());
                    return;
                }

                if (output == 1)
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


            Assign(dates, output);
        }

        private static void Assign(DateTime[] dates, Dictionary<string,List<Activity>> output)
        {
            ActivitiesDict = output;

            Pages[0] = new DailyPage();
            Pages[1] = new WeeklyPage();
            Pages[2] = new MonthlyPage();
            Pages[3] = new TotalPage();

            Pages[4] = dates == null ? new SettingsPage() : new SettingsPage(dates[0], dates[1]);
            Pages[5] = new Yesterday();
            Pages[6] = dates == null ? new CustomPage() : new CustomPage(dates[0], dates[1]);
        }

        private void RefreshTimer()
        {
            Timer timer = new Timer(1000 * 300); // 5 Min
            timer.AutoReset = true;
            timer.Elapsed += timer_elapsed;
            timer.Start();
        }
        private void timer_elapsed(object y, EventArgs x)
        {
            Dispatcher?.Invoke(() =>
            {
                Pages[4].RefreshButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                SetPage(CurrentPage.Daily);
                MyFrame.Content = Pages[0];
            });
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
        private static DateTime[] GetSavedDates()
        {
            DateTime[] defaultDates = new[] { DateTime.ParseExact("01.01.2021", "dd.MM.yyyy", null), DateTime.ParseExact("01.01.2022", "dd.MM.yyyy", null) };

            if (!File.Exists(ActivityLoggerPath + @"\MyActivityLogs\Dates.json"))
            {
                return defaultDates;
            }

            string jsonFromFile;
            using (var reader = new StreamReader(ActivityLoggerPath + @"\MyActivityLogs\Dates.json"))
            {
                jsonFromFile = reader.ReadToEnd();
                if (!jsonFromFile.Contains("[")) { return null; }
            }

            try
            {
                var dates = JsonConvert.DeserializeObject<List<string>>(jsonFromFile);
                return new[] { DateTime.ParseExact(dates[0], "dd.MM.yyyy", null), DateTime.ParseExact(dates[1], "dd.MM.yyyy", null) };
            }
            catch { return defaultDates; }
        }
        private static void ShowPopUp(string message, string path = "")
        {
            Popup popup = new Popup(message, path);
            popup.Show();
        }
        private void SetPage(CurrentPage currentPage)
        {
            // Hide Every Rectangle
            DailySelected.Visibility = Visibility.Hidden;
            WeeklySelected.Visibility = Visibility.Hidden;
            MonthlySelected.Visibility = Visibility.Hidden;
            TotalSelected.Visibility = Visibility.Hidden;
            YesterdaySelected.Visibility = Visibility.Hidden;
            SettingsSelected.Visibility = Visibility.Hidden;
            CustomSelected.Visibility = Visibility.Hidden;

            switch (currentPage)
            {
                case CurrentPage.Daily:
                    DailySelected.Visibility = Visibility.Visible;
                    break;
                case CurrentPage.Weekly:
                    WeeklySelected.Visibility = Visibility.Visible;
                    break;
                case CurrentPage.Monthly:
                    MonthlySelected.Visibility = Visibility.Visible;
                    break;
                case CurrentPage.Total:
                    TotalSelected.Visibility = Visibility.Visible;
                    break;
                case CurrentPage.Settings:
                    SettingsSelected.Visibility = Visibility.Visible;
                    break;
                case CurrentPage.Yesterday:
                    YesterdaySelected.Visibility = Visibility.Visible;
                    break;
                case CurrentPage.Custom:
                    CustomSelected.Visibility = Visibility.Visible;
                    break;
            }
        }
        private void PlayAnimation() => AnimationRectangle.BeginAnimation(OpacityProperty, Animation);

        public static List<Activity> ActivitiesToHours(List<Activity> activities)
        {
            foreach (var t in activities)
                t.TimeSpent = ((float)t.TimeSpentint / 60).ToString("0.0") + " Hours";

            return activities;
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
        public static List<Activity> AddToListForCustom(DateTime start, DateTime end, Dictionary<string, List<Activity>> dict, List<Activity> list)
        {
            while (start.Date != end.Date)
            {
                // If on that date nothing was logged we skip that date
                if (!dict.ContainsKey(DateFormat(start)))
                {
                    start = start.AddDays(1);
                    continue;
                }

                list = AddToListPerDay(start, dict, list, true);
                start = start.AddDays(1);
            }

            return AddToListPerDay(end, dict, list, true);
        }
        public static List<Activity> AddToListPerDay(DateTime date, Dictionary<string, List<Activity>> dict, List<Activity> list, bool checkIfEntryAlreadyExists, bool apply5MinRule = true)
        {
            if (!dict.ContainsKey(DateFormat(date)))
            {
                return list ?? new List<Activity>();
            }

            if (checkIfEntryAlreadyExists == false)
            {
                foreach (Activity item in dict[DateFormat(date)])
                {
                    if (apply5MinRule && int.Parse(item.TimeSpent.Remove(item.TimeSpent.Length - 7)) < 5)
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

            List<Activity> myActivities = (dict.ContainsKey(DateFormat(date))) ? dict[DateFormat(date)] : new List<Activity>();
            bool skipped = false;

            for (int i = 0; i < myActivities.Count; i++)
            {
                int temp = int.Parse(myActivities[i].TimeSpent.Remove(myActivities[i].TimeSpent.Length - 7));
                if (apply5MinRule && temp < 4)
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

                if (skipped)
                {
                    skipped = false;
                    continue;
                }

                list.Add(new Activity
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
            int biggestNumber = 0;
            foreach (Activity item in list)
            {
                sum += item.TimeSpentint;

                if (item.TimeSpentint > biggestNumber)
                {
                    biggestNumber = item.TimeSpentint;
                }
            }

            if (biggestNumber > sum / 2)
            {
                foreach (Activity item in list)
                {
                    item.ProgressBarMaxValue = biggestNumber + biggestNumber / 10;
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
            float baseIndex = list.Count * 10;
            float indexItem = baseIndex;
            float subtractValue = baseIndex / 5f;

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
            if (date.Date != DateTime.ParseExact("01.01.0001", "dd.MM.yyyy", null).Date)
            {
                return date.ToString("dd.MM.yyyy");
            }
            return DateTime.Now.ToString("dd.MM.yyyy");
        }
        private static string GetDirectory()
        {
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            return Environment.OSVersion.Version.Major >= 6 ? Directory.GetParent(path).ToString() : "";
        }

        public void DailyButton([Optional] object sender, [Optional] RoutedEventArgs e)
        {
            PlayAnimation();
            SetPage(CurrentPage.Daily);
            MyFrame.Content = Pages[0];
        }
        private void WeeklyButton(object sender, RoutedEventArgs e)
        {
            PlayAnimation();
            SetPage(CurrentPage.Weekly);
            MyFrame.Content = Pages[1];
        }
        private void MonthlyButton(object sender, RoutedEventArgs e)
        {
            PlayAnimation();
            SetPage(CurrentPage.Monthly);
            MyFrame.Content = Pages[2];
        }
        private void TotalButton(object sender, RoutedEventArgs e)
        {
            PlayAnimation();
            SetPage(CurrentPage.Total);
            MyFrame.Content = Pages[3];
        }
        private void SettingsButton(object sender, RoutedEventArgs e)
        {
            PlayAnimation();
            SetPage(CurrentPage.Settings);
            MyFrame.Content = Pages[4];
        }
        private void YesterdayButton(object sender, RoutedEventArgs e)
        {
            PlayAnimation();
            SetPage(CurrentPage.Yesterday);
            MyFrame.Content = Pages[5];
        }
        private void CustomButton(object sender, RoutedEventArgs e)
        {
            PlayAnimation();
            SetPage(CurrentPage.Custom);
            MyFrame.Content = Pages[6];
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
