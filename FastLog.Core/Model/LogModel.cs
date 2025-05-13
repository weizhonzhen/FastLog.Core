using System;

namespace FastLog.Core.Model
{
    public class LogModel
    {
        /// <summary>
        /// index
        /// </summary>
        public string Type { get; set; }

        [Column(type = "keyword")]
        public string title { get; set; }

        [Column(type = "keyword")]
        public string content { get; set; }

        [Column(type = "keyword")]
        public string person { get; set; }

        [Column(type = "keyword")]
        public DateTime dateTime { get; set; }

        [Column(type = "keyword")]
        public string ip { get; set; }

        [Column(type = "keyword")]
        public string id { get; internal set; } = Guid.NewGuid().ToString();
    }
}