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

        // Daily, Weekly, Monthly, Total
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
                    break;

                case ActivityStatus.Monthly:
                    break;

                case ActivityStatus.Total:
                    break;

            }
        }

        private void LoadDaily(Dictionary<string, List<Activity>> output)
        {
            foreach (Activity item in output[DateFormat()])
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

            activities = SortList(activities);
            CalculateSumOfList(activities);
            activities = SetProgressBarColor(activities);

            ActivitiesItemsControl.ItemsSource = activities;
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
        private static string GetDirectory()
        {
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            if (Environment.OSVersion.Version.Major >= 6)
            {
                return Directory.GetParent(path).ToString();
            }
            return "";
        }
        private static string DateFormat()
        {
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
