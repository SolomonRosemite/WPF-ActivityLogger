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
        // Dll Imports
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // Activity Logger Data
        private static readonly string activityLoggerPath = GetDirectory() + @"\TMRosemite\ActivityLogger";
        private static readonly string[] ignoreList = new string[]
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
        private static readonly Dictionary<string, string> renameDict = new Dictionary<string, string>{
            {"WindowsTerminal", "Console"},
            {"cmd", "Console"},
            {"WINWORD", "Word"},
            {"EXCEL", "Excel"},
        };
        private static List<Activity> activities = new List<Activity>();
        private static Dictionary<string, List<Activity>> activityDictionary = new Dictionary<string, List<Activity>>();

        private const int waitSeconds = 60;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Activity Logger Started\n");

                // Checks if Directory is fine and reads json
                Load();

                // Clear not needed items
                Clear();

                // Waits 60 Seconds
                System.Threading.Thread.Sleep(1000 * waitSeconds);

                MyMain();
            }
            catch (Exception e)
            {
                string day = DateTime.Now.Day.ToString();
                string month = DateTime.Now.Month.ToString();
                string year = DateTime.Now.Year.ToString();
                File.WriteAllText($"Error {day}/{month}/{year}.txt", e.Message);
            }
        }

        static void Clear()
        {
            if (activityDictionary == null)
                return;
            else if (activityDictionary.Count == 0)
                return;

            foreach (var item in activityDictionary.ToList())
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    if (int.Parse(item.Value[i].TimeSpent.Remove(item.Value[i].TimeSpent.Length - 7)) < 5)
                    {
                        item.Value.RemoveRange(i, item.Value.Count - i);
                        activityDictionary[item.Key] = item.Value;
                        break;
                    }
                    System.Console.WriteLine(item.Value[i].TimeSpent);
                }
            }
        }

        static void loadJson()
        {
            // Read Json
            string jsonFromFile;
            using (var reader = new StreamReader(activityLoggerPath + @"\SavedActivities.json"))
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
            if (!Directory.Exists(activityLoggerPath))
            {
                Directory.CreateDirectory(activityLoggerPath);
            }

            if (File.Exists(activityLoggerPath + @"\SavedActivities.json"))
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
            // If new day just started we need to restart
            checkForRestart();

            // If ignoreList contains fileName don't save
            if (Array.Exists(ignoreList, element => element == fileName.ToLower())) { return; }

            // If fileName is empty don't save
            if (fileName.Trim().Length == 0) { return; }

            // Renames the name of the fileName if found in renameList
            fileName = renameActivity(fileName);

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

            // Apply List to Dictionary
            activityDictionary[DateFormat()] = activities;

            // Save
            string json = JsonConvert.SerializeObject(activityDictionary, Formatting.Indented);

            try { File.WriteAllText(activityLoggerPath + @"\SavedActivities.json", json); }
            catch (Exception e)
            {
                string error = $"The File was probably in use... \n StackTrace Exception:\n{e}";
                File.WriteAllText(activityLoggerPath + @"\Error.txt", error);
            }
        }
        static string GetActiveProcessFileName()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            Process p = Process.GetProcessById((int)pid);

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

        private static void checkForRestart()
        {
            if (!activityDictionary.ContainsKey(DateFormat()) & activities.Count != 0)
            {
                Process.Start(activityLoggerPath + @"\ActivityLogger.exe");
                Environment.Exit(0);
                return;
            }
        }

        private static string renameActivity(string fileName)
        {
            foreach (var item in renameDict)
                if (item.Key == fileName)
                    return item.Value;

            return fileName;
        }
        private static string DateFormat() => DateTime.Now.ToString("dd.MM.yyyy");
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

        public int MinutesSpent() => Int32.Parse(TimeSpent.Remove(TimeSpent.Length - 7));
    }
}
