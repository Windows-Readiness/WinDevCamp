using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TODOAdaptiveUISample.Models;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Navigation;

namespace TODOAdaptiveUISample.ViewModels
{
    public class TodoItemViewModel : Mvvm.ViewModelBase
    {
        #region Properties
        Repositories.ITodoItemRepository _todoItemRepository;

        private Models.TodoItem _TodoItem = default(Models.TodoItem);
        public Models.TodoItem TodoItem { get { return _TodoItem; } set { Set(ref _TodoItem, value); } }

        bool _busy = false;
        public bool Busy { get { return _busy; } set { Set(ref _busy, value); } }

        #endregion

        #region Constructors

        public TodoItemViewModel()
        {
            _todoItemRepository = Repositories.TodoItemFileRepository.GetInstance();

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // designtime sample data
                var data = _todoItemRepository.Sample(1).First();
                this.TodoItem = data;
            }
        }

        public TodoItemViewModel(Models.TodoItem todo)
        {
            _todoItemRepository = Repositories.TodoItemFileRepository.GetInstance();

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // designtime sample data
                var data = _todoItemRepository.Sample(1).First();
                this.TodoItem = data;
            }
            else
            {
                this.TodoItem = todo;
            }
        }
        #endregion

        #region Methods
        public override async Task OnNavigatedToAsync(string parameter, NavigationMode mode)
        {
            TodoItem = await _todoItemRepository.RefreshTodoItemAsync(parameter);
            this.RemoveItemCommand.RaiseCanExecuteChanged();
        }

        public override async Task OnNavigatedFromAsync(bool suspending)
        {
            // logic to execute when navigating from this page
            if (this.TodoItem != null)
                await _todoItemRepository.UpdateTodoItem(this.TodoItem);
        }
        #endregion

        #region Commands

        Mvvm.Command _SelectPictureCommand = default(Mvvm.Command);
        public Mvvm.Command SelectPictureCommand { get { return _SelectPictureCommand ?? (_SelectPictureCommand = new Mvvm.Command(ExecuteSelectPictureCommand, CanExecuteSelectPictureCommand)); } }
        private bool CanExecuteSelectPictureCommand() { return !Busy; }
        private async void ExecuteSelectPictureCommand()
        {
            await SelectPicture();
        }

        public async Task SelectPicture()
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
                    TodoItem.ImageUri = new Uri("ms-appdata:///local/" + file.Name);
                }
            }
            finally { Busy = false; }
        }


        Mvvm.Command _TakePictureCommand = default(Mvvm.Command);
        public Mvvm.Command TakePictureCommand { get { return _TakePictureCommand ?? (_TakePictureCommand = new Mvvm.Command(ExecuteTakePictureCommand, CanExecuteSelectPictureCommand)); } }
        private bool CanExecuteTakePictureCommand() { return !Busy && Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Media.Capture.CameraCaptureUI"); }
        private async void ExecuteTakePictureCommand()
        {
            await TakePicture();
        }

        public async Task TakePicture()
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
                    TodoItem.ImageUri = new Uri("ms-appdata:///local/" + file.Name);
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
            if (((App)App.Current).NavigationService.CanGoBack)
                ((App)App.Current).NavigationService.GoBack();
        }

        Mvvm.Command<Models.TodoItem> _UpdateItemCommand = default(Mvvm.Command<Models.TodoItem>);
        public Mvvm.Command<Models.TodoItem> UpdateItemCommand { get { return _UpdateItemCommand ?? (_UpdateItemCommand = new Mvvm.Command<Models.TodoItem>(ExecuteUpdateItemCommand, CanExecuteUpdateItemCommand)); } }
        private bool CanExecuteUpdateItemCommand(Models.TodoItem param) { return this.TodoItem != null; }
        private async void ExecuteUpdateItemCommand(Models.TodoItem param)
        {
            await _todoItemRepository.UpdateTodoItem(this.TodoItem);
        }

        Mvvm.Command _ToggleCompletedCommand = default(Mvvm.Command);
        public Mvvm.Command ToggleCompletedCommand { get { return _ToggleCompletedCommand ?? (_ToggleCompletedCommand = new Mvvm.Command(ExecuteToggleCompletedCommand, CanExecuteToggleCompletedCommand)); } }
        private bool CanExecuteToggleCompletedCommand() { return !Busy; }
        private  void ExecuteToggleCompletedCommand()
        {
            this.TodoItem.IsComplete = !this.TodoItem.IsComplete;
        }

        Mvvm.Command _ToggleFavoriteCommand = default(Mvvm.Command);
        public Mvvm.Command ToggleFavoriteCommand { get { return _ToggleFavoriteCommand ?? (_ToggleFavoriteCommand = new Mvvm.Command(ExecuteToggleFavoriteCommand, CanExecuteToggleFavoriteCommand)); } }
        private bool CanExecuteToggleFavoriteCommand() { return !Busy; }
        private void ExecuteToggleFavoriteCommand()
        {
            this.TodoItem.IsFavorite = !this.TodoItem.IsFavorite;
        }

        #endregion  
    }
}
