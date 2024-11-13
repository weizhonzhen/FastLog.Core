using System;

namespace FastLog.Core.Model
{
    internal class LogTypeModel
    {
        /// <summary>
        /// index
        /// </summary>
        internal string Name { get; set; }

        public string LogType { get; } = "logtype";

        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}
