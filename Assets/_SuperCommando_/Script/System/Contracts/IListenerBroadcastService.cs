using System;

public interface IListenerBroadcastService
{
    void Notify(Action<IListener> notifyAction);
}
