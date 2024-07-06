namespace Crux.CRL.NotificationSystem
{
    //These are the wappers for notification messages
    public class NotificationWrapper : INotificationWrapper
    {

    }

    public class NotificationWrapper<TNotification> : INotificationWrapper
    {
        public TNotification notification;

        public NotificationWrapper(TNotification pNotification)
        {
            notification = pNotification;
        }
    }

    public class NotificationWrapper<TNotification1, TNotification2> : INotificationWrapper
    {
        public TNotification1 notification1;

        public TNotification2 notification2;

        public NotificationWrapper(TNotification1 pNotification1, TNotification2 pNotification2)
        {
            notification1 = pNotification1;
            notification2 = pNotification2;
        }
    }

    public class NotificationWrapper<TNotification1, TNotification2, TNotification3> : INotificationWrapper
    {
        public TNotification1 notification1;

        public TNotification2 notification2;

        public TNotification3 notification3;

        public NotificationWrapper(TNotification1 pNotification1, TNotification2 pNotification2, TNotification3 pNotification3)
        {
            notification1 = pNotification1;
            notification2 = pNotification2;
            notification3 = pNotification3;
        }
    }

    public class NotificationWrapper<TNotification1, TNotification2, TNotification3, TNotification4> : INotificationWrapper
    {
        public TNotification1 notification1;

        public TNotification2 notification2;

        public TNotification3 notification3;

        public TNotification4 notification4;

        public NotificationWrapper(TNotification1 pNotification1, TNotification2 pNotification2, TNotification3 pNotification3, TNotification4 pNotification4)
        {
            notification1 = pNotification1;
            notification2 = pNotification2;
            notification3 = pNotification3;
            notification4 = pNotification4;
        }
    }
}