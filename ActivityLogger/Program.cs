using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.IO;
using System;

namespace ActivityLogger
{
    class Program
    {
        // Imports
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();


        private static readonly string ActivityLoggerPath = GetDirectory() + @"\TMRosemite\ActivityLogger";
        private static Dictionary<string, List<Activity>> activityDictionary = new Dictionary<string, List<Activity>>();
        private static List<Activity> activities = new List<Activity>();

        private static Task firebaseDownloadTask;
        private static Config config;

        private const int WaitSeconds = 60;

        private static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Activity Logger Started");

                // Checks if Directory is fine and reads json
                Load();

                // Clear not needed items
                Clear();

                // Start the Firebase Client
                LaunchFirebaseClient();

                // Waits 60 Seconds
                Thread.Sleep(1000 * WaitSeconds);

                // Main Loop
                await MyMain();
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
            DateTime date = DateTime.Now;

            if (date.Day != 1) { return; }
            if (activityDictionary == null) { return; }
            if (activityDictionary.Count == 0) { return; }

            foreach (var (key, value) in activityDictionary.ToList())
            {
                for (int i = 0; i < value.Count; i++)
                {
                    if (value[i].MinutesSpent() < 10)
                    {
                        value.RemoveRange(i, value.Count - i);
                        activityDictionary[key] = value;
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

        private static void LaunchFirebaseClient()
        {
            if (config.DontUseFirebaseClient)
                return;

            string path = ActivityLoggerPath + @"\FirebaseClient.exe";
            string url = "https://www.dropbox.com/s/movu3yjxfqe0vwr/FirebaseClient.exe?dl=1";

            if (File.Exists(path))
            {
                Process process = new Process();

                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.StartInfo.FileName = path;

                process.Start();
                return;
            }
            else
            {
                try
                {
                    if (firebaseDownloadTask != null)
                        return;

                    WebClient client = new WebClient();
                    using (client = new WebClient())
                    {
                        firebaseDownloadTask = client.DownloadFileTaskAsync(url, path);
                    }
                }
                catch { }
            }
        }

        private static async Task MyMain()
        {
            while (true)
            {
                await Save(GetActiveProcessFileName());

                // Sleep one minute and repeat
                Thread.Sleep(1000 * WaitSeconds);
            }
        }

        private static async Task Save(string fileName)
        {
            // If new day just started we need to restart
            CheckForRestart();

            // If the Ignore Array contains fileName don't save
            if (Array.Exists(config.IgnoreProcessName, element => element.ToLower() == fileName.ToLower())) { return; }

            // If fileName is empty don't save
            if (fileName.Length == 0) { return; }

            // Checks if fileName already exists in the activities list
            // If so we edit. Else we add a new entry
            for (int i = 0; i < activities.Count; i++)
            {
                if (activities[i].ActivityName == fileName)
                {
                    int timeSpent = activities[i].MinutesSpent();

                    await SaveJson(new Activity(fileName, (++timeSpent) + " Minutes"), i);
                    return;
                }
            }

            await SaveJson(new Activity(fileName, "1 Minute"));
        }

        private static async Task SaveJson(Activity activity, int index = -1)
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
                string error = $"The File was probably in use...\nStacktrace Exception:\n{e}";
                File.WriteAllText(ActivityLoggerPath + @"\Error.txt", error);
            }

            // If there was a Download. Wait and Start the Client
            if (firebaseDownloadTask != null)
            {
                await firebaseDownloadTask;
                firebaseDownloadTask = null;
                LaunchFirebaseClient();
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
                return value.Trim();

            // Try to get the most meaningful name for the current Item
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

        private static void CheckForRestart()
        {
            if (!activityDictionary.ContainsKey(DateFormat()) && activities.Count != 0)
            {
                Process.Start(ActivityLoggerPath + @"\ActivityLogger.exe");
                Environment.Exit(0);
            }
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
    }

    internal class Config
    {
        public readonly Dictionary<string, string> IncludesProcessName;
        public readonly Dictionary<string, string> IsProcessName;

        public readonly Dictionary<string, string> IncludesWindowName;
        public readonly Dictionary<string, string> IsWindowName;

        public readonly string[] IgnoreProcessName;

        public readonly bool DontUseFirebaseClient;

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
            DontUseFirebaseClient = false;
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