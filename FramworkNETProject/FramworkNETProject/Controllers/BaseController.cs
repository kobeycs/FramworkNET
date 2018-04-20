using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using DLMS.SupportClasses;
using DataAccess.SqlServer;
using SupportClasses;
using ViewModels;
using Helpers;

namespace Controllers
{
    public class BaseController : Controller
    {
        protected IDataContext _dc;

        public static List<SupportedLanguage> Languages = new List<SupportedLanguage>
            {
                new SupportedLanguage{ LanguageCode = "zh-cn", LanguageName="简体中文"},
                new SupportedLanguage{ LanguageCode = "en", LanguageName="English"}
            };
        public SupportedLanguage CurrentLanguage { get; private set; }
        public FFSession _ffSession = new FFSession();

        public OCFUser FFUser
        {
            get
            {
                try
                {
                    _ffSession.Session = this.Session;
                }
                catch { }
                if (_ffSession["FFUser"] == null)
                {

                    // 判断是否session过期，如过期则报错并通知applicationerror
                    _ffSession["SessionError"] = "1";
                    throw new Exception("SessionError");


                    //if (User != null)
                    //{
                    //    string itcode = "";
                    //    if (User.Identity.Name.Contains('\\'))
                    //    {
                    //        itcode = User.Identity.Name.Split('\\')[1];
                    //    }
                    //    else
                    //    {
                    //        itcode = User.Identity.Name;
                    //    }
                    //    OCFUser user = OCFHelper.GetOCFUser(itcode);
                    //    _ffSession["FFUser"] = user;
                    //}
                }
                return _ffSession["FFUser"] as OCFUser;
            }
        }

        public void SetFFUser(OCFUser user)
        {
            _ffSession["FFUser"] = user;
        }

        public BaseController()
        {
        }

        public T CreateVM<T>(long? ID = null) where T : BaseVM
        {
            _ffSession.Session = this.Session;
            var ctor = typeof(T).GetConstructor(Type.EmptyTypes);
            T rv = ctor.Invoke(null) as T;
            rv.Session = this._ffSession;
            rv.FFUser = this.FFUser;
            rv.DC = this._dc;

            if (rv is IPagedList)
            {
                dynamic se = Session[rv.GetType().FullName];
                if (se != null)
                {
                    (se as BaseSearcher).TotalPages = 0;
                    (se as BaseSearcher).CurrentPage = 0;
                    (rv as dynamic).Searcher = se;
                }
                else
                {
                    se = Session[(rv as dynamic).Searcher.GetType().FullName];
                    if (se != null)
                    {
                        //(se as BaseSearcher).TotalPages = 0;
                        //(se as BaseSearcher).CurrentPage = 0;
                        (rv as dynamic).Searcher = se;
                    }
                }
            }

            //如果ViewModel T继承自BaseCRUDVM<>且ID有值，那么自动调用ViewModel的GetByID方法
            if (ID != null && typeof(T).IsSubclassOf(typeof(BaseCRUDVM<>).MakeGenericType((rv as dynamic).Entity.GetType())))
            {
                (rv as dynamic).Entity = (rv as dynamic).GetByID(ID.Value);
            }

            return rv;
        }

        protected ContentResult CloseWindowResult(string reloadPage = null, string alertMessage = null)
        {
            ContentResult rv = new ContentResult();
            if (!string.IsNullOrEmpty(alertMessage))
            {
                rv.Content += string.Format("<script>FF_OpenSimpleDialog('{0}','{1}','{2}');</script>", alertMessage, Resources.Language.确定, Resources.Language.提示, 200, 100);
            }

            DateTime now = DateTime.Now;
            rv.Content += "<p id='" + now.Ticks + "'></p>";
            rv.Content += "<script>var div = $('#" + now.Ticks + "');var dialog = FF_GetDialog(div);dialog.remove(); dialog.dialog('close');try{CloseEvent()}catch(e){}";
            if (!string.IsNullOrEmpty(reloadPage))
            {
                rv.Content += string.Format("FF_LoadPage('{0}','{1}','{2}');", reloadPage, Resources.Language.数据加载中, Resources.Language.提示);
            }
            rv.Content += "</script>";
            return rv;
        }





        /// <summary>
        /// 刷新弹出框 add by dufei
        /// </summary>
        /// <param name="reloadPage">弹出框的url</param>
        /// <param name="divID">弹出框的ID</param>
        /// <param name="alertMessage">提示消息</param>
        /// <returns></returns>
        protected ContentResult CloseAndReoladParentDialogResult(string reloadPage, string divID, string alertMessage)
        {
            ContentResult rv = new ContentResult();
            if (!string.IsNullOrEmpty(alertMessage))
            {
                rv.Content += string.Format("<script>FF_OpenSimpleDialog('{0}','{1}','{2}');</script>", alertMessage, Resources.Language.确定, Resources.Language.提示, 200, 100);
            }

            DateTime now = DateTime.Now;
            rv.Content += "<p id='" + now.Ticks + "'></p>";
            rv.Content += "<script>var div = $('#" + now.Ticks + "');var dialog = FF_GetDialog(div);dialog.remove(); dialog.dialog('close');";
            if (!string.IsNullOrEmpty(reloadPage))
            {
                rv.Content += string.Format("FF_RelodDialog('{0}','{1}');", reloadPage, divID);
            }
            rv.Content += "</script>";
            return rv;
        }

        protected ActionResult AlertResult(string url, string Title,int Width, int Height,string formID,string DialogID)
        {
            ContentResult rv = new ContentResult();

                rv.Content += string.Format("<script>alert('1');FF_OpenSubmitDialog2('{0}','{1}','{2}','{3}',{4},{5},'{6}','{7}');</script>", url, Resources.Language.提交, Resources.Language.取消,Title,Width,Height,formID,DialogID);

                rv.Content += string.Format("<script>alert('2');FF_LoadPage('{0}','{1}','{2}');</script>", url, Resources.Language.数据加载中, Resources.Language.提示);
            return rv;
        }
        protected ActionResult AlertResult(string url, string alertMessage)
        {
            ContentResult rv = new ContentResult();
            if (!string.IsNullOrEmpty(alertMessage))
            {
                rv.Content += string.Format("<script>FF_OpenSimpleDialog('{0}','{1}','{2}');</script>", alertMessage, Resources.Language.确定, Resources.Language.提示, 200, 100);
            }

            rv.Content += string.Format("<script>FF_LoadPage('{0}','{1}','{2}');</script>", url, Resources.Language.数据加载中, Resources.Language.提示);
            return rv;
        }

        protected ActionResult AlertResultAction(string alertMessage)
        {
            ContentResult rv = new ContentResult();
            if (!string.IsNullOrEmpty(alertMessage))
            {
                rv.Content += string.Format("<script>alert('" + alertMessage + "');</script>");
            }
            return rv;
        }

        protected FileResult ExcelResult(string fileName, IPagedList list)
        {
            list.NeedPage = false;
            list.DoSearch();            
            return File(list.GenerateExcel(), "application/x-excel", fileName + ".xls");
        }

        /// <summary>
        /// 通过office com组件返回excel 要求服务器必须装有office2003+版本
        /// </summary>
        /// <param name="table">数据源</param>
        /// <param name="fileName">要导出的文件名称 例:Text.xls</param>
        /// <returns></returns>
        protected FileResult ExcelResult(System.Data.DataTable table, string fileName, List<Utils.ExcelXML.XMLColumn> xmlCloumns = null, bool IsNoSplitStr=false)
        {
            string fName = FFUser.ITCode + "_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "_" + fileName;
            string path = System.Web.HttpContext.Current.Server.MapPath("~") + "DownLoads\\" + fName;
            //ExcelHelpers.GetExportExcel(table, path);
            ExcelHelpers.GetExportXML(table, path, xmlCloumns,IsNoSplitStr);
            return File(path, "application/x-excel", fileName);
        }

        protected FileResult ExcelResultByDataset(System.Data.DataSet dataset, string fileName)
        {
            string fName = FFUser.ITCode + "_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "_" + fileName;
            string path = System.Web.HttpContext.Current.Server.MapPath("~") + "DownLoads\\" + fName;
            //ExcelHelpers.GetExportExcel(table, path);
            ExcelHelpers.GetExportXMLByDataset(dataset, path);
            return File(path, "application/x-excel", fileName);
        }

        protected FileResult PdfResult(string fileName, IPagedList list)
        {
            list.NeedPage = false;
            list.DoSearch();
            return File(list.GeneratePDF(), "application/pdf", fileName + ".pdf");
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {

            SetCurrentLanguage(requestContext.HttpContext.Request, requestContext.HttpContext.Response);
            base.Initialize(requestContext);
        }

        protected override void ExecuteCore()
        {
            base.ExecuteCore();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {


            //// If session exists
            //if (filterContext.HttpContext.Session != null)
            //{
            //    //if new session
            //    if (filterContext.HttpContext.Session.IsNewSession)
            //    {
            //        string cookie = filterContext.HttpContext.Request.Headers["Cookie"];

            //        //if cookie exists and sessionid index is greater than zero
            //        if ((cookie != null) && (cookie.IndexOf("ASP.NET_SessionId") >= 0))
            //        {

            //            //redirect to desired session 
            //            //expiration action and controller
            //            //filterContext.Result = RedirectToAction("index", "Logon");
            //            Logon(filterContext);
            //            return;
            //        }
            //    }
            //}





            _ffSession.Session = this.Session;
            base.OnActionExecuting(filterContext);
            foreach (var item in filterContext.ActionParameters)
            {
                if (item.Value is BaseVM)
                {
                    var model = item.Value as BaseVM;
                    model.Session = this._ffSession;
                    model.FFUser = this.FFUser;
                    model.DC = this._dc;
                }
                if (item.Value is IPagedList)
                {
                    dynamic dy = item.Value;
                    Session[item.Value.GetType().FullName] = dy.Searcher;
                }
                if (item.Value is ISearcher)
                {
                    Session[item.Value.GetType().FullName] = item.Value;
                }
            }
        }


        private void Logon(ActionExecutingContext filterContext)
        {
            System.Web.Routing.RouteValueDictionary dictionary = new System.Web.Routing.RouteValueDictionary 
                (new
                {
                    controller = "Logon",
                    action = "Index",
                    returnUrl = filterContext.HttpContext.Request.RawUrl
                });
            filterContext.Result = new RedirectToRouteResult(dictionary);
        }


        protected override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.Result is PartialViewResult)
            {
                string url = "/Home/Index?pv=" + filterContext.RequestContext.HttpContext.Request.Url.ToString().Replace("&", "_and_");
                filterContext.HttpContext.Response.Write("<script>if(typeof mainwindow == 'undefined'){ window.location.href='" + url + "' }else{FF_Startup();}</script>");
            }
            base.OnResultExecuting(filterContext);
        }


        public bool RedoValidation(BaseVM VM)
        {
            if (ControllerContext == null)
            {
                ControllerContext = new ControllerContext();
            }
            VM.DC = this._dc;
            bool rv = TryValidateModel(VM);
            if (VM.GetType().GetProperty("Entity") != null)
            {
                dynamic dVM = VM;
                rv = TryValidateModel(dVM.Entity);
            }
            return rv;
        }

        private void SetCurrentLanguage(HttpRequestBase request, HttpResponseBase response)
        {
            var cookie = request.Cookies["FFLanguage"];
            CurrentLanguage = null;
            if (cookie != null)
            {
                CurrentLanguage = Languages.Where(x => x.LanguageCode.ToLower() == cookie.Value.ToLower()).FirstOrDefault();
            }
            else
            {
                var lans = request.UserLanguages;
                foreach (var item in lans)
                {
                    string longName = item;
                    if (item.Contains(';'))
                    {
                        longName = item.Split(';').First();
                    }
                    string shortName = longName;
                    if (longName.Contains('-'))
                    {
                        shortName = longName.Split('-').First();
                    }
                    CurrentLanguage = Languages.Where(x => x.LanguageCode.ToLower() == longName.ToLower() || x.LanguageCode.ToLower() == shortName.ToLower()).FirstOrDefault();
                    if (CurrentLanguage != null)
                    {
                        break;
                    }
                }
            }
            if (CurrentLanguage == null)
            {
                CurrentLanguage = Languages.Where(x => x.LanguageCode == "zh-cn").FirstOrDefault();
            }
            CultureInfo ci = new CultureInfo(CurrentLanguage.LanguageCode);
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            if (cookie == null)
            {
                HttpCookie newCookie = new HttpCookie("FFLanguage", CurrentLanguage.LanguageCode);
                newCookie.Expires = DateTime.Today.AddYears(10);
                response.Cookies.Add(newCookie);
            }
        }
    }
}
