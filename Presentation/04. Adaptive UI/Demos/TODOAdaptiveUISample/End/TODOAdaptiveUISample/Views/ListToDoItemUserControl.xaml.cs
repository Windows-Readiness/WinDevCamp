using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TODOAdaptiveUISample.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TODOAdaptiveUISample.Views
{
    public sealed partial class ListToDoItemUserControl : UserControl
    {
        public ListToDoItemUserControl()
        {
            this.InitializeComponent();
        }

        private void Left_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch) return;
            if (DataContext != null)
            {
                (DataContext as TodoItemViewModel).ToggleCompletedCommand.Execute(null);
            }
            e.Handled = true;
        }

        private void Right_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch) return;
            if (DataContext != null)
            {
                (DataContext as TodoItemViewModel).ToggleFavoriteCommand.Execute(null);
            }
            e.Handled = true;
        }
    }
}
