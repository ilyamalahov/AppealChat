using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Models;

namespace Tpr.Chat.Web.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly IChatRepository chatRepository;

        public ApiController(IChatRepository chatRepository)
        {
            this.chatRepository = chatRepository;
        }

        public JsonResult CreateChat([FromBody] CreateChatViewModel viewModel)
        {
            return Json("");
        }
    }
}