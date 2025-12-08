using Microsoft.Extensions.Configuration;

namespace FoutloosTypen.Core.Data.Helpers
{
    public static class ConnectionHelper
    {
        public static string ConnectionStringValue(string name)
        {
            // Try to load appsettings.json optionally; provide sensible fallback if missing
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

            IConfigurationRoot config = builder.Build();
            IConfigurationSection section = config.GetSection("ConnectionStrings");
            var value = section.GetValue<string>(name);
            return string.IsNullOrWhiteSpace(value) ? "foutloostypen.db" : value;
        }
    }
}