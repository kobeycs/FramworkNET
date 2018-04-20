using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Reflection;
using System.Globalization;
using System.Text;
using System.Data;
using SupportClasses;
using Models;
using ViewModels;
using Controllers;

namespace Helpers
{
    public enum BoolComboTypes { 是否, 有效无效, 男女 };

    public static class UtilsTool
    {
        public static string GetPropertyName(LambdaExpression expression, bool getAll = true)
        {
            MemberExpression me = null;
            LambdaExpression le = expression as LambdaExpression;
            if (le.Body is MemberExpression)
            {
                me = le.Body as MemberExpression;
            }
            if (le.Body is UnaryExpression)
            {
                me = (le.Body as UnaryExpression).Operand as MemberExpression;
            }
            string rv = "";
            if (me != null)
            {
                rv = me.Member.Name;
            }
            while (me != null && getAll && me.NodeType == ExpressionType.MemberAccess)
            {
                Expression exp = me.Expression;
                if (exp is MemberExpression)
                {
                    rv = (exp as MemberExpression).Member.Name + "." + rv;
                    me = exp as MemberExpression;
                }
                else
                {
                    break;
                }
            }
            return rv;
        }

        public static PropertyInfo GetPropertyInfo(LambdaExpression expression)
        {
            MemberExpression me = null;
            LambdaExpression le = expression as LambdaExpression;
            if (le.Body is MemberExpression)
            {
                me = le.Body as MemberExpression;
            }
            if (le.Body is UnaryExpression)
            {
                me = (le.Body as UnaryExpression).Operand as MemberExpression;
            }
            PropertyInfo rv = null;
            if (me != null)
            {
                rv = me.Member.DeclaringType.GetProperty(me.Member.Name);
            }
            return rv;
        }

        public static SupportedLanguage GetCurrentLanguage()
        {
            var cul = System.Threading.Thread.CurrentThread.CurrentUICulture;
            return BaseController.Languages.Where(x => x.LanguageCode.ToLower() == cul.Name.ToLower() || x.LanguageCode.ToLower() == cul.TwoLetterISOLanguageName.ToLower()).SingleOrDefault();
        }

        public static string GetInfoWindowScript(string url, string title, int width, int height, string dialogID = null)
        {
            if (dialogID == null)
            {
                return string.Format("javascript:FF_OpenInfoDialog('{0}','{1}','{2}',{3},{4});return false;", url, Resources.Language.确定, title, width, height);
            }
            else
            {
                return string.Format("javascript:FF_OpenInfoDialog('{0}','{1}','{2}',{3},{4},'{5}');return false;", url, Resources.Language.确定, title, width, height, dialogID);
            }
        }

        public static string GetSubmitWindowScript(string url, string title, int width, int height)
        {
            return string.Format("javascript:FF_OpenSubmitDialog('{0}','{1}','{2}','{3}',{4},{5});return false;", url, Resources.Language.提交, Resources.Language.取消, title, width, height);
        }

        /// <summary>
        /// 弹出窗体，并刷新指定页面（add by dufei）
        /// </summary>
        /// <param name="url">要加载的url</param>
        /// <param name="title">弹出页面的标题</param>
        /// <param name="width">弹出窗体的宽度</param>
        /// <param name="height">弹出窗体的高度</param>
        /// <param name="reloadUrl">要刷新的url</param>
        /// <returns></returns>
        public static string GetReloadWindowScript(string url, string title, int width, int height, string reloadUrl)
        {
            return string.Format("javascript:FF_OpenAndReloadDialog('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}');return false;", url, Resources.Language.关闭, title, width, height, reloadUrl, Resources.Language.数据加载中, Resources.Language.提示);
        }

        /// <summary>
        /// 当第二个dialog关闭时，刷新第一个dialog
        /// </summary>
        /// <param name="url"></param>
        /// <param name="title"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="reloadUrl"></param>
        /// <param name="divID"></param>
        /// <returns></returns>
        public static string GetReloadParentDialogWindowScript(string url, string title, int width, int height, string reloadUrl, string divID)
        {
            return string.Format("javascript:FF_DialogRelodWhenSubDialogClose('{0}','{1}','{2}','{3}','{4}','{5}','{6}');return false;", url, Resources.Language.关闭, title, width, height, reloadUrl, divID);
        }

        public static string GetYesNoSubmitWindowScript(string url, string title, int width, int height)
        {
            return string.Format("javascript:FF_OpenSubmitDialog('{0}','{1}','{2}','{3}',{4},{5});return false;", url, Resources.Language.是, Resources.Language.否, title, width, height);
        }

        public static string GetLoadPageScript(string url, string msg = null)
        {
            if (string.IsNullOrEmpty(msg))
            {
                return string.Format("javascript:FF_LoadPage('{0}','{1}','{2}');return false;", url, Resources.Language.数据加载中, Resources.Language.提示);
            }
            else
            {
                return string.Format("javascript:FF_LoadPage('{0}','{1}','{2}','{3}');return false;", url, Resources.Language.数据加载中, Resources.Language.提示, msg);
            }
        }

        public static void CheckDifference<T>(List<T> oldList, List<T> newList, out List<T> ToRemove, out List<T> ToAdd) where T : BasePoco
        {
            ToRemove = new List<T>();
            ToAdd = new List<T>();
            foreach (var oldItem in oldList)
            {
                bool exist = false;
                foreach (var newItem in newList)
                {
                    if (oldItem.ID == newItem.ID)
                    {
                        exist = true;
                        break;
                    }
                }
                if (exist == false)
                {
                    ToRemove.Add(oldItem);
                }
            }
            foreach (var newItem in newList)
            {
                bool exist = false;
                foreach (var oldItem in oldList)
                {
                    if (newItem.ID == oldItem.ID)
                    {
                        exist = true;
                        break;
                    }
                }
                if (exist == false)
                {
                    ToAdd.Add(newItem);
                }
            }
        }

        /// <summary>
        /// 根据生日日期计算 即时年龄 本页面调用
        /// </summary>
        /// <param name="strBirthday"></param>
        /// <returns></returns>
        public static int GetAge(string strBirthday)
        {
            if (strBirthday == "")
                return 0;

            DateTime Birthday = Convert.ToDateTime(strBirthday);
            int year;
            int month = Birthday.Month;
            int day = Birthday.Day;
            if (month > DateTime.Now.Month || (month == DateTime.Now.Month && day > DateTime.Now.Day))
                year = DateTime.Now.Year - Birthday.Year - 1;
            else
                year = DateTime.Now.Year - Birthday.Year;
            return year;
        }

        /// <summary>
        /// 去掉日期后面的时分秒
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns> 
        public static MvcHtmlString FormatDateTime(DateTime? dt)
        {
            if (dt != null)
            {
                return MvcHtmlString.Create(dt.Value.ToString("yyyy-MM-dd"));
            }
            else
            {
                return MvcHtmlString.Create("");
            }
        }

        public static org.in2bits.MyXls.Color GetExcelColor(System.Drawing.Color color)
        {
            if (color == System.Drawing.Color.Black)
            {
                return org.in2bits.MyXls.Colors.Black;
            }
            if (color == System.Drawing.Color.Blue)
            {
                return org.in2bits.MyXls.Colors.Blue;
            }
            if (color == System.Drawing.Color.DarkBlue)
            {
                return org.in2bits.MyXls.Colors.DarkBlue;
            }
            if (color == System.Drawing.Color.Gray)
            {
                return org.in2bits.MyXls.Colors.Grey;
            }
            if (color == System.Drawing.Color.Green)
            {
                return org.in2bits.MyXls.Colors.Green;
            }
            if (color == System.Drawing.Color.Red)
            {
                return org.in2bits.MyXls.Colors.Red;
            }
            if (color == System.Drawing.Color.Green)
            {
                return org.in2bits.MyXls.Colors.Green;
            }
            if (color == System.Drawing.Color.Cyan)
            {
                return org.in2bits.MyXls.Colors.Cyan;
            }
            if (color == System.Drawing.Color.DarkGreen)
            {
                return org.in2bits.MyXls.Colors.DarkGreen;
            }
            if (color == System.Drawing.Color.DarkRed)
            {
                return org.in2bits.MyXls.Colors.DarkRed;
            }
            if (color == System.Drawing.Color.Magenta)
            {
                return org.in2bits.MyXls.Colors.Magenta;
            }
            if (color == System.Drawing.Color.Olive)
            {
                return org.in2bits.MyXls.Colors.Olive;
            }
            if (color == System.Drawing.Color.Purple)
            {
                return org.in2bits.MyXls.Colors.Purple;
            }
            if (color == System.Drawing.Color.Silver)
            {
                return org.in2bits.MyXls.Colors.Silver;
            }
            if (color == System.Drawing.Color.White)
            {
                return org.in2bits.MyXls.Colors.White;
            }
            if (color == System.Drawing.Color.Yellow)
            {
                return org.in2bits.MyXls.Colors.Yellow;
            }
            if (color == System.Drawing.Color.Lime)
            {
                return org.in2bits.MyXls.Colors.Default32;
            }
            if (color == System.Drawing.Color.Orange)
            {
                return org.in2bits.MyXls.Colors.Default33;
            }
            if (color == System.Drawing.Color.DarkOrange)
            {
                return org.in2bits.MyXls.Colors.Default34;
            }

            return org.in2bits.MyXls.Colors.White;
        }

        public static List<SelectListItem> GetBoolCombo(BoolComboTypes boolType, bool hasSelect = true, bool? defaultValue = null)
        {
            List<SelectListItem> rv = new List<SelectListItem>();
            if (hasSelect == true)
            {
                SelectListItem select = new SelectListItem();
                select.Text = Resources.Language.请选择;
                select.Value = "";
                if (defaultValue == null)
                {
                    select.Selected = true;
                }
                rv.Add(select);
            }
            string yesText = "";
            string noText = "";
            switch (boolType)
            {
                case BoolComboTypes.是否:
                    yesText = Resources.Language.是;
                    noText = Resources.Language.否;
                    break;
                case BoolComboTypes.有效无效:
                    yesText = Resources.Language.有效;
                    noText = Resources.Language.无效;
                    break;
                case BoolComboTypes.男女:
                    yesText = Resources.Language.男;
                    noText = Resources.Language.女;
                    break;
                default:
                    break;
            }
            SelectListItem yesItem = new SelectListItem();
            yesItem.Text = yesText;
            yesItem.Value = "true";
            if (defaultValue == true)
            {
                yesItem.Selected = true;
            }
            SelectListItem noItem = new SelectListItem();
            noItem.Text = noText;
            noItem.Value = "false";
            if (defaultValue == false)
            {
                noItem.Selected = true;
            }
            rv.Add(yesItem);
            rv.Add(noItem);
            return rv;
        }

        #region 根据农历得到公历日期

        private static ChineseLunisolarCalendar calendar = new ChineseLunisolarCalendar();

        /// <summary>
        /// 获取指定年份春节当日（正月初一）的阳历日期
        /// </summary>
        /// <param name="year">指定的年份</param>
        public static DateTime GetLunarNewYearDate(int year)
        {
            DateTime dt = new DateTime(year, 1, 1);
            int cnYear = calendar.GetYear(dt);
            int cnMonth = calendar.GetMonth(dt);

            int num1 = 0;
            int num2 = calendar.IsLeapYear(cnYear) ? 13 : 12;

            while (num2 >= cnMonth)
            {
                num1 += calendar.GetDaysInMonth(cnYear, num2--);
            }

            num1 = num1 - calendar.GetDayOfMonth(dt) + 1;
            return dt.AddDays(num1);
        }

        /// <summary>
        /// 得到农历日期的公历日期
        /// </summary>
        /// <param name="year">农历的年</param>
        /// <param name="month">农历的月</param>
        /// <param name="day">农历的日</param>
        /// <param name="IsLeapMonth">是否是闰月</param>
        /// <returns></returns>
        public static DateTime GetDateFromLunarDate(int year, int month, int day, bool IsLeapMonth = false)
        {
            int num1 = 0, num2 = 0;
            int leapMonth = calendar.GetLeapMonth(year);

            if (((leapMonth == month + 1) && IsLeapMonth) || (leapMonth > 0 && leapMonth <= month))
                num2 = month;
            else
                num2 = month - 1;

            while (num2 > 0)
            {
                num1 += calendar.GetDaysInMonth(year, num2--);
            }

            DateTime dt = GetLunarNewYearDate(year);
            return dt.AddDays(num1 + day - 1);
        }

        #endregion

        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="value">需要截取的字符串</param>
        /// <param name="maxLength">最大长度</param>
        /// <param name="endFlag">需要添加的后缀</param>
        /// <returns></returns>
        public static string CutString(string value, int maxLength, string endFlag)
        {
            //如果长度比需要的长度n小,返回原字符串
            if (maxLength <= 0 || Encoding.Default.GetByteCount(value) <= maxLength)
            {
                return value;
            }
            else
            {
                int totalCount = 0;
                StringBuilder sbSubStr = new StringBuilder();
                string mChar;

                for (int i = 0; i < value.Length; i++)
                {
                    mChar = value.Substring(i, 1);
                    int byteCount = Encoding.Default.GetByteCount(mChar);//关键点判断字符所占的字节数

                    if (totalCount + byteCount > maxLength)
                    {
                        break;
                    }
                    else
                    {
                        totalCount += byteCount;
                        sbSubStr.Append(mChar);
                    }
                }

                return (sbSubStr.ToString() + endFlag);
            }
        }

        /// <summary>
        /// Author:董俊甫
        /// Function:得到"2011-11-04"格式的日期
        /// </summary>
        /// <param name="d">DateTime</param>
        /// <returns>string</returns>
        public static string GetCustomizeDateString(DateTime? d)
        {
            if (d == null)
            {
                return "";
            }
            else
            {
                StringBuilder sb = new StringBuilder(d.Value.Year.ToString() + "-");

                if (d.Value.Month < 10)
                {
                    sb.Append("0" + d.Value.Month.ToString() + "-");
                }
                else
                {
                    sb.Append(d.Value.Month.ToString() + "-");
                }
                if (d.Value.Day < 10)
                {
                    sb.Append("0" + d.Value.Day.ToString());
                }
                else
                {
                    sb.Append(d.Value.Day.ToString());
                }
                return sb.ToString();
            }

        }

        /*
        * 将输入字符过滤为可作为XML属性值/内容的字符
        * by hongjw 2012/3/16
        */
        public static StringBuilder ToXMLContent(this StringBuilder src)
        {
            return src.Replace("<", "&lt;").Replace(">", "&gt;").Replace("&", "&amp;").Replace("'", "&apos;").Replace("\"", "&quot;");
        }

    }


    public static class SearcherHtmlTagHelper
    {
        public static string GetHtmlID<T>(this T self, Expression<Func<T, object>> Exp) where T : BaseVM
        {
            return UtilsTool.GetPropertyName(Exp);
        }
    }

    public static class ListToSelectsHelper
    {
        public static List<SelectListItem> ToListItems<T, V>(this List<T> self, Expression<Func<T, List<V>>> middleField, Expression<Func<V, string>> valueField, string TopText = "", Expression<Func<T, bool>> selectedCondition = null)
            where T : IMLData<V>
            where V : MLContent
        {
            if (TopText == "")
            {
                TopText = Resources.Language.请选择;
            }
            var rv = new List<SelectListItem>();
            if (!string.IsNullOrEmpty(TopText))
            {
                SelectListItem first = new SelectListItem();
                first.Text = TopText;
                first.Value = "";
                rv.Add(first);
            }
            if (self != null)
            {
                foreach (var item in self)
                {
                    item.GetLanguageSpecificContent(valueField);
                    string text = item.GetLanguageSpecificContent(valueField);
                    string value = (item as BasePoco).ID.ToString();
                    SelectListItem li = new SelectListItem();
                    li.Text = text;
                    li.Value = value;
                    if (selectedCondition != null)
                    {
                        if (selectedCondition.Compile().Invoke(item))
                        {
                            li.Selected = true;
                        }
                    }
                    rv.Add(li);
                }
            }
            if (rv.Where(x => x.Selected == true).FirstOrDefault() == null)
            {
                if (rv.Count > 0)
                {
                    rv[0].Selected = true;
                }
            }
            return rv;
        }


        public static List<SelectListItem> ToListItems<T>(this List<T> self, Expression<Func<T, object>> textField, Expression<Func<T, object>> valueField, string TopText = "", Expression<Func<T, bool>> selectedCondition = null) where T : class
        {
            var rv = new List<SelectListItem>();
            if (TopText == "")
            {
                TopText = Resources.Language.请选择;
            }
            if (!string.IsNullOrEmpty(TopText))
            {
                SelectListItem first = new SelectListItem();
                first.Text = TopText;
                first.Value = "";
                rv.Add(first);
            }
            if (self != null)
            {
                foreach (var item in self)
                {
                    string text = textField.Compile().Invoke(item).ToString();
                    string value = valueField.Compile().Invoke(item).ToString();
                    SelectListItem li = new SelectListItem();
                    li.Text = text;
                    li.Value = value;
                    if (selectedCondition != null)
                    {
                        if (selectedCondition.Compile().Invoke(item))
                        {
                            li.Selected = true;
                        }
                    }
                    rv.Add(li);
                }
            }
            if (rv.Where(x => x.Selected == true).FirstOrDefault() == null)
            {
                if (rv.Count > 0)
                {
                    rv[0].Selected = true;
                }
            }
            return rv;
        }


    }

    public static class MLDataHelper
    {
        public static string GetLanguageSpecificContent<ML>(this IMLData<ML> self, Expression<Func<ML, string>> LanguageFormat) where ML : MLContent
        {
            //6-7 renyj修改
            if (self == null)
            {
                return "";
            }
            if (self.MLContents == null)
            {
                return "";
            }
            string rv = self.MLContents.Where(x => x.LanguageCode == UtilsTool.GetCurrentLanguage().LanguageCode).Select(LanguageFormat.Compile()).FirstOrDefault();
            return rv;
        }
    }
    //郝志强 1367 7/28
    /// <summary>
    /// 图标按钮
    /// </summary>
    public static class Icon
    {
        /// <summary>
        /// 添加图标
        /// </summary>
        public static string Add { get { return "icon_add"; } }
        /// <summary>
        /// 保存图标
        /// </summary>
        public static string Save { get { return "icon_save"; } }
        /// <summary>
        /// 返回图标
        /// </summary>
        public static string Return { get { return "icon_return"; } }
        /// <summary>
        /// Exccel图标
        /// </summary>
        public static string Excel { get { return "icon_excel"; } }
        /// <summary>
        /// 查询图标
        /// </summary>
        public static string Search { get { return "icon_search"; } }
        /// <summary>
        /// 列表图标
        /// </summary>
        public static string List { get { return "icon_list"; } }
    }


}
