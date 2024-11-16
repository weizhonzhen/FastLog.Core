﻿using FastLog.Core.Elasticsearch;
using FastLog.Core.ES.Model;
using FastLog.Core.Model;
using FastRabbitMQ.Core.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            var list = client.GetList(nameof(LogTypeModel.IdxLogType).ToLower(),size).List;
            list.ForEach(a =>
            {
                data.Add(a[nameof(LogTypeModel.Name)].ToString());
            });
            return data.Distinct().ToList();
        }

        public EsResponse Page(LogModel model, int pageId = 1, int pageSize = 10, bool isWildCard = false, bool isDesc = true)
        {
            model.Type = model.Type ?? string.Empty;
            model.Type = new string(model.Type.ToLower().Where(c => !filters.Contains(c)).ToArray());
            var client = ServiceContext.Engine.Resolve<IElasticsearch>();
            var query = new Dictionary<string, object>();
            var wildcard = new Dictionary<string, object>();
            var match = new Dictionary<string, object>();
            object sort = new[] { new { DateTime = new { order = isDesc ? "desc" : "asc" } } };

            if (!string.IsNullOrEmpty(model.Title) && isWildCard)
                wildcard.Add("Title", $"{model.Title}*");

            if (!string.IsNullOrEmpty(model.Content) && isWildCard)
                wildcard.Add("Content", $"{model.Content}*");

            if (!string.IsNullOrEmpty(model.Person) && isWildCard)
                wildcard.Add("Person", $"{model.Person}*");

            if (!string.IsNullOrEmpty(model.Title) && !isWildCard)
                match.Add("Title", model.Title);

            if (!string.IsNullOrEmpty(model.Content) && !isWildCard)
                match.Add("Content", model.Content);

            if (!string.IsNullOrEmpty(model.Person) && !isWildCard)
                match.Add("Person", model.Person);

            if (model.DateTime != DateTime.MinValue)
                match.Add("DateTime", model.DateTime);

            if (match.Count == 0 && wildcard.Count == 0)
                query.Add("match_all", new Dictionary<string,object>());

            if (match.Count > 0)
                query.Add("match", match);

            if (wildcard.Count > 0)
                query.Add("wildcard", wildcard);

            return client.Page(pageSize, pageId, model.Type, query, sort);
        }

        public EsResponse Count(string type)
        {
            type = type ?? string.Empty;
            type = new string(type.ToLower().Where(c => !filters.Contains(c)).ToArray());
            var client = ServiceContext.Engine.Resolve<IElasticsearch>();
            return client.Count(type);
        }

        public EsResponse GetList(string type, int size = 10)
        {
            type = type ?? string.Empty;
            type = new string(type.ToLower().Where(c => !filters.Contains(c)).ToArray());
            var client = ServiceContext.Engine.Resolve<IElasticsearch>();
            return client.GetList(type,size);
        }
    }
}