using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Server
{
    public class AuthOptions
    {
        public string ISSUER;
        public string AUDIENCE;
        static string KEY;

        public AuthOptions(IConfiguration configuration)
        {
            ISSUER = configuration["TokenSettings:ISSUER"];
            AUDIENCE = configuration["TokenSettings:AUDIENCE"];
            KEY = configuration["TokenSettings:KEY"];
        }
        public SymmetricSecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
    }
}
