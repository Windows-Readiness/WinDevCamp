using System;
using System.Linq;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Activation;
using System.Diagnostics;
using Windows.Networking.PushNotifications;
using Microsoft.WindowsAzure.Messaging;
using Windows.ApplicationModel.Background;

namespace TODOAdaptiveUISample.Common
{
    // BootStrapper is a drop-in replacement of Application
    // - OnInitializeAsync is the first in the pipeline, if launching
    // - OnLaunchedAsync is required, and second in the pipeline
    // - OnActivatedAsync is first in the pipeline, if activating
    // - NavigationService is an automatic property of this class
    public abstract class BootStrapper : Application
    {

        string connectionStr = "<CONNECTION STR>";
        string hubPath = "<HUB PATH>";

        static readonly string ACTIONABLE_BACKGROUND_ENTRY_POINT = typeof(NotificationTask.NotificationTask).FullName;
        static readonly string RAW_BACKGROUND_ENTRY_POINT = typeof(NotificationTask.RawNotificationTask).FullName;

        /// <summary>
        /// Event to allow views and viewmodels to intercept the Hardware/Shell Back request and 
        /// implement their own logic, such as closing a dialog. In your event handler, set the
        /// Handled property of the BackRequestedEventArgs to true if you do not want a Page
        /// Back navigation to occur.
        /// </summary>
        public event EventHandler<Windows.UI.Core.BackRequestedEventArgs> BackRequested;

        public BootStrapper()
        {
            this.Resuming += (s, e) =>
            {
                OnResuming(s, e);
            };
            this.Suspending += async (s, e) =>
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                this.NavigationService.Suspending();
                await OnSuspendingAsync(s, e);
                deferral.Complete();
            };
        }

        #region properties

        public Frame RootFrame { get; set; }
        public Services.NavigationService.NavigationService NavigationService { get; private set; }
        protected Func<SplashScreen, Page> SplashFactory { get; set; }

        #endregion

        #region activated

        protected override async void OnActivated(IActivatedEventArgs e) { await InternalActivatedAsync(e); }
        protected override async void OnCachedFileUpdaterActivated(CachedFileUpdaterActivatedEventArgs args) { await InternalActivatedAsync(args); }
        protected override async void OnFileActivated(FileActivatedEventArgs args) { await InternalActivatedAsync(args); }
        protected override async void OnFileOpenPickerActivated(FileOpenPickerActivatedEventArgs args) { await InternalActivatedAsync(args); }
        protected override async void OnFileSavePickerActivated(FileSavePickerActivatedEventArgs args) { await InternalActivatedAsync(args); }
        protected override async void OnSearchActivated(SearchActivatedEventArgs args) { await InternalActivatedAsync(args); }
        protected override async void OnShareTargetActivated(ShareTargetActivatedEventArgs args) { await InternalActivatedAsync(args); }

        private async Task InternalActivatedAsync(IActivatedEventArgs e)
        {
            await this.OnActivatedAsync(e);
            Window.Current.Activate();
        }

        #endregion

        protected override void OnLaunched(LaunchActivatedEventArgs e) { InternalLaunchAsync(e as ILaunchActivatedEventArgs); }

        private async void InternalLaunchAsync(ILaunchActivatedEventArgs e)
        {
            UIElement splashScreen = default(UIElement);
            if (this.SplashFactory != null)
            {
                splashScreen = this.SplashFactory(e.SplashScreen);
                Window.Current.Content = splashScreen;
            }

            this.RootFrame = this.RootFrame ?? new Frame();
            this.RootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
            this.NavigationService = new Services.NavigationService.NavigationService(this.RootFrame);

            // the user may override to set custom content
            await OnInitializeAsync();

            if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                try { /* TODOAdaptiveUISample: restore state */ }
                finally { await this.OnLaunchedAsync(e); }
            }
            else
            {
                await this.OnLaunchedAsync(e);
            }

            // if the user didn't already set custom content use rootframe
            if (Window.Current.Content == splashScreen)
            {
                Window.Current.Content = this.RootFrame;
            }
            if (Window.Current.Content == null)
            {
                Window.Current.Content = this.RootFrame;
            }
            Window.Current.Activate();

            // Install VCD
            try
            {
                var storageFile =
                await Windows.Storage.StorageFile
                .GetFileFromApplicationUriAsync(new Uri("ms-appx:///vcd.xml"));

                await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager
                    .InstallCommandDefinitionsFromStorageFileAsync(storageFile);
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }

            // Hook up notifications
            // Modify variables at the begining of this page and uncomment these lines
            //var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            //NotificationHub hub = new NotificationHub(hubPath, connectionStr);
            //await hub.RegisterNativeAsync(channel.Uri);

            // Hook up background task for notifications
            await RegisterRawNotificationBackgroundTask();
            await RegisterActionableToastBackgroundTask();

            // Hook up the default Back handler
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        /// <summary>
        /// Default Hardware/Shell Back handler overrides standard Back behavior that navigates to previous app
        /// in the app stack to instead cause a backward page navigation.
        /// Views or Viewodels can override this behavior by handling the BackRequested event and setting the
        /// Handled property of the BackRequestedEventArgs to true.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            if (BackRequested != null)
            {
                BackRequested(this, e);
            }

            if (!e.Handled)
            {
                if (this.RootFrame.CanGoBack)
                {
                    RootFrame.GoBack();
                    e.Handled = true;
                }
            }
        }

        private async Task<bool> RegisterActionableToastBackgroundTask()
        {
            // Unregister any previous exising background task
            UnregisterBackgroundTask(ACTIONABLE_BACKGROUND_ENTRY_POINT);

            // Request access
            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();

            // If denied
            if (status != BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity && status != BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity)
                return false;

            // Construct the background task
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder()
            {
                Name = ACTIONABLE_BACKGROUND_ENTRY_POINT,
                TaskEntryPoint = ACTIONABLE_BACKGROUND_ENTRY_POINT
            };

            // Set trigger for Toast History Changed
            builder.SetTrigger(new ToastNotificationActionTrigger());

            // And register the background task
            BackgroundTaskRegistration registration = builder.Register();

            return true;
        }

        private async Task<bool> RegisterRawNotificationBackgroundTask()
        {
            // Unregister any previous exising background task
            UnregisterBackgroundTask(RAW_BACKGROUND_ENTRY_POINT);

            // Request access
            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();

            // If denied
            if (status != BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity && status != BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity)
                return false;

            // Construct the background task
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder()
            {
                Name = RAW_BACKGROUND_ENTRY_POINT,
                TaskEntryPoint = RAW_BACKGROUND_ENTRY_POINT
            };

            // Set trigger for Toast History Changed
            builder.SetTrigger(new PushNotificationTrigger());

            // And register the background task
            BackgroundTaskRegistration registration = builder.Register();

            return true;
        }

        private static void UnregisterBackgroundTask(string taskName)
        {
            var task = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(i => i.Name.Equals(taskName));

            if (task != null)
                task.Unregister(true);
        }

        #region overrides

        public virtual Task OnInitializeAsync() { return Task.FromResult<object>(null); }
        public virtual async Task OnActivatedAsync(IActivatedEventArgs e)
        {
            // TODO: make this better, fix it

            UIElement splashScreen = default(UIElement);
            if (this.SplashFactory != null)
            {
                splashScreen = this.SplashFactory(e.SplashScreen);
                Window.Current.Content = splashScreen;
            }

            this.RootFrame = this.RootFrame ?? new Frame();
            this.RootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
            this.NavigationService = new Services.NavigationService.NavigationService(this.RootFrame);

            // the user may override to set custom content
            await OnInitializeAsync();

            if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                try { /* TODOAdaptiveUISample: restore state */ }
                finally { await this.OnLaunchedAsync(null); }
            }
            else
            {
                await this.OnLaunchedAsync(null);
            }

            // if the user didn't already set custom content use rootframe
            if (Window.Current.Content == splashScreen)
            {
                Window.Current.Content = this.RootFrame;
            }
            if (Window.Current.Content == null)
            {
                Window.Current.Content = this.RootFrame;
            }
            Window.Current.Activate();

            // Hook up the default Back handler
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        public abstract Task OnLaunchedAsync(ILaunchActivatedEventArgs e);
        protected virtual void OnResuming(object s, object e) { }
        protected virtual Task OnSuspendingAsync(object s, SuspendingEventArgs e) { return Task.FromResult<object>(null); }

        #endregion
    }
}
