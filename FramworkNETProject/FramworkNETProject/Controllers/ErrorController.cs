using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult General(Exception exception)
        {
            if (exception == null)
            {
                exception = new ApplicationException(Resources.Language.错误);
            }
            if (exception.Message == "SessionError" || (Session != null && Session["SessionError"] == "1"))
            {
                return Content("<script>location.href='/logon/index'</script>");
            }
            else if (exception.Message == "没有权限")
            {
                return Content("<script>alert('" + Resources.Language.您没有权限访问此功能 + "')</script>");
            }
            else
            {
                HandleErrorInfo error = new HandleErrorInfo(exception, "Error", "General");
                ViewData["error"] = exception;
                return View("Error", error);
            }
        }
    }
}
