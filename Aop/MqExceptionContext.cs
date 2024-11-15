using System;
using System.Collections.Generic;

namespace FastLog.Core.Aop
{
    public class MqExceptionContext
    {
        public Exception Exception { get; set; }

        public Dictionary<string, object> Content { get; set; }
    }
}
