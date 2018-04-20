using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Utils
{
    public class StringHelper
    {
        public static string LDAPSpecialCharacterReplace(string accountName)
        {//.Replace(@",", "_")
            return accountName.Replace(@"/", "_").Replace(@"\", "_").Replace(@"[", "_").Replace(@"]", "_")
                .Replace(@":", "_").Replace(@";", "_").Replace(@"|", "_").Replace(@"=", "_")
                .Replace(@"+", "_").Replace(@"*", "_").Replace(@"?", "_").Replace(@"<", "_").Replace(@">", "_");
        }

        /// <summary>
        /// 过滤特殊字符串为空，防止对AD 2016.01.14
        /// </summary>
        /// <param name="accountName"></param>
        /// <returns></returns>
        public static string LDAPSpecialCharacterReplaceNew(string accountName)
        {
            return accountName.Replace(@"(", "").Replace(@")", "").Replace(@">", "").Replace(@"<", "")
                .Replace(@":", "").Replace(@";", "").Replace(@"|", "").Replace(@"=", "")
                .Replace(@"+", "").Replace(@"*", "").Replace(@"?", "");
        }

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public StringHelper()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #endregion

        #region 生成随机的数字
        /// <summary>
        /// 生成随机的数字
        /// </summary>
        /// <param name="intLength">生成字母的个数</param>
        /// <returns>string</returns>
        public static string GenerateRndNum(int intLength)
        {
            string Vchar = "0,1,2,3,4,5,6,7,8,9";
            string[] VcArray = Vchar.Split(',');
            string VNum = ""; //由于字符串很短，就不用StringBuilder了

            int temp = -1; //记录上次随机数值，尽量避免生产几个一样的随机数


            //采用一个简单的算法以保证生成随机数的不同
            Random rand = new Random();
            for (int i = 1; i < intLength + 1; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(i * temp * unchecked((int)DateTime.Now.Ticks));
                }
                int t = rand.Next(VcArray.Length);
                if (temp != -1 && temp == t)
                {
                    return GenerateRndNum(intLength);
                }
                temp = t;
                VNum += VcArray[t];
            }
            return VNum;
        }
        #endregion

        #region 验证字符串是否为空
        /// <summary>
        /// 验证字符串是否为空
        /// </summary>
        /// <param name="str">被判断的字符串</param>
        /// <returns>bool值</returns>
        public static bool IsEmptyString(string str)
        {
            if (str == null || str.Length == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 获取字符串的实际字节长度的方法
        /// <summary>
        /// 获取字符串的实际字节长度的方法
        /// </summary>
        /// <param name="source">字符串</param>
        /// <returns>实际长度</returns>
        public static int GetRealLength(string source)
        {
            return Encoding.Default.GetByteCount(source);
        }
        #endregion

        #region 按字节数截取字符串的方法
        /// <summary>
        /// 按字节数截取字符串的方法
        /// </summary>
        /// <param name="source">要截取的字符串</param>
        /// <param name="n">要截取的字节数</param>
        /// <param name="needEndDot">是否需要结尾的省略号</param>
        /// <returns>截取后的字符串</returns>
        public static string SubString(string source, int n, bool needEndDot)
        {
            string temp = string.Empty;
            if (GetRealLength(source) <= n)//如果长度比需要的长度n小,返回原字符串
            {
                return source;
            }
            else
            {
                int t = 0;
                char[] q = source.ToCharArray();
                for (int i = 0; i < q.Length && t < n; i++)
                {
                    if ((int)q[i] > 127)//是否汉字
                    {
                        temp += q[i];
                        t += 2;
                    }
                    else
                    {
                        temp += q[i];
                        t++;
                    }
                }
                if (needEndDot)
                    temp += "...";
                return temp;
            }
        }
        #endregion

        #region 过滤脚本攻击:sql注入,跨站脚本,flash嵌入

        /// <summary>
        /// 过滤字符串中注入SQL脚本的方法
        /// </summary>
        /// <param name="source">传入的字符串</param>
        /// <returns>过滤后的字符串</returns>
        public static string SqlFilter(string source)
        {
            //半角括号替换为全角括号
            source = source.Replace("'", "''").Replace(";", "；").Replace("(", "（").Replace(")", "）");

            //去除执行SQL语句的命令关键字
            source = Regex.Replace(source, "select", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "insert", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "update", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "delete", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "drop", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "truncate", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "declare", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "xp_cmdshell", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "/add", "", RegexOptions.IgnoreCase);
            //Regex.Replace(source, "asc(", "", RegexOptions.IgnoreCase);
            //Regex.Replace(source, "mid(", "", RegexOptions.IgnoreCase);
            //Regex.Replace(source, "char(", "", RegexOptions.IgnoreCase);
            //Regex.Replace(source, "count(", "", RegexOptions.IgnoreCase);
            //fetch 
            //IS_SRVROLEMEMBER
            //Cast(

            source = Regex.Replace(source, "net user", "", RegexOptions.IgnoreCase);

            //去除执行存储过程的命令关键字 
            source = Regex.Replace(source, "exec", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "execute", "", RegexOptions.IgnoreCase);

            //去除系统存储过程或扩展存储过程关键字
            source = Regex.Replace(source, "xp_", "x p_", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, "sp_", "s p_", RegexOptions.IgnoreCase);

            //防止16进制注入
            source = Regex.Replace(source, "0x", "0 x", RegexOptions.IgnoreCase);

            return source;
        }

        /// <summary>
        /// 过滤字符串中的注入跨站脚本(先进行UrlDecode再过滤脚本关键字),段少飞
        /// 过滤脚本如:<script>window.alert("test");</script>输出window.alert("test");
        /// 如<a href = "javascript:onclick='fun1();'">输出<a href=" onXXX='fun1();'">
        /// 过滤掉javascript和 onXXX
        /// </summary>
        /// <param name="source">需要过滤的字符串</param>
        /// <returns>过滤后的字符串</returns>
        public static string XSSFilter(string source)
        {
            if (source == "") return source;

            string result = HttpUtility.UrlDecode(source);

            string replaceEventStr = " onXXX =";
            string tmpStr = "";

            string patternGeneral = @"<[^<>]*>";                              //例如 <abcd>
            string patternEvent = @"([\s]|[:])+[o]{1}[n]{1}\w*\s*={1}";     // 空白字符或: on事件=
            string patternScriptBegin = @"\s*((javascript)|(vbscript))\s*[:]?";  // javascript或vbscript:
            string patternScriptEnd = @"<([\s/])*script.*>";                       // </script>   
            string patternTag = @"<([\s/])*\S.+>";                       // 例如</textarea>

            Regex regexGeneral = new Regex(patternGeneral, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex regexEvent = new Regex(patternEvent, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex regexScriptEnd = new Regex(patternScriptEnd, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex regexScriptBegin = new Regex(patternScriptBegin, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex regexTag = new Regex(patternTag, RegexOptions.Compiled | RegexOptions.IgnoreCase);


            Match matchGeneral, matchEvent, matchScriptEnd, matchScriptBegin, matchTag;

            //符合类似 <abcd> 条件的
            #region 符合类似 <abcd> 条件的
            //过滤字符串中的 </script>   
            for (matchGeneral = regexGeneral.Match(result); matchGeneral.Success; matchGeneral = matchGeneral.NextMatch())
            {
                tmpStr = matchGeneral.Groups[0].Value;
                matchScriptEnd = regexScriptEnd.Match(tmpStr);
                if (matchScriptEnd.Success)
                {
                    tmpStr = regexScriptEnd.Replace(tmpStr, " ");
                    result = result.Replace(matchGeneral.Groups[0].Value, tmpStr);
                }
            }

            //过滤字符串中的脚本事件
            for (matchGeneral = regexGeneral.Match(result); matchGeneral.Success; matchGeneral = matchGeneral.NextMatch())
            {
                tmpStr = matchGeneral.Groups[0].Value;
                matchEvent = regexEvent.Match(tmpStr);
                if (matchEvent.Success)
                {
                    tmpStr = regexEvent.Replace(tmpStr, replaceEventStr);
                    result = result.Replace(matchGeneral.Groups[0].Value, tmpStr);
                }
            }

            //过滤字符串中的 javascript或vbscript:
            for (matchGeneral = regexGeneral.Match(result); matchGeneral.Success; matchGeneral = matchGeneral.NextMatch())
            {
                tmpStr = matchGeneral.Groups[0].Value;
                matchScriptBegin = regexScriptBegin.Match(tmpStr);
                if (matchScriptBegin.Success)
                {
                    tmpStr = regexScriptBegin.Replace(tmpStr, " ");
                    result = result.Replace(matchGeneral.Groups[0].Value, tmpStr);
                }
            }

            #endregion

            //过滤字符串中的事件 例如 onclick --> onXXX
            for (matchEvent = regexEvent.Match(result); matchEvent.Success; matchEvent = matchEvent.NextMatch())
            {
                tmpStr = matchEvent.Groups[0].Value;
                tmpStr = regexEvent.Replace(tmpStr, replaceEventStr);
                result = result.Replace(matchEvent.Groups[0].Value, tmpStr);
            }

            //过滤掉html标签，类似</textarea>
            for (matchTag = regexTag.Match(result); matchTag.Success; matchTag = matchTag.NextMatch())
            {
                tmpStr = matchTag.Groups[0].Value;
                tmpStr = regexTag.Replace(tmpStr, "");
                result = result.Replace(matchTag.Groups[0].Value, tmpStr);
            }


            return result;
        }

        /// <summary>
        /// 过滤字符串中注入Flash代码
        /// </summary>
        /// <param name="htmlCode">输入字符串</param>
        /// <returns>过滤后的字符串</returns>
        public static string FlashFilter(string htmlCode)
        {
            string pattern = @"\w*<OBJECT\s+.*(macromedia)[\s*|\S*]{1,1300}</OBJECT>";

            return Regex.Replace(htmlCode, pattern, "", RegexOptions.Multiline);
        }

        #endregion

        #region 移除Html标签的方法
        /// <summary>
        /// 移除html标记
        /// </summary>
        /// <param name="source">移除Html标签之前的字符串</param>
        /// <returns>移除Html标签之后的字符串</returns>
        public static string RemoveHtmlTag(string source)
        {
            return Regex.Replace(source, "<[^>]*>", "");
        }
        #endregion

        #region 读取指定URL的内容
        /// <summary>
        /// 读取指定URL的内容
        /// </summary>
        /// <param name="URL">指定URL</param>
        /// <param name="Content">该URL包含的内容</param>
        /// <returns>读取URL的状态</returns>
        public static string ReadHttp(string URL, ref string Content)
        {
            string status = "ERROR";
            HttpWebRequest Webreq = (HttpWebRequest)WebRequest.Create(URL);
            HttpWebResponse Webresp = null;
            StreamReader strm = null;
            try
            {
                Webresp = (HttpWebResponse)Webreq.GetResponse();
                status = Webresp.StatusCode.ToString();
                strm = new StreamReader(Webresp.GetResponseStream(), Encoding.GetEncoding("GB2312"));
                Content = strm.ReadToEnd();
            }
            catch
            {
            }
            finally
            {
                if (Webresp != null) Webresp.Close();
                if (strm != null) strm.Close();
            }
            return (status);
        }
        #endregion

        #region 去除价格小数点后末尾“0”的方法
        /// <summary>
        /// 去除价格小数点后末尾“0”的方法
        /// </summary>
        /// <param name="price">去“0”之前的价格字符串</param>
        /// <returns>去“0”之后的价格字符串</returns>
        public static string TrimEndZeroForPrice(string price)
        {
            while (price.EndsWith("0") && price.IndexOf(".") > 0)
            {
                price = price.TrimEnd('0');
            }
            if (price.EndsWith("."))
                price = price.Substring(0, price.Length - 1);
            return price;
        }
        #endregion

        #region 把浮点类型格式化为百分比字符串
        /// <summary>
        /// 把浮点类型格式化为百分比字符串
        /// </summary>
        /// <param name="num">浮点类型数据</param>
        /// <returns>百分比字符串</returns>
        public static string FormatDoubleToPercent(double num)
        {
            return (num * 100).ToString("#0.00") + "%";
        }
        #endregion


        /// <summary>
        /// 将单引号和双引号替换成对应的HTML码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string AcuteQuotReplaceHtml(string str)
        {
            return str.Replace("'", "&acute;").Replace("\"", "&quot;");
        }

        /// <summary>
        /// 将HTML码替换回单引号和双引号
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string HtmlReplaceAcuteQuot(string str)
        {
            return str.Replace("&acute;", "'").Replace("&quot;", "\"");
        }
    }
}

