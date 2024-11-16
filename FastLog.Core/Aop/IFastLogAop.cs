namespace FastLog.Core.Aop
{
    public interface IFastLogAop
    {
        void MqReceive(MqReceiveContext context);

        void MqException(MqExceptionContext context);

        void EsAdd(EsAddContext context);

        void EsRemove(EsRemoveContext context);
    }
}