using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using ToDo10240Service.DataObjects;
using ToDo10240Service.Models;
using System.Collections.Generic;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.Mobile.Server.Config;
using System;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Web.Http.Tracing;

namespace ToDo10240Service.Controllers
{
    public class TodoItemController : TableController<TodoItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            ToDo10240Context context = new ToDo10240Context();
            DomainManager = new EntityDomainManager<TodoItem>(context, Request);
        }

        // GET tables/TodoItem
        public IQueryable<TodoItem> GetAllTodoItems()
        {
            return Query();
        }

        // GET tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<TodoItem> GetTodoItem(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<TodoItem> PatchTodoItem(string id, Delta<TodoItem> patch)
        {
            object containerName, resourceName, imageUploadpending;
            // Client only sets ImageUploadPending when it has an image to upload
            patch.TryGetPropertyValue("ImageUploadPending", out imageUploadpending);
            if (imageUploadpending != null && ((bool)imageUploadpending == true))
            {
                patch.TryGetPropertyValue("ContainerName", out containerName);
                patch.TryGetPropertyValue("ResourceName", out resourceName);

                if (!string.IsNullOrEmpty(containerName.ToString()) && !string.IsNullOrEmpty(resourceName.ToString()))
                {
                    var asparams = new AzureStorageParams { ContainerName = containerName.ToString(), ResourceName = resourceName.ToString() };
                    await SetTodoItemForImageUpload(asparams);
                    patch.TrySetPropertyValue("SasQueryString", asparams.SasQueryString);
                    patch.TrySetPropertyValue("AzureImageUri", asparams.AzureImageUri);
                    // Clear the flag also
                    patch.TrySetPropertyValue("ImageUploadPending", false);
                }
            }

            return await UpdateAsync(id, patch);
        }

        // POST tables/TodoItem
        public async Task<IHttpActionResult> PostTodoItem(TodoItem item)
        {
            var asparams = new AzureStorageParams { ContainerName = item.ContainerName, ResourceName = item.ResourceName };
            await SetTodoItemForImageUpload(asparams);
            item.SasQueryString = asparams.SasQueryString;
            item.AzureImageUri = asparams.AzureImageUri;
            item.ImageUploadPending = false;

            // Complete the insert operation.
            TodoItem current = await InsertAsync(item);

            // get Notification Hubs credentials associated with this Mobile App
            MobileAppSettingsDictionary settings = this.Configuration.GetMobileAppSettingsProvider().GetMobileAppSettings();
            string notificationHubName = settings.NotificationHubName;
            string notificationHubConnection = settings.Connections[MobileAppSettingsKeys.NotificationHubConnectionString].ConnectionString;

            // connect to notification hub
            NotificationHubClient Hub = NotificationHubClient.CreateClientFromConnectionString(notificationHubConnection, notificationHubName);

            // windows payload
            var windowsToastPayload = @"<toast><visual><binding template=""ToastText01""><text id=""1"">" + item.Title + @"</text></binding></visual></toast>";

            await Hub.SendWindowsNativeNotificationAsync(windowsToastPayload);

            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        private async Task SetTodoItemForImageUpload(AzureStorageParams asparams)
        {
            string storageAccountName;
            string storageAccountKey;

            MobileAppSettingsDictionary settings = this.Configuration.GetMobileAppSettingsProvider().GetMobileAppSettings();

            // Try to get the Azure storage account token from app settings.  
            if (!(settings.TryGetValue("STORAGE_ACCOUNT_NAME", out storageAccountName) |
            settings.TryGetValue("STORAGE_ACCOUNT_ACCESS_KEY", out storageAccountKey)))
            {
                ITraceWriter traceWriter = this.Configuration.Services.GetTraceWriter();
                traceWriter.Error("Could not retrieve storage account settings.");
            }

            // Set the URI for the Blob Storage service.
            Uri blobEndpoint = new Uri(string.Format("https://{0}.blob.core.windows.net", storageAccountName));

            // Create the BLOB service client.
            CloudBlobClient blobClient = new CloudBlobClient(blobEndpoint,
                new StorageCredentials(storageAccountName, storageAccountKey));

            if (asparams.ContainerName != null)
            {
                // Set the BLOB store container name on the item, which must be lowercase.
                asparams.ContainerName = asparams.ContainerName.ToLower();

                // Create a container, if it doesn't already exist.
                CloudBlobContainer container = blobClient.GetContainerReference(asparams.ContainerName);
                await container.CreateIfNotExistsAsync();

                // Create a shared access permission policy. 
                BlobContainerPermissions containerPermissions = new BlobContainerPermissions();

                // Enable anonymous read access to BLOBs.
                containerPermissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                container.SetPermissions(containerPermissions);

                // Define a policy that gives write access to the container for 5 minutes.                                   
                SharedAccessBlobPolicy sasPolicy = new SharedAccessBlobPolicy()
                {
                    SharedAccessStartTime = DateTime.UtcNow,
                    SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(5),
                    Permissions = SharedAccessBlobPermissions.Write
                };

                // Get the SAS as a string.
                asparams.SasQueryString = container.GetSharedAccessSignature(sasPolicy);

                // Set the URL used to store the image.
                asparams.AzureImageUri = string.Format("{0}{1}/{2}", blobEndpoint.ToString(),
                    asparams.ContainerName, asparams.ResourceName);
            }

        }

        // DELETE tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteTodoItem(string id)
        {
            return DeleteAsync(id);
        }
    }

    internal class AzureStorageParams
    {
        public string ContainerName { get; set; }
        public string ResourceName { get; set; }
        public string SasQueryString { get; set; }
        public string AzureImageUri { get; set; }
    }
}