using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Utils
{

    //public enum DQZT { 启用, 禁用 }
    //public enum SYQX { 是, 否 }
    //public enum XSZT { 隐藏, 系统可见, 用户可见 }
    //public enum SFKY { 禁用, 可用, 隐藏 }
    ///// <summary>
    ///// 零级别最高，依次往低 0-1-2-3-4-5
    ///// </summary>
    //public enum JSJB { 零, 一, 二, 三, 四, 五 }

    public class LanguageHelper
    {
        public static string[] englishLang = new string[] { 
            "en-ZW", "en-IE", "en-AU", "en-BZ", "en-PH", 
            "en-029", "en-CA", "en-MY", "en-US", "en-ZA", "en-TT", "en-SG", 
            "en-NZ", "en-JM", "en-IN", "en-GB", "en" };

        public static bool IsChinese
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    string[] userLans = HttpContext.Current.Request.UserLanguages;
                    if (userLans.Length > 0)
                    {
                        string firstLan = userLans[0];
                        if (firstLan.IndexOf("zh", StringComparison.CurrentCultureIgnoreCase) > -1)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }



    



    public class BaseHelper
    {
        /// <summary>
        /// 文件Stream对象转换成byte
        /// </summary>
        /// <param name="contentLength"></param>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public static byte[] ConvertToByte(int contentLength, System.IO.Stream inputStream)
        {
            byte[] buffer = new byte[contentLength];
            System.IO.Stream fs;
            fs = (System.IO.Stream)inputStream;
            fs.Read(buffer, 0, contentLength);
            fs.Close();
            return buffer;
        }
    }

}
