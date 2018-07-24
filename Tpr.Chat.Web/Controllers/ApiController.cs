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
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ApiController : Controller
    {
        [Authorize]
        public JsonResult CreateChat([FromBody] CreateChatViewModel viewModel)
        {
            return Json("");
        }

        public JsonResult UpdateInfo(Guid appealId, string expertKey = null, string expertPassword = null)
        {
            return Json("");
        }
    }
}