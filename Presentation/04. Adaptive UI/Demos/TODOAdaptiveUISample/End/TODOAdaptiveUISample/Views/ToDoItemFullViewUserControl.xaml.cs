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
        public event EventHandler<CommandCompletedEventArgs> CommandCompleted;

        public ToDoItemFullViewUserControl()
        {
            this.InitializeComponent();
            Categories = new List<Models.TaskColor>();
            Categories.Add(Models.TaskColor.Purple);
            Categories.Add(Models.TaskColor.Pink);
            Categories.Add(Models.TaskColor.Orange);
            Categories.Add(Models.TaskColor.Yellow);
            Categories.Add(Models.TaskColor.Teal);
            Categories.Add(Models.TaskColor.Green);
            Categories.Add(Models.TaskColor.Gray);
            Categories.Add(Models.TaskColor.Blue);
        }

        private void Delete_Clicked(object sender, RoutedEventArgs e)
        {
            // if using from mainpage, use eventHandler so thing are updated
            if (DeleteItemClicked != null)
                DeleteItemClicked(this, new EventArgs());
            else
                (DataContext as TodoItemViewModel).RemoveItemCommand.Execute(null);
        }

        List<Models.TaskColor> Categories;

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                (DataContext as TodoItemViewModel).TodoItem.Color = (Models.TaskColor)e.AddedItems[0];
            }
        }

        private async void TakePictureButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DataContext as TodoItemViewModel);
            await vm.TakePicture();
            if (CommandCompleted != null)
            {
                CommandCompleted(this, new CommandCompletedEventArgs() { ViewModel = vm });
            }
        }

        private async void SelectPictureButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DataContext as TodoItemViewModel);
            await vm.SelectPicture();
            if (CommandCompleted != null)
            {
                CommandCompleted(this, new CommandCompletedEventArgs() { ViewModel = vm });
            }
        }
    }

    public class CommandCompletedEventArgs : EventArgs
    {
        public TodoItemViewModel ViewModel { get; set; }
    }
}
