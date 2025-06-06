﻿using System.Collections.Generic;

namespace FastLog.Core.Aop
{
    public class EsAddContext
    {
        public string QueueName { get; set; }

        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public Dictionary<string, object> Content { get; set; }
    }
}
