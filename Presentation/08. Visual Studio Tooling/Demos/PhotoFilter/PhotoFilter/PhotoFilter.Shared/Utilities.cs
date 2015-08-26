using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PhotoFilter
{
    public static class StorageFolderExtensions
    {
        public static async Task<StorageFolder> GetOrCreateFolderAsync(this StorageFolder folder, string name)
        {
            try
            {
               return await folder.GetFolderAsync(name);
            }
            catch (FileNotFoundException)
            {
                return await folder.CreateFolderAsync(name);
            }
        }

        public static StorageFolder GetFolder(this StorageFolder folder, string name)
        {
            return folder.GetFolderAsync(name).AsTask().Result;
        }
    }

    class Utilities
    {
        public static StorageFolder FolderFromPath(string path)
        {
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

        public static string FolderNameFromPath(string path)
        {
            string currentPath = path;
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
}
