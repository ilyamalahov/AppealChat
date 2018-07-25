using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Tpr.Chat.Web.Jwt;

namespace Tpr.Chat.Web.Service
{
    public class TokenService
    {
        public string CreateToken(ClaimsIdentity identity)
        {
            return null;
        }

        private ClaimsIdentity GetIdentity(Guid appealId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, appealId.ToString())
            };

            return new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        }
    }
}
