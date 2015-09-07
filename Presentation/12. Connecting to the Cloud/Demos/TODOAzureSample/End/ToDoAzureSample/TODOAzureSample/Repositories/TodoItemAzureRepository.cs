using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TODOAzureSample.Models;
using Windows.UI.Popups;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;  // offline sync
using Microsoft.WindowsAzure.MobileServices.Sync;         // offline sync
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Windows.Storage;

namespace TODOAzureSample.Repositories
{
    public class TodoItemAzureRepository : ITodoItemRepository
    {
        //private IMobileServiceTable<TodoItem> todoTable = App.MobileService.GetTable<TodoItem>();
        private IMobileServiceSyncTable<TodoItem> todoTable = App.MobileService.GetSyncTable<TodoItem>(); // offline sync


        public async Task DeleteTodoItem(TodoItem todoItem)
        {
            // This code deletes a TodoItem from the database. 
            await todoTable.DeleteAsync(todoItem);

            await SyncWithImagesAsync(); // offline sync        
        }

        public async Task InsertTodoItem(TodoItem todoItem)
        {
            try
            {
                if (todoItem.ImageUploadPending)
                {
                    // Set blob properties of TodoItem.
                    todoItem.ContainerName = "todoitemimages";

                    // Use a unigue GUID to avoid collisions.
                    todoItem.ResourceName = Guid.NewGuid().ToString();
                }

                // This code inserts a new TodoItem into the database. 
                await todoTable.InsertAsync(todoItem);

            }
            catch (Exception)
            {
                throw;
            }

            await SyncWithImagesAsync(); // offline sync
        }

        public async Task<TodoItem> RefreshTodoItemAsync(string key)
        {
            MobileServiceInvalidOperationException exception = null;
            MobileServiceCollection<TodoItem, TodoItem> items = null;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
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
            if (todoItem.ImageUploadPending)
            {
                // Set blob properties of TodoItem.
                todoItem.ContainerName = "todoitemimages";

                // Use a unigue GUID to avoid collisions.
                todoItem.ResourceName = Guid.NewGuid().ToString();
            }

            // This code updates a TodoItem in the local database. 
            await todoTable.UpdateAsync(todoItem);

            await SyncWithImagesAsync(); // offline sync
        }

        #region Offline sync

        public async Task InitLocalStoreAsync()
        {
            if (!App.MobileService.SyncContext.IsInitialized)
            {
                var store = new MobileServiceSQLiteStore("localstore.db");
                store.DefineTable<TodoItem>();
                await App.MobileService.SyncContext.InitializeAsync(store);
            }

            await SyncWithImagesAsync();
        }

        private async Task<bool> SyncWithImagesAsync()
        {
            // Before syncing, record which item(s) have a pending image upload along with their localImageUri
            var imagePendingItems = await todoTable
                .Where(l => l.ImageUploadPending == true)
                .ToCollectionAsync();

            bool syncOK = await SyncAsync();

            // Process queued image uploads
            if (imagePendingItems != null && imagePendingItems.Count > 0)
            {
                foreach (var pendingItem in imagePendingItems)
                {
                    // Get the updated version from the server with the SAS properties filled in
                    var todoItem = (await todoTable
                        .Where(l => l.Id == pendingItem.Id)
                        .ToCollectionAsync()).FirstOrDefault();
                    if (todoItem != null)
                    {
                        await UploadImage(todoItem);
                    }
                }
            }

            return syncOK;
        }

        /// <summary>
        /// Syncs local data with the server, pushes local changes and pulls down server changes
        /// </summary>
        /// <returns>true if successful, false if failed</returns>
        private async Task<bool> SyncAsync()
        {
            String errorString = null;

            try
            {
                await App.MobileService.SyncContext.PushAsync();
                await todoTable.PullAsync("todoItems", todoTable.CreateQuery()); // first param is query ID, used for incremental sync
            }

            catch (MobileServicePushFailedException ex)
            {
                errorString = "Push failed because of sync errors: " +
                  ex.PushResult.Errors.Count + " errors, message: " + ex.Message;
            }
            catch (Exception ex)
            {
                errorString = "Pull failed: " + ex.Message +
                  "\n\nIf you are still in an offline scenario, " +
                  "you can try your Pull again when connected with your Mobile Service.";
            }

            if (errorString != null)
            {
                MessageDialog d = new MessageDialog(errorString);
                await d.ShowAsync();
                return false; // sync failed
            }

            // Sync was successful
            return true; // synced OK
        }

        private async Task UploadImage(TodoItem todoItem)
        {
            // If we have a returned SAS, then upload the blob.
            if (!string.IsNullOrEmpty(todoItem.SasQueryString))
            {
                // Get the URI generated that contains the SAS 
                // and extract the storage credentials.
                StorageCredentials cred = new StorageCredentials(todoItem.SasQueryString);

                // Instantiate a Blob store container based on the info in the returned item.
                CloudBlobContainer container = new CloudBlobContainer(
                    new Uri(string.Format("https://{0}/{1}",
                        todoItem.ImageUri.Host, todoItem.ContainerName)), cred);

                // Get the new local image as a stream.
                StorageFile media = await StorageFile.GetFileFromApplicationUriAsync(new Uri(todoItem.LocalImageUri));
                using (var inputStream = await media.OpenReadAsync())
                {
                    // Upload the new image as a BLOB from the stream.
                    CloudBlockBlob blobFromSASCredential =
                        container.GetBlockBlobReference(todoItem.ResourceName);
                    await blobFromSASCredential.UploadFromStreamAsync(inputStream);
                }

                // Clear the flag on the item and related fields
                todoItem.ImageUploadPending = false;
                todoItem.LocalImageUri = string.Empty;
                todoItem.SasQueryString = string.Empty;
                todoItem.ResourceName = string.Empty;

                // This code updates a TodoItem in the local database. 
                await todoTable.UpdateAsync(todoItem);

                await SyncAsync();
            }
        }


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

