using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Newtonsoft.Json;

namespace ActivityLogger
{
    class Program
    {
        // Imports
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // Activity Logger Data
        private static readonly string ActivityLoggerPath = GetDirectory() + @"\TMRosemite\ActivityLogger";

        private static Dictionary<string, List<Activity>> activityDictionary = new Dictionary<string, List<Activity>>();
        private static List<Activity> activities = new List<Activity>();
        private static Config config;

        private const int WaitSeconds = 0;

        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Activity Logger Started\n");

                // Checks if Directory is fine and reads json
                Load();

                // Clear not needed items
                Clear();

                // Waits 60 Seconds
                Thread.Sleep(1000 * WaitSeconds);

                // Main Loop
                MyMain();
            }
            catch (Exception e)
            {
                string day = DateTime.Now.Day.ToString();
                string month = DateTime.Now.Month.ToString();
                string year = DateTime.Now.Year.ToString();

                File.WriteAllText(@"Error " + day + '.' + month + '.' + year + ".txt", e.ToString());
            }
        }

        private static void Clear()
        {
            if (DateTime.Now.Day != 1) { return; }
            else if (activityDictionary == null) { return; }
            else if (activityDictionary.Count == 0) { return; }

            foreach (var item in activityDictionary.ToList())
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    if (item.Value[i].MinutesSpent() < 10)
                    {
                        item.Value.RemoveRange(i, item.Value.Count - i);
                        activityDictionary[item.Key] = item.Value;
                        break;
                    }
                }
            }
        }
        private static void LoadActivities()
        {
            // Read Json
            string jsonFromFile;

            using (var reader = new StreamReader(ActivityLoggerPath + @"\SavedActivities.json"))
            {
                jsonFromFile = reader.ReadToEnd();
                if (!jsonFromFile.Contains("{"))
                {
                    activities = new List<Activity>();
                    return;
                }
            }

            activityDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<Activity>>>(jsonFromFile);

            activityDictionary.TryGetValue(DateFormat(), out var tempActivities);

            if (tempActivities == null) { return; }

            activities.AddRange(tempActivities);
        }
        private static void LoadConfig()
        {
            string jsonFromFile;

            using (var reader = new StreamReader(ActivityLoggerPath + @"\Config.json"))
            {
                jsonFromFile = reader.ReadToEnd();
                if (!jsonFromFile.Contains("{"))
                {
                    config = new Config();
                    return;
                }
            }

            config = JsonConvert.DeserializeObject<Config>(jsonFromFile);
        }
        private static void Load()
        {
            // Check If required Path Exists
            if (!Directory.Exists(ActivityLoggerPath))
            {
                // If not we Create it
                Directory.CreateDirectory(ActivityLoggerPath);
            }

            if (File.Exists(ActivityLoggerPath + @"\SavedActivities.json"))
            {
                // Loads Json and updates activities list
                try { LoadActivities(); } catch { }
            }

            if (File.Exists(ActivityLoggerPath + @"\Config.json"))
            {
                // Loads Config file
                try { LoadConfig(); } catch { config = new Config(); }
                return;
            }

            config = new Config();
        }

        private static void MyMain()
        {
            while (true)
            {
                Save(GetActiveProcessFileName());

                // Sleep one minute and repeat
                Thread.Sleep(1000 * WaitSeconds);
            }
        }

        private static void Save(string fileName)
        {
            // If new day just started we need to restart
            CheckForRestart();

            // If the Ignore Array contains fileName don't save
            if (Array.Exists(config.IgnoreProcessName, element => element.ToLower() == fileName.ToLower())) { return; }

            // If fileName is empty don't save
            if (fileName.Trim().Length == 0) { return; }

            // Checks if fileName already exists in the activities list
            // If so we edit. Else we add a new entry
            for (int i = 0; i < activities.Count; i++)
            {
                if (activities[i].ActivityName == fileName)
                {
                    int timeSpent = activities[i].MinutesSpent();

                    SaveJson(new Activity(fileName, (++timeSpent) + " Minutes"), i);
                    return;
                }
            }
            SaveJson(new Activity(fileName, "1 Minute"));
        }

        private static void SaveJson(Activity activity, int index = -1)
        {
            // Setting up list
            if (index != -1) { activities.RemoveAt(index); }

            activities.Add(activity);

            activities = activities.OrderByDescending(o => o.MinutesSpent()).ToList();

            // Apply List to Dictionary
            activityDictionary[DateFormat()] = activities;

            // Save
            string json = JsonConvert.SerializeObject(activityDictionary, Formatting.Indented);

            try { File.WriteAllText(ActivityLoggerPath + @"\SavedActivities.json", json); }
            catch (Exception e)
            {
                string error = $"The File was probably in use... \n StackTrace Exception:\n{e}";
                File.WriteAllText(ActivityLoggerPath + @"\Error.txt", error);
            }
        }
        private static string GetActiveProcessFileName()
        {
            // Get ForegroundWindowProcess
            IntPtr pointer = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(pointer, out pid);
            Process p = Process.GetProcessById((int)pid);

            string value = config.RenameActivity(p.MainWindowTitle, p.ProcessName);
            if (value != null)
                return value;
            
            // Try to get the most meaningful name for the current Item
            if (p.MainWindowTitle.Contains(p.ProcessName))
            {
                if (!p.MainWindowTitle.Contains('-')) { return p.ProcessName; }

                return p.MainWindowTitle.Split("-")[p.MainWindowTitle.Split("-").Length - 1].TrimStart();
            }

            if (!p.MainWindowTitle.Contains("-")) { return p.MainWindowTitle; }

            string FileName = p.MainWindowTitle.Split("-")[p.MainWindowTitle.Split("-").Length - 1].TrimStart();

            string[] SplitPath;

            try { SplitPath = p.MainModule.FileName.Split("\\"); } catch { return p.ProcessName; }

            // Checks by path
            foreach (string item in SplitPath)
            {
                if (item.ToLower().Contains(FileName.ToLower()) || FileName.ToLower().Contains(item.ToLower()))
                {
                    return p.MainWindowTitle.Split("-")[p.MainWindowTitle.Split("-").Length - 1].TrimStart();
                }
            }

            return p.ProcessName;
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

        private static void CheckForRestart()
        {
            if (!activityDictionary.ContainsKey(DateFormat()) & activities.Count != 0)
            {
                Process.Start(ActivityLoggerPath + @"\ActivityLogger.exe");
                Environment.Exit(0);
            }
        }
        private static string DateFormat() => DateTime.Now.ToString("dd.MM.yyyy");
    }

    internal  class Activity
    {
        public string ActivityName { get; set; }
        public string TimeSpent { get; set; }

        public Activity(string ActivityName, string TimeSpent)
        {
            this.ActivityName = ActivityName;
            this.TimeSpent = TimeSpent;
        }

        public int MinutesSpent() => int.Parse(TimeSpent.Remove(TimeSpent.Length - 7));
    }

    internal class Config
    {
        public readonly Dictionary<string, string> IncludesProcessName;
        public readonly Dictionary<string, string> IsProcessName;
        
        public readonly Dictionary<string, string> IncludesWindowName;
        public readonly Dictionary<string, string> IsWindowName;
        
        public readonly string[] IgnoreProcessName;

        public string RenameActivity(string windowName, string processName)
        {
            // Check for matches
            var processNameMatch = IsProcessName.Where(name => name.Key == processName);
            if (processNameMatch.Any())
                return processNameMatch.First().Value;
            
            var windowNameMatch = IsWindowName.Where(name => name.Key == windowName);
            if (windowNameMatch.Any())
                return windowNameMatch.First().Value;

            var processNameContains = IncludesProcessName.Where(name => processName.Contains(name.Key));
            if (processNameContains.Any())
            {
                return processNameContains.First().Value;
            }
            
            var windowNameContains = IncludesWindowName.Where(name => windowName.Contains(name.Key));
            if (windowNameContains.Any())
            {
                return windowNameContains.First().Value;
            }

            return null;
        }
        
        public Config()
        {
            IncludesProcessName = new Dictionary<string, string>();
            IsProcessName = new Dictionary<string, string>();
            IncludesWindowName = new Dictionary<string, string>();
            IsWindowName = new Dictionary<string, string>();
            
            IgnoreProcessName = new string[0];
        }
        
        public Config(
            Dictionary<string, string> IncludesProcessName,
            Dictionary<string, string> IsProcessName,
            Dictionary<string, string> IncludesWindowName,
            Dictionary<string, string> IsWindowName,
            string[] IgnoreProcessName
        )
        {
            this.IncludesProcessName = IncludesProcessName;
            this.IsProcessName = IsProcessName;
            this.IncludesWindowName = IncludesWindowName;
            this.IncludesWindowName = IncludesWindowName;
            this.IsWindowName = IsWindowName;
            this.IgnoreProcessName = IgnoreProcessName;
        }
    }
}