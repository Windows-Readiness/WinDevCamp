using Microsoft.Azure.Mobile.Server;
using System;

namespace ToDo10240Service.DataObjects
{
    public class TodoItem : EntityData
    {
        public string Title { get; set; }

        public DateTime DueDate { get; set; }

        public bool IsComplete { get; set; }

        public string Details { get; set; }

        public bool IsFavorite { get; set; }

        public string AzureImageUri { get; set; }

        public string LocalImageUri { get; set; }

        public bool ImageUploadPending { get; set; }

        public string ContainerName { get; set; }

        public string ResourceName { get; set; }

        public string SasQueryString { get; set; }
    }
}