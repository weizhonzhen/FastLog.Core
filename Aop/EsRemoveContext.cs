using System.Collections.Generic;

namespace FastLog.Core.Aop
{
    public class EsRemoveContext
    {
        public bool IsSuccess { get; set; }
        public Dictionary<string, object> Content { get; set; }
    }
}