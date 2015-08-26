using System;
using System.Collections.Generic;
using System.Linq;
//using ExtensionMethods;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PhotoFilter.WPF
{
    public static class Utilities
    {
        static string PhotoPathForUser = @"\Pictures";
        public static string[] ValidFileExtensions = new string[] { "jpg", "png" };

        public static string[] GetPictureList()
        {
            return Directory.GetFiles(PhotoPath);
        }

        public static bool IsValidImageFile(string file)
        {

            var fileParts = file.Split('.');
            if (fileParts.Length < 2)
            {
                return false;
            }

            var extension = fileParts[fileParts.Length - 1];

            if (ValidFileExtensions.Contains(extension))
            {
                return true;
            }

            return false;
        }

        #region PrivateAPIs
        static string CurrentUser
        {
            get
            {
                string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                if (userName.Contains("\\"))
                {
                    string[] strs = userName.Split('\\');
                    userName = strs[1];
                }
                return userName;
            }
        }

        static string PhotoPath
        {
            get
            {
                string photoPath = @"C:\users\" + Utilities.CurrentUser + PhotoPathForUser;

                return photoPath;
            }
        }
        #endregion

    }
}
