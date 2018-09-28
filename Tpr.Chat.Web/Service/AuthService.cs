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

        public string CreateToken(Guid appealId, string expertKey = null)
        {
            try
            {
                var jwtConfiguration = Configuration.GetSection("JWT");

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, appealId.ToString())
                };

                // Expert
                if (expertKey != null)
                {
                    claims.Add(new Claim("expertkey", expertKey));
                }

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration["SecretKey"]));

                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    jwtConfiguration["Issuer"],
                    jwtConfiguration["Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddSeconds(30),
                    signingCredentials: credentials
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
