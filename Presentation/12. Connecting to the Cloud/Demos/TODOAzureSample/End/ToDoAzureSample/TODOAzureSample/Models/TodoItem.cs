using System;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media.Imaging;

namespace TODOAzureSample.Models
{
    public class TodoItem : Mvvm.BindableBase
    {
        private string _Id = default(string);
        public string Id { get { return _Id; } set { Set(ref _Id, value); } }

        private string _Title = default(string);
        [JsonProperty(PropertyName = "title")]
        public string Title { get { return _Title; } set { Set(ref _Title, value); } }

        private DateTime _DueDate = default(DateTime);
        [JsonProperty(PropertyName = "dueDate")]
        public DateTime DueDate { get { return _DueDate; } set { Set(ref _DueDate, value); } }

        private bool _IsComplete = default(bool);
        [JsonProperty(PropertyName = "isComplete")]
        public bool IsComplete { get { return _IsComplete; } set { Set(ref _IsComplete, value); } }

        private string _Details = default(string);
        [JsonProperty(PropertyName = "details")]
        public string Details { get { return _Details; } set { Set(ref _Details, value); } }

        private bool _IsFavorite = default(bool);
        [JsonProperty(PropertyName = "isFavorite")]
        public bool IsFavorite { get { return _IsFavorite; } set { Set(ref _IsFavorite, value); } }

        // This is the property to which the UI databinds
        private Uri _ImageUri = default(Uri);
        [JsonIgnore]
        public Uri ImageUri
        {
            get { return _ImageUri; }
            set
            {
                Set(ref _ImageUri, value);
            }
        }

        // This is used for over the air communication with the cloud service
        private string azureImageUri = default(string);
        [JsonProperty(PropertyName = "azureImageUri")]
        public string AzureImageUri
        {
            get { return azureImageUri; }
            set
            {
                Set(ref azureImageUri, value);

                // Set a local image if the cloud service does not return an image uri string
                if (string.IsNullOrEmpty(azureImageUri)) azureImageUri = "ms-appx:///Images/NotFound.png";
                // Use this to set the ImageUri property to which the UI binds
                ImageUri = new Uri(azureImageUri);
            }
        }

        // This is the property where the app stores Uri to the local picture when adding or updating
        // a ToDo Item
        [JsonProperty(PropertyName = "localImageUri")]
        public string LocalImageUri { get; set; }

        // Flag to indicate to server there is an image to upload
        [JsonProperty(PropertyName = "imageUploadPending")]
        public bool ImageUploadPending { get; set; }

        [JsonProperty(PropertyName = "containerName")]
        public string ContainerName { get; set; }

        [JsonProperty(PropertyName = "resourceName")]
        public string ResourceName { get; set; }

        [JsonProperty(PropertyName = "sasQueryString")]
        public string SasQueryString { get; set; }

    }
}
