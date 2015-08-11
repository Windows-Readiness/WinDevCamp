using MyFilesModelsWin10App.Controllers;
using MyFilesWin10App.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MyFilesWin10App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ItemDetail : Page
    {
        private bool initialized = false;

        public ItemDetail()
        {
            this.InitializeComponent();
            this.Loaded += ItemDetail_Loaded;
        }
        
        private async void ItemDetail_Loaded(object sender, RoutedEventArgs e)
        {
            var last = (App.Current as App).NavigationStack.Last();
            this.DataContext = (App.Current as App).FileItems;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () => { imgFlipView.SelectedIndex = (App.Current as App).FileItems.IndexOf(last); });
            await updateUI(last);
            wait.IsActive = false;
            initialized = true;
        }

        private async void imgFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //load new picture
            if (imgFlipView.SelectedIndex != -1 && initialized)
            {
                wait.IsActive = true;
                var item = (App.Current as App).FileItems[imgFlipView.SelectedIndex];
                await updateUI(item);
                wait.IsActive = false;
            }
        }

        private async Task updateUI(MyFilesModel item)
        {
            if (!item.ImageLoaded)
            {
                item.Bitmap = await MyFilesController.GetImage(item, (int)Window.Current.Bounds.Width);
                item.ImageLoaded = true;
            }
        }
    }
}
