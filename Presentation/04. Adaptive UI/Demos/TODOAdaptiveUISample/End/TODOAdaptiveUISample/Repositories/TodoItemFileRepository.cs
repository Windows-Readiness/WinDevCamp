using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TODOAdaptiveUISample.Models;

namespace TODOAdaptiveUISample.Repositories
{
    public class TodoItemFileRepository : ITodoItemRepository
    {
        List<Models.TodoItem> _cache;
        const string cachekey = "cache-todoitem";
        Services.FileService.FileService _fileService;
        static TodoItemFileRepository _todoItemFileRepository;

        static public TodoItemFileRepository GetInstance()
        {
            if (_todoItemFileRepository == null)
                _todoItemFileRepository = new TodoItemFileRepository();
            return _todoItemFileRepository;
        }

        private TodoItemFileRepository()
        {
            _fileService = new Services.FileService.FileService();
        }

        public async Task<List<Models.TodoItem>> RefreshTodoItemsAsync()
        {
            Debug.WriteLine("Starting RefreshAll");
            return _cache ?? (_cache = await _fileService.ReadAsync<Models.TodoItem>(cachekey) ?? new List<TodoItem>());
        }

        public async Task<Models.TodoItem> RefreshTodoItemAsync(string key)
        {
            return (await this.RefreshTodoItemsAsync()).FirstOrDefault(x => x.Id.Equals(key));
        }

        public async Task InsertTodoItem(TodoItem item)
        {
            _cache.Add(item);
            await _fileService.WriteAsync<Models.TodoItem>(cachekey, _cache);
        }

        public async Task DeleteTodoItem(TodoItem todoitem)
        {
            var item = (from t in _cache
                        where t.Id == todoitem.Id
                        select t).FirstOrDefault();
            _cache.Remove(item);
            await _fileService.WriteAsync<Models.TodoItem>(cachekey, _cache);
        }

        public async Task UpdateTodoItem(TodoItem todoitem)
        {
            var item = (from t in _cache
                        where t.Id == todoitem.Id
                        select t).FirstOrDefault();
            _cache.Remove(item);
            _cache.Add(todoitem);
            await _fileService.WriteAsync<Models.TodoItem>(cachekey, _cache);
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
                DueDate = DateTime.Now.AddDays(7),
                Color = (TaskColor)(new Random().Next(8))
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
