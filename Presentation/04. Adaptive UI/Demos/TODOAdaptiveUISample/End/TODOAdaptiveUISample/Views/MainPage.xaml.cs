using System;
using TODOAdaptiveUISample.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace TODOAdaptiveUISample.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.ViewModel = this.DataContext as ViewModels.MainPageViewModel;
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            
            

            var titleBar = CoreApplication.GetCurrentView().TitleBar;
            //titleBar.ExtendViewIntoTitleBar = true;
            //TitleBar.Height = titleBar.Height;
            //Window.Current.SetTitleBar(TitleBar);

            //viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            //viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;
        }

        ViewModels.MainPageViewModel ViewModel { get; set; }

        // using a tapped event so we can have hitable areas inside the listviewitem without
        // actualy selecting the item
        private void TodoItem_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // If the inline viewer is visible, set the data context to the selected item
            if (InlineToDoItemViewGrid.Visibility == Visibility.Visible && (sender as Border).DataContext != null)
            {
                // First save changes on the previous one
                // TODO add a 'Dirty' flag to the ViewModel so we only save changes if there is something to save
                TodoItemViewModel vm = InlineViewerEditor.DataContext as TodoItemViewModel;
                if (vm != null)
                    vm.UpdateItemCommand.Execute(vm.TodoItem);

                // Set to the new item
                InlineViewerEditor.DataContext = (sender as Border).DataContext;
            }

            // If the inline panel is not showing, navigate to the separate editing page
            if (InlineToDoItemViewGrid.Visibility == Visibility.Collapsed && (sender as Border).DataContext != null)
            {
                ((App)(Application.Current)).NavigationService.Navigate(typeof(ToDoEditorPage), ((TodoItemViewModel)(sender as Border).DataContext).TodoItem.Id);
            }
        }

        private TextBox NewToDoItemNameTextBox = null;

        private AppBarButton AddNewItemConfirmButton = null;

        private void AddNewItemConfirmButton_Loaded(object sender, RoutedEventArgs e)
        {
            // This button is in a data template, so we can use the Loaded event to get a reference to it
            // You can't get at controls in Data Templates in Item Templates using their name
            AddNewItemConfirmButton = sender as AppBarButton;
        }

        private void TextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            NewToDoItemNameTextBox = textBox;

            if (!string.IsNullOrEmpty(textBox.Text)
                && textBox.Text.Length > 1)
            {
                if (AddNewItemConfirmButton != null)
                    AddNewItemConfirmButton.IsEnabled = true;

                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    // Handle 'Enter' key for keyboard users
                    if (e.Key == Windows.System.VirtualKey.Enter)
                    {
                        e.Handled = true;
                        CreateNewToDoItem(textBox);
                    }
                }
            }
            else
            {
                if (AddNewItemConfirmButton != null)
                    AddNewItemConfirmButton.IsEnabled = false;
            }
        }

        private void CreateNewToDoItem(TextBox textBox)
        {
            var vm = textBox.DataContext as ViewModels.MainPageViewModel;
            vm.AddItemCommand.Execute(textBox.Text);
            textBox.Text = string.Empty;
            textBox.Focus(Windows.UI.Xaml.FocusState.Programmatic);

            if (AddNewItemConfirmButton != null)
                AddNewItemConfirmButton.IsEnabled = false;
        }

        private void AddNewItemConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewToDoItemNameTextBox != null)
            {
                CreateNewToDoItem(NewToDoItemNameTextBox);
            }
        }

        
    }
}
