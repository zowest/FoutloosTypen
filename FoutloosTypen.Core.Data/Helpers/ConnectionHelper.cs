using Microsoft.Extensions.Configuration;

namespace FoutloosTypen.Core.Data.Helpers
{
    public static class ConnectionHelper
    {
        private static IConfigurationRoot BuildConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true);
            return builder.Build();
        }

        public static string GetProvider()
        {
            var config = BuildConfig();
            return config.GetValue<string>("Database:Provider") ?? "Sqlite";
        }

        public static string GetConnectionString()
        {
            var config = BuildConfig();
            var provider = GetProvider();

            var value = config
                .GetSection("ConnectionStrings")
                .GetValue<string>(provider);

            return provider == "Sqlite"
                ? value ?? "foutloostypen.db"
                : value ?? throw new InvalidOperationException("MySQL connectionstring ontbreekt");
        }
    }
}
