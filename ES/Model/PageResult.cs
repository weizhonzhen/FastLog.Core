using System.Collections.Generic;

namespace FastLog.Core.ES.Model
{
    public class PageResult
    {
        public PageModel page = new PageModel();

        public List<Dictionary<string, object>> list { get; set; } = new List<Dictionary<string, object>>();
    }

    public class PageModel
    {
        public int TotalRecord { get; set; }

        public int TotalPage { get; set; }

        public int PageId { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
