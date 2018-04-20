using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.International.Converters.PinYinConverter;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace Utils
{
    public class Common
    {


        /// <summary>
        /// 把一个整数分解为二进制输出
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static List<int> BinaryOperate(int number)
        {
            var result = new List<int>();
            List<char> binary = Convert.ToString(number, 2).ToList();

            while (binary.Count > 0)
            {
                if (binary[0] == '1')
                {
                    result.Add(1 << binary.Count - 1);
                }
                binary.RemoveAt(0);
            }
            return result;
        }




        /// <summary>
        /// 获取当前时间和密码过期时间的差值 返回相差多少天
        /// </summary>
        /// <param name="accountExpires">过期时间</param>
        /// <returns></returns>
        public static int GetJiangeTimePwd(DateTime accountExpires)
        {
            return Convert.ToInt32((accountExpires.Date - DateTime.Now.Date).TotalDays);
        }


        /// <summary>
        /// 获取当前时间和过期时间的差值 返回相差多少天
        /// </summary>
        /// <param name="accountExpires">过期时间</param>
        /// <returns></returns>
        public static int GetJiangeTimescan(DateTime accountExpires)
        {
            return Convert.ToInt32((accountExpires.AddDays(-1).Date - DateTime.Now.Date).TotalDays);
        }

        /// <summary>
        /// 输入汉字返回首字母大写
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string GetPinyinFirst(string txt)
        {
            char[] ch = txt.ToArray();
            StringBuilder pinyinStr = new StringBuilder();
            foreach (char c in ch)
            {
                if (ChineseChar.IsValidChar(c))
                {
                    ChineseChar chineseChar = new ChineseChar(c);
                    ReadOnlyCollection<string> pinyin = chineseChar.Pinyins;
                    pinyinStr.Append(pinyin[0].Substring(0, 1));
                    //pinyinStr.Append(pinyin[0].Substring(1, pinyin[0].Length - 2).ToLower());
                }
                else
                {
                    pinyinStr.Append(c.ToString().ToUpper());
                }
            }
            return pinyinStr.ToString();
        }

        /// <summary>
        /// 输入汉字返回拼音 首字母大写
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string GetPinyin(string txt)
        {
            char[] ch = txt.ToArray();
            StringBuilder pinyinStr = new StringBuilder();
            foreach (char c in ch)
            {
                if (ChineseChar.IsValidChar(c))
                {
                    ChineseChar chineseChar = new ChineseChar(c);
                    ReadOnlyCollection<string> pinyin = chineseChar.Pinyins;
                    pinyinStr.Append(pinyin[0].Substring(0, 1));
                    pinyinStr.Append(pinyin[0].Substring(1, pinyin[0].Length - 2).ToLower());
                }
                else
                {
                    pinyinStr.Append(c.ToString());
                }
            }
            return pinyinStr.ToString();
        }



        /// <summary>
        /// 输入汉字返回拼音 首字母大写
        /// 输出是否包含多音字
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string GetPinyin(string txt, out bool polyphone)
        {
            polyphone = false;
            char[] ch = txt.ToArray();
            StringBuilder pinyinStr = new StringBuilder();
            foreach (char c in ch)
            {
                if (ChineseChar.IsValidChar(c))
                {
                    ChineseChar chineseChar = new ChineseChar(c);
                    ReadOnlyCollection<string> pinyin = chineseChar.Pinyins;
                    List<string> py = pinyin.Where(n => !string.IsNullOrEmpty(n)).ToList();
                    List<string> py1 = py.Select(n => n.Substring(0, n.Length - 1)).Distinct().ToList();
                    if (py1.Count() > 1)
                    {
                        polyphone = true;
                    }
                    pinyinStr.Append(pinyin[0].Substring(0, 1));
                    pinyinStr.Append(pinyin[0].Substring(1, pinyin[0].Length - 2).ToLower());
                }
                else
                {
                    pinyinStr.Append(c.ToString());
                }
            }
            return pinyinStr.ToString();
        }

        /// <summary>
        /// 获取单个字符的拼音
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static List<string> GetPinyinByChar(char c)
        {
            if (ChineseChar.IsValidChar(c))
            {
                ChineseChar chineseChar = new ChineseChar(c);
                ReadOnlyCollection<string> pinyin = chineseChar.Pinyins;
                List<string> py = pinyin.Where(n => !string.IsNullOrEmpty(n)).ToList();
                List<string> py1 = py.Select(n => n.Substring(0, n.Length - 1)).Distinct().ToList();

                return py1;
                //pinyinStr.Append(pinyin[0].Substring(0, 1));
                //pinyinStr.Append(pinyin[0].Substring(1, pinyin[0].Length - 2).ToLower());
            }
            else
            {
                return new List<string>() { c.ToString() };
            }
        }

        /// <summary> 
        /// C#中随机生成指定长度的密码
        /// </summary>
        public static string MakePassword(int pwdLength)
        {     //声明要返回的字符串    
            string tmpstr = "";
            //密码中包含的字符数组    
            //string pwdchars = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string pwdchars = "0123456789";
            //数组索引随机数    
            int iRandNum;
            //随机数生成器    
            Random rnd = new Random();
            for (int i = 0; i < pwdLength; i++)
            {      //Random类的Next方法生成一个指定范围的随机数     
                iRandNum = rnd.Next(pwdchars.Length);
                //tmpstr随机添加一个字符     
                tmpstr += pwdchars[iRandNum];
            }
            return tmpstr;
        }

        public static DataTable CreateTable(string[] FileColumns, string[] ColumnsType = null)
        {
            DataTable dt = new DataTable();
            for (int i = 0; i < FileColumns.Length; i++)
            {
                DataColumn dc = new DataColumn();
                dc.ColumnName = FileColumns[i];
                if (ColumnsType != null)
                {
                    switch (ColumnsType[i].Trim())
                    {
                        case "string":
                            dc.DataType = Type.GetType("System.String");
                            break;
                        case "datetime":
                            dc.DataType = Type.GetType("System.DateTime");
                            break;
                        case "int":
                            dc.DataType = Type.GetType("System.Int32");
                            break;
                        case "decimal":
                            dc.DataType = Type.GetType("System.Decimal");
                            break;
                        case "null":
                            dc.DataType = Type.GetType("System.DBNull");
                            break;
                        default:
                            dc.DataType = Type.GetType("System.String");
                            break;
                    }
                }
                dt.Columns.Add(dc);
            }
            dt.AcceptChanges();
            return dt;
        }

        /// <summary>
        /// 日期转换成yyyy-MM-dd
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string DateConvert_yyyyMMdd(string date)
        {
            if (!string.IsNullOrEmpty(date))
            {
                string newdate = Convert.ToDateTime(date).ToString("yyyy-MM-dd");
                if (newdate.Equals("1900-01-01"))
                {
                    return "";
                }
                else
                {
                    return newdate;
                }
            }
            return "";
        }

        /// <summary>
        /// 读取指定URL地址的HTML，用来以后发送网页用
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string ScreenScrapeHtml(string url)
        {
            //读取stream并且对于中文页面防止乱码
            StreamReader reader = new StreamReader(System.Net.WebRequest.Create(url).GetResponse().GetResponseStream(), System.Text.Encoding.UTF8);
            string str = reader.ReadToEnd();
            reader.Close();
            return str;
        }
        /// <summary>
        /// 将Json格式的时间字符串替换为"yyyy-MM-dd HH:mm:ss"格式的字符串
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string ReplaceJsonDateToDateString(string json)
        {
            return Regex.Replace(json, @"\\/Date\((\d+)\)\\/", match =>
            {
                DateTime dt = new DateTime(1970, 1, 1);
                dt = dt.AddMilliseconds(long.Parse(match.Groups[1].Value));
                dt = dt.ToLocalTime();
                return dt.ToString("yyyy-MM-dd");
            });
        }
    }
}
