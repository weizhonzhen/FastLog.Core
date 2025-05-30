﻿using Elasticsearch.Net;
using FastLog.Core;
using FastLog.Core.Aop;
using FastLog.Core.Elasticsearch;
using FastLog.Core.RabbitMQ.Aop;
using FastLog.Core.RabbitMQ.Context;
using FastLog.Core.RabbitMQ.Model;
using FastRabbitMQ.Core.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FastLogExtension
    {
        private static string mqKey = "RabbitMQ";
        private static string esKey = "Elasticsearch";
        private static string receiveQueueName = "LogReceive";

        internal static ConfigModel config = new ConfigModel
        {
            QueueName = "Log",
            IsAutoAsk = true,
            IsDurable = true,
            Exchange = new Exchange { ExchangeType = FastLog.Core.RabbitMQ.Model.ExchangeType.direct }
        };

        public static IServiceCollection AddFastLog(this IServiceCollection serviceCollection, Action<ConfigDataES> actionES, Action<ConfigDataMQ> actionMQ, IFastLogAop fastLogAop = null)
        {
            var configES = new ConfigDataES();
            var configMQ = new ConfigDataMQ();

            actionES(configES);
            actionMQ(configMQ);

            Init(serviceCollection, configES, configMQ);

            return serviceCollection;
        }

        public static IServiceCollection AddFastLog(this IServiceCollection serviceCollection, string dbFile = "db.json", IFastLogAop fastLogAop = null)
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());
            build.AddJsonFile(dbFile, optional: true, reloadOnChange: true);
            var configES = new ServiceCollection().AddOptions().Configure<ConfigDataES>(build.Build().GetSection(esKey)).BuildServiceProvider().GetService<IOptions<ConfigDataES>>().Value;
            var configMQ = new ServiceCollection().AddOptions().Configure<ConfigDataMQ>(build.Build().GetSection(mqKey)).BuildServiceProvider().GetService<IOptions<ConfigDataMQ>>().Value;

            if (configMQ.Host == null || configES.Host == null)
                throw new Exception(@"services.AddFastLog(a => {  })");

            Init(serviceCollection, configES, configMQ, false, fastLogAop);

            return serviceCollection;
        }

        public static IServiceCollection AddFastLogReceive(this IServiceCollection serviceCollection, string dbFile = "db.json", IFastLogAop fastLogAop = null)
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());
            build.AddJsonFile(dbFile, optional: true, reloadOnChange: true);
            var configES = new ServiceCollection().AddOptions().Configure<ConfigDataES>(build.Build().GetSection(esKey)).BuildServiceProvider().GetService<IOptions<ConfigDataES>>().Value;
            var configMQ = new ServiceCollection().AddOptions().Configure<ConfigDataMQ>(build.Build().GetSection(mqKey)).BuildServiceProvider().GetService<IOptions<ConfigDataMQ>>().Value;

            if (configMQ.Host == null || configES.Host == null)
                throw new Exception(@"services.AddFastLog(a => {  })");

            Init(serviceCollection, configES, configMQ, true, fastLogAop);

            return serviceCollection;
        }

        public static IServiceCollection AddFastLogReceive(this IServiceCollection serviceCollection, Action<ConfigDataES> actionES, Action<ConfigDataMQ> actionMQ, IFastLogAop fastLogAop = null)
        {
            var configES = new ConfigDataES();
            var configMQ = new ConfigDataMQ();

            actionES(configES);
            actionMQ(configMQ);

            Init(serviceCollection, configES, configMQ, true, fastLogAop);

            return serviceCollection;
        }

        private static void Init(IServiceCollection serviceCollection, ConfigDataES configES, ConfigDataMQ configMQ, bool isReceive = false, IFastLogAop fastLogAop = null)
        {
            //ES
            var node = new List<Node>();
            configES.Host.ForEach(a => { node.Add(new Node(new Uri(a))); });
            var pool = new StaticConnectionPool(node);
            var conn = new ConnectionConfiguration(pool).EnableHttpCompression().ServerCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true);
            conn.BasicAuthentication(configES.UserName, configES.PassWord);
            var client = new ElasticLowLevelClient(conn);

            serviceCollection.AddSingleton(client);

            serviceCollection.AddScoped<IElasticsearch, FastLog.Core.Elasticsearch.Elasticsearch>();

            //RabbitMQ
            config.QueueName = configMQ.QueueName;
            config.Exchange.RouteKey = configMQ.RouteKey;
            config.Exchange.ExchangeName = configMQ.ExchangeName;

            IConnectionFactory factory = new ConnectionFactory
            {
                HostName = configMQ.Host,
                Port = configMQ.Port,
                UserName = configMQ.UserName,
                Password = configMQ.PassWord,
                VirtualHost = configMQ.VirtualHost,
                AutomaticRecoveryEnabled = true
            };

            var mqConn = factory.CreateConnection();
            serviceCollection.AddSingleton<RabbitMQ.Client.IConnection>(mqConn);

            //MQ Receive           
            var mqRabbit = new FastRabbit();
            var mqAop = new FastRabbitAop();

            //RabbitMQ send by receive aop
            if (fastLogAop != null && isReceive == false)
            {
                var receiveConfig = new ConfigModel()
                {
                    QueueName = receiveQueueName,
                    IsAutoAsk = true,
                    IsDurable = true,
                    Exchange = new Exchange
                    {
                        ExchangeType = FastLog.Core.RabbitMQ.Model.ExchangeType.direct,
                        RouteKey = configMQ.RouteKey,
                        ExchangeName = configMQ.ExchangeName
                    }
                };

                MqReceive(mqConn, mqAop, receiveConfig, fastLogAop);
            }

            if (isReceive)
                MqReceive(mqConn, mqAop, config, fastLogAop, true);

            serviceCollection.AddSingleton<IFastRabbitAop>(mqAop);
            serviceCollection.AddSingleton<IFastRabbit>(mqRabbit);
            serviceCollection.AddSingleton<IFastLog, FastLog.Core.FastLog>();

            if (fastLogAop != null)
                serviceCollection.AddSingleton<IFastLogAop>(fastLogAop);

            ServiceContext.Init(new ServiceEngine(serviceCollection.BuildServiceProvider()));
        }

        private static void MqReceive(RabbitMQ.Client.IConnection mqConn, FastRabbitAop mqAop, ConfigModel config, IFastLogAop fastLogAop = null, bool isSendReceive = false)
        {
            var content = new Dictionary<string, object>();
            var channe = mqConn.CreateModel();
            Dictionary<string, object> arguments = null;

            if (config.MaxPriority != null)
                arguments = new Dictionary<string, object>
                        {
                            { "x-max-priority", config.MaxPriority.Value }
                        };

            channe.QueueDeclare(config.QueueName, config.IsDurable, config.IsExclusive, config.IsAutoDelete, arguments);
            channe.ExchangeDeclare(config.Exchange.ExchangeName, config.Exchange.ExchangeType.ToString(), config.IsDurable, config.IsAutoDelete);
            channe.QueueBind(config.QueueName, config.Exchange.ExchangeName, config.Exchange.RouteKey);

            if (!config.IsAutoAsk)
                channe.BasicQos(0, 1, true);
            var consumer = new EventingBasicConsumer(channe);
            consumer.Received += (a, b) =>
            {
                content = FastRabbit.ToDic(Encoding.UTF8.GetString(b.Body.ToArray()));

                var receive = new ReceiveContext();
                receive.config = config;
                receive.content = content;

                if (isSendReceive && fastLogAop != null)
                    fastLogAop.MqReceive(new MqReceiveContext { Content = content, QueueName = config.QueueName });

                mqAop.Receive(receive);

                if (!config.IsAutoAsk)
                    channe.BasicAck(b.DeliveryTag, false);
            };
            channe.BasicConsume(config.QueueName, config.IsAutoAsk, consumer);
        }
    }
}