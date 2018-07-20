using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Jwt
{
    public class AuthOptions
    {
        // издатель токена
        public const string ISSUER = "MyAuthServer";
        
        // потребитель токена
        public const string AUDIENCE = "http://localhost:51884/";
        
        // ключ для шифрации
        const string KEY = "mysupersecret_secretkey!123";
        
        // время жизни токена - 1 минута
        public const int LIFETIME = 1;

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
