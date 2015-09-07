using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Networking.PushNotifications;
using Windows.UI.Core;

namespace TODOAzureSample
{
    sealed partial class App : Common.BootStrapper
    {
        // This MobileServiceClient has been configured to communicate with the Azure Mobile Service and
        // Azure Gateway using the application key. You're all set to start working with your Mobile Service!
        public static MobileServiceClient MobileService =
            new MobileServiceClient(
                "https://todo10240-code.azurewebsites.net",
                "https://default-web-westeuropedadb315bf36143afbefc3a441bcf037b.azurewebsites.net",
                "fQwvWbbMlAdpluosGcQKPXSsRTpcOG70"
            );

        // Uncomment this code for configuring the MobileServiceClient to communicate with your local
        // test project for debugging purposes.
        //public static MobileServiceClient MobileService = new MobileServiceClient(
        //    "http://localhost:50003"
        //);
        public App() : base()
        {
            this.InitializeComponent();
        }

        private async void InitNotificationsAsync()
        {
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

            await MobileService.GetPush().RegisterAsync(channel.Uri);
        }

        public override Task OnLaunchedAsync(ILaunchActivatedEventArgs e)
        {
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SetPreferredMinSize(new Windows.Foundation.Size(320, 500));

            InitNotificationsAsync();

            // Apply shell drawn Back button
            this.RootFrame.Navigated += (s, a) =>
            {
                if (this.RootFrame.CanGoBack)
                {
                    // Setting this visible is ignored on Mobile and when in tablet mode!     
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                }
                else
                {
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                }
            };

            // Navigate to MainPage
            this.NavigationService.Navigate(typeof(Views.MainPage));
            return Task.FromResult<object>(null);
        }

        protected async override Task OnSuspendingAsync(object s, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // TODO Add any logic required on app suspension
            //await Task.Delay(500);

            deferral.Complete();
        }
    }
}
