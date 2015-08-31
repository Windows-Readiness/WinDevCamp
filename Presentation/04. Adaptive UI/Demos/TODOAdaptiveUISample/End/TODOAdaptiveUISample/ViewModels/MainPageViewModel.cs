using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TODOAdaptiveUISample.Mvvm;
using Windows.UI.Xaml.Navigation;

namespace TODOAdaptiveUISample.ViewModels
{
    public class MainPageViewModel : Mvvm.ViewModelBase
    {
        #region Properties
        Repositories.ITodoItemRepository _todoItemRepository;

        bool _busy = false;
        public bool Busy { get { return _busy; } set { Set(ref _busy, value); } }

        private ObservableCollection<ViewModels.TodoItemViewModel> _ItemVMs = new ObservableCollection<TodoItemViewModel>();
        public ObservableCollection<ViewModels.TodoItemViewModel> ItemVMs { get { return _ItemVMs; } private set { Set(ref _ItemVMs, value); } }

        private ViewModels.TodoItemViewModel _SelectedItem = default(ViewModels.TodoItemViewModel);
        public ViewModels.TodoItemViewModel SelectedItem { get { return _SelectedItem; } set { Set(ref _SelectedItem, value); SelectedItemIsSelected = (value != null); } }

        private bool _SelectedItemIsSelected = default(bool);
        public bool SelectedItemIsSelected { get { return _SelectedItemIsSelected; } set { Set(ref _SelectedItemIsSelected, value); } }
        #endregion

        #region Constructor
        public MainPageViewModel()
        {
            _todoItemRepository = Repositories.TodoItemFileRepository.GetInstance();

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // designtime sample data
                var data = _todoItemRepository.Sample().Select(x => new ViewModels.TodoItemViewModel(x));
                this.ItemVMs = new ObservableCollection<ViewModels.TodoItemViewModel>(data);
            }
            else
            {
                // update commands
                this.PropertyChanged += (s, e) =>
                {
                    this.AddItemCommand.RaiseCanExecuteChanged();
                    this.RemoveItemCommand.RaiseCanExecuteChanged();
                };
            }
        }
        #endregion

        #region Methods
        public override Task OnNavigatedToAsync(string parameter, NavigationMode mode)
        {
            LoadCommand.Execute(null);
            return Task.FromResult<object>(null);
        }

        public override Task OnNavigatedFromAsync(bool suspending)
        {
            // TODO add any logic to execute when navigating from this page
            return Task.FromResult<object>(null);
        }
        #endregion

        #region Commands

        Mvvm.Command _LoadCommand = default(Mvvm.Command);
        public Mvvm.Command LoadCommand { get { return _LoadCommand ?? (_LoadCommand = new Mvvm.Command(ExecuteLoadCommand, CanExecuteLoadCommand)); } }
        private bool CanExecuteLoadCommand() { return !Busy; }
        private async void ExecuteLoadCommand()
        {
            try
            {
                Busy = true;

                var data = await _todoItemRepository.RefreshTodoItemsAsync();
                this.ItemVMs.Clear();
                foreach (var item in data.OrderBy(x => x.Title))
                {
                    this.ItemVMs.Add(new ViewModels.TodoItemViewModel(item));
                }
            }
            finally { Busy = false; }
        }

        Mvvm.Command<string> _AddItemCommand = default(Mvvm.Command<string>);
        public Mvvm.Command<string> AddItemCommand { get { return _AddItemCommand ?? (_AddItemCommand = new Mvvm.Command<string>(ExecuteAddItemCommand, CanExecuteAddItemCommand)); } }
        private bool CanExecuteAddItemCommand(string title) { return true; }
        private async void ExecuteAddItemCommand(string title)
        {
            try
            {
                var item = _todoItemRepository.Factory(title: title);
                await _todoItemRepository.InsertTodoItem(item);

                var itemVM = new ViewModels.TodoItemViewModel(item);
                var index = this.ItemVMs.IndexOf(this.SelectedItem);
                this.ItemVMs.Insert((index > -1) ? index : 0, itemVM);
                this.SelectedItem = itemVM;
            }
            catch { this.SelectedItem = null; }
        }

        Mvvm.Command<Models.TodoItem> _RemoveItemCommand = default(Mvvm.Command<Models.TodoItem>);
        public Mvvm.Command<Models.TodoItem> RemoveItemCommand { get { return _RemoveItemCommand ?? (_RemoveItemCommand = new Mvvm.Command<Models.TodoItem>(ExecuteRemoveItemCommand, CanExecuteRemoveItemCommand)); } }
        private bool CanExecuteRemoveItemCommand(Models.TodoItem param) { return this.SelectedItem != null; }
        private async void ExecuteRemoveItemCommand(Models.TodoItem param)
        {
            try
            {
                var index = this.ItemVMs.IndexOf(this.SelectedItem);

                await _todoItemRepository.DeleteTodoItem(this.SelectedItem.TodoItem);

                this.ItemVMs.Remove(this.SelectedItem);
                //this.SelectedItem = this.ItemVMs.First();
            }
            catch { this.SelectedItem = null; }
        }

        Mvvm.Command<Models.TodoItem> _UpdateItemCommand = default(Mvvm.Command<Models.TodoItem>);
        public Mvvm.Command<Models.TodoItem> UpdateItemCommand { get { return _UpdateItemCommand ?? (_UpdateItemCommand = new Mvvm.Command<Models.TodoItem>(ExecuteUpdateItemCommand, CanExecuteUpdateItemCommand)); } }
        private bool CanExecuteUpdateItemCommand(Models.TodoItem param) { return this.SelectedItem != null; }
        private async void ExecuteUpdateItemCommand(Models.TodoItem param)
        {
            try
            {
                var index = this.ItemVMs.IndexOf(this.SelectedItem);

                await _todoItemRepository.UpdateTodoItem(this.SelectedItem.TodoItem);

                this.SelectedItem = this.ItemVMs[index];
            }
            catch { this.SelectedItem = null; }
        }

        #endregion  
    }
}
