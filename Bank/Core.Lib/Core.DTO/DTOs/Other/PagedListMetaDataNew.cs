using X.PagedList;

namespace Core.DTO.DTOs.Other
{
    public class PagedListMetaDataNew
    {
        public int PageCount { get; set; }
        public int TotalItemCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public bool IsFirstPage { get; set; }
        public bool IsLastPage { get; set; }
    }

    public static class PagedListMetaDataExtension
    {
        public static PagedListMetaDataNew GetPagedEntityMetaData(this IPagedList list)
        {
            return new PagedListMetaDataNew
            {
                PageCount = list.PageCount,
                TotalItemCount = list.TotalItemCount,
                PageNumber = list.PageNumber,
                PageSize = list.PageSize,
                HasPreviousPage = list.HasPreviousPage,
                HasNextPage = list.HasNextPage,
                IsFirstPage = list.IsFirstPage,
                IsLastPage = list.IsLastPage
            };
        }
    }
}
