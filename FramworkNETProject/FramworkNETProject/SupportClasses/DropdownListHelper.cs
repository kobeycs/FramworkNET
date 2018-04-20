/* =============================================
 * 创建人：杜飞
 * 创建时间： 2011/7/4
 * 功能描述：下拉列表帮助类
  =============================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SupportClasses
{
    public class DropdownListHelper
    {
        /// <summary>
        /// 下拉列表文本
        /// </summary>
        public string ListText { get; set; }

        /// <summary>
        /// 下拉列表的值
        /// </summary>
        public long ListValue { get; set; } 
    }
}