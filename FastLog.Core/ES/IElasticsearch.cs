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

        EsResponse Delete(string type, object query);

        EsResponse Delete(string type);

        EsResponse Delete(string type, string id);

        EsResponse Delete(string type, List<string> id);

        EsResponse Delete(string type, PostData body);

        EsResponse Page(int pageSize, int pageId, string index, object query, object sort);

        EsResponse Count(string type);

        EsResponse GetList(string type, int size = 10);

        EsResponse Create<T>(string index);
    }
}
