using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    /// <summary>
    /// 用于记录标志7个类型的申请单每日流水号1 招聘需求2 异动申请3 离职申请4 加班申请5 补卡申请6 休假申请7 旷工申请
    /// </summary>
    [Table("System_ApplySerialNumber")]
    public class ApplySerialNumber : BasePoco
    {
        /// <summary>
        /// 生成日期（和系统 日期 比较，如不等于则更新为当前系统日期，并将种子数字改为1）
        /// </summary>
        public DateTime DateOfCreate { get; set; }
        /// <summary>
        /// 每天初始为1，每次取数后应递增数字1
        /// </summary>
        public int SerialSeedNumber { get; set; }
    }
}
