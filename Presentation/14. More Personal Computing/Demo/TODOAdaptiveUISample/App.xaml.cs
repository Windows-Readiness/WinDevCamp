using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;

namespace TODOAdaptiveUISample
{
    sealed partial class App : Common.BootStrapper
    {
        public App() : base()
        {
            this.InitializeComponent();
        }

        public override Task OnLaunchedAsync(ILaunchActivatedEventArgs e)
        {
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SetPreferredMinSize(new Windows.Foundation.Size(320, 500));

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
            this.NavigationService.Navigate(typeof(Views.UserSelect));
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
