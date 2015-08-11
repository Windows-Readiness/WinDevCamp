using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TODOAzureSample.Models;
using Windows.UI.Popups;
//using Microsoft.WindowsAzure.MobileServices.SQLiteStore;  // offline sync
//using Microsoft.WindowsAzure.MobileServices.Sync;         // offline sync

namespace TODOAzureSample.Repositories
{
        public class TodoItemAzureRepository : ITodoItemRepository
        {
            private IMobileServiceTable<TodoItem> todoTable = App.MobileService.GetTable<TodoItem>();
            //private IMobileServiceSyncTable<TodoItem> todoTable = App.MobileService.GetSyncTable<TodoItem>(); // offline sync


            public async Task DeleteTodoItem(TodoItem todoItem)
            {
                // This code deletes a TodoItem from the database. 
                await todoTable.DeleteAsync(todoItem);

                //await SyncAsync(); // offline sync        
            }

            public async Task InsertTodoItem(TodoItem todoItem)
            {
                try
                {

                    // This code inserts a new TodoItem into the database. 
                    await todoTable.InsertAsync(todoItem);

                }
                catch (Exception)
                {
                    throw;
                }
                //await SyncAsync(); // offline sync
            }

            public async Task<TodoItem> RefreshTodoItemAsync(string key)
            {
                MobileServiceInvalidOperationException exception = null;
                MobileServiceCollection<TodoItem, TodoItem> items = null;
                try
                {
                    // This code refreshes the entries in the list view by querying the TodoItems table.
                    // The query excludes completed TodoItems
                    items = await todoTable
                        .Where(l => l.Id == key)
                        .ToCollectionAsync();
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                if (exception != null)
                {
                    await new MessageDialog(exception.Message, "Error loading item").ShowAsync();
                }

                return items.ToList<TodoItem>().FirstOrDefault();
            }

            public async Task<List<TodoItem>> RefreshTodoItemsAsync()
            {
                MobileServiceInvalidOperationException exception = null;
                MobileServiceCollection<TodoItem, TodoItem> items = null;
                try
                {
                    // This code refreshes the entries in the list view by querying the TodoItems table.
                    // The query excludes completed TodoItems
                    items = await todoTable.ToCollectionAsync();
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                if (exception != null)
                {
                    await new MessageDialog(exception.Message, "Error loading items").ShowAsync();
                }

                return items.ToList();
            }

            public async Task UpdateTodoItem(TodoItem todoItem)
            {
                // This code updates a TodoItem in the database. 
                await todoTable.UpdateAsync(todoItem);

                //await SyncAsync(); // offline sync
            }

        #region Offline sync

        //private async Task InitLocalStoreAsync()
        //{
        //    if (!App.MobileService.SyncContext.IsInitialized)
        //    {
        //        var store = new MobileServiceSQLiteStore("localstore.db");
        //        store.DefineTable<TodoItem>();
        //        await App.MobileService.SyncContext.InitializeAsync(store);
        //    }
        //
        //    await SyncAsync();
        //}

        //private async Task SyncAsync()
        //{
        //    String errorString = null;

        //    try
        //    {
        //        await App.MobileService.SyncContext.PushAsync();
        //        await todoTable.PullAsync("todoItems", todoTable.CreateQuery()); // first param is query ID, used for incremental sync
        //    }

        //    catch (MobileServicePushFailedException ex)
        //    {
        //        errorString = "Push failed because of sync errors: " +
        //          ex.PushResult.Errors.Count + " errors, message: " + ex.Message;
        //    }
        //    catch (Exception ex)
        //    {
        //        errorString = "Pull failed: " + ex.Message +
        //          "\n\nIf you are still in an offline scenario, " +
        //          "you can try your Pull again when connected with your Mobile Serice.";
        //    }

        //    if (errorString != null)
        //    {
        //        MessageDialog d = new MessageDialog(errorString);
        //        await d.ShowAsync();
        //    }
        //}

        #endregion

        static TodoItemAzureRepository _todoItemAzureRepository;

        static public TodoItemAzureRepository GetInstance()
        {
            if (_todoItemAzureRepository == null)
                _todoItemAzureRepository = new TodoItemAzureRepository();
            return _todoItemAzureRepository;
        }

        private TodoItemAzureRepository()
        {
        }

        public Models.TodoItem Factory(string id = null, bool? complete = null, string title = null, DateTime? dueDate = null)
            {
                string imageNum = new Random().Next(5).ToString();
                // Uri format is different at design-time than at runtime - set appropriately
                Uri imageUri = null;
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    imageUri = new Uri("ms-appx:/Images/Placeholder" + imageNum + ".png");
                }
                else
                {
                    imageUri = new Uri("ms-appx:///Images/Placeholder" + imageNum + ".png");
                }
                var item = new Models.TodoItem
                {
                    Id = id ?? Guid.NewGuid().ToString(),
                    IsComplete = complete ?? false,
                    Title = title ?? string.Empty,
                    ImageUri = imageUri,
                    DueDate = DateTime.Now.AddDays(7)
                };
                return item;
            }

            public Models.TodoItem Clone(Models.TodoItem item)
            {
                return Factory
                    (
                        Guid.Empty.ToString(),
                        false,
                        item.Title,
                        item.DueDate
                    );
            }

        public IEnumerable<Models.TodoItem> Sample(int count = 5)
        {
            var random = new Random((int)DateTime.Now.Ticks);
            foreach (var item in Enumerable.Range(1, count))
            {
                yield return Factory
                    (
                        Guid.NewGuid().ToString(),
                        false,
                        "Task-" + Guid.NewGuid().ToString(),
                        DateTime.Now.AddHours(random.Next(1, 200))
                    );
            }
        }
    }
}
