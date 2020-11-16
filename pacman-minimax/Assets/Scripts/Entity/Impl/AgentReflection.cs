using System.Collections.Generic;
using UnityEngine;

public class AgentReflection : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _pause;
    private float _usePause;
    private bool _isGoing;
    private IList<Vector3> _path;
    private Grid _grid;
    private int _current;
    private bool _isPlayer;
    private bool _isKilled;

    private GameObject _me;

    void FixedUpdate()
    {
        if (_isGoing)
        {
            if (_usePause <= 0)
            {
                transform.position =
                    Vector2.MoveTowards(transform.position, _path[_current], _speed * Time.fixedDeltaTime);
                if (Vector2.Distance(transform.position, _path[_current]) == 0)
                {
                    if (_isPlayer)
                    {
                        _grid.EatGoalSpite(Grid.GetGridPositon(_path[_current]));
                    }

                    _current--;
                    _usePause = _pause;
                    if (_current < 0)
                    {
                        _isGoing = false;
                        if (_isPlayer && _isKilled)
                        {
                            Destroy(_me);
                        }
                    }
                }
            }
            else
            {
                _usePause -= Time.fixedDeltaTime;
            }
        }
    }

    public float Speed
    {
        get => _speed;
        set => _speed = value;
    }

    public void SetStartPosition(Vector3 vector3)
    {
        transform.position = vector3;
    }

    public GameObject Me
    {
        get => _me;
        set => _me = value;
    }

    public void Go(IList<Vector3> path, Grid grid, bool isPlayer, bool isKilled)
    {
        _grid = grid;
        _current = path.Count - 1;
        _isPlayer = isPlayer;
        _isKilled = isKilled;
        _path = path;
        _isGoing = true;
    }
}