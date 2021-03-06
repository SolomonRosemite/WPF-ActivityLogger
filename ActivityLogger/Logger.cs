﻿using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using System;
using ActivityLogger.events;

namespace ActivityLogger
{
    public class Logger
    {
        // Imports
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();


        public static readonly string ActivityLoggerPath = GetDirectory() + @"\TMRosemite\ActivityLogger";

        private Dictionary<string, List<Activity>> activityDictionary = new Dictionary<string, List<Activity>>();
        private List<Activity> activities = new List<Activity>();

        private Config config;
        private const int WaitSeconds = 60;

        public Process FirebaseClient { get; private set; }

        public event EventHandler<InitializedLoggerEventArgs> OnInitializedLogger;
        public event EventHandler<InitializedLoggerFailedEventArgs> OnInitializedLoggerFailed;

        public void InitializeLogger()
        {
            try
            {
                Console.WriteLine("Activity Logger Started");

                // Checks if Directory is fine and reads json
                Load();

                // Clears not needed items
                Clear();

                // Maps Activities names to recent Config
                MapRecentConfigNames();

                // Starts the Firebase Client
                LaunchFirebaseClient();

                OnInitializedLogger?.Invoke(this, new InitializedLoggerEventArgs { logger = this });

                // Waits 60 Seconds
                Thread.Sleep(1000 * WaitSeconds);

                // Repeats Main loop cycle
                MainCycle();
            }
            catch (Exception e)
            {
                var day = DateTime.Now.Day.ToString();
                var month = DateTime.Now.Month.ToString();
                var year = DateTime.Now.Year.ToString();

                File.WriteAllText(@"Error " + day + '.' + month + '.' + year + ".txt", e.ToString());

                OnInitializedLoggerFailed?.Invoke(this, new InitializedLoggerFailedEventArgs { ExceptionMessage = e });
            }
        }

        private void Clear()
        {
            DateTime date = DateTime.Now;

            if (date.Day != 1) { return; }
            if (activityDictionary == null) { return; }
            if (activityDictionary.Count == 0) { return; }

            foreach (var (key, value) in activityDictionary.ToList())
            {
                for (int i = 0; i < value.Count; i++)
                {
                    if (value[i].MinutesSpent() >= 10) continue;

                    value.RemoveRange(i, value.Count - i);
                    activityDictionary[key] = value;
                    break;
                }
            }
        }
        private void LoadActivities()
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
        private void LoadConfig()
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
        private void Load()
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

        private  void MapRecentConfigNames()
        {
            foreach (var (key, values) in activityDictionary.ToList())
            {
                for (int i = 0; i < values.Count; i++)
                {
                    var name = values[i].ActivityName;
                    var res = config.RenameActivity(name, name);

                    if (res == null)
                        continue;

                    var value = values.Where((activity, j) => activity.ActivityName == res).FirstOrDefault();
                    if (value != null)
                    {
                        values[i].ActivityName = res;
                        values[i--].AddMinutesSpent(value.MinutesSpent());
                        values.Remove(value);
                        continue;
                    }

                    values[i].ActivityName = res;
                }

                if (key == DateFormat())
                    activities = values;

                activityDictionary[key] = values;
            }
        }

        public void LaunchFirebaseClient()
        {
            if (config.DontUseFirebaseClient.HasValue && config.DontUseFirebaseClient.Value)
                return;

            FirebaseClient = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    FileName = @$"{ActivityLoggerPath}\launch.bat",
                }
            };

            FirebaseClient.Start();
        }

        private void MainCycle()
        {
            while (true)
            {
                Save(GetActiveProcessFileName());

                // Sleep one minute and repeat
                Thread.Sleep(1000 * WaitSeconds);
            }
        }

        private void Save(string fileName)
        {
            // If new day just started we need to restart
            CheckForRestart();

            // If the Ignore Array contains the process name its going to return. So we dont want to save
            if (fileName == null) { return; }

            // If fileName is empty don't save
            if (fileName.Length == 0) { return; }

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

        private void SaveJson(Activity activity, int index = -1)
        {
            // Setting up list
            if (index != -1) { activities.RemoveAt(index); }

            activities.Add(activity);

            activities = activities.OrderByDescending(o => o.MinutesSpent()).ToList();

            // Apply List to Dictionary
            activityDictionary[DateFormat()] = activities;

            // Save
            string json = JsonConvert.SerializeObject(activityDictionary, Formatting.Indented);

            try
            {
                File.WriteAllText(ActivityLoggerPath + @"\SavedActivities.json", json);
            }
            catch (Exception e)
            {
                string error = $"The File was probably in use...\nStacktrace Exception:\n{e}";
                File.WriteAllText(ActivityLoggerPath + @"\Error.txt", error);
            }
        }
        private string GetActiveProcessFileName()
        {
            // Get ForegroundWindowProcess
            IntPtr pointer = GetForegroundWindow();
            GetWindowThreadProcessId(pointer, out var pid);
            Process p = Process.GetProcessById((int)pid);

            // If the Ignore Array contains fileName don't save
            if (Array.Exists(config.IgnoreProcessName, element => element.ToLower() == p.ProcessName.ToLower())) { return null; }

            string value = config.RenameActivity(p.MainWindowTitle, p.ProcessName);
            if (value != null)
                return value.Trim();

            // Try to get the most meaningful name for the current Item
            if (p.MainWindowTitle.Contains('/') || p.MainWindowTitle.Contains('\\'))
                return p.ProcessName.Trim();

            if (p.MainWindowTitle.ToLower().Contains(p.ProcessName.ToLower()))
            {
                if (!p.MainWindowTitle.Contains('-')) { return p.ProcessName.Trim(); }

                return p.MainWindowTitle.Split("-")[p.MainWindowTitle.Split("-").Length - 1].TrimStart().Trim();
            }

            if (!p.MainWindowTitle.Contains("-")) { return p.MainWindowTitle; }

            string fileName = p.MainWindowTitle.Split("-")[p.MainWindowTitle.Split("-").Length - 1].TrimStart();

            string[] splitPath;

            try { splitPath = p.MainModule.FileName.Split("\\"); } catch { return p.ProcessName.Trim(); }

            // Checks by path
            if (splitPath.Any(item => item.ToLower().Contains(fileName.ToLower()) || fileName.ToLower().Contains(item.ToLower())))
            {
                return p.MainWindowTitle.Split("-")[p.MainWindowTitle.Split("-").Length - 1].TrimStart().Trim();
            }

            return p.ProcessName.Trim();
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

        private void CheckForRestart()
        {
            if (activityDictionary.ContainsKey(DateFormat()) || activities.Count == 0) return;

            Restart();
        }

        public void Restart()
        {
            ExitFirebaseClient();

            Process.Start(ActivityLoggerPath + @"\ActivityLogger.exe");
            Environment.Exit(0);
        }

        public void ExitFirebaseClient()
        {
            if (FirebaseClient == null) return;

            try
            {
                KillFirebaseClientProcess(FirebaseClient.Id);
            }
            catch{ }
        }

        private static void KillFirebaseClientProcess(int pid)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    Arguments = $"/C taskkill /F /PID {pid} /T"
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private static string DateFormat() => DateTime.Now.ToString("dd.MM.yyyy");
    }

    internal class Activity
    {
        public string ActivityName { get; set; }
        public string TimeSpent { get; set; }

        public Activity(string ActivityName, string TimeSpent)
        {
            this.ActivityName = ActivityName;
            this.TimeSpent = TimeSpent;
        }

        public int MinutesSpent() => int.Parse(TimeSpent.Remove(TimeSpent.Length - 7));

        public void AddMinutesSpent(int value)
        {
            TimeSpent = $"{value + int.Parse(TimeSpent.Remove(TimeSpent.Length - 7))} Minutes";
        }
    }

    internal class Config
    {
        public readonly Dictionary<string, string> IncludesProcessName;
        public readonly Dictionary<string, string> IsProcessName;

        public readonly Dictionary<string, string> IncludesWindowName;
        public readonly Dictionary<string, string> IsWindowName;

        private string[] ignoreProcessName;

        public string[] IgnoreProcessName
        {
            get => ignoreProcessName;
            set
            {
                if (ignoreProcessName == null || ignoreProcessName.Length == 0)
                {
                    ignoreProcessName = value;
                }
            }
        }

        public bool? dontUseFirebaseClient;
        public bool? DontUseFirebaseClient
        {
            get => this.dontUseFirebaseClient;
            set
            {
                if (this.dontUseFirebaseClient == null)
                {
                    this.dontUseFirebaseClient = value;
                }
            }
        }

        public string RenameActivity(string windowName, string processName)
        {
            windowName = windowName.ToLower();
            processName = processName.ToLower();

            // Check for matches
            var processNameMatch = IsProcessName.Where(name => name.Key.ToLower() == processName);
            if (processNameMatch.Any())
                return processNameMatch.First().Value;

            var windowNameMatch = IsWindowName.Where(name => name.Key.ToLower() == windowName);
            if (windowNameMatch.Any())
                return windowNameMatch.First().Value;

            var processNameContains = IncludesProcessName.Where(name => processName.Contains(name.Key.ToLower()));
            if (processNameContains.Any())
            {
                return processNameContains.First().Value;
            }

            var windowNameContains = IncludesWindowName.Where(name => windowName.Contains(name.Key.ToLower()));
            if (windowNameContains.Any())
            {
                return windowNameContains.First().Value;
            }

            return null;
        }

        public Config()
        {
            this.IncludesProcessName = new Dictionary<string, string>();
            this.IsProcessName = new Dictionary<string, string>();
            this.IncludesWindowName = new Dictionary<string, string>();
            this.IsWindowName = new Dictionary<string, string>();

            this.IgnoreProcessName = new string[0];
            this.DontUseFirebaseClient = null;
        }

        public Config(
            Dictionary<string, string> IncludesProcessName,
            Dictionary<string, string> IsProcessName,
            Dictionary<string, string> IncludesWindowName,
            Dictionary<string, string> IsWindowName,
            string[] IgnoreProcessName,
            bool DontUseFirebaseClient
        )
        {
            this.IncludesProcessName = IncludesProcessName;
            this.IsProcessName = IsProcessName;
            this.IncludesWindowName = IncludesWindowName;
            this.IncludesWindowName = IncludesWindowName;
            this.IsWindowName = IsWindowName;
            this.IgnoreProcessName = IgnoreProcessName;
            this.DontUseFirebaseClient = DontUseFirebaseClient;
        }

    }
}
