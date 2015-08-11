using Microsoft.Office365.Discovery;
using Microsoft.Office365.SharePoint.CoreServices;
using Microsoft.Office365.SharePoint.FileServices;
using MyFilesWin10App;
using MyFilesWin10App.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MyFilesModelsWin10App.Controllers
{
    public class MyFilesController
    {
        private static async Task<string> GetAccessTokenForResource(string resource)
        {
            string token = null;

            //first try to get the token silently
            WebAccountProvider aadAccountProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.windows.net");
            WebTokenRequest webTokenRequest = new WebTokenRequest(aadAccountProvider, String.Empty, App.Current.Resources["ida:ClientID"].ToString(), WebTokenRequestPromptType.Default);
            webTokenRequest.Properties.Add("authority", "https://login.windows.net");
            webTokenRequest.Properties.Add("resource", resource);
            WebTokenRequestResult webTokenRequestResult = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(webTokenRequest);
            if (webTokenRequestResult.ResponseStatus == WebTokenRequestStatus.Success)
            {
                WebTokenResponse webTokenResponse = webTokenRequestResult.ResponseData[0];
                token = webTokenResponse.Token;
            }
            else if (webTokenRequestResult.ResponseStatus == WebTokenRequestStatus.UserInteractionRequired)
            {
                //get token through prompt
                webTokenRequest = new WebTokenRequest(aadAccountProvider, String.Empty, App.Current.Resources["ida:ClientID"].ToString(), WebTokenRequestPromptType.ForceAuthentication);
                webTokenRequest.Properties.Add("authority", "https://login.windows.net");
                webTokenRequest.Properties.Add("resource", resource);
                webTokenRequestResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest);
                if (webTokenRequestResult.ResponseStatus == WebTokenRequestStatus.Success)
                {
                    WebTokenResponse webTokenResponse = webTokenRequestResult.ResponseData[0];
                    token = webTokenResponse.Token;
                }
            }

            return token;
        }

        private static async Task<SharePointClient> EnsureClient()
        {
            DiscoveryClient discoveryClient = new DiscoveryClient(
                    async () => await GetAccessTokenForResource("https://api.office.com/discovery/"));

            // Get the "MyFiles" capability.
            CapabilityDiscoveryResult result = await discoveryClient.DiscoverCapabilityAsync("MyFiles");
            
            return new SharePointClient(result.ServiceEndpointUri, async () => {
                return await GetAccessTokenForResource(result.ServiceResourceId);
            });
        }

        public static async Task<ObservableCollection<MyFilesModel>> GetMyImages(string id)
        {
            ObservableCollection<MyFilesModel> results = new ObservableCollection<MyFilesModel>();

            //ensure client created
            var client = await EnsureClient();

            //get files
            List<IItem> items;
            if (id != String.Empty)
                items = (await client.Files.GetById(id).ToFolder().Children.ExecuteAsync()).CurrentPage.ToList();
            else
                items = (await client.Files.ExecuteAsync()).CurrentPage.ToList();

            //process items
            foreach (var item in items)
            {
                //only load folders and images
                if (item is Folder ||
                    item.Name.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
                    item.Name.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase) ||
                    item.Name.EndsWith(".gif", StringComparison.CurrentCultureIgnoreCase))
                    results.Add(new MyFilesModel()
                    {
                        Name = item.Name,
                        Url = item.WebUrl,
                        Size = (int)item.Size,
                        Type = item.Type,
                        Id = item.Id
                    });
            }

            return results;
        }

        /// <summary>
        /// Retrieves a binary image and resizes it accordingly
        /// </summary>
        /// <param name="item">MyFilesModel object</param>
        /// <returns>resized BitmapSource</returns>
        public static async Task<BitmapSource> GetImage(MyFilesModel item, int w)
        {
            BitmapImage img = new BitmapImage();

            //ensure client created
            var client = await EnsureClient();

            //get the file stream
            using (Stream stream = await client.Files.GetById(item.Id).ToFile().DownloadAsync())
            {
                using (var memStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memStream);
                    memStream.Position = 0;
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(memStream.AsRandomAccessStream());
                    using (InMemoryRandomAccessStream ras = new InMemoryRandomAccessStream())
                    {
                        //first get original specs so we can scale the resize best
                        BitmapEncoder enc = await BitmapEncoder.CreateForTranscodingAsync(ras, decoder);
                        await enc.FlushAsync();
                        BitmapImage original = new BitmapImage();
                        original.SetSource(ras);
                        int height = original.PixelHeight;
                        int width = original.PixelWidth;

                        //rewind and take a second pass...this time resizing
                        ras.Seek(0);
                        enc = await BitmapEncoder.CreateForTranscodingAsync(ras, decoder);
                        enc.BitmapTransform.ScaledHeight = (uint)(((double)height / (double)width) * w);
                        enc.BitmapTransform.ScaledWidth = (uint)w;
                        await enc.FlushAsync();
                        img.SetSource(ras);
                    }
                }
            }

            return img;
        }
    }
}
