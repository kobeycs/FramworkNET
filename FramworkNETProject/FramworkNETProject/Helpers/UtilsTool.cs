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
    public enum BoolComboTypes { �Ƿ�, ��Ч��Ч, ��Ů };

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
                return string.Format("javascript:FF_OpenInfoDialog('{0}','{1}','{2}',{3},{4});return false;", url, Resources.Language.ȷ��, title, width, height);
            }
            else
            {
                return string.Format("javascript:FF_OpenInfoDialog('{0}','{1}','{2}',{3},{4},'{5}');return false;", url, Resources.Language.ȷ��, title, width, height, dialogID);
            }
        }

        public static string GetSubmitWindowScript(string url, string title, int width, int height)
        {
            return string.Format("javascript:FF_OpenSubmitDialog('{0}','{1}','{2}','{3}',{4},{5});return false;", url, Resources.Language.�ύ, Resources.Language.ȡ��, title, width, height);
        }

        /// <summary>
        /// �������壬��ˢ��ָ��ҳ�棨add by dufei��
        /// </summary>
        /// <param name="url">Ҫ���ص�url</param>
        /// <param name="title">����ҳ��ı���</param>
        /// <param name="width">��������Ŀ��</param>
        /// <param name="height">��������ĸ߶�</param>
        /// <param name="reloadUrl">Ҫˢ�µ�url</param>
        /// <returns></returns>
        public static string GetReloadWindowScript(string url, string title, int width, int height, string reloadUrl)
        {
            return string.Format("javascript:FF_OpenAndReloadDialog('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}');return false;", url, Resources.Language.�ر�, title, width, height, reloadUrl, Resources.Language.���ݼ�����, Resources.Language.��ʾ);
        }

        /// <summary>
        /// ���ڶ���dialog�ر�ʱ��ˢ�µ�һ��dialog
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
            return string.Format("javascript:FF_DialogRelodWhenSubDialogClose('{0}','{1}','{2}','{3}','{4}','{5}','{6}');return false;", url, Resources.Language.�ر�, title, width, height, reloadUrl, divID);
        }

        public static string GetYesNoSubmitWindowScript(string url, string title, int width, int height)
        {
            return string.Format("javascript:FF_OpenSubmitDialog('{0}','{1}','{2}','{3}',{4},{5});return false;", url, Resources.Language.��, Resources.Language.��, title, width, height);
        }

        public static string GetLoadPageScript(string url, string msg = null)
        {
            if (string.IsNullOrEmpty(msg))
            {
                return string.Format("javascript:FF_LoadPage('{0}','{1}','{2}');return false;", url, Resources.Language.���ݼ�����, Resources.Language.��ʾ);
            }
            else
            {
                return string.Format("javascript:FF_LoadPage('{0}','{1}','{2}','{3}');return false;", url, Resources.Language.���ݼ�����, Resources.Language.��ʾ, msg);
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
        /// �����������ڼ��� ��ʱ���� ��ҳ�����
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
        /// ȥ�����ں����ʱ����
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
                select.Text = Resources.Language.��ѡ��;
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
                case BoolComboTypes.�Ƿ�:
                    yesText = Resources.Language.��;
                    noText = Resources.Language.��;
                    break;
                case BoolComboTypes.��Ч��Ч:
                    yesText = Resources.Language.��Ч;
                    noText = Resources.Language.��Ч;
                    break;
                case BoolComboTypes.��Ů:
                    yesText = Resources.Language.��;
                    noText = Resources.Language.Ů;
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

        #region ����ũ���õ���������

        private static ChineseLunisolarCalendar calendar = new ChineseLunisolarCalendar();

        /// <summary>
        /// ��ȡָ����ݴ��ڵ��գ����³�һ������������
        /// </summary>
        /// <param name="year">ָ�������</param>
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
        /// �õ�ũ�����ڵĹ�������
        /// </summary>
        /// <param name="year">ũ������</param>
        /// <param name="month">ũ������</param>
        /// <param name="day">ũ������</param>
        /// <param name="IsLeapMonth">�Ƿ�������</param>
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
        /// ��ȡ�ַ���
        /// </summary>
        /// <param name="value">��Ҫ��ȡ���ַ���</param>
        /// <param name="maxLength">��󳤶�</param>
        /// <param name="endFlag">��Ҫ��ӵĺ�׺</param>
        /// <returns></returns>
        public static string CutString(string value, int maxLength, string endFlag)
        {
            //������ȱ���Ҫ�ĳ���nС,����ԭ�ַ���
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
                    int byteCount = Encoding.Default.GetByteCount(mChar);//�ؼ����ж��ַ���ռ���ֽ���

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
        /// Author:������
        /// Function:�õ�"2011-11-04"��ʽ������
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
        * �������ַ�����Ϊ����ΪXML����ֵ/���ݵ��ַ�
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
                TopText = Resources.Language.��ѡ��;
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
                TopText = Resources.Language.��ѡ��;
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
            //6-7 renyj�޸�
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
    //��־ǿ 1367 7/28
    /// <summary>
    /// ͼ�갴ť
    /// </summary>
    public static class Icon
    {
        /// <summary>
        /// ���ͼ��
        /// </summary>
        public static string Add { get { return "icon_add"; } }
        /// <summary>
        /// ����ͼ��
        /// </summary>
        public static string Save { get { return "icon_save"; } }
        /// <summary>
        /// ����ͼ��
        /// </summary>
        public static string Return { get { return "icon_return"; } }
        /// <summary>
        /// Exccelͼ��
        /// </summary>
        public static string Excel { get { return "icon_excel"; } }
        /// <summary>
        /// ��ѯͼ��
        /// </summary>
        public static string Search { get { return "icon_search"; } }
        /// <summary>
        /// �б�ͼ��
        /// </summary>
        public static string List { get { return "icon_list"; } }
    }


}
