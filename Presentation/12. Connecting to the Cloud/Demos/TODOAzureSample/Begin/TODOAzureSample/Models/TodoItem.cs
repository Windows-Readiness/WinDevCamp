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

        private string cloudImageUri = default(string);
        [JsonProperty(PropertyName = "imageUri")]
        public string CloudImageUri
        {
            get { return cloudImageUri; }
            set
            {
                // Set a local image if the cloud service does not return an image uri string
                string uri = value;
                if (string.IsNullOrEmpty(uri)) uri = "ms-appx:///Images/Placeholder2.png";
                ImageUri = new Uri(uri);
            }
        }

        //[JsonProperty(PropertyName = "containerName")]
        //public string ContainerName { get; set; }

        //[JsonProperty(PropertyName = "resourceName")]
        //public string ResourceName { get; set; }

        //[JsonProperty(PropertyName = "sasQueryString")]
        //public string SasQueryString { get; set; }

        //[JsonIgnore]
        //public string localImageUri { get; set; }
    }
}
