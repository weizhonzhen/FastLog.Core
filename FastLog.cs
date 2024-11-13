using FastLog.Core.Elasticsearch;
using FastLog.Core.ES.Model;
using FastLog.Core.Model;
using FastRabbitMQ.Core.Repository;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace FastLog.Core
{
    public class FastLog : IFastLog
    {
        public void Save(string type, string title, string message)
        {
            var client = ServiceContext.Engine.Resolve<IFastRabbit>();
            var model = new LogModel();
            var dic = new Dictionary<string, object>();

            model.Title = title;
            model.Content = message;
            model.Type = type.ToLower();

            dic.Add("Add", model);

            client.Send(FastLogExtension.config, dic);
        }

        public void Delete(string type, string title)
        {
            var client = ServiceContext.Engine.Resolve<IFastRabbit>();
            var model = new LogModel();
            var dic = new Dictionary<string, object>();

            model.Title = title;
            model.Type = type.ToLower();

            dic.Add("Delete", model);

            client.Send(FastLogExtension.config, dic);
        }

        public List<string> Type()
        {
            var data = new List<string>();
            var client = ServiceContext.Engine.Resolve<IElasticsearch>();
            var list = client.GetList(nameof(LogTypeModel.IdxLogType).ToLower());
            list.ForEach(a =>
            {
                data.Add(a[nameof(LogTypeModel.Name)].ToString());
            });
            return data;
        }

        public PageResult Page(string type, string title, string content, int pageId = 1, int pageSize = 10, bool isWildCard = false, bool isDesc = true)
        {
            var client = ServiceContext.Engine.Resolve<IElasticsearch>();
            object sort = new[] { new { DateTime = new { order = isDesc ? "desc" : "asc" } } };
            object query = new { match_all = new { } };

            if (!string.IsNullOrEmpty(title) && string.IsNullOrEmpty(content) && isWildCard == false)
                query = new { match = new { Title = title } };

            if (!string.IsNullOrEmpty(title) && string.IsNullOrEmpty(content) && isWildCard)
                query = new { wildcard = new { Title = $"{title}*" } };

            if (!string.IsNullOrEmpty(content) && string.IsNullOrEmpty(title) && isWildCard == false)
                query = new { match = new { Content = content } };

            if (!string.IsNullOrEmpty(content) && string.IsNullOrEmpty(title))
                query = new { wildcard = new { Content = $"{content}*" } };

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(content) && isWildCard == false)
                query = new { match = new { Title = title, Content = content } };

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(content))
                query = new { wildcard = new { Title = $"{title}*", Content = $"{content}*" } };

            return client.Page(pageSize, pageId, type.ToLower(), query, sort);
        }

        public int Count(string type)
        {
            var client = ServiceContext.Engine.Resolve<IElasticsearch>();
            return client.Count(type.ToLower());
        }

        public List<Dictionary<string, object>> GetList(string type)
        {
            var client = ServiceContext.Engine.Resolve<IElasticsearch>();
            return client.GetList(type.ToLower());
        }
    }
}