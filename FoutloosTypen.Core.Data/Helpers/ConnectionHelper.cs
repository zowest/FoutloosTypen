using Microsoft.Extensions.Configuration;

namespace FoutloosTypen.Core.Data.Helpers
{
    public static class ConnectionHelper
    {
        public static string? ConnectionStringValue(string name)
        {
            IConfigurationRoot config = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json").Build();
            IConfigurationSection section = config.GetSection("ConnectionStrings");
            return section.GetValue<string>(name);
        }
    }
}
