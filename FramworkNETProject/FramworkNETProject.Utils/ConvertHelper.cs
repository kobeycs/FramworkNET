using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class ConvertHelper
    {
        #region 将对象变量转成字符串变量的方法
        /// <summary>
        /// 将对象变量转成字符串变量的方法
        /// </summary>
        /// <param name="obj">对象变量</param>
        /// <returns>字符串变量</returns>
        public static string GetString(object obj)
        {
            return (obj == DBNull.Value || obj == null) ? "" : obj.ToString();
        }
        #endregion

        #region 将对象变量转成32位整数型变量的方法
        /// <summary>
        /// 将对象变量转成32位整数型变量的方法
        /// </summary>
        /// <param name="obj">对象变量</param>
        /// <returns>32位整数型变量</returns>
        public static int GetInteger(object obj)
        {
            return ConvertStringToInteger(GetString(obj));
        }
        #endregion

        #region 将对象变量转成64位整数型变量的方法
        /// <summary>
        /// 将对象变量转成64位整数型变量的方法
        /// </summary>
        /// <param name="obj">对象变量</param>
        /// <returns>64位整数型变量</returns>
        public static long GetLong(object obj)
        {
            return ConvertStringToLong(GetString(obj));
        }
        #endregion

        #region 将对象变量转成双精度浮点型变量的方法
        /// <summary>
        /// 将对象变量转成双精度浮点型变量的方法
        /// </summary>
        /// <param name="obj">对象变量</param>
        /// <returns>双精度浮点型变量</returns>
        public static double GetDouble(object obj)
        {
            return ConvertStringToDouble(GetString(obj));
        }
        #endregion

        #region 将对象变量转成十进制数字变量的方法
        /// <summary>
        /// 将对象变量转成十进制数字变量的方法
        /// </summary>
        /// <param name="obj">对象变量</param>
        /// <returns>十进制数字变量</returns>
        public static decimal GetDecimal(object obj)
        {
            return ConvertStringToDecimal(GetString(obj));
        }

        /// <summary>
        /// 根据小数位数取舍
        /// </summary>
        /// <param name="o">需要取舍的数</param>
        /// <param name="digits">小数位数</param>
        /// <returns></returns>
        public static string GetNumericalStringBydigit(object o, int digits)
        {
            if (o == DBNull.Value)
            {
                return string.Empty;
            }
            double i = 0;
            if (double.TryParse(o.ToString(), out i))
            {
                if (digits != 0)
                {
                    return Math.Round(i, digits).ToString();
                }
                return Math.Round(i).ToString();
            }
            else
            {
                return string.Empty;
            }
        }
        #endregion

        #region 将对象变量转成布尔型变量的方法
        /// <summary>
        /// 将对象变量转成布尔型变量的方法
        /// </summary>
        /// <param name="obj">对象变量</param>
        /// <returns>布尔型变量</returns>
        public static bool GetBoolean(object obj)
        {
            return (obj == DBNull.Value || obj == null) ? false :
                GetString(obj).Length == 0 ? false : Convert.ToBoolean(obj);
        }
        #endregion

        #region 将对象变量转成日期时间型字符串变量的方法
        /// <summary>
        /// 将对象变量转成日期时间型字符串变量的方法
        /// </summary>
        /// <param name="obj">对象变量</param>
        /// <param name="sFormat">格式字符</param>
        /// <returns>时间型字符串变量</returns>
        public static string GetDateTimeString(object obj, string sFormat)
        {
            return (obj == DBNull.Value || obj == null) ? "" : Convert.ToDateTime(obj).ToString(sFormat);
        }

        /// <summary>
        /// 将对象变量转成日期字符串变量（短）的方法
        /// </summary>
        /// <param name="obj">对象变量</param>
        /// <returns>日期字符串变量</returns>
        public static string GetShortDateString(object obj)
        {
            return GetDateTimeString(obj, "yyyy-MM-dd");
        }

        /// <summary>
        /// 将对象变量转成日期字符串变量（长）的方法
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetLongDateString(object obj)
        {
            return GetDateTimeString(obj, "yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 将对象变量转成日期型变量的方法
        /// </summary>
        /// <param name="obj">对象变量</param>
        /// <returns>日期型变量</returns>
        public static DateTime GetDateTime(object obj)
        {
            return ConvertStringToDateTime(GetString(obj));
        }

        /// <summary>
        /// 时间格式转换成yyyy-MM-dd 或者 yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <param name="o">时间对象</param>
        /// <param name="shortime">是否转成yyyy-MM-dd</param>
        /// <returns>时间格式字符串</returns>
        public static string DateTimeFormat(object o, bool shortime)
        {
            if (o == DBNull.Value)
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(o.ToString()))
            {
                return string.Empty;
            }
            string convertDateString = string.Empty;

            if (shortime)
            {
                convertDateString = Convert.ToDateTime(o).ToString("yyyy-MM-dd");
            }
            else
            {
                convertDateString = Convert.ToDateTime(o).ToString("yyyy-MM-dd HH:mm:ss");
            }

            return convertDateString.IndexOf("1900") < 0 ? convertDateString : string.Empty;
        }
        #endregion

        #region 私有方法

        #region 将字符串转成32位整数型变量的方法
        /// <summary>
        /// 将字符串转成32位整数型变量的方法
        /// </summary>
        /// <param name="s">字符串</param>
        /// <returns>32位整数型变量</returns>
        private static int ConvertStringToInteger(string s)
        {
            int result = 0;
            int.TryParse(s, out result);
            return result;
        }
        #endregion

        #region 将字符串转成64位整数型变量的方法
        /// <summary>
        /// 将字符串转成64位整数型变量的方法
        /// </summary>
        /// <param name="s">字符串</param>
        /// <returns>64位整数型变量</returns>
        private static long ConvertStringToLong(string s)
        {
            long result = 0;
            long.TryParse(s, out result);
            return result;
        }
        #endregion

        #region 将字符串转成双精度浮点型变量的方法
        /// <summary>
        /// 将字符串转成双精度浮点型变量的方法
        /// </summary>
        /// <param name="s">字符串</param>
        /// <returns>双精度浮点型变量</returns>
        private static double ConvertStringToDouble(string s)
        {
            double result = 0;
            double.TryParse(s, out result);
            return result;
        }
        #endregion

        #region 将字符串转成十进制数字变量的方法
        /// <summary>
        /// 将字符串转成十进制数字变量的方法
        /// </summary>
        /// <param name="s">字符串</param>
        /// <returns>十进制数字变量</returns>
        private static decimal ConvertStringToDecimal(string s)
        {
            decimal result = 0;
            decimal.TryParse(s, out result);
            return result;
        }
        #endregion

        #region 将字符串转成时间变量的方法
        /// <summary>
        /// 将字符串转成时间变量的方法
        /// </summary>
        /// <param name="s">字符串</param>
        /// <returns>时间变量</returns>
        private static DateTime ConvertStringToDateTime(string s)
        {
            DateTime result;
            DateTime.TryParse(s, out result);
            return result;
        }
        #endregion

        #endregion
    }
}
