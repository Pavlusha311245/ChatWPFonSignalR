using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Server.Providers
{
    public class CustomUserIdProvider  : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}
