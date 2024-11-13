using Elasticsearch.Net;
using FastLog.Core.ES.Model;
using FastLog.Core.Model;
using System.Collections.Generic;

namespace FastLog.Core.Elasticsearch
{
    internal interface IElasticsearch
    {
        bool Add(LogModel model);

        bool Add(LogTypeModel model);

        bool delete(string type, object query);

        bool delete(string type);

        bool delete(string type, PostData body);

        PageResult Page(int pageSize, int pageId, string index, object query, object sort);

        int Count(string type);

        List<Dictionary<string, object>> GetList(string type, int size = 10);
    }
}
