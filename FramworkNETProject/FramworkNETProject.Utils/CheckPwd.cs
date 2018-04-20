using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utils
{
    public class CheckPwd
    {
        /// <summary>
        /// 密码规则（修改密码用）
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public static bool CheckPassWord(string name, string pwd)
        {
            string pwdlower = pwd.ToLower();
            if (pwd.Length < 8)
            {
                return false;
            }
            if (pwd.Contains(' ')) { return false; }
            int num = 0;
            if (Regex.Matches(pwd, "[0-9]").Count > 0) { num++; }
            if (Regex.Matches(pwd, "[a-z]").Count > 0) { num++; }
            if (Regex.Matches(pwd, "[A-Z]").Count > 0) { num++; }
            if (Regex.Matches(pwd, "((?=[\x21-\x7e]+)[^A-Za-z0-9])").Count > 0) { num++; }
            if (num < 3)
            {
                return false;
            }

            string str = "";
            for (int i = 0; i < pwdlower.Length - 2; i++)
            {
                str = pwd.Substring(i, 3);
                if (name.Contains(str))
                {
                    return false;
                }
            }
            return true;
        }



        /// <summary>
        /// 获取新密码（重置密码用）
        /// </summary>
        /// <param name="itcode"></param>
        /// <returns></returns>
        public static string GetNewPassWord(string itcode)
        {
            string pwd = GetRandomPassword();
            bool isok = CheckPassWord(itcode, pwd);
            bool ok = Read(pwd);
            if (isok && ok)
            {
                return pwd;
            }
            else
            {
                pwd = GetNewPassWord(itcode);
            }
            return pwd;
        }


        /// <summary>
        ///     生成随机密码
        /// </summary>
        /// <returns></returns>
        private static string GetRandomPassword()
        {
            char[] chr =
            {
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's',
                't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N',
                'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
            };
            string result = "";
            var random = new Random();
            int n = chr.Length;

            for (int i = 0; i < 4; i++)
            {
                int rnd = random.Next(0, n);
                result += chr[rnd];
            }
            result += "-" + Convert.ToString(random.Next(1000, 9999));
            return result;
        }

        /// <summary>
        /// 弱密码读取
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public static bool Read(string pwd)
        {
            string path = WebConfig.GetAppSetting("WeakPassword");
            //按行读取为字符串数组
            string[] lines = System.IO.File.ReadAllLines(path);
            List<string> list = lines.ToList<string>();
            if (list.FindAll(p => p == pwd) != null && list.FindAll(s => s == pwd).Count() > 0)
            {
                return false;
            }
            return true;
        }



    }
}
