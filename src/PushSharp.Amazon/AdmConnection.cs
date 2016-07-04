namespace PushSharp.Amazon
{
    using Core;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class AdmServiceConnectionFactory : IServiceConnectionFactory<AdmNotification>
    {
        public AdmServiceConnectionFactory(AdmConfiguration configuration)
        {
            Configuration = configuration;
        }

        public AdmConfiguration Configuration { get; }

        public IServiceConnection<AdmNotification> Create()
        {
            return new AdmServiceConnection(Configuration);
        }
    }

    public class AdmServiceBroker : ServiceBroker<AdmNotification>
    {
        public AdmServiceBroker(AdmConfiguration configuration) : base(new AdmServiceConnectionFactory(configuration))
        {
        }
    }

    public class AdmServiceConnection : IServiceConnection<AdmNotification>
    {
        private readonly HttpClient http = new HttpClient();

        public AdmServiceConnection(AdmConfiguration configuration)
        {
            Configuration = configuration;

            Expires = DateTime.UtcNow.AddYears(-1);

            http.DefaultRequestHeaders.Add("X-Amzn-Type-Version", "com.amazon.device.messaging.ADMMessage@1.0");
            http.DefaultRequestHeaders.Add("X-Amzn-Accept-Type", "com.amazon.device.messaging.ADMSendResult@1.0");
            http.DefaultRequestHeaders.Add("Accept", "application/json");
            http.DefaultRequestHeaders.ConnectionClose = true;

            http.DefaultRequestHeaders.Remove("connection");
        }

        public AdmConfiguration Configuration { get; }

        public DateTime Expires { get; set; }

        public DateTime LastRequest { get; private set; }

        public string LastAmazonRequestId { get; private set; }

        public string AccessToken { get; private set; }

        public async Task Send(AdmNotification notification)
        {
            try
            {
                if (string.IsNullOrEmpty(AccessToken) || Expires <= DateTime.UtcNow)
                {
                    await UpdateAccessToken();
                    http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + AccessToken);
                }

                var sc = new StringContent(notification.ToJson());
                sc.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await http.PostAsync(string.Format(Configuration.AdmSendUrl, notification.RegistrationId), sc);

                // We're done here if it was a success
                if (response.IsSuccessStatusCode)
                {
                    return;
                }

                var data = await response.Content.ReadAsStringAsync();

                var json = JObject.Parse(data);

                var reason = json["reason"].ToString();

                var regId = notification.RegistrationId;

                if (json["registrationID"] != null)
                {
                    regId = json["registrationID"].ToString();
                }

                switch (response.StatusCode)
                {
                    case HttpStatusCode.BadGateway:
                    case HttpStatusCode.BadRequest:
                        if ("InvalidRegistrationId".Equals(reason, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new DeviceSubscriptionExpiredException(notification)
                            {
                                OldSubscriptionId = regId,
                                ExpiredAt = DateTime.UtcNow
                            };
                        }

                        throw new NotificationException("Notification Failed: " + reason, notification);

                    case HttpStatusCode.Unauthorized:
                        // Access token expired
                        AccessToken = null;
                        throw new UnauthorizedAccessException("Access token failed authorization");

                    case HttpStatusCode.Forbidden:
                        throw new AdmRateLimitExceededException(reason, notification);

                    case HttpStatusCode.RequestEntityTooLarge:
                        throw new AdmMessageTooLargeException(notification);

                    default:
                        throw new NotificationException("Unknown ADM Failure", notification);
                }
            }
            catch (Exception ex)
            {
                throw new NotificationException("Unknown ADM Failure", notification, ex);
            }
        }

        private async Task UpdateAccessToken()
        {
            var http = new HttpClient();

            var param = new Dictionary<string, string>();
            param.Add("grant_type", "client_credentials");
            param.Add("scope", "messaging:push");
            param.Add("client_id", Configuration.ClientId);
            param.Add("client_secret", Configuration.ClientSecret);

            var result = await http.PostAsync(Configuration.AdmAuthUrl, new FormUrlEncodedContent(param));
            var data = await result.Content.ReadAsStringAsync();

            var json = JObject.Parse(data);

            AccessToken = json["access_token"].ToString();

            JToken expiresJson = new JValue(3540);
            if (json.TryGetValue("expires_in", out expiresJson))
            {
                Expires = DateTime.UtcNow.AddSeconds(expiresJson.ToObject<int>() - 60);
            }
            else
            {
                Expires = DateTime.UtcNow.AddSeconds(3540);
            }

            if (result.Headers.Contains("X-Amzn-RequestId"))
            {
                this.LastAmazonRequestId = string.Join("; ", result.Headers.GetValues("X-Amzn-RequestId"));
            }

            LastRequest = DateTime.UtcNow;
        }
    }
}
