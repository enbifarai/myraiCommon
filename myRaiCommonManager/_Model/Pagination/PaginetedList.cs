using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel.Pagination
{
    public class PaginatedList<T> : List<T>
    {
        private const int DEFAULT_PAGE_SIZE = 25;
        public int PageIndex { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public bool HasNext { get; private set; }

        //public int TotalFiltered { get; }

        private PaginatedList(List<T> paginatedItems, int pageIndex, int totalCount, bool hasNext)
        {
            this.PageIndex = pageIndex;         
            this.TotalCount = totalCount;
            this.HasNext = hasNext;
            base.AddRange(paginatedItems);
        }

        public PaginatedList() { }

        public static PaginatedList<T> CratePaginatedList(IEnumerable<T> itemsQueryable, int pageIndex = 1)
        {
            var itemPaginated = itemsQueryable.Skip(DEFAULT_PAGE_SIZE * (pageIndex - 1)).Take(DEFAULT_PAGE_SIZE).ToList();
            var totalCount = itemsQueryable.Count();
            bool hasNext = false;
            //if (totalCount > DEFAULT_PAGE_SIZE)
            if (totalCount > (itemPaginated.Count() + (DEFAULT_PAGE_SIZE * (pageIndex - 1))))
                hasNext = true;

            return  new PaginatedList<T>(itemPaginated,pageIndex,totalCount, hasNext);          
        }

    }
}
