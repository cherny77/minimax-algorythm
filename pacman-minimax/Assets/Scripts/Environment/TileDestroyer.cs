
using UnityEngine;

public class TileDestroyer : Observer
{
    private (int, int) _pos;
    [SerializeField] private GameObject _tile;
    // Start is called before the first frame update


    public void SetPositon((int, int) pos)
    {
        _pos = pos;
    }
    public void SetSubject(Subject subject)
    {
        subject.RegisterObserver(this);
    }
    
    public override void OnNotify(object val, NotificationType notificationType)
    {

        switch (notificationType)
        {
            case NotificationType.DESTROY_ALL:
                Destroy(_tile);
                break;
            case NotificationType.DESTROY_ONE :
                
                if (_pos == ((int, int)) val)
                {
                    Destroy(_tile);
                }
                break;

        }
        
    }
}
