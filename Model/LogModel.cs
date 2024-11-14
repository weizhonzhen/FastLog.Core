using System;

namespace FastLog.Core.Model
{
    public class LogModel
    {
        /// <summary>
        /// index
        /// </summary>
        public string Type { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string Person { get; set; }

        public DateTime DateTime { get; set; }

        public string Ip { get; set; }

        public string Id { get; internal set; } = Guid.NewGuid().ToString();
    }
}