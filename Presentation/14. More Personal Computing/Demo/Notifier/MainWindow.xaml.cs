using Microsoft.Azure.NotificationHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Notifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NotificationHubClient hub;

        string connectionStr = "<CONNECTION STR>";
        string hubPath = "<HUB PATH>";
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += (s, e) => { Combo.ItemsSource = ToastItem.Defaults(); Combo.SelectedIndex = 1; };
        }

        private async Task sendNotification(string content)
        {
            if (hub == null)
                hub = NotificationHubClient.CreateClientFromConnectionString(connectionStr, hubPath);

            //var toast = @"<toast launch=""MainPage.xaml?param=neworder""><visual><binding template=""ToastText01""><text id=""1"">" + msg + @"</text></binding></visual></toast>";

            if (rawCheck.IsChecked == true)
            {
                Notification notification = new WindowsNotification(content);
                notification.Headers.Add("X-WNS-Type", "wns/raw");
                await hub.SendNotificationAsync(notification);
            }
            else
            {
                await hub.SendWindowsNativeNotificationAsync(content);
            }

            
        }
        

        

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
            sendNotification(Box.Text);
        }

        private void ComboBox_Selected(object sender, RoutedEventArgs e)
        {
            var item = (sender as ComboBox).SelectedItem;
            if (item != null)
            {
                Box.Text = (item as ToastItem).Content;
                rawCheck.IsChecked = (item as ToastItem).Raw;
            }
        }
    }

    public class ToastItem
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public bool Raw { get; set; } = false;

        public override string ToString()
        {
            return Name;
        }

        public static List<ToastItem> Defaults()
        {
            var list = new List<ToastItem>();

            // http://blogs.msdn.com/b/tiles_and_toasts/archive/2015/07/02/adaptive-and-interactive-toast-notifications-for-windows-10.aspx

            list.Add(new ToastItem() { Name = "TextOnly", Content = 
$@"<toast launch=""MainPage.xaml?param=neworder"">
    <visual>
        <binding template=""ToastText01"">
            <text id=""1"">This is a toast</text>
        </binding>
    </visual>
</toast>"
            });

            list.Add(new ToastItem()
            {
                Name = "Reminder",
                Content =
$@"<toast activationType='background' launch='args' scenario='reminder' arguments='hello'>
    <visual>
        <binding template='ToastGeneric'>
            <text>Prepare notification demo for MVA</text>
        </binding>
    </visual>
    <actions hint-systemCommands = 'SnoozeAndDismiss' >
        <action content='complete' activationType='background' arguments='complete' />
    </actions>
</toast>"
            });

            list.Add(new ToastItem()
            {
                Name = "ToDoItemDue",
                Content =
$@"<toast activationType='background' launch='args' scenario='reminder' arguments='fe8f5b3c-8ea4-4c23-8bc8-555b39c0f4da'>
    <visual>
        <binding template='ToastGeneric'>
            <text>Don't forget</text>
            <text>'Prepare notification demo for MVA' is due today</text>
            <text>You should do this so you don't look stupid in front of everyone</text>
            <image src='ms-appdata:///Local/andy.jpg'/>
        </binding>
    </visual>
    <actions hint-systemCommands = 'SnoozeAndDismiss' >
        <action content='Edit Item' activationType='foreground' arguments='edit:fe8f5b3c-8ea4-4c23-8bc8-555b39c0f4da' />
        <action content='Complete' activationType='background' arguments='complete:fe8f5b3c-8ea4-4c23-8bc8-555b39c0f4da' />
    </actions>
</toast>"
            });

            list.Add(new ToastItem()
            {
                Name = "UpdateBadge",
                Content ="new_items:3",
                Raw = true
            });


            return list;

        }
    }
}
