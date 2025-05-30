﻿using Elasticsearch.Net;
using FastLog.Core.ES.Model;
using FastLog.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FastLog.Core.Elasticsearch
{
    internal class Elasticsearch : IElasticsearch
    {
        private static List<char> filters = new List<char> { '\\', '/', '*', '?', '\"', '<', '>', '|', ' ', '#', '%', '{', '}', ':', '@', '&', '=' };
        private ElasticLowLevelClient client = ServiceContext.Engine.Resolve<ElasticLowLevelClient>();
        private JsonSerializerOptions jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        public EsResponse Add(LogModel model)
        {
            var data = new EsResponse();
            var result = client.Index<StringResponse>(GetIndex(model.Type), model.id, PostData.Serializable(model));
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;
            return data;
        }

        public EsResponse Add(LogTypeModel model)
        {
            var data = new EsResponse();
            var result = client.Index<StringResponse>(GetIndex(model.IdxLogType), model.Id, PostData.Serializable(model));
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;
            return data;
        }

        public EsResponse Page(int pageSize, int pageId, string type, object query, object sort)
        {
            var result = new EsResponse();
            result.PageResult.Page.PageId = pageId;
            result.PageResult.Page.PageSize = pageSize;

            var data = new Dictionary<string, object>();
            StringResponse page;

            if (!string.IsNullOrEmpty(type))
                page = client.Search<StringResponse>(GetIndex(type), PostData.Serializable(new { size = pageSize, from = (pageId - 1) * pageSize, query = query, sort = sort }));
            else
                page = client.Search<StringResponse>(PostData.Serializable(new { size = pageSize, from = (pageId - 1) * pageSize, query = query, sort = sort }));

            if (page?.Success == true)
            {
                result.PageResult.IsSuccess = true;
                var body = page.Body;
                if (IsChinese(page.Body))
                    body = Uri.UnescapeDataString(page.Body);

                var list = JsonSerializer.Deserialize<EsResult>(body, jsonOption);

                list.hits.hits.ForEach(a =>
                {
                    result.PageResult.List.Add(a._source);
                });

                result.PageResult.Page.TotalRecord = list.hits.total.value;
            }
            else
                result.PageResult.Exception = page?.OriginalException;

            result.PageResult.Page.TotalPage = result.PageResult.Page.TotalRecord / pageSize + 1;

            if ((result.PageResult.Page.TotalRecord % result.PageResult.Page.PageSize) == 0)
                result.PageResult.Page.TotalPage = result.PageResult.Page.TotalRecord / result.PageResult.Page.PageSize;
            else
                result.PageResult.Page.TotalPage = (result.PageResult.Page.TotalRecord / result.PageResult.Page.PageSize) + 1;

            if (result.PageResult.Page.PageId > result.PageResult.Page.TotalPage)
                result.PageResult.Page.PageId = result.PageResult.Page.TotalPage;

            return result;
        }

        public EsResponse Count(string type)
        {
            var data = new EsResponse();
            StringResponse page;
            if (!string.IsNullOrEmpty(type))
                page = client.Search<StringResponse>(GetIndex(type), PostData.Empty);
            else
                page = client.Search<StringResponse>(PostData.Empty);

            data.IsSuccess = page.Success;
            data.Exception = page.OriginalException;

            if (page.Success)
            {
                var list = JsonSerializer.Deserialize<EsResult>(page.Body, jsonOption);

                data.Count = list.hits.total.value;
            }
            else
                data.Count = 0;

            return data;
        }

        public EsResponse Delete(string type, object query)
        {
            var data = new EsResponse();
            var result = client.DeleteByQuery<StringResponse>(GetIndex(type), PostData.Serializable(new { query = query }));
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;
            return data;
        }

        public EsResponse Delete(string type)
        {
            var data = new EsResponse();
            var result = client.DeleteByQuery<StringResponse>(GetIndex(type), PostData.Serializable(new { query = new { match_all = new { } } }));
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;
            return data; ;
        }

        public EsResponse Delete(string type, string id)
        {
            var data = new EsResponse();
            var result = client.Delete<StringResponse>(GetIndex(type), id);
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;
            return data;
        }

        public EsResponse Delete(string type, List<string> _id)
        {
            var data = new EsResponse();
            var result = client.DeleteByQuery<StringResponse>(GetIndex(type), PostData.Serializable(new { query = new { terms = new { _id } } }));
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;
            return data;
        }

        public EsResponse Delete(string type, PostData body)
        {
            var data = new EsResponse();
            var client = ServiceContext.Engine.Resolve<ElasticLowLevelClient>();
            var result = client.DeleteByQuery<StringResponse>(GetIndex(type), body);
            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;
            return data;
        }

        public EsResponse GetList(string type, int size = 10)
        {
            var result = new EsResponse();
            StringResponse stringResponse;
            if (!string.IsNullOrEmpty(type))
                stringResponse = client.Search<StringResponse>(GetIndex(type), PostData.Serializable(new { query = new { match_all = new { } }, size = size }));
            else
                stringResponse = client.Search<StringResponse>(PostData.Serializable(new { query = new { match_all = new { } }, size = size }));
            if (stringResponse.Success)
            {
                result.IsSuccess = true;
                var body = stringResponse.Body;
                if (IsChinese(stringResponse.Body))
                    body = Uri.UnescapeDataString(stringResponse.Body);

                var list = JsonSerializer.Deserialize<EsResult>(body, jsonOption);
                list.hits.hits.ForEach(a =>
                {
                    result.List.Add(a._source);
                });

                return result;
            }
            else
            {
                result.Exception = stringResponse.OriginalException;
                return result;
            }
        }

        internal string GetIndex(string index)
        {
            if (string.IsNullOrEmpty(index))
                return string.Empty;
            else
                return new string(index.ToLower().Where(c => !filters.Contains(c)).ToArray());
        }

        private static bool IsChinese(string text)
        {
            return Regex.IsMatch(text, @"[\u4e00-\u9fff\u3400 -\u4DBF\u20000 -\u2A6DF]");
        }

        public EsResponse Create<T>(string index)
        {
            var data = new EsResponse();
            var properties = new Dictionary<string, object>();

            typeof(T).GetProperties().ToList().ForEach(a =>
            {
                var column = a.GetCustomAttribute<ColumnAttribute>();
                if (column != null)
                {
                    properties.Add(a.Name, new { type = column.type });
                }
            });

            var result = client.Indices.Exists<StringResponse>(GetIndex(index));

            if (result != null && !result.Success)
                result = client.Indices.Create<StringResponse>(GetIndex(index), PostData.Serializable(new { mappings = new { properties = properties } }));

            data.IsSuccess = result != null ? result.Success : false;
            data.Exception = result?.OriginalException;

            return data;
        }
    }
}