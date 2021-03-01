using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace M_url.Data.Helpers
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; private set; }

        public int TotalPages { get; private set; }

        public int PageSize { get; private set; }

        public int TotalCount { get; private set; }

        public bool HasPrevious => (CurrentPage > 1);

        public bool HasNext => (CurrentPage < TotalPages);

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)PageSize);
            AddRange(items);
        }

        public static async Task<PagedList<T>> Create(IQueryable<T> source, ResourceParameters.IPagingResourceParameters paging)
        {
            var count = source.Count();
            var items = await source
              .Skip((paging.PageNumber - 1) * paging.PageSize)
              .Take(paging.PageSize).ToListAsync();

            return new PagedList<T>(items, count, paging.PageNumber, paging.PageSize);
        }
    }
}
