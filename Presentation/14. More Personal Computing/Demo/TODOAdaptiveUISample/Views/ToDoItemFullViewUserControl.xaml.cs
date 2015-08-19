using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TODOAdaptiveUISample.Views
{
    public sealed partial class ToDoItemFullViewUserControl : UserControl
    {
        public ToDoItemFullViewUserControl()
        {
            this.InitializeComponent();
        }

        private void Image_DragOver(object sender, DragEventArgs e)
        {
            if (DataContext is ViewModels.TodoItemViewModel)
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
        }

        private async void Image_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();

                if (items.Any())
                {
                    var storageFile = items[0] as StorageFile;

                    if (storageFile.ContentType.Contains("image"))
                    {
                        var viewModel = DataContext as ViewModels.TodoItemViewModel;
                        viewModel.SavePictureCommand.Execute(storageFile);
                    }
                }
            }
        }

        
    }
}
