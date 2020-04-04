using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyActivityLogs
{
    public partial class MainWindow : Window
    {
        List<Activity> items = new List<Activity>();
        public MainWindow()
        {
            InitializeComponent();
            items.Add(new Activity() { Title = "Learn C#", Completion = 55 });
            items.Add(new Activity() { Title = "Complete this WPF tutorial", Completion = 1 });
            items.Add(new Activity() { Title = "Homework", Completion = 90 }); // -1

            SortList();

            ActivitiesItemsControl.ItemsSource = items;
        }

        private void SortList()
        {
            items = items.OrderBy(o => o.Completion).ToList();
            items.Reverse();
        }
    }


    public class Activity
    {
        public string Title { get; set; }
        public int Completion { get; set; }
    }
}
