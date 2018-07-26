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
        public string Issuer { get; set; }

        // потребитель токена
        public string Audience { get; set; }

        // ключ для шифрации
        public string SecretKey { get; set; }
    }
}
