namespace PushSharp.Tests
{
    using Google;
    using Xunit;

    [Collection(nameof(TestCollection)), Trait("Provider", "Google")]
    public class GcmTests
    {
        [Fact]
        public void GcmNotification_Priority_Should_Serialize_As_String_High()
        {
            var n = new GcmNotification();
            n.Priority = GcmNotificationPriority.High;

            var str = n.ToString();

            Assert.True(str.Contains("high"));
        }

        [Fact]
        public void GcmNotification_Priority_Should_Serialize_As_String_Normal()
        {
            var n = new GcmNotification();
            n.Priority = GcmNotificationPriority.Normal;

            var str = n.ToString();

            Assert.True(str.Contains("normal"));
        }
    }
}
