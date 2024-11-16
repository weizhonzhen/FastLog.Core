using System;

namespace FastLog.Core.RabbitMQ.Model
{
    internal enum ExchangeType
    {
        topic = 0,
        direct = 1,
        fanout = 2
    }
}
