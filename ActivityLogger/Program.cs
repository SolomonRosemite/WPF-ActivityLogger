using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using System;

namespace ActivityLogger
{
    class Program
    {
        // needed imports
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // Activity Logger data
        private static string ActivityLoggerPath = GetDirectory() + @"\TMRosemite\ActivityLogger";
        private static string[] ignoreList = new string[]
        {
            "applicationframehost",
            "discord updater",
            "task switching",
            "wirelesssetup",
            "task manager",
            "explorer",
            "settings",
            "regedit",
            "Taskmgr",
            "drag"
        };
        private static List<Activity> activities = new List<Activity>();
        private static Dictionary<string, List<Activity>> activityDictionary = new Dictionary<string, List<Activity>>();

        private const int waitSeconds = 60;

        static void Main(string[] args)
        {
            Console.WriteLine("Activity Logger Started\n");

            // Checks if Directory is fine and reads json
            Load();

            // Waits 60 Seconds
            System.Threading.Thread.Sleep(1000 * waitSeconds);

            MyMain();
        }

        static void loadJson()
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

            List<Activity> tempactivities;
            activityDictionary.TryGetValue(DateFormat(), out tempactivities);

            if (tempactivities == null)
                return;

            activities.AddRange(tempactivities);
        }

        private static void Load()
        {
            if (!Directory.Exists(ActivityLoggerPath))
            {
                Directory.CreateDirectory(ActivityLoggerPath);
            }

            if (File.Exists(ActivityLoggerPath + @"\SavedActivities.json"))
            {
                // Loads Json and updates activities list
                try { loadJson(); } catch { }
            }
        }

        static void MyMain()
        {
            Save(GetActiveProcessFileName());

            // Sleep one minute and repeat
            System.Threading.Thread.Sleep(1000 * waitSeconds);
            MyMain();
        }

        static void Save(string fileName)
        {
            // If ignoreList contains fileName don't save
            if (Array.Exists(ignoreList, element => element == fileName.ToLower())) { return; }

            // If string is empty don't save
            if (fileName.Trim().Length == 0) { return; }

            Console.WriteLine(fileName);

            // If new day just started
            if (!activityDictionary.ContainsKey(DateFormat()) & activities.Count != 0)
            {
                Process.Start(ActivityLoggerPath + @"\ActivityLogger.exe");
                Environment.Exit(0);
                return;
            }

            // Checks if fileName already exists in the activities list
            // If so we edit. Else we add a new entry
            for (int i = 0; i < activities.Count; i++)
            {
                if (activities[i].ActivityName == fileName)
                {
                    int TimeSpent = int.Parse(activities[i].TimeSpent.Remove(activities[i].TimeSpent.Length - 7));

                    SaveJson(new Activity(ActivityName: fileName, TimeSpent: (++TimeSpent).ToString() + " Minutes"), i);
                    return;
                }
            }
            SaveJson(new Activity(ActivityName: fileName, TimeSpent: "1 Minute"));
        }

        public static void SaveJson(Activity activity, int index = -1)
        {
            // Setting up list
            if (index != -1)
                activities.RemoveAt(index);

            activities.Add(activity);

            activities = activities.OrderBy(o => o.MinutesSpent()).ToList();
            activities.Reverse();

            // Setting up Dictionary
            activityDictionary[DateFormat()] = activities;

            // Save
            string json = JsonConvert.SerializeObject(activityDictionary, Formatting.Indented);
            File.WriteAllText(ActivityLoggerPath + @"\SavedActivities.json", json);
        }
        static string GetActiveProcessFileName()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            Process p = Process.GetProcessById((int)pid);

            if (p.MainWindowTitle.Contains(p.ProcessName))
            {
                return p.MainWindowTitle.Split("-")[p.MainWindowTitle.Split("-").Length - 1].TrimStart();
            }

            if (!p.MainWindowTitle.Contains("-"))
            {
                return p.MainWindowTitle;
            }

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
        private static string DateFormat()
        {
            return DateTime.Now.ToString("dd.MM.yyyy");
        }
    }

    class Activity
    {
        public string ActivityName { get; set; }
        public string TimeSpent { get; set; }

        public Activity(string ActivityName, string TimeSpent)
        {
            this.ActivityName = ActivityName;
            this.TimeSpent = TimeSpent;
        }

        public int MinutesSpent()
        {
            return Int32.Parse(TimeSpent.Remove(TimeSpent.Length - 7));
        }
    }
}
