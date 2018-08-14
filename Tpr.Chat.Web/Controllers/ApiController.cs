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
    [Route("api/[controller]")]
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

        public JsonResult SetExpert(Guid appealId, int expertKey)
        {
            // 
            var experts = chatRepository.GetExperts(appealId);

            // 
            if (experts.Count() == 2)
            {
                var response = new { error = "Max count of experts attached to this session" };

                return Json(response);
            }

            //
            var result = chatRepository.AddExpert(appealId, expertKey);

            if(!result)
            {
                var response = new { error = "Add expert error" };

                return Json(response);
            }

            // Set current expert to appeal session
            var chatSession = chatRepository.GetChatSession(appealId);

            chatSession.CurrentExpertKey = expertKey;

            // 
            if(!chatRepository.UpdateSession(chatSession))
            {
                var response = new { error = "Update session error" };

                return Json(response);
            }

            // 
            return Json(new { expertKey });
        }
    }
}