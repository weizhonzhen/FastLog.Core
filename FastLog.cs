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
        public void Save(LogModel model)
        {
            model.Type = model.Type ?? string.Empty;
            model.Type = new string(model.Type.ToLower().Where(c => !filters.Contains(c)).ToArray());
            var client = ServiceContext.Engine.Resolve<IFastRabbit>();
            var dic = new Dictionary<string, object>();
            dic.Add("Add", model);
            client.Send(FastLogExtension.config, dic);
        }

        public void Delete(string type, string title, string id)
        {
            type = type ?? string.Empty;
            type = new string(type.ToLower().Where(c => !filters.Contains(c)).ToArray());
            var client = ServiceContext.Engine.Resolve<IFastRabbit>();
            var model = new LogModel();
            var dic = new Dictionary<string, object>();

            model.Title = title;
            model.Type = type;
            model.Id = id;

            dic.Add("Delete", model);

            client.Send(FastLogExtension.config, dic);
        }

        public void Delete(string type, string title, List<string> id)
        {
            type = type ?? string.Empty;
            type = new string(type.ToLower().Where(c => !filters.Contains(c)).ToArray());
            var client = ServiceContext.Engine.Resolve<IFastRabbit>();
            var model = new LogModel();
            var dic = new Dictionary<string, object>();

            model.Title = title;
            model.Type = type;
            model.Id = string.Join(",", id);

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
            return data.Distinct().ToList();
        }

        public PageResult Page(string type, string title, string content, string person, int pageId = 1, int pageSize = 10, bool isWildCard = false, bool isDesc = true)
        {
            type = type ?? string.Empty;
            type = new string(type.ToLower().Where(c => !filters.Contains(c)).ToArray());
            var client = ServiceContext.Engine.Resolve<IElasticsearch>();
            var query = new Dictionary<string, object>();
            var wildcard = new Dictionary<string, object>();
            var match = new Dictionary<string, object>();
            object sort = new[] { new { DateTime = new { order = isDesc ? "desc" : "asc" } } };

            if (!string.IsNullOrEmpty(title) && isWildCard)
                wildcard.Add("Title", $"{title}*");

            if (!string.IsNullOrEmpty(content) && isWildCard)
                wildcard.Add("Content", $"{content}*");

            if (!string.IsNullOrEmpty(person) && isWildCard)
                wildcard.Add("Person", $"{person}*");

            if (!string.IsNullOrEmpty(title) && !isWildCard)
                match.Add("Title", title);

            if (!string.IsNullOrEmpty(content) && !isWildCard)
                match.Add("Content", content);

            if (!string.IsNullOrEmpty(person) && !isWildCard)
                match.Add("Person", person);

            if (match.Count == 0 && wildcard.Count == 0)
                query.Add("match_all", new Dictionary<string,object>());

            if (match.Count > 0)
                query.Add("query", match);

            if (wildcard.Count > 0)
                query.Add("query", match);

            return client.Page(pageSize, pageId, type, query, sort);
        }

        public int Count(string type)
        {
            type = type ?? string.Empty;
            type = new string(type.ToLower().Where(c => !filters.Contains(c)).ToArray());
            var client = ServiceContext.Engine.Resolve<IElasticsearch>();
            return client.Count(type);
        }

        public List<Dictionary<string, object>> GetList(string type, int size = 10)
        {
            type = type ?? string.Empty;
            type = new string(type.ToLower().Where(c => !filters.Contains(c)).ToArray());
            var client = ServiceContext.Engine.Resolve<IElasticsearch>();
            return client.GetList(type,size);
        }
    }
}