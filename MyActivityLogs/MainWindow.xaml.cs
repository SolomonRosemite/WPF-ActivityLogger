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
        private Dictionary<string, List<Activity>> activityDictionary;
        private List<Activity> activities = new List<Activity>();

        public float sum { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Load();
        }

        void Load()
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

            activityDictionary = output;

            foreach (Activity item in activityDictionary[DateFormat()])
            {
                activities.Add(new Activity() { 
                    ActivityName = item.ActivityName,
                    TimeSpent = item.TimeSpent,
                    TimeSpentint = int.Parse(item.TimeSpent.Remove(item.TimeSpent.Length - 7)),
                    Color = "LightGreen"
                });
            }

            SortList();
            Calculatesum();

            ActivitiesItemsControl.ItemsSource = activities;
        }

        private void ErrorMessage(string message)
        {
            // Show Popup
        }
        dynamic ReadJson()
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
        private void SortList()
        {
            activities = activities.OrderBy(o => o.TimeSpentint).ToList();
            activities.Reverse();
        }
        private void Calculatesum()
        {
            sum = 0; 
            foreach (Activity item in activities)
            {
                sum += item.TimeSpentint;
            }
            sum *= sum;

            Console.WriteLine(sum);
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
    }
}
