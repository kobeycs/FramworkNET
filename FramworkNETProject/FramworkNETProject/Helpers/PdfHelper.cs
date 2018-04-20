using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace Helpers
{
    public static class PdfHelper 
    {
        /// <summary>
        /// pdf-Writer的事件
        /// </summary>
        public class PDFEvent : PdfPageEventHelper
        {
            /// <summary>
            /// 页面基础字体
            /// </summary>
            public BaseFont BfFont { get; private set; }
            /// <summary>
            /// 页面大小
            /// </summary>
            public Rectangle PageSize { get; private set; }

            /// <summary>
            /// 页面标题部分数据
            /// </summary>
            public PdfTableTitle TableTitle { get; set; }

            public PDFEvent(BaseFont bfFont,Rectangle pagesize)
            {
                BfFont = bfFont;
                PageSize = pagesize;
            }

            public override void OnEndPage(PdfWriter writer, Document document)
            {
                CreateHeaderFooterTemplate(writer);
            }

            /// <summary>
            /// 设置标题
            /// </summary>
            /// <param name="writer"></param>
            public void CreateHeaderFooterTemplate(PdfWriter writer)
            {
                #region 标题
                var tempHeader = writer.DirectContent.CreateTemplate(300, 50);
                tempHeader.BeginText();

                tempHeader.SetFontAndSize(BfFont, 10);
                tempHeader.SetTextMatrix(0, 10);
                tempHeader.ShowText(TableTitle.Title);
                tempHeader.EndText();
                writer.DirectContent.AddTemplate(tempHeader, PageSize.Width * 0.4f, PageSize.Height - 50);
                #endregion

                #region 左边查询文档
                if (TableTitle.SearchList != null && TableTitle.SearchList.Count > 0)
                {
                    int length = TableTitle.SearchList.Count * 10;
                    var tempLeft = writer.DirectContent.CreateTemplate(PageSize.Width - 80, length);
                    tempLeft.BeginText();
                    tempLeft.SetFontAndSize(BfFont, 8);
                    foreach (var str in TableTitle.SearchList)
                    {
                        length = length - 10;
                        tempLeft.SetTextMatrix(0, length);
                        tempLeft.ShowText(str.Key + ":" + str.Value.ToString());
                    }
                    tempLeft.EndText();
                    writer.DirectContent.AddTemplate(tempLeft, 10, PageSize.Height - 100);
                }
                #endregion

                #region 右边时间显示文档
                var tempRight = writer.DirectContent.CreateTemplate(80, 30);
                tempRight.BeginText();

                tempRight.SetFontAndSize(BfFont, 8);
                tempRight.SetTextMatrix(0, 20);
                tempRight.ShowText("PageNumber:" + writer.CurrentPageNumber.ToString());
                tempRight.SetTextMatrix(0, 10);
                tempRight.ShowText("Run Date:" + TableTitle.RunTime.ToString("yyyy-MM-dd"));
                tempRight.SetTextMatrix(0, 0);
                tempRight.ShowText("Run Time:" + TableTitle.RunTime.ToString("HH:mm:ss"));
                tempRight.EndText();
                writer.DirectContent.AddTemplate(tempRight, PageSize.Width - 100, PageSize.Height - 100);
                #endregion

                #region 尾部
                PdfTemplate template = writer.DirectContent.CreateTemplate(500, 50);
                template.BeginText();
                template.SetFontAndSize(BfFont, 9);
                template.SetTextMatrix(10, 10);
                template.ShowText("提交人签字________________核对人签字_____________审批人签字____________________");
                template.EndText();
                writer.DirectContent.AddTemplate(template, 100, 10);
                #endregion
            }

        }

        /// <summary>
        /// 表头专用显示模版
        /// </summary>
        public class PdfTableTitle 
        {
            /// <summary>
            /// 查询列表
            /// </summary>
            public Dictionary<string, object> SearchList { get; set; }
            /// <summary>
            /// 标题
            /// </summary>
            public string Title { get; set; }
            /// <summary>
            /// 查询时间
            /// </summary>
            public DateTime RunTime { get; set; }

            /// <summary>
            /// 每页显示数量
            /// </summary>
            public int Count { get; set; }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="count">每页显示数量</param>
            /// <param name="title">页头内容</param>
            /// <param name="searchlist"></param>
            public PdfTableTitle(int count=15,string title = "", Dictionary<string, object> searchlist = null)
            {
                Count = count;
                if (searchlist == null)
                {
                    searchlist = new Dictionary<string, object>();
                }
                SearchList = searchlist;
                Title = title;

                RunTime = DateTime.Now;
            }
            
        }
        
    }
}