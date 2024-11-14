using FastLog.Core.Elasticsearch;
using FastLog.Core.ES.Model;
using FastLog.Core.Model;
using FastRabbitMQ.Core.Repository;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace FastLog.Core
{
    public class FastLog : IFastLog
    {
        private static List<char> filters = new List<char> { '\\', '/', '*', '?', '\"', '<', '>', '|', ' ', '#', '%', '{', '}', ':', '@', '&', '=' };
        public void Save(string type, string title, string message)
        {
            type = new string(type.ToLower().Where(c => !filters.Contains(c)).ToArray());
            var client = ServiceContext.Engine.Resolve<IFastRabbit>();
            var model = new LogModel();
            var dic = new Dictionary<string, object>();

            model.Title = title;
            model.Content = message;
            model.Type = type;

            dic.Add("Add", model);

            client.Send(FastLogExtension.config, dic);
        }

        public void Delete(string type, string title)
        {
            type = new string(type.ToLower().Where(c => !filters.Contains(c)).ToArray());
            var client = ServiceContext.Engine.Resolve<IFastRabbit>();
            var model = new LogModel();
            var dic = new Dictionary<string, object>();

            model.Title = title;
            model.Type = type;

            dic.Add("Delete", model);

            client.Send(FastLogExtension.config, dic);
        }

        public List<string> Type(int size = 10)
        {
            var data = new List<string>();
            var client = ServiceContext.Engine.Resolve<IElasticsearch>();
            var list = client.GetList(nameof(LogTypeModel.IdxLogType).ToLower(),size);
            list.ForEach(a =>
            {
                data.Add(a[nameof(LogTypeModel.Name)].ToString());
            });
            return data;
        }

        public PageResult Page(string type, string title, string content, int pageId = 1, int pageSize = 10, bool isWildCard = false, bool isDesc = true)
        {
            type = new string(type.ToLower().Where(c => !filters.Contains(c)).ToArray());
            var client = ServiceContext.Engine.Resolve<IElasticsearch>();
            object sort = new[] { new { DateTime = new { order = isDesc ? "desc" : "asc" } } };
            object query = new { match_all = new { } };

            if (!string.IsNullOrEmpty(title) && string.IsNullOrEmpty(content) && isWildCard == false)
                query = new { match = new { Title = title } };

            if (!string.IsNullOrEmpty(title) && string.IsNullOrEmpty(content) && isWildCard)
                query = new { wildcard = new { Title = $"{title}*" } };

            if (!string.IsNullOrEmpty(content) && string.IsNullOrEmpty(title) && isWildCard == false)
                query = new { match = new { Content = content } };

            if (!string.IsNullOrEmpty(content) && string.IsNullOrEmpty(title) && isWildCard)
                query = new { wildcard = new { Content = $"{content}*" } };

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(content) && isWildCard == false)
                query = new { match = new { Title = title, Content = content } };

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(content) && isWildCard)
                query = new { wildcard = new { Title = $"{title}*", Content = $"{content}*" } };

            return client.Page(pageSize, pageId, type, query, sort);
        }

        public int Count(string type)
        {
            type = new string(type.ToLower().Where(c => !filters.Contains(c)).ToArray());
            var client = ServiceContext.Engine.Resolve<IElasticsearch>();
            return client.Count(type);
        }

        public List<Dictionary<string, object>> GetList(string type, int size = 10)
        {
            type = new string(type.ToLower().Where(c => !filters.Contains(c)).ToArray());
            var client = ServiceContext.Engine.Resolve<IElasticsearch>();
            return client.GetList(type,size);
        }
    }
}