using FastLog.Core.RabbitMQ.Model;
using System.Collections.Generic;

namespace FastRabbitMQ.Core.Repository
{
    internal interface IFastRabbit
    {
        void Send(ConfigModel config, Dictionary<string, object> content);

        void Delete(ConfigModel config);
    }
}
