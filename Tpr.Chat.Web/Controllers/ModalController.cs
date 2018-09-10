using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Tpr.Chat.Web.Controllers
{
    public class ModalController : Controller
    {
        public IActionResult ChangeExpertWait(Guid appealId)
        {
            return PartialView("ChangeExpertWait");
        }
    }
}