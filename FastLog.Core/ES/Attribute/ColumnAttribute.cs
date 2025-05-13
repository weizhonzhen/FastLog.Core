using System;

namespace FastLog.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string type { get; set; }

        public int ignore_above { get; set; }
    }
}
