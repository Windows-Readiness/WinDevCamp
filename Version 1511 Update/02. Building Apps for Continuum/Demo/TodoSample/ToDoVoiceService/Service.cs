using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace ToDoVoiceService
{
    public sealed class Service : XamlRenderingBackgroundTask
    {
        private BackgroundTaskDeferral serviceDeferral;
        VoiceCommandServiceConnection voiceServiceConnection;

        TODOAdaptiveUISample.Repositories.ITodoItemRepository _todoItemRepository;

        protected override async void OnRun(IBackgroundTaskInstance taskInstance)
        {
            this.serviceDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnTaskCanceled;

            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            
            VoiceCommandResponse response;
            try
            {
                voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);
                voiceServiceConnection.VoiceCommandCompleted += VoiceCommandCompleted;
                VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();
                VoiceCommandUserMessage userMessage = new VoiceCommandUserMessage();

                List<VoiceCommandContentTile> contentTiles;

                switch (voiceCommand.CommandName)
                {
                    case "what":

                        _todoItemRepository = TODOAdaptiveUISample.Repositories.TodoItemFileRepository.GetInstance();
                        var data = await _todoItemRepository.RefreshTodoItemsAsync();

                        contentTiles = new List<VoiceCommandContentTile>();
                        
                        userMessage.SpokenMessage = "Your Top To Do's are: ";

                        foreach (var item in data.Where(x => x.IsComplete == false).OrderBy(x => x.DueDate).Take((int)VoiceCommandResponse.MaxSupportedVoiceCommandContentTiles))
                        {
                            var tile = new VoiceCommandContentTile();
                            tile.ContentTileType = VoiceCommandContentTileType.TitleWithText;
                            tile.Title = item.Title;
                            //tile.TextLine1 = item.Details;
                            contentTiles.Add(tile);

                            userMessage.SpokenMessage += item.Title + ", ";
                        }

                        userMessage.DisplayMessage = "Here are the top " + contentTiles.Count + " To Do's";

                        
                        
                        response = VoiceCommandResponse.CreateResponse(userMessage, contentTiles);
                        await voiceServiceConnection.ReportSuccessAsync(response);

                        break;


                    case "new":
                        var todo = voiceCommand.Properties["todo"][0];

                        var responseMessage = new VoiceCommandUserMessage()
                        {
                            DisplayMessage = String.Format("Add \"{0}\" to your To Do's?", todo),
                            SpokenMessage = String.Format("Do you want me to add \"{0}\" to your To Do's?", todo)
                        };

                        var repeatMessage = new VoiceCommandUserMessage()
                        {
                            DisplayMessage = String.Format("Are you sure you want me to add \"{0}\" to your To Do's?", todo),
                            SpokenMessage = String.Format("Are you sure you want me to add \"{0}\" to your To Do's?", todo)
                        };

                        bool confirmed = false;
                        response = VoiceCommandResponse.CreateResponseForPrompt(responseMessage, repeatMessage);
                        try
                        {
                            var confirmation = await voiceServiceConnection.RequestConfirmationAsync(response);
                            confirmed = confirmation.Confirmed;
                        }
                        catch
                        {

                        }
                        if (confirmed)
                        {
                            _todoItemRepository = TODOAdaptiveUISample.Repositories.TodoItemFileRepository.GetInstance();
                            var i = _todoItemRepository.Factory(title: todo);
                            await _todoItemRepository.InsertTodoItem(i);

                            var todos = await _todoItemRepository.RefreshTodoItemsAsync();

                            contentTiles = new List<VoiceCommandContentTile>();

                            foreach (var itm in todos.Where(x => x.IsComplete == false).OrderBy(x => x.DueDate).Take((int)VoiceCommandResponse.MaxSupportedVoiceCommandContentTiles))
                            {
                                var tile = new VoiceCommandContentTile();
                                tile.ContentTileType = VoiceCommandContentTileType.TitleWithText;
                                tile.Title = itm.Title;
                                contentTiles.Add(tile);
                            }

                            userMessage.SpokenMessage = "Done and Done! Here are your top To Do's";
                            userMessage.DisplayMessage = "Here are your top " + contentTiles.Count + " To Do's";

                            response = VoiceCommandResponse.CreateResponse(userMessage, contentTiles);
                            await voiceServiceConnection.ReportSuccessAsync(response);
                        }
                        else
                        {
                            userMessage.DisplayMessage = userMessage.SpokenMessage = "OK then";
                            response = VoiceCommandResponse.CreateResponse(userMessage);
                            await voiceServiceConnection.ReportSuccessAsync(response);
                        }



                        break;

                }
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            finally
            {
                if (this.serviceDeferral != null)
                {
                    //Complete the service deferral
                    this.serviceDeferral.Complete();
                }
            }
        }


        private void VoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (this.serviceDeferral != null)
            {
                this.serviceDeferral.Complete();
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (this.serviceDeferral != null)
            {
                this.serviceDeferral.Complete();
            }
        }
    }
}
