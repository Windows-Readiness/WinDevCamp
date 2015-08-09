using LifecycleDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LifecycleDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Page2 : Page
    {
        public Page2()
        {
            this.InitializeComponent();

            ViewModel = new TheViewModel();
            DataContext = ViewModel;

            ViewModel.LoadData();
        }

        TheViewModel ViewModel { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ((App)App.Current).BackRequested += Page2_BackRequested;

            if (e.NavigationMode == NavigationMode.New)
            {
                // If this is a new navigation, this is a fresh launch so we can
                // discard any saved state
                ApplicationData.Current.LocalSettings.Values.Remove("TheWorkInProgress");
            }
            else
            {
                // Try to restore state if any, in case we were terminated
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("TheWorkInProgress"))
                {
                    var composite = ApplicationData.Current.LocalSettings.Values["TheWorkInProgress"] as ApplicationDataCompositeValue;
                    textBox1.Text = (string)composite["Field1"];
                    textBox2.Text = (string)composite["Field2"];

                    // We're done with it, so remove it
                    ApplicationData.Current.LocalSettings.Values.Remove("TheWorkInProgress");
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ((App)App.Current).BackRequested -= Page2_BackRequested;

            bool suspending = ((App)App.Current).IsSuspending;
            if (suspending)
            {
                // Save volatile state in case we get terminated later on, then
                // we can restore as if we'd never been gone :)
                var composite = new ApplicationDataCompositeValue();
                composite["Field1"] = textBox1.Text;
                composite["Field2"] = textBox2.Text;
                ApplicationData.Current.LocalSettings.Values["TheWorkInProgress"] = composite;
            }
        }

        private void Page2_BackRequested(object sender, BackRequestedEventArgs e)
        {
            // When leaving the page, save the app data
            ViewModel.SaveData();
        }
    }
}
