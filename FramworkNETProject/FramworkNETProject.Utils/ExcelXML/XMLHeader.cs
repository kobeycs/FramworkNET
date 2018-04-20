using System;
using System.Collections.Generic;
using System.Data;

namespace Utils.ExcelXML
{
    public class XMLHeader
    {
        /// <summary>
        /// 列集合
        /// </summary>
        public List<XMLColumn> Columns { get; set; }

        public XMLHeader()
        {
            Columns = new List<XMLColumn>();
        }

        public void GenrateColumns(DataColumnCollection dataColumns)
        {
            if (dataColumns != null && dataColumns.Count > 0)
            {
                List<Type> numberType = new List<Type> { typeof(decimal), typeof(decimal?), typeof(int), typeof(int?), typeof(long), typeof(long?) };
                foreach (DataColumn col in dataColumns)
                {
                    var xmlCol = new XMLColumn(col.ColumnName);
                    xmlCol.DataName = col.ColumnName;
                    var dbtype = col.DataType;
                    //先判断是否为时间
                    if (dbtype == typeof(DateTime))
                    {
                        xmlCol.ColumnType = XMLColumnType.DateTime;
                    }
                    //再判断是否为数字
                    else if (numberType.Contains(dbtype))
                    {
                        xmlCol.ColumnType = XMLColumnType.Number;
                    }
                    //否则，为字符串
                    else
                    {
                        xmlCol.ColumnType = XMLColumnType.String;
                    }
                    Columns.Add(xmlCol);
                }
            }
        }
    }
}
