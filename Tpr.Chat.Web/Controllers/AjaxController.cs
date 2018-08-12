using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Tpr.Chat.Web.Controllers
{
    [Route("ajax")]
    public class AjaxController : Controller
    {
        [Route("complete")]
        public IActionResult ShowCompleteModal()
        {
            return PartialView("_CompleteModal");
        }

        [Route("changeexpert")]
        public IActionResult ShowChangeExpertModal()
        {
            return PartialView("_ChangeExpertModal");
        }
    }
}