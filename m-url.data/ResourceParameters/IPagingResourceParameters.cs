using System;

namespace M_url.Data.ResourceParameters
{
    public interface IPagingResourceParameters
    {
        int PageNumber { get; set; }

        int PageSize { get; set; }
    }
}
