using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TODOAzureSample.Models;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Navigation;

namespace TODOAzureSample.ViewModels
{
    public class TodoItemViewModel : Mvvm.ViewModelBase
    {
        #region Properties
        Repositories.ITodoItemRepository _todoItemRepository;

        private Models.TodoItem _TodoItem = default(Models.TodoItem);
        public Models.TodoItem TodoItem { get { return _TodoItem; } set { Set(ref _TodoItem, value); } }

        bool _busy = false;
        public bool Busy { get { return _busy; } set { Set(ref _busy, value); } }

        public bool IsItemModified { get; set; }

        #endregion

        #region Constructors

        public TodoItemViewModel()
        {
            //_todoItemRepository = Repositories.TodoItemFileRepository.GetInstance();
            _todoItemRepository = Repositories.TodoItemAzureRepository.GetInstance();

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // designtime sample data
                var data = _todoItemRepository.Sample(1).First();
                this.TodoItem = data;
            }
        }

        public TodoItemViewModel(Models.TodoItem todo)
        {
            //_todoItemRepository = Repositories.TodoItemFileRepository.GetInstance();
            _todoItemRepository = Repositories.TodoItemAzureRepository.GetInstance();

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // designtime sample data
                var data = _todoItemRepository.Sample(1).First();
                this.TodoItem = data;
            }
            else
            {
                this.TodoItem = todo;
                IsItemModified = false;

                this.TodoItem.PropertyChanged += (s, a) => IsItemModified = true;
            }
        }
        #endregion

        #region Methods
        public override async void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            TodoItem = await _todoItemRepository.RefreshTodoItemAsync((string)parameter);
            this.RemoveItemCommand.RaiseCanExecuteChanged();
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            // logic to execute when navigating from this page
            if (this.TodoItem != null && IsItemModified)
                await _todoItemRepository.UpdateTodoItem(this.TodoItem);
        }
        #endregion

        #region Commands

        Mvvm.Command _SelectPictureCommand = default(Mvvm.Command);
        public Mvvm.Command SelectPictureCommand { get { return _SelectPictureCommand ?? (_SelectPictureCommand = new Mvvm.Command(ExecuteSelectPictureCommand, CanExecuteSelectPictureCommand)); } }
        private bool CanExecuteSelectPictureCommand() { return !Busy; }
        private async void ExecuteSelectPictureCommand()
        {
            try
            {
                Busy = true;

                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.ViewMode = PickerViewMode.Thumbnail;
                openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".png");

                StorageFile file = await openPicker.PickSingleFileAsync();

                if (file != null)
                {
                    // Copy the file into local folder
                    await file.CopyAsync(ApplicationData.Current.LocalFolder, file.Name, NameCollisionOption.ReplaceExisting);
                    // Save in the ToDoItem
                    TodoItem.LocalImageUri = "ms-appdata:///local/" + file.Name;
                }
            }
            finally { Busy = false; }
        }


        Mvvm.Command _TakePictureCommand = default(Mvvm.Command);
        public Mvvm.Command TakePictureCommand { get { return _TakePictureCommand ?? (_TakePictureCommand = new Mvvm.Command(ExecuteTakePictureCommand, CanExecuteSelectPictureCommand)); } }
        private bool CanExecuteTakePictureCommand() { return !Busy && Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Media.Capture.CameraCaptureUI"); }
        private async void ExecuteTakePictureCommand()
        {
            try
            {
                Busy = true;

                StorageFile file = null;

                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Media.Capture.CameraCaptureUI"))
                {
                    // Using Windows.Media.Capture.CameraCaptureUI API to capture a photo
                    CameraCaptureUI dialog = new CameraCaptureUI();
                    Size aspectRatio = new Size(16, 9);
                    dialog.PhotoSettings.CroppedAspectRatio = aspectRatio;

                    file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);
                }

                if (file != null)
                {
                    // Copy the file into local folder
                    await file.CopyAsync(ApplicationData.Current.LocalFolder, file.Name, NameCollisionOption.ReplaceExisting);
                    // Save in the ToDoItem
                    TodoItem.LocalImageUri = "ms-appdata:///local/" + file.Name;
                    // Temporarily set the main ImageUri property to the local file - after upload to Azure Storage
                    // this will change to a cloud Uri
                    TodoItem.ImageUri = new Uri(TodoItem.LocalImageUri);
                    // Set the flag
                    TodoItem.ImageUploadPending = true;
                }
            }
            finally { Busy = false; }
        }

        Mvvm.Command<Models.TodoItem> _RemoveItemCommand = default(Mvvm.Command<Models.TodoItem>);
        public Mvvm.Command<Models.TodoItem> RemoveItemCommand { get { return _RemoveItemCommand ?? (_RemoveItemCommand = new Mvvm.Command<Models.TodoItem>(ExecuteRemoveItemCommand, CanExecuteRemoveItemCommand)); } }
        private bool CanExecuteRemoveItemCommand(Models.TodoItem param) { return this.TodoItem != null; }
        private async void ExecuteRemoveItemCommand(Models.TodoItem param)
        {
            await _todoItemRepository.DeleteTodoItem(this.TodoItem);

            this.TodoItem = null;

            // Navigate back
            ((App)App.Current).NavigationService.GoBack();
        }

        Mvvm.Command<Models.TodoItem> _UpdateItemCommand = default(Mvvm.Command<Models.TodoItem>);
        public Mvvm.Command<Models.TodoItem> UpdateItemCommand { get { return _UpdateItemCommand ?? (_UpdateItemCommand = new Mvvm.Command<Models.TodoItem>(ExecuteUpdateItemCommand, CanExecuteUpdateItemCommand)); } }
        private bool CanExecuteUpdateItemCommand(Models.TodoItem param) { return this.TodoItem != null && IsItemModified == true; }
        private async void ExecuteUpdateItemCommand(Models.TodoItem param)
        {
            await _todoItemRepository.UpdateTodoItem(this.TodoItem);
            IsItemModified = false;
        }

        #endregion  
    }
}
