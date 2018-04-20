using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class SqlSplit
    {
        #region 取得分页SQL
        /// <summary>
        /// 数据分页 (得到指定页记录的SQL语句)
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="orderField">字段名(排序)</param>
        /// <param name="selectFields">查询字段(多个字段以逗号隔开,查询全部字段用星号)</param>
        /// <param name="pageSize">面尺寸 一页显示的记录数</param>
        /// <param name="pageIndex">页码 当前页</param>
        /// <param name="recordCount">总记录数</param>
        /// <param name="asc">排序类型 true 升 false 降</param>
        /// <param name="where">查询的条件(注 不带WHERE)</param>
        /// <returns>返回SQL语句</returns>
        public static string GetPageSqlOld(string tableName, string orderField, string selectFields, int pageSize, int pageIndex, bool asc, string where)
        {
            //用于返回的取记录的SQL字符串
            int numFrom = Math.Max(pageIndex - 1, 0) * pageSize + 1;
            int numTo = pageIndex * pageSize;
            string sql = string.Format(@"
SELECT * FROM
(
    SELECT {0}, ROW_NUMBER() OVER(ORDER BY {1} {2}) AS rowNum,(select count(*) from {3} where {4}) as count
    FROM {3}
    WHERE {4}
) As myTable
WHERE rowNum BETWEEN {5} AND {6}"
                    , selectFields
                    , orderField
                    , asc ? "ASC" : "DESC"
                    , tableName
                    , where
                    , numFrom
                    , numTo);

            return sql;
        }

        /// <summary>
        /// 数据分页 (得到指定页记录的SQL语句)
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="orderField">字段名(排序)</param>
        /// <param name="selectFields">查询字段(多个字段以逗号隔开,查询全部字段用星号)</param>
        /// <param name="pageSize">面尺寸 一页显示的记录数</param>
        /// <param name="pageIndex">页码 当前页</param>
        /// <param name="recordCount">总记录数</param>
        /// <param name="asc">排序类型 true 升 false 降</param>
        /// <param name="where">查询的条件(注 不带WHERE)</param>
        /// <returns>返回SQL语句</returns>
        public static string GetPageSql(string tableName, string orderField, string selectFields, int pageSize, int pageIndex, bool asc, string where)
        {
            //用于返回的取记录的SQL字符串
            int numFrom = Math.Max(pageIndex - 1, 0) * pageSize;// + 1
            //int numTo = pageIndex * pageSize;
            int numTo = pageSize;
            string sql = string.Format(@"
    SELECT {0},(select count(*) from {3} where {4}) as count
    FROM {3}
    WHERE {4} ORDER BY {1} {2} limit {5},{6};"
                    , selectFields
                    , orderField
                    , asc ? "ASC" : "DESC"
                    , tableName
                    , where
                    , numFrom
                    , numTo);

            return sql;
        }
        #endregion




    }
}
