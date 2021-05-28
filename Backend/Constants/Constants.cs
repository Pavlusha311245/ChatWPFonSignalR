using System.Configuration;

namespace Server.Constants
{
    public class Constants
    {
        public static string ConnectionString => ConfigurationManager.AppSettings["ConnectionString"] ?? "";
    }
}
