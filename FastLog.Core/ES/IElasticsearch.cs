using Elasticsearch.Net;
using FastLog.Core.ES.Model;
using FastLog.Core.Model;
using System.Collections.Generic;

namespace FastLog.Core.Elasticsearch
{
    internal interface IElasticsearch
    {
        EsResponse Add(LogModel model);

        EsResponse Add(LogTypeModel model);

        EsResponse delete(string type, object query);

        EsResponse delete(string type);

        EsResponse delete(string type, string id);

        EsResponse delete(string type, List<string> id);

        EsResponse delete(string type, PostData body);

        EsResponse Page(int pageSize, int pageId, string index, object query, object sort);

        EsResponse Count(string type);

        EsResponse GetList(string type, int size = 10);
    }
}
