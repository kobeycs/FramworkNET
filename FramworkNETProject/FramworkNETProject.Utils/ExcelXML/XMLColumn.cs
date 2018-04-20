using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Utils.ExcelXML
{
    /// <summary>
    /// 单元格
    /// </summary>
    public class XMLColumn
    {
        private string _columnName;

        /// <summary>
        /// 类型
        /// </summary>
        public XMLColumnType ColumnType { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string ColumnName
        {
            get
            {
                return _columnName;
            }
            set
            {
                if (value != null)
                {
                    _columnName = value;
                }
                else
                {
                    _columnName = "";
                }
            }
        }

        public string DataName { get; set; }

        public XMLColumn(string columnName, XMLColumnType columnType = XMLColumnType.String,string dataName="")
        {
            this.ColumnType = columnType;
            this.ColumnName = columnName;
            this.DataName = dataName;
            
        }

        public Func<DataRow,object> Format { get; set; }
    }

    public enum XMLColumnType
    {
        String = 1,
        Number = 2,
        DateTime = 3
    }
}
