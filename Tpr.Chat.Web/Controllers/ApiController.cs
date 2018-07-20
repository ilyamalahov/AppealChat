using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tpr.Chat.Web.ViewModels;

namespace Tpr.Chat.Web.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ApiController : Controller
    {
        public JsonResult CreateChat([FromBody] CreateChatViewModel viewModel)
        {
            return Json("");
        }
    }
}