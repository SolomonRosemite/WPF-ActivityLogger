using System.Windows.Controls;
using System.Diagnostics;
using System.Windows;
using System.IO;

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using QRCoder;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Image = System.Drawing.Image;
using System.Windows.Navigation;

namespace MyActivityLogs.Pages
{
    public partial class SettingsPage : Page
    {
        private DateTime start;
        private DateTime end;

        public SettingsPage() => InitPage();

        public SettingsPage(DateTime start, DateTime end)
        {
            InitPage();

            this.start = start;
            this.end = end;

            MyDatePickerStart.SelectedDate = start;
            MyDatePickerEnd.SelectedDate = end;
        }

        private void InitPage()
        {
            InitializeComponent();

            string userSecret = GetUserSecret();

            if (userSecret == null)
            {
                QrCodeDescription.Visibility = Visibility.Hidden;
                QrCodeTitle.Visibility = Visibility.Hidden;
                QrCodeTryOutBtn.Visibility = Visibility.Hidden;
                QrCodeImage.Visibility = Visibility.Hidden;
                return;
            }

            var qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(userSecret, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap bitmap = qrCode.GetGraphic(20);

            QrCodeImage.Source = ConvertMap(bitmap);
        }

        private static string GetUserSecret()
        {
            if (!File.Exists(MainWindow.ActivityLoggerPath + @"\Config.json")) return null;

            string jsonFromFile;
            using (var reader = new StreamReader(MainWindow.ActivityLoggerPath + @"\Config.json"))
            {
                jsonFromFile = reader.ReadToEnd();
                if (!jsonFromFile.Contains("{")) return null;
            }

            try
            {
                var res = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonFromFile);
                res.TryGetValue("userSecret", out var secret);

                if (secret != null) return secret.ToString();
            }
            catch { }

            return null;
        }

        private static BitmapImage ConvertMap(Image src)
        {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            BitmapImage image = new BitmapImage();
            ms.Seek(0, SeekOrigin.Begin);

            image.BeginInit();
            image.StreamSource = ms;
            image.EndInit();

            return image;
        }

        private void GetAppEvent(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "https://rosemitedocs.web.app/docs/WPF-ActivityLogger-getting-started/#mobile-app");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(MainWindow.ActivityLoggerPath + @"\SavedActivities.json"))
            {
                OpenWithDefaultProgram(MainWindow.ActivityLoggerPath);
                return;
            }

            OpenWithDefaultProgram(MainWindow.ActivityLoggerPath + @"\SavedActivities.json");
        }

        private static void OpenWithDefaultProgram(string path)
        {
            Process p = new Process { StartInfo = { FileName = "explorer", Arguments = "\"" + path + "\"" } };
            p.Start();
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            MainWindow.Load();

            MainWindow.mainWindow.DailyButton();
        }

        private void ConvertTime(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowInHours = !MainWindow.ShowInHours;

            if (MainWindow.ShowInHours)
            {
                ConvertTimeButton.Content = "View in Minutes";
            }
            else
            {
                ConvertTimeButton.Content = "View in Hours";
            }

            MainWindow.Load(true);

            MainWindow.mainWindow.DailyButton();
        }

        private void UpdateDates(object sender, RoutedEventArgs e)
        {
            if (start.Date <= end.Date)
            {
                const string dateFormat = "dd.MM.yyyy";

                // Save Updates dates
                string json = JsonConvert.SerializeObject(new[] { start.ToString(dateFormat), end.ToString(dateFormat) }, Formatting.Indented);
                File.WriteAllText(MainWindow.ActivityLoggerPath + @"\MyActivityLogs\Dates.json", json);

                Popup popup = new Popup("Dates have been Updated.");
                popup.Show();

                MainWindow.Load();
            }
            else
            {
                Popup popup = new Popup("The Starting date can't be after the Ending date.");
                popup.Show();
            }
        }

        private void MyDatePickerStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            start = (DateTime)e.AddedItems[0];
        }

        private void MyDatePickerEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            end = (DateTime)e.AddedItems[0];
        }
    }
}
