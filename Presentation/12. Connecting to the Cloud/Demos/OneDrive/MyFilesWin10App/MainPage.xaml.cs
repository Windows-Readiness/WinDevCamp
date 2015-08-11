using MyFilesModelsWin10App.Controllers;
using MyFilesWin10App.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MyFilesWin10App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var last = (App.Current as App).NavigationStack.Last();
            (App.Current as App).Items = await MyFilesController.GetMyImages(last.Id);
            this.DataContext = (App.Current as App).Items;
            wait.IsActive = false;
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //get the contextual item tapped
            MyFilesModel item = (MyFilesModel)((StackPanel)sender).DataContext;

            //add to navigation stack
            (App.Current as App).NavigationStack.Add(item);

            //determine if this is a folder or file
            if (item.Type == "Folder")
            {
                //navigate to the detail page and pass the selected index
                Frame.Navigate(typeof(MainPage), (App.Current as App).Items.IndexOf((MyFilesModel)((StackPanel)sender).DataContext));
            }
            else
            {
                //navigate to the detail page and pass the selected index
                Frame.Navigate(typeof(ItemDetail), (App.Current as App).Items.IndexOf((MyFilesModel)((StackPanel)sender).DataContext));
            }
        }
    }
}
