namespace PushSharp.Windows
{
    using Core;

    public class WnsNotificationException : NotificationException
    {
        public WnsNotificationException(WnsNotificationStatus status)
            : base(status.ErrorDescription, status.Notification)
        {
            Notification = status.Notification;
            Status = status;
        }

        public new WnsNotification Notification { get; set; }

        public WnsNotificationStatus Status { get; }

        public override string ToString()
        {
            return base.ToString() + " Status = " + Status.HttpStatus;
        }
    }
}
