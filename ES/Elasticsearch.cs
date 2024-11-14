using Elasticsearch.Net;
using FastLog.Core.ES.Model;
using FastLog.Core.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace FastLog.Core.Elasticsearch
{
    internal class Elasticsearch : IElasticsearch
    {
        JsonSerializerOptions jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        public bool Add(LogModel model)
        {
            var client = ServiceContext.Engine.Resolve<ElasticLowLevelClient>();
            var result = client.Index<StringResponse>(model.Type, model.Id, PostData.Serializable(model));
            return result != null ? result.Success : false;
        }

        public bool Add(LogTypeModel model)
        {
            var client = ServiceContext.Engine.Resolve<ElasticLowLevelClient>();
            var result = client.Index<StringResponse>(model.IdxLogType, model.Id, PostData.Serializable(model));
            return result != null ? result.Success : false;
        }

        public PageResult Page(int pageSize, int pageId, string type, object query, object sort)
        {
            var result = new PageResult();
            result.page.PageId = pageId;
            result.page.PageSize = pageSize;

            var data = new Dictionary<string, object>();
            var client = ServiceContext.Engine.Resolve<ElasticLowLevelClient>();
            var page = client.Search<StringResponse>(type, PostData.Serializable(new { size = pageSize, from = (pageId - 1) * pageSize, query = query, sort = sort }));
            if (page.Success)
            {
                var list = JsonSerializer.Deserialize<EsResult>(page.Body, jsonOption);

                list.hits.hits.ForEach(a =>
                {
                    result.list.Add(a._source);
                });

                result.page.TotalRecord = list.hits.total.value;
            }

            result.page.TotalPage = result.page.TotalRecord / pageSize + 1;

            if ((result.page.TotalRecord % result.page.PageSize) == 0)
                result.page.TotalPage = result.page.TotalRecord / result.page.PageSize;
            else
                result.page.TotalPage = (result.page.TotalRecord / result.page.PageSize) + 1;

            if (result.page.PageId > result.page.TotalPage)
                result.page.PageId = result.page.TotalPage;

            return result;
        }

        public int Count(string type)
        {
            var client = ServiceContext.Engine.Resolve<ElasticLowLevelClient>();
            var page = client.Search<StringResponse>(type, PostData.Empty);
            if (page.Success)
            {
                var list = JsonSerializer.Deserialize<EsResult>(page.Body, jsonOption);

                return list.hits.total.value;
            }
            else
                return 0;
        }

        public bool delete(string type, object query)
        {
            var client = ServiceContext.Engine.Resolve<ElasticLowLevelClient>();
            var result = client.DeleteByQuery<StringResponse>(type, PostData.Serializable(new { query = query }));
            return result != null ? result.Success : false;
        }

        public bool delete(string type)
        {
            var client = ServiceContext.Engine.Resolve<ElasticLowLevelClient>();
            var result = client.DeleteByQuery<StringResponse>(type, PostData.Serializable(new { query = new { match_all = new { } } }));
            return result != null ? result.Success : false;
        }

        public bool delete(string type, string id)
        {
            var client = ServiceContext.Engine.Resolve<ElasticLowLevelClient>();
            var result = client.Delete<StringResponse>(type, id);
            return result != null ? result.Success : false;
        }

        public bool delete(string type, List<string> _id)
        {
            var client = ServiceContext.Engine.Resolve<ElasticLowLevelClient>();
            var result = client.DeleteByQuery<StringResponse>(type, PostData.Serializable(new { query = new { terms = new { _id } } }));
            return result != null ? result.Success : false;
        }

        public bool delete(string type, PostData body)
        {
            var client = ServiceContext.Engine.Resolve<ElasticLowLevelClient>();
            var result = client.DeleteByQuery<StringResponse>(type, body);
            return result != null ? result.Success : false;
        }

        public List<Dictionary<string, object>> GetList(string type, int size = 10)
        {
            var client = ServiceContext.Engine.Resolve<ElasticLowLevelClient>();
            var result = client.Search<StringResponse>(type, PostData.Serializable(new { query = new { match_all = new { } }, size =  size }));
            if (result.Success)
            {
                var data = new List<Dictionary<string, object>>();
                var list = JsonSerializer.Deserialize<EsResult>(result.Body, jsonOption);
                list.hits.hits.ForEach(a =>
                {
                    data.Add(a._source);
                });

                return data;
            }
            else
                return new List<Dictionary<string, object>>();
        }
    }
}