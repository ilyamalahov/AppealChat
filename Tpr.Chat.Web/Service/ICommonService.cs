﻿using System;
using System.Security.Claims;

namespace Tpr.Chat.Web.Service
{
    public interface ICommonService
    {
        ClaimsIdentity GetIdentity(Guid appealId, int key = 0, string secretKey = null);
        string CreateToken(ClaimsIdentity identity, DateTime beginDate, DateTime finishDate);
    }
}