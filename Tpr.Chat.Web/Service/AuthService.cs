using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public class AuthService : IAuthService
    {
        public IConfiguration Configuration { get; }

        public AuthService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public ClaimsIdentity GetIdentity(Guid appealId, string expertKey = null)
        {
            // Identity claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, appealId.ToString())
            };

            // Expert
            if (expertKey != null)
            {
                claims.Add(new Claim("expertkey", expertKey));
            }

            return new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
        }

        public string CreateToken(ClaimsIdentity identity, TimeSpan expiryTime)
        {
            var jwtConfiguration = Configuration.GetSection("JWT");

            try
            {
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration["SecretKey"]));

                var token = new JwtSecurityToken(
                    jwtConfiguration["Issuer"],
                    jwtConfiguration["Audience"],
                    identity.Claims,
                    expires: DateTime.UtcNow.Add(expiryTime),
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                return null;
            }
        }
    }
}
