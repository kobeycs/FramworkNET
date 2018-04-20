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
        /// ����pdf
        /// </summary>
        /// <param name="IsHorizontal">�Ƿ�Ϊˮƽҳ��</param>
        /// <param name="TableTitle">��ѯ�б�,��ͷ����</param>
        /// <returns></returns>
        byte[] GeneratePDF(bool IsHorizontal = false, Helpers.PdfHelper.PdfTableTitle TableTitle = null);
    }
}
