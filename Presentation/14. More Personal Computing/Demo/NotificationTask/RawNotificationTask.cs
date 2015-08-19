using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Networking.PushNotifications;
using Windows.UI.Notifications;

namespace NotificationTask
{
    public sealed class RawNotificationTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferal = taskInstance.GetDeferral();
            if (taskInstance.TriggerDetails is RawNotification)
            {
                var details = taskInstance.TriggerDetails as RawNotification;
                var arguments = details.Content.Split(':');

                if (arguments.Count() > 0)
                {

                    switch (arguments[0])
                    {
                        case "new_items":
                            if (arguments.Count() > 1)
                            {
                                XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
                                XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
                                badgeElement.SetAttribute("value", arguments[1]);
                                BadgeNotification badge = new BadgeNotification(badgeXml);
                                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
                            }
                            break;
                    }
                }
            }


            

            


            deferal.Complete();
        }
    }
}
