using System;

namespace M_url.Data.ResourceParameters
{
    public class PagingResourceParameters : IPagingResourceParameters
    {
        const int maxPageSize = 20;

        protected int m_pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize 
        {
            get => m_pageSize;
            set => m_pageSize = (value > maxPageSize) ? maxPageSize : value;
        }
    }
}
