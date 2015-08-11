using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace MyFilesWin10App.Models
{
    public class MyFilesModel : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public int Size { get; set; }
        public string Type { get; set; }
        private BitmapSource bitmap;
        public BitmapSource Bitmap
        {
            get { return bitmap; }
            set
            {
                //set value and raise the property changed event
                bitmap = value;
                OnPropertyChanged("Bitmap");
            }
        }

        public bool ImageLoaded { get; set; }

        public string Icon
        {
            get
            {
                //get icon based on extension
                if (this.Type == "Folder")
                    return "ms-appx:///assets/app/folder.png";
                if (this.Name.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase))
                    return "ms-appx:///assets/app/jpg.png";
                else if (this.Name.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase))
                    return "ms-appx:///assets/app/png.png";
                if (this.Name.EndsWith(".gif", StringComparison.CurrentCultureIgnoreCase))
                    return "ms-appx:///assets/app/gif.png";
                else
                    return "ms-appx:///assets/app/gif.png";
            }
        }

        //INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}