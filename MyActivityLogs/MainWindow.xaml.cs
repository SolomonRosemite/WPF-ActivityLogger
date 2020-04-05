using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows.Navigation;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Data;
using System.Threading;
using Newtonsoft.Json;
using System.Windows;
using System.Text;
using System.Linq;
using System.IO;
using System;

namespace MyActivityLogs
{
    public partial class MainWindow : Window
    {
        private static string ActivityLoggerPath = GetDirectory() + @"\TMRosemite\ActivityLogger";
        private enum ActivityStatus
        {
            Daily,
            Weekly,
            Monthly,
            Total
        }

        private List<Activity> activities = new List<Activity>();

        public MainWindow()
        {
            InitializeComponent();

            Load();
        }

        void Load(ActivityStatus status = ActivityStatus.Daily)
        {
            var output = ReadJson();

            // Checking if json could be read
            if (output is int)
            {
                if (output == 0)
                {
                    // Json could not be found
                    ErrorMessage("Json could not be found");
                    return;
                }
                else if (output == 1)
                {
                    // Json is corrupted
                    ErrorMessage("Json is corrupted");
                    return;
                }
            }

            if (output.Count == 0)
            {
                ErrorMessage("IDK no logs for today yet");
                return;
            }

            switch (status)
            {
                case ActivityStatus.Daily:
                    LoadDaily(output);
                    break;

                case ActivityStatus.Weekly:
                    LoadWeekly(output);
                    break;

                case ActivityStatus.Monthly:
                    LoadMonthly(output);
                    break;

                case ActivityStatus.Total:
                    LoadTotal(output);
                    break;

            }
        }

        private void LoadDaily(Dictionary<string, List<Activity>> output)
        {
            if (!output.ContainsKey(DateFormat()))
            {
                ErrorMessage("IDK no logs for today yet");
                return;
            }

            AddToListPerDay(DateTime.Now, output, false);

            LoadFinish();
        }
        private void LoadWeekly(Dictionary<string, List<Activity>> output)
        {
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    // Only Today
                    AddToListForWeekly(0, output);
                    break;
                case DayOfWeek.Tuesday:
                    // Today and yesterday and so on...
                    AddToListForWeekly(-1, output);
                    break;
                case DayOfWeek.Wednesday:
                    AddToListForWeekly(-2, output);
                    break;
                case DayOfWeek.Thursday:
                    AddToListForWeekly(-3, output);
                    break;
                case DayOfWeek.Friday:
                    AddToListForWeekly(-4, output);
                    break;
                case DayOfWeek.Saturday:
                    AddToListForWeekly(-5, output);
                    break;
                case DayOfWeek.Sunday:
                    AddToListForWeekly(-6, output);
                    break;
            }

            LoadFinish();
        }
        private void LoadMonthly(Dictionary<string, List<Activity>> output)
        {
            LoadFinish();
        }
        private void LoadTotal(Dictionary<string, List<Activity>> output)
        {
            LoadFinish();
        }

        private void ErrorMessage(string message)
        {
            // Show Popup
        }
        private dynamic ReadJson()
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

            return JsonConvert.DeserializeObject<Dictionary<string, List<Activity>>>(jsonFromFile);
        }
        private void AddToListForWeekly(int daysBehind, Dictionary<string, List<Activity>> dict)
        {
            DateTime date = DateTime.Now;

            // Today would be monday which there are no daysBehind because the week just started
            if (daysBehind == 0)
            {
                AddToListPerDay(date, dict, false);
                return;
            }

            while (daysBehind++ != 0)
            {
                // If on that date nothing was logged we skip that date
                if (!dict.ContainsKey(DateFormat(date.AddDays(daysBehind))))
                {
                    continue;
                }

                AddToListPerDay(date.AddDays(daysBehind), dict, true);
            }
        }
        private void AddToListPerDay(DateTime date, Dictionary<string, List<Activity>> output, bool checkIfEntryAlreadyExists)
        {
            if (checkIfEntryAlreadyExists == false)
            {
                foreach (Activity item in output[DateFormat(date)])
                {
                    if (int.Parse(item.TimeSpent.Remove(item.TimeSpent.Length - 7)) < 10)
                    {
                        continue;
                    }


                    activities.Add(new Activity()
                    {
                        ActivityName = item.ActivityName,
                        TimeSpent = item.TimeSpent,
                        TimeSpentint = int.Parse(item.TimeSpent.Remove(item.TimeSpent.Length - 7))
                    });
                }
                return;
            }

            List<Activity> myActivities = output[DateFormat(date)];
            bool skipped = false;

            for (int i = 0; i < myActivities.Count; i++)
            {
                int temp;
                if ((temp = int.Parse(myActivities[i].TimeSpent.Remove(myActivities[i].TimeSpent.Length - 7))) < 10)
                {
                    continue;
                }

                // If that ActivityName already exists we dont add and just add the minutes
                for (int j = 0; j < activities.Count; j++)
                {
                    if (activities[j].ActivityName == myActivities[i].ActivityName)
                    {

                        activities[j].TimeSpent = (activities[j].TimeSpentint + temp).ToString() + " Minutes";
                        activities[j].TimeSpentint += temp;
                        skipped = true;
                        break;
                    }
                }

                if (skipped == true)
                {
                    skipped = false;
                    continue;
                }

                activities.Add(new Activity()
                {
                    ActivityName = myActivities[i].ActivityName,
                    TimeSpent = myActivities[i].TimeSpent,
                    TimeSpentint = int.Parse(myActivities[i].TimeSpent.Remove(myActivities[i].TimeSpent.Length - 7))
                });
            }
        }
        private List<Activity> SortList(List<Activity> list)
        {
            return list.OrderBy(o => o.TimeSpentint).ToList();
        }
        private void CalculateSumOfList(List<Activity> list)
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
                return;
            }

            foreach (Activity item in list)
            {
                item.ProgressBarMaxValue = sum / 2;
            }
        }
        private List<Activity> SetProgressBarColor(List<Activity> list)
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
        private void LoadFinish()
        {
            activities = SortList(activities);
            CalculateSumOfList(activities);
            activities = SetProgressBarColor(activities);

            ActivitiesItemsControl.ItemsSource = activities;
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
        private static string DateFormat([Optional] DateTime date)
        {
            if (date.ToString() != "01.01.0001 00:00:00")
            {
                return date.ToString("dd.MM.yyyy");
            }
            return DateTime.Now.ToString("dd.MM.yyyy");
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
