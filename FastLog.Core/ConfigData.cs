using System;
using System.Collections.Generic;

namespace FastLog.Core
{
    public class ConfigDataMQ
    {
        public string Host { get; set; }

        public int Port { get; set; } = 5672;

        public string UserName { get; set; }

        public string PassWord { get; set; }

        public string VirtualHost { get; set; } = "/";

        public string QueueName { get; set; } = "Log";

        public string ExchangeName { get; set; } = "LogBox";

        public string RouteKey { get; set; } = "LogKey";
    }

    public class ConfigDataES
    {
        public List<string> Host { get; set; } = new List<string>();

        public string UserName { get; set; }

        public string PassWord { get; set; }
    }
}
