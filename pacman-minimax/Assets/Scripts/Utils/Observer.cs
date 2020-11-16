
using System.Collections.Generic;
using UnityEngine;


public abstract class Observer : MonoBehaviour
{
    public abstract void OnNotify(object val, NotificationType notificationType);
}

public abstract class Subject : MonoBehaviour
{
    private List<Observer> _observers = new List<Observer>();

    public void RegisterObserver(Observer observer)
    {
        _observers.Add(observer);
    }


    public void Notify( object val, NotificationType notificationType)
    {
        foreach (var observer in _observers)
        {
            observer.OnNotify(val, notificationType);
        }
    }
}

public enum NotificationType
{
    DESTROY_ONE, DESTROY_ALL, GO
}
