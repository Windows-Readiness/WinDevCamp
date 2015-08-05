using System;
using TODOAdaptiveUISample.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TODOAdaptiveUISample.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.ViewModel = this.DataContext as ViewModels.MainPageViewModel;
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;
        }

        ViewModels.MainPageViewModel ViewModel { get; set; }

        private void TodoItem_ItemClicked(object sender, ItemClickEventArgs e)
        {
            //MainPageViewModel vm = this.DataContext as MainPageViewModel;
            //var itemEditorDialog = new ToDoEditorContentDialog();
            //itemEditorDialog.DataContext = e.ClickedItem;
            //itemEditorDialog.PrimaryButtonCommand = vm.UpdateItemCommand;
            //itemEditorDialog.PrimaryButtonCommandParameter = ((TodoItemViewModel)(e.ClickedItem)).TodoItem;
            //itemEditorDialog.SecondaryButtonCommand = vm.RemoveItemCommand;
            //itemEditorDialog.SecondaryButtonCommandParameter = ((TodoItemViewModel)(e.ClickedItem)).TodoItem;
            //await itemEditorDialog.ShowAsync();

            ((App)(Application.Current)).NavigationService.Navigate(typeof(ToDoEditorPage), ((TodoItemViewModel)e.ClickedItem).TodoItem.Id);
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
