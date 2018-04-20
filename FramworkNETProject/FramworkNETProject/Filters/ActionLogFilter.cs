using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml.Serialization;
using Models.System;
using Models;
using SupportClasses;
using Controllers;
using DataAccess.SqlServer;
using ViewModels;

namespace Filters
{
    public class ActionLogFilter : ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //if (filterContext.Controller is ErrorController || 
            //    (filterContext.Controller is HomeController && (filterContext.ActionDescriptor.ActionName == "TopBar" || filterContext.ActionDescriptor.ActionName == "Menu")))
            //{
            //    base.OnActionExecuting(filterContext);
            //    return;
            //}
            ActionLog log = new ActionLog();
            log.LogType = ActionLogTypes.正常.ToString();
            log.ActionTime = DateTime.Now;
            string itcode = "";
            if (filterContext.HttpContext.Session == null || filterContext.HttpContext.Session["FFUser"] == null)
            {
                if (filterContext.HttpContext.User != null)
                    itcode = filterContext.HttpContext.User.Identity.Name;
            }
            else
            {
                itcode = (filterContext.HttpContext.Session["FFUser"] as OCFUser).ITCode;
            }
            log.ITCode = itcode;
            var ControllerDes = filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(ActionDescriptionAttribute), false).Cast<ActionDescriptionAttribute>().FirstOrDefault();
            var ActionDes = filterContext.ActionDescriptor.GetCustomAttributes(typeof(ActionDescriptionAttribute), false).Cast<ActionDescriptionAttribute>().FirstOrDefault();

            log.MLContents = new List<ActionLogMLContent>();
            foreach (var item in BaseController.Languages)
            {
                ActionLogMLContent alm = new ActionLogMLContent { LanguageCode = item.LanguageCode };
                CultureInfo culture = CultureInfo.CreateSpecificCulture(item.LanguageCode);
                if (ControllerDes != null)
                {
                    alm.ModelName = Resources.Language.ResourceManager.GetString(ControllerDes.Description, culture);
                }
                if (ActionDes != null)
                {
                    alm.ActionName = Resources.Language.ResourceManager.GetString(ActionDes.Description, culture);
                }
                log.MLContents.Add(alm);
            }
            log.ActionUrl = filterContext.HttpContext.Request.Url.ToString();
            var paras = filterContext.ActionDescriptor.GetParameters();
            log.Remark = "";
            foreach (var item in filterContext.ActionParameters)
            {
                string s = "";
                if (item.Value != null)
                {
                    if (item.Value is BasePoco || item.Value is BaseVM || item.Value is BaseSearcher)
                    {
                        try
                        {
                            XmlSerializer x = new XmlSerializer(item.Value.GetType());
                            MemoryStream ms = new MemoryStream();
                            TextWriter writer = new StreamWriter(ms);
                            x.Serialize(writer, item.Value);
                            TextReader reader = new StreamReader(ms);
                            ms.Position = 0;
                            s = reader.ReadToEnd();
                            s = Regex.Replace(s, "<\\?.*?\\?>", "");
                            s = Regex.Replace(s, "\\s+xmlns:xs.=\".*?\"", "");
                        }
                        catch
                        {
                            s = string.Empty;
                        }
                    }
                    else
                    {
                        s = item.Value.ToString();
                    }
                }
                log.Remark += item.Key + "=" + s + Environment.NewLine;
            }
            filterContext.Controller.ViewBag.FFLog = log;
            base.OnActionExecuting(filterContext);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);

            try
            {
                ActionLog log = filterContext.Controller.ViewBag.FFLog;
                if (log == null)
                {
                    return;
                }
                if (filterContext.Exception != null)
                {
                    log.Remark += Environment.NewLine + "=====Exception=====" + Environment.NewLine + filterContext.Exception.ToString();
                    log.LogType = ActionLogTypes.异常.ToString();
                }
                if (filterContext.Controller is ErrorController)
                {
                    log.LogType = ActionLogTypes.异常.ToString();
                }
                log.Duration = DateTime.Now.Subtract(log.ActionTime).TotalSeconds;
                DataContext dc = new DataContext();
                dc.ActionLogs.Add(log);
                dc.SaveChanges();
            }
            catch { }
        }
    }
}