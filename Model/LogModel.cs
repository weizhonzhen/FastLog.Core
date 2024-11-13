using System;

namespace FastLog.Core.Model
{
    internal class LogModel
    {
        /// <summary>
        /// index
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
                
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// ip
        /// </summary>
        public string Ip { get; set; }

        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}