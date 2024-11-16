using FastLog.Core.RabbitMQ.Model;
using System;
using System.Collections.Generic;

namespace FastLog.Core.RabbitMQ.Aop
{
    internal class ExceptionContext
    {
        public Dictionary<string, object> content { get; internal set; }

        public bool isSend { get; set; }

        public bool isReceive { get; set; }

        public bool isDelete { get; set; }

        public ConfigModel config { get; set; }

        public Exception ex { get; internal set; }
    }
}
