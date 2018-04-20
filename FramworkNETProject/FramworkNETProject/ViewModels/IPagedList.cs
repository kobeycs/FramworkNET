using ViewModels;
using System.Collections.Generic;

namespace ViewModels
{
    public interface IPagedList<TModel,TSearcher> : IPagedList
        where TModel : class  
        where TSearcher: BaseSearcher
    {
        TSearcher Searcher { get; set; }
        List<TModel> EntityList { get; set; }
        List<IGridColumn<TModel>> ListColumns { get; set; }
    }

    public interface IPagedList
    {
        int RecordsPerPage { get; set; }
        bool NeedPage { get; set; }
        bool ShowHeader { get; set; }

        void DoSearch();
        byte[] GenerateExcel();
        
        /// <summary>
        /// 导出pdf
        /// </summary>
        /// <param name="IsHorizontal">是否为水平页面</param>
        /// <param name="TableTitle">查询列表,表头数据</param>
        /// <returns></returns>
        byte[] GeneratePDF(bool IsHorizontal = false, Helpers.PdfHelper.PdfTableTitle TableTitle = null);
    }
}
