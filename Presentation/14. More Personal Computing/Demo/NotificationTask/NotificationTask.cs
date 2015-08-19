using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace NotificationTask
{
    public sealed class NotificationTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferal = taskInstance.GetDeferral();
            var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
            if (details == null)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }

            var arguments = details.Argument.Split(':');

            if (arguments.Count() > 0)
            {

                switch(arguments[0])
                {
                    case "complete":
                        if (arguments.Count() > 1)
                        {
                            string itemId = arguments[1];
                            var todoItemRepository = TODOAdaptiveUISample.Repositories.TodoItemFileRepository.GetInstance();

                            var toDo = await todoItemRepository.RefreshTodoItemAsync(itemId);
                            toDo.IsComplete = true;
                            await todoItemRepository.UpdateTodoItem(toDo);
                        }
                        break;
                }
            }


            deferal.Complete();
        }
    }
}
