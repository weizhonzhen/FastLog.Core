using FastLog.Core.RabbitMQ.Context;
using System;

namespace FastLog.Core.RabbitMQ.Aop
{
    internal interface IFastRabbitAop
    {
        void Send(SendContext context);

        void Receive(ReceiveContext context);

        void Exception(ExceptionContext context);

        void Delete(DeleteContext context);
    }
}