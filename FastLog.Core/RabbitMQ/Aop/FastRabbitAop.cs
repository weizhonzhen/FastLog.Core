using FastLog.Core.Aop;
using FastLog.Core.Elasticsearch;
using FastLog.Core.Model;
using FastLog.Core.RabbitMQ.Context;
using FastRabbitMQ.Core.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace FastLog.Core.RabbitMQ.Aop
{
    internal class FastRabbitAop : IFastRabbitAop
    {
        public void Delete(DeleteContext context) { }

        public void Exception(ExceptionContext context) { }

        public void Receive(ReceiveContext context)
        {
            var logAop = ServiceContext.Engine.Resolve<IFastLogAop>();
            var client = ServiceContext.Engine.Resolve<IElasticsearch>();

            if (logAop != null)
            {
                var reContent = new MqReceiveContext();

                reContent.Content = context.content;
                logAop.MqReceive(reContent);
            }

            if (context.content.Keys.ToList().Exists(a => a == "Add"))
            {
                var result = false;
                var log = JsonSerializer.Deserialize<LogModel>(context.content["Add"].ToString());
                if (log != null)
                {
                    result = client.Add(log).IsSuccess;

                    if (logAop != null)
                    {
                        var esContext = new EsAddContext();
                        esContext.Content = context.content;
                        esContext.IsSuccess = result;
                        esContext.QueueName = context.config.QueueName;
                        logAop.EsAdd(esContext);
                    }
                }

                var list = client.GetList(nameof(LogTypeModel.IdxLogType).ToLower(), 100).List;
                if (!list.Exists(a => string.Compare(a[nameof(LogTypeModel.Name)].ToString(), log.Type, true) == 0) && result)
                    client.Add(new LogTypeModel() { Name = log.Type });
            }

            if (context.content.Keys.ToList().Exists(a => a == "Delete"))
            {
                var isSuccess = false;
                var log = JsonSerializer.Deserialize<LogModel>(context.content["Delete"].ToString());

                log.Id = JsonSerializer.Deserialize<string>(FastRabbit.ToDic(context.content["Delete"].ToString()).GetValue("id").ToString());
                if (log == null || string.IsNullOrEmpty(log.Type))
                    return;
                var list = log.Id.Split(',').ToList();
                if (!string.IsNullOrEmpty(log.Id) && list.Count == 1)
                    isSuccess = client.Delete(log.Type, log.Id).IsSuccess;
                if (!string.IsNullOrEmpty(log.Id) && list.Count > 1)
                    isSuccess = client.Delete(log.Type, list).IsSuccess;
                if (!string.IsNullOrEmpty(log.Title))
                    isSuccess = client.Delete(log.Type, new { match = new { Title = log.Title } }).IsSuccess;
                if (string.IsNullOrEmpty(log.Title) && string.IsNullOrEmpty(log.Id))
                    isSuccess = client.Delete(log.Type).IsSuccess;

                if (logAop != null)
                {
                    var esContext = new EsRemoveContext();
                    esContext.Content = context.content;
                    esContext.QueueName = context.config.QueueName;
                    esContext.IsSuccess = isSuccess;
                    logAop.EsRemove(esContext);
                }
            }
        }

        public void Send(SendContext context) { }
    }
}