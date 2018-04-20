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
                exception = new ApplicationException(Resources.Language.����);
            }
            if (exception.Message == "SessionError" || (Session != null && Session["SessionError"] == "1"))
            {
                return Content("<script>location.href='/logon/index'</script>");
            }
            else if (exception.Message == "û��Ȩ��")
            {
                return Content("<script>alert('" + Resources.Language.��û��Ȩ�޷��ʴ˹��� + "')</script>");
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
