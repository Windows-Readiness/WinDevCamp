using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TODOAdaptiveUISample.Common;
using TODOAdaptiveUISample.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TODOAdaptiveUISample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserSelect : Page
    {
        private ListView accountListView;
        static public List<Account> accountList;

        public UserSelect()
        {
            this.InitializeComponent();
            accountListView = listView_UserTileList;
            accountListView.SelectionChanged += Button_SelectUser_Click;
            this.Loaded += OnLoadedActions;
        }

        private void OnLoadedActions(object sender, RoutedEventArgs e)
        {
            SetUpAccounts();
        }

        private void Button_SelectUser_Click(object sender, RoutedEventArgs e)
        {
            if (((ListView)sender).SelectedValue != null)
            {
                Account account = (Account)((ListView)sender).SelectedValue;
                this.Frame.Navigate(typeof(SignInView), account);
            }
        }

        private void Button_AddUser_Click(object sender, RoutedEventArgs e)
        {
            ((App)(Application.Current)).NavigationService.Navigate(typeof(SignInView));
        }

        private async void SetUpAccounts()
        {
            accountList = await AccountsHelper.LoadAccountList();
            accountListView.ItemsSource = accountList;

            if (accountList.Count == 0)
            {
                ((App)(Application.Current)).NavigationService.Navigate(typeof(SignInView));
            }
        }
    }
}
