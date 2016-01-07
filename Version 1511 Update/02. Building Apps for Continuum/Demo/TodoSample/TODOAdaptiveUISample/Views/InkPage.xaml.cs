using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class InkPage : Page
    {
        Repositories.ITodoItemRepository _todoItemRepository;

        private Models.TodoItem _TodoItem = default(Models.TodoItem);

        public InkPage()
        {
            this.InitializeComponent();
            _todoItemRepository = Repositories.TodoItemFileRepository.GetInstance();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string)
            {
                _TodoItem = await _todoItemRepository.RefreshTodoItemAsync(e.Parameter as string);
                await inkNotes.Load(_TodoItem.InkUri);
            }
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (this._TodoItem != null)
            {
                // TODO: figure out how to resolve locked file from binding
                //if (this._TodoItem.InkUri != null)
                //    await inkNotes.Save(this._TodoItem.InkUri);
                //else
                    this._TodoItem.InkUri = await inkNotes.Save();
                await _todoItemRepository.UpdateTodoItem(this._TodoItem);
            }
        }


    }
}
