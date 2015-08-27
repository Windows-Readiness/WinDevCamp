using System;
using TODOAdaptiveUISample.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
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

            Window.Current.SizeChanged += Current_SizeChanged;
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width < 400)
            {
                splitView.OpenPaneLength = e.Size.Width - 20;
            }
            else if (splitView.OpenPaneLength != 400)
            {
                splitView.OpenPaneLength = 400;
            }

            if (e.Size.Width >= 600 && ToDoListView.SelectedItem == null)
            {
                try
                {
                    ToDoListView.SelectedIndex = 0;
                }
                catch
                {

                }
            }
        }

        ViewModels.MainPageViewModel ViewModel { get; set; }

        // using a tapped event so we can have hitable areas inside the listviewitem without
        // actualy selecting the item
        private void TodoItem_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            

            // If the inline panel is not showing, navigate to the separate editing page
            if ((sender as Border).DataContext != null)
            {
                ToDoListView.SelectedItem = ((TodoItemViewModel)(sender as Border).DataContext);
                //((App)(Application.Current)).NavigationService.Navigate(typeof(ToDoEditorPage), ((TodoItemViewModel)(sender as Border).DataContext).TodoItem.Id);
            }
        }

        private void ToDoListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InlineViewerEditor.DataContext != null && InlineViewerEditor.DataContext is TodoItemViewModel)
            {
                (InlineViewerEditor.DataContext as TodoItemViewModel).UpdateItemCommand.Execute(null);
            }

            if (ToDoListView.SelectedItem == null)
            {
                splitView.IsPaneOpen = false;
                InlineViewerEditor.DataContext = null;
            }
            else
            {
                InlineViewerEditor.DataContext = ToDoListView.SelectedItem;
                splitView.IsPaneOpen = true;
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

        private void splitView_PaneClosed(SplitView sender, object args)
        {
            if (Window.Current.Bounds.Width > 600)
                try
                {
                    ToDoListView.SelectedIndex = 0;
                }
                catch{}
            else
                ToDoListView.SelectedItem = null;

        }

        private void InlineViewerEditor_DeleteItemClicked(object sender, EventArgs e)
        {
            if (InlineViewerEditor.DataContext != null && InlineViewerEditor.DataContext is TodoItemViewModel)
            {
                var vm = this.DataContext as MainPageViewModel;
                vm.RemoveItemCommand.Execute(null);
            }
            
        }
    }
}
