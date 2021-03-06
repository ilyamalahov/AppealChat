﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tpr.Chat.Core.Repositories;

namespace Tpr.Chat.Web.Controllers
{
    public class ModalController : Controller
    {
        private readonly IChatRepository chatRepository;

        public ModalController(IChatRepository chatRepository)
        {
            this.chatRepository = chatRepository;
        }
        
        public IActionResult ChangeExpert()
        {
            return PartialView("ChangeExpert");
        }

        public IActionResult WaitChange(Guid appealId)
        {
            return PartialView("WaitChange");
        }
        
        public IActionResult CompleteChat(Guid appealId)
        {
            return PartialView("CompleteChat");
        }

        public IActionResult Contacts()
        {
            return PartialView("Contacts");
        }

        public async Task<IActionResult> AppealInfo(Guid appealId)
        {
            var chatSession = await chatRepository.GetChatSession(appealId);

            if (chatSession == null)
            {
                return BadRequest("Сессии по данному ID апеллянта не существует");
            }

            ViewBag.IsBefore = DateTime.Now < chatSession.StartTime;

            ViewBag.Session = chatSession;

            return PartialView();
        }

    }
}