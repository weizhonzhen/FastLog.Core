using FastLog.Core.Elasticsearch;
using FastLog.Core.Model;
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
            var client = ServiceContext.Engine.Resolve<IElasticsearch>();
            if (context.content.Keys.ToList().Exists(a => a == "Add"))
            {
                var result = false;
                var log = JsonSerializer.Deserialize<LogModel>(context.content["Add"].ToString());
                if (log != null)
                    result = client.Add(log);

                var list = client.GetList(nameof(LogTypeModel.LogType).ToLower());
                if (!list.Exists(a => string.Compare(a[nameof(LogTypeModel.LogType)].ToString(), log.Type, true) == 0) && result)
                    client.Add(new LogTypeModel { Name = log.Type });
            }

            if (context.content.Keys.ToList().Exists(a => a == "Delete"))
            {
                var log = JsonSerializer.Deserialize<LogModel>(context.content["Delete"].ToString());
                if (log != null)
                    client.delete(log.Type, new { match = new { Title = log.Title } });
            }
        }

        public void Send(SendContext context) { }
    }
}