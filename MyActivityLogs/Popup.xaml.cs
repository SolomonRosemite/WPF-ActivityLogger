using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyActivityLogs
{
    public partial class Popup : Window
    {
        string pathpathToDirectory;
        public Popup(string message, string pathpathToDirectory = "")
        {
            InitializeComponent();
            MessageLabel.Content = message;

            if (pathpathToDirectory.Length != 0)
            {
                this.pathpathToDirectory = pathpathToDirectory;
                return;
            }
            FixButton.Visibility = Visibility.Hidden;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Fix(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", pathpathToDirectory);
            Close();
        }
    }
}
