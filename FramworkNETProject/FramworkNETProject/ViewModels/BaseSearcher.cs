namespace ViewModels
{
    public class BaseSearcher : ISearcher
    {
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string SortField { get; set; }
        public string SortDir { get; set; }
    }

    public interface ISearcher
    {
    }
}