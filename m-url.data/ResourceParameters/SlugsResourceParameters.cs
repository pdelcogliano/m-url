using System;

namespace M_url.Data.ResourceParameters
{
    public class SlugsResourceParameters : PagingResourceParameters
    {
        public string MainCategory { get; set; }

        public string SearchQuery { get; set; }

    }
}
