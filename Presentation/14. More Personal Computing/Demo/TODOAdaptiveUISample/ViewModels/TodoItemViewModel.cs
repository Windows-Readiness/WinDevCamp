using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TODOAdaptiveUISample.Models;
using TODOAdaptiveUISample.Views;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
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

        Mvvm.Command<StorageFile> _SavePictureCommand = default(Mvvm.Command<StorageFile>);
        public Mvvm.Command<StorageFile> SavePictureCommand { get { return _SavePictureCommand ?? (_SavePictureCommand = new Mvvm.Command<StorageFile>(ExecuteSavePictureCommand, CanExecuteSavePictureCommand)); } }
        private bool CanExecuteSavePictureCommand(StorageFile file) { return !Busy; }
        private async void ExecuteSavePictureCommand(StorageFile file)
        {
            try
            {
                if (file != null)
                {
                    // Copy the file into local folder
                    await file.CopyAsync(ApplicationData.Current.LocalFolder, file.Name, NameCollisionOption.GenerateUniqueName);
                    // Save in the ToDoItem
                    TodoItem.ImageUri = new Uri("ms-appdata:///local/" + file.Name);
                }

            }
            finally { Busy = false; }
        }

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
                SavePictureCommand.Execute(file);

            }
            finally { /*Busy = false;*/ }
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

                SavePictureCommand.Execute(file);
            }
            finally { /*Busy = false;*/ }
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
        private bool CanExecuteUpdateItemCommand(Models.TodoItem param) { return this.TodoItem != null; }
        private async void ExecuteUpdateItemCommand(Models.TodoItem param)
        {
            await _todoItemRepository.UpdateTodoItem(this.TodoItem);
        }

        Mvvm.Command<Models.TodoItem> _EditInkNotesCommand = default(Mvvm.Command<Models.TodoItem>);
        public Mvvm.Command<Models.TodoItem> EditInkNotesCommand { get { return _EditInkNotesCommand ?? (_EditInkNotesCommand = new Mvvm.Command<Models.TodoItem>(ExecuteEditInkNotesCommand, CanExecuteEditInkNotesCommand)); } }
        private bool CanExecuteEditInkNotesCommand(Models.TodoItem param) { return !Busy; }
        private async void ExecuteEditInkNotesCommand(Models.TodoItem param)
        {
            ((App)(Application.Current)).NavigationService.Navigate(typeof(InkPage), this.TodoItem.Id);
        }

        #endregion  
    }
}
