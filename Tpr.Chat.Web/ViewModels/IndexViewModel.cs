﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.ViewModels
{
    public class IndexViewModel
    {
        public Guid AppealId { get; set; }

        public bool IsExpert { get; internal set; }
    }
}