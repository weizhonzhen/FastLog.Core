﻿using System.Collections.Generic;
using System;

namespace FastLog.Core.ES.Model
{
    public class PageResult
    {
        public Exception Exception { get; set; }
        
        public bool IsSuccess { get; set; }

        public PageModel Page = new PageModel();

        public List<Dictionary<string, object>> List { get; set; } = new List<Dictionary<string, object>>();
    }

    public class PageModel
    {
        public int TotalRecord { get; set; }

        public int TotalPage { get; set; }

        public int PageId { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }

    public class EsResponse
    {
        public bool IsSuccess { get; set; }

        public Exception Exception { get; set; }

        public PageResult PageResult { get; set; } = new PageResult();

        public List<Dictionary<string, object>> List { get; set; } = new List<Dictionary<string, object>>();

        public int Count {  get; set; }
    }
}
