using System.Collections.Generic;

namespace FastLog.Core.Aop
{
    public class MqReceiveContext
    {
        public string QueueName { get; set; }

        public Dictionary<string, object> Content { get; set; }
    }
}
