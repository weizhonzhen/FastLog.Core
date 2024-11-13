using System;

namespace FastLog.Core.RabbitMQ.Model
{
    internal class Exchange
    {
        public string ExchangeName { get; set; }

        public ExchangeType ExchangeType { get; set; } = ExchangeType.direct;

        public string RouteKey { get; set; }
    }
}
