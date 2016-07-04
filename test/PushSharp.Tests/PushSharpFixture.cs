namespace PushSharp.Tests
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.PlatformAbstractions;
    using System;

    public class PushSharpFixture : IDisposable
    {
        public PushSharpFixture()
        {
            this.BasePath = PlatformServices.Default.Application.ApplicationBasePath;

            var builder = new ConfigurationBuilder()
                .SetBasePath(BasePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.development.json", optional: true);

            this.Configuration = builder.Build();

            var services = new ServiceCollection();

            services.AddOptions();

            this.Services = services.BuildServiceProvider();
        }

        public IConfigurationRoot Configuration { get; }

        public IServiceProvider Services { get; }

        public string BasePath { get; }

        public void Dispose()
        {
        }
    }
}
