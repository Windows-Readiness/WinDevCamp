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
    public sealed partial class ToDoItemFullViewUserControl : UserControl
    {

        public event EventHandler DeleteItemClicked;

        public ToDoItemFullViewUserControl()
        {
            this.InitializeComponent();
        }

        private void Delete_Clicked(object sender, RoutedEventArgs e)
        {
            // if using from mainpage, use eventHandler so thing are updated
            if (DeleteItemClicked != null)
                DeleteItemClicked(this, new EventArgs());
            else
                (DataContext as TodoItemViewModel).RemoveItemCommand.Execute(null);
        }
    }
}
