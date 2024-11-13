using System;

namespace FastLog.Core.Model
{
    internal class LogTypeModel
    {
        public string Name { get; set; }

        /// <summary>
        /// index
        /// </summary>
        public string IdxLogType { get; set; } = "idxlogtype";

        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}
