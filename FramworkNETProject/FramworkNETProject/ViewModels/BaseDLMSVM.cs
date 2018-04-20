/* =============================================
 * 创建人：杜飞
 * 创建时间： 2011-6-15
 * 文件功能描述：基本字段数据的添加
  =============================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ViewModels;
using Models;

namespace ViewModels
{
    public class BaseDLMSVM<TModel> : BaseCRUDVM<TModel> where TModel : BaseDLMSPoco
    {
        public override void DoAdd()
        {
            Entity.IsValid = true;
            Entity.Memo = string.Empty;
            Entity.Modifier = FFUser.ITCode;
            Entity.Creator = FFUser.ITCode;
            Entity.CreateDate = DateTime.Now;
            Entity.ModifyDate = DateTime.Now;
            base.DoAdd();
        }

        public override void DoEdit()
        {
            Entity.ModifyDate = DateTime.Now;
            Entity.Modifier = FFUser.ITCode;
            Entity.Memo = string.Empty;
            base.DoEdit();
        }
    }
}