using FastLog.Core.RabbitMQ.Model;
using System.Collections.Generic;

namespace FastLog.Core.RabbitMQ.Context
{
    internal class ReceiveContext
    {
        public Dictionary<string, object> content { get; internal set; }

        public ConfigModel config { get; set; }
    }
}
