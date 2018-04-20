using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DLMS.Models;

namespace DLMS.Filters
{
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {        
        public override void OnException(ExceptionContext filterContext)
        {                
             base.OnException(filterContext);
        }
    }
}