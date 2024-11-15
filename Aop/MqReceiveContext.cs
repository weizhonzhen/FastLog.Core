using System.Collections.Generic;

namespace FastLog.Core.Aop
{
    public class MqReceiveContext
    {
        public Dictionary<string, object> Content { get; set; }
    }
}
