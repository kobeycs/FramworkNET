using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Utils
{
    public class WebConfig
    {
        private static Dictionary<string, string> DecryptDic = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        public static string GetAppSetting(string key, bool decrypt = false)
        {
            try
            {
                if (decrypt)
                {
                    string appSettingKey = "appsetting" + key;
                    if (!DecryptDic.ContainsKey(appSettingKey))
                    {
                        DecryptDic[appSettingKey] = AESHelper.DecryptString(ConfigurationManager.AppSettings[key]);
                    }
                    return DecryptDic[appSettingKey];
                }
                else
                    return ConfigurationManager.AppSettings[key];
            }
            catch
            {
                return "";
            }
        }

        public static string GetConnectionString(string key, bool decrypt = false)
        {
            if (decrypt)
            {
                string connStringKey = "connString" + key;
                if (!DecryptDic.ContainsKey(connStringKey))
                {
                    DecryptDic[connStringKey] = AESHelper.DecryptString(ConfigurationManager.ConnectionStrings[key].ConnectionString);
                }
                return DecryptDic[connStringKey];
            }
            else
                return ConfigurationManager.ConnectionStrings[key].ConnectionString;
        }
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string DefaultConnectionString
        {
            get { return GetConnectionString("Sys_ConnectionString", false); }
        }
        /// <summary>
        /// 日志库连接字符串
        /// </summary>
        public static string LogConnectionString
        {
            get { return ""; }
        }


    }
}
