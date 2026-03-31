using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class SceneListenerBroadcastService : IListenerBroadcastService
{
    public void Notify(Action<IListener> notifyAction)
    {
        IEnumerable<IListener> listeners = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().OfType<IListener>();
        foreach (IListener listener in listeners)
            notifyAction(listener);
    }
}
