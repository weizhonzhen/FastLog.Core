using FastLog.Core;
using FastLog.Core.Aop;
using FastLog.Core.RabbitMQ.Aop;
using FastLog.Core.RabbitMQ.Context;
using FastLog.Core.RabbitMQ.Model;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace FastRabbitMQ.Core.Repository
{
    internal class FastRabbit : IFastRabbit
    {
        public void Delete(ConfigModel config)
        {
            var logAop = ServiceContext.Engine.Resolve<IFastLogAop>();
            var conn = ServiceContext.Engine.Resolve<IConnection>();
            var aop = ServiceContext.Engine.Resolve<IFastRabbitAop>();
            try
            {
                using (var channe = conn.CreateModel())
                {
                    if (config.Exchange == null)
                        channe.QueueDelete(config.QueueName, config.IsUnused, config.IsEmpty);
                    else
                        channe.ExchangeDelete(config.Exchange.ExchangeName, config.IsUnused);

                    var delete = new DeleteContext();
                    delete.config = config;
                    aop.Delete(delete);
                }
            }
            catch (Exception ex)
            {
                if (logAop != null)
                {
                    var mqContent = new MqExceptionContext();
                    mqContent.Exception = ex;
                    mqContent.QueueName = config.QueueName;
                    logAop.MqException(mqContent);
                }

                var context = new ExceptionContext();
                context.ex = ex;
                context.isDelete = true;
                context.config = config;
                aop.Exception(context);
            }
        }

        public void Send(ConfigModel config, Dictionary<string, object> content)
        {
            var logAop = ServiceContext.Engine.Resolve<IFastLogAop>();
            var jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            var conn = ServiceContext.Engine.Resolve<IConnection>();
            var aop = ServiceContext.Engine.Resolve<IFastRabbitAop>();
            try
            {
                using (var channe = conn.CreateModel())
                {
                    IBasicProperties property = null;
                    if (config.IsDurable)
                    {
                        property = channe.CreateBasicProperties();
                        property.Persistent = true;
                    }

                    Dictionary<string, object> arguments = null;
                    if (config.MaxPriority != null)
                        arguments = new Dictionary<string, object>
                        {
                            { "x-max-priority", config.MaxPriority.Value }
                        };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(content, jsonOption));
                    channe.QueueDeclare(config.QueueName, config.IsDurable, config.IsExclusive, config.IsAutoDelete, arguments);
                    if (config.Exchange == null)
                        channe.BasicPublish("", config.QueueName, property, body);
                    else
                    {
                        channe.ExchangeDeclare(config.Exchange.ExchangeName, config.Exchange.ExchangeType.ToString(), config.IsDurable, config.IsAutoDelete, arguments);
                        channe.QueueBind(config.QueueName, config.Exchange.ExchangeName, config.Exchange.RouteKey);
                        channe.BasicPublish(config.Exchange.ExchangeName, config.Exchange.RouteKey, property, body);
                    }

                    var send = new SendContext();
                    send.config = config;
                    send.content = content;
                    aop.Send(send);
                }
            }
            catch (Exception ex)
            {
                if (logAop != null)
                {
                    var mqContent = new MqExceptionContext();
                    mqContent.Exception = ex;
                    mqContent.Content = content;
                    mqContent.QueueName = config.QueueName;
                    logAop.MqException(mqContent);
                }
                var context = new ExceptionContext();
                context.content = content;
                context.ex = ex;
                context.isSend = true;
                context.config = config;
                aop.Exception(context);
            }
        }

        internal static Dictionary<string, object> ToDic(string content)
        {
            var jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            var dic = new Dictionary<string, object>();

            if (string.IsNullOrEmpty(content))
                return dic;

            using (var document = JsonDocument.Parse(content))
            {
                foreach (var element in document.RootElement.EnumerateObject())
                {
                    dic.Add(element.Name, element.Value.GetRawText());
                }
            }
            return dic;
        }
    }
}

namespace System.Collections.Generic
{
    internal static class Dic
    {
        public static Object GetValue(this Dictionary<string, object> item, string key)
        {
            if (string.IsNullOrEmpty(key))
                return "";

            if (item == null)
                return "";

            key = item.Keys.ToList().Find(a => string.Compare(a, key, true) == 0);

            if (string.IsNullOrEmpty(key))
                return "";
            else
                return item[key];
        }
    }
}