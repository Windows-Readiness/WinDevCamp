using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TODOAdaptiveUISample.Models;

namespace TODOAdaptiveUISample.Repositories
{
    public interface ITodoItemRepository
    {
        TodoItem Clone(TodoItem item);
        Task DeleteTodoItem(TodoItem todoitem);
        TodoItem Factory(string id = null, bool? complete = default(bool?), string title = null, DateTime? dueDate = default(DateTime?));
        Task InsertTodoItem(TodoItem item);
        Task<TodoItem> RefreshTodoItemAsync(string key);
        Task<List<TodoItem>> RefreshTodoItemsAsync();
        IEnumerable<TodoItem> Sample(int count = 5);
        Task UpdateTodoItem(TodoItem todoitem);
    }
}