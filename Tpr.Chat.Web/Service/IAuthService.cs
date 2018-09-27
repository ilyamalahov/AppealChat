using System;
using System.Security.Claims;

namespace Tpr.Chat.Web.Service
{
    public interface IAuthService
    {
        ClaimsIdentity GetIdentity(Guid appealId, string expertKey = null);
        string CreateToken(ClaimsIdentity identity, TimeSpan expiryTime);
    }
}