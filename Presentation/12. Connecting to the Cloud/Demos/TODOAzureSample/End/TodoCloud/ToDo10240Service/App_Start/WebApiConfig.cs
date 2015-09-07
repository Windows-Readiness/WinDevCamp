using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Config;
using ToDo10240Service.DataObjects;
using ToDo10240Service.Models;

namespace ToDo10240Service
{
    public static class WebApiConfig
    {
        public static void Register()
        {
            HttpConfiguration config = new HttpConfiguration();

            new MobileAppConfiguration()
                .UseDefaultConfiguration()
                .ApplyTo(config);

            Database.SetInitializer(new ToDo10240Initializer());
        }
    }

    public class ToDo10240Initializer : ClearDatabaseSchemaIfModelChanges<ToDo10240Context>
    {
        protected override void Seed(ToDo10240Context context)
        {

            DateTime dueDate = DateTime.Now.AddDays(2);

            List<TodoItem> todoItems = new List<TodoItem>
            {
                new TodoItem { Id = Guid.NewGuid().ToString(), Title = "First item", IsComplete = false, IsFavorite = false, Details = "First item details", DueDate = dueDate, AzureImageUri = "http://res1.windows.microsoft.com/resbox/en/windows/main/8461c40e-b054-491a-ba53-d0cd72cda3a3_7.png", ImageUploadPending=false, },
                new TodoItem { Id = Guid.NewGuid().ToString(), Title = "Second item", IsComplete = false, IsFavorite = false, Details = "Second item details", DueDate = dueDate, AzureImageUri = "http://res1.windows.microsoft.com/resbox/en/windows/main/8461c40e-b054-491a-ba53-d0cd72cda3a3_7.png" , ImageUploadPending=false, },
            };

            foreach (TodoItem todoItem in todoItems)
            {
                context.Set<TodoItem>().Add(todoItem);
            }

            base.Seed(context);
        }
    }
}

