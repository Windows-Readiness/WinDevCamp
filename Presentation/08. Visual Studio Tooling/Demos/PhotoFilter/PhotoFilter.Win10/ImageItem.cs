using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace PhotoFilter.Win10
{
    class ImageItem
    {
        private StorageFile m_file;
        private StorageItemThumbnail m_thumbNail;
        private BitmapImage m_photo;
        public string Name { get; set; }
        public string Path { get; set; }
        public StorageFolder Folder
        {
            get
            {
                string path = Path;
                if (!path.Contains("Pictures"))
                {
                    throw new ArgumentException();
                }
                StorageFolder folder = KnownFolders.PicturesLibrary;
                string currentPath = path.Substring(path.IndexOf("Pictures") + "Pictures".Length + 1);
                string folderName = "Pictures";

                int count = currentPath.Split('\\').Length - 1;
                while (count > 0)
                {
                    folderName = currentPath.Split('\\')[0];
                    folder = folder.GetFolder(folderName);

                    // prepare next iteration of the loop
                    currentPath = currentPath.Substring(currentPath.IndexOf("\\") + 1);
                    count = currentPath.Split('\\').Length - 1;
                }
                return folder;
            }
        }

        public string FolderName
        {
            get
            {
                string currentPath = Path;
                string folderName = currentPath.Split('\\')[0];
                int count = currentPath.Split('\\').Length - 1;
                while (count > 0)
                {
                    folderName = currentPath.Split('\\')[0];
                    // prepare next iteration of the loop
                    currentPath = currentPath.Substring(currentPath.IndexOf("\\") + 1);
                    count = currentPath.Split('\\').Length - 1;
                }
                return folderName;
            }
        }

        public BitmapImage Photo
        {
            get
            {
                if (m_photo == null)
                {
                    m_photo = new BitmapImage();
                    m_photo.SetSource(m_thumbNail);
                }
                return m_photo;
            }
        }

        public ImageItem(StorageFile file)
        {
            this.m_file = file;
            this.Name = file.Name;
            this.Path = file.Path;
            //this.Folder = folder;
        }

        public async Task LoadImageFromDisk()
        {
            m_thumbNail = await m_file.GetThumbnailAsync(ThumbnailMode.PicturesView);
            m_photo = new BitmapImage();
            m_photo.SetSource(m_thumbNail);
        }

        public async Task FetchPictureOnBgThread()
        {
            m_thumbNail = await m_file.GetThumbnailAsync(ThumbnailMode.PicturesView);
        }

        public async Task<WriteableBitmap> GetPictureAsync()
        {
            return await BitmapCache.Instance.GetBitmapAsync(m_file);
        }

        public async Task<byte[]> GetScaledPixelsAsync(int height, int width)
        {
            using (IRandomAccessStream fileStream = await m_file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                // Scale image to appropriate size 
                BitmapTransform transform = new BitmapTransform()
                {
                    ScaledWidth = Convert.ToUInt32(width),
                    ScaledHeight = Convert.ToUInt32(height)
                };
                PixelDataProvider pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8, // WriteableBitmap uses BGRA format 
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation, // This sample ignores Exif orientation 
                    ColorManagementMode.DoNotColorManage
                );

                // An array containing the decoded image data, which could be modified before being displayed 
                byte[] sourcePixels = pixelData.DetachPixelData();

                return sourcePixels;
            }
        }
    }
}
