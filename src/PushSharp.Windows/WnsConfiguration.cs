namespace PushSharp.Windows
{
    public class WnsConfiguration
    {
        public WnsConfiguration(string packageName, string packageSecurityIdentifier, string clientSecret)
        {
            PackageName = packageName;
            PackageSecurityIdentifier = packageSecurityIdentifier;
            ClientSecret = clientSecret;
        }

        public string PackageName { get; }

        public string PackageSecurityIdentifier { get; }

        public string ClientSecret { get; }
    }
}

