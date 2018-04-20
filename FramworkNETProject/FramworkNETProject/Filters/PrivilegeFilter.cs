using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DLMS.DataAccess;
using DLMS.Controllers;
using DLMS.SupportClasses;
using System.Web.Routing;

namespace DLMS.Filters
{
    public class PrivilegeFilter : ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.Controller is ErrorController)
            {
                base.OnActionExecuting(filterContext);
                return;
            }
            //排除登陆界面controller验证权限
            if (filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.ToLower() == "logon")
            {
                return;
            }
            OCFUser user = (filterContext.Controller as BaseController).FFUser;
            DataContext dc = new DataContext();
            string url = "/" + filterContext.ActionDescriptor.ControllerDescriptor.ControllerName + "/" + filterContext.ActionDescriptor.ActionName;
            if (dc.PageFunctions.Where(x => x.Url.ToLower() == url || (x.Url.ToLower().Contains(url.ToLower()) && x.Url.Contains("*"))).Count() > 0)
            {
                if (!user.OCFRoles.Contains("Administrator") && filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.ToLower() != "home")
                {
                    var check = dc.PageFunctions.Where(x => x.Privileges.Any(y => user.OCFRoles.Contains(y.RoleName)) && ((x.Url.ToLower() == url.ToLower()) || (x.Url.ToLower().Contains(url.ToLower()) && x.Url.Contains("*")))).FirstOrDefault();
                    if (check == null)
                    {
                        //filterContext.HttpContext.Response.Clear();
                        //var routeData = new RouteData();
                        var exp = new ApplicationException("没有权限");
                        //routeData.Values["controller"] = "Error";
                        //routeData.Values["action"] = "General";
                        //routeData.Values["exception"] = exp;
                        //IController errorsController = new ErrorController();
                        //var rc = new RequestContext(filterContext.HttpContext, routeData);
                        //errorsController.Execute(rc);
                        throw exp;
                    }
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}