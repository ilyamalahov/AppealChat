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
    public class CommonService : ICommonService
    {
        private readonly IConfiguration configuration;

        public CommonService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public ClaimsIdentity GetIdentity(Guid appealId, int key, string secretKey)
        {
            try
            {
                // Nick name
                string nickname = key > 0 ? "Член конфликтной комиссии №" + key : "Аппелянт";

                // Identity claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, appealId.ToString()),
                    new Claim("Nickname", nickname),
                };

                // Expert
                if (key > 0)
                {
                    // Secret checkings, etc...

                    claims.Add(new Claim("ExpertKey", key.ToString()));
                }

                return new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string CreateToken(ClaimsIdentity identity, DateTime beginDate, DateTime finishDate)
        {
            try
            {
                var jwtConfiguration = configuration.GetSection("JWT");

                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration["SecretKey"]));

                var token = new JwtSecurityToken(
                    jwtConfiguration["Issuer"],
                    jwtConfiguration["Audience"],
                    identity.Claims,
                    beginDate,
                    finishDate,
                    new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
