using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;


public class Grid : Subject
{
    private int _width;
    private int _height;
    private static float _cellSize;
    private NodeState[,] _grid;
    private bool _isEditable;
    private ISet<(int, int)> _goals;
    private IList<ICharacter> _ghosts;
    private IList<ICharacter> _players;

    public const int RECURSIVE_LEVEL_PLAYER = 5;
    public const int RECURSIVE_LEVEL_GHOST = 3;
    private int _maxSteps = 100;

    private bool _setCharacter;
    private bool _setWall;
    private bool _setGhost;
    private bool _setChest;

    private IDictionary<NodePair, int> distances;


    [SerializeField] private GameObject _walkableSprite;
    [SerializeField] private GameObject _blockedSprite;
    [SerializeField] private GameObject _goalSprite;
    [SerializeField] private GameObject _characterPrefab;
    [SerializeField] private GameObject _ghostPrefab;

    private void Start()
    {
        _goals = new HashSet<(int, int)>();
        _ghosts = new List<ICharacter>();
        _players = new List<ICharacter>();
        _width = 5;
        _height = 5;
        _cellSize = 1f;
        _isEditable = true;
        FillGrid();
        BuildGrid();
    }

    public void SetEditableFlag(bool isEditable)
    {
        _isEditable = isEditable;
    }

    private void Update()
    {
        if (Input.GetMouseButton((int) MouseButton.LeftMouse) && _isEditable)
        {
            // 

            if (_setCharacter)
            {
                (int, int) node = GetGridPositon(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                addPlayer(node);
            }
            else if (_setGhost)
            {
                (int, int) node = GetGridPositon(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                addGhost(node);
            }
            else if (_setWall)
            {
                SetValue(Camera.main.ScreenToWorldPoint(Input.mousePosition), NodeState.BLOCKED);
            }

            else if (_setChest)
            {
                SetValue(Camera.main.ScreenToWorldPoint(Input.mousePosition), NodeState.GOAL);
            }
        }
    }

    
    

    public void Run()
    {
        _players = _players.Where(x => !((Player) x).IsKilled).ToList();
        int steps = 0;
        if (_isEditable || distances == null)
        {
            distances = new Dictionary<NodePair, int>();
        }

        while (!IsGameOver() && steps < _maxSteps)
        {
            steps++;
            IDictionary<(int, int), int> playerWeight = GenerateWeightsForPlayer();
            foreach (ICharacter player in _players.Where(x => !((Player) x).IsKilled).ToList())
            {
                player.NextStep(RECURSIVE_LEVEL_PLAYER, this, playerWeight);
            }

            IDictionary<(int, int), int> ghostWeight = GenerateWeightsForGhost();

            foreach (ICharacter ghost in _ghosts)
            {
                ghost.NextStep(RECURSIVE_LEVEL_GHOST, this, ghostWeight);
                foreach (Player player in _players.Where(x => !((Player) x).IsKilled).ToList())
                {
                    if (player.GetCurrentNode() == ghost.GetCurrentNode())
                    {
                        player.IsKilled = true;
                    }
                }
            }
        }

        foreach (ICharacter player in _players)
        {
            player.Go(this);
        }

        foreach (ICharacter ghost in _ghosts)
        {
            ghost.Go(this);
        }
    }


    public void SetWidth(float width)
    {
        _width = (int) width;
        ResizeGrid();
        BuildGrid();
    }

    public void SetMaxSteps(float width)
    {
        _maxSteps = (int) width;
    }

    public void SetHeight(float height)
    {
        _height = (int) height;
        ResizeGrid();
        BuildGrid();
    }

    public void EatGoalSpite((int, int) node)
    {
        SetValue(node, NodeState.WALKABLE);
    }

    public bool IsWalkable((int, int) pos)
    {
        return _grid[pos.Item1, pos.Item2] == NodeState.WALKABLE || _grid[pos.Item1, pos.Item2] == NodeState.GOAL;
    }

    public bool IsBlocked((int, int) pos)
    {
        return _grid[pos.Item1, pos.Item2] == NodeState.BLOCKED;
    }
    
    public bool IsBlocked((int, int) pos , int entity)
    {
        if (entity == 1)
        {
            return _grid[pos.Item1, pos.Item2] == NodeState.BLOCKED || _players.Count(x => x.GetCurrentNode() == pos) != 0;
        }
        else
        {
            return _grid[pos.Item1, pos.Item2] == NodeState.BLOCKED || _ghosts.Count(x => x.GetCurrentNode() == pos) != 0;
        }
        
    }

    public bool SetCharacter
    {
        get => _setCharacter;
        set => _setCharacter = value;
    }

    public bool SetWall
    {
        get => _setWall;
        set => _setWall = value;
    }

    public bool SetGhost
    {
        get => _setGhost;
        set => _setGhost = value;
    }

    public bool SetChest
    {
        get => _setChest;
        set => _setChest = value;
    }

    public bool IsGoal((int, int) pos)
    {
        return _grid[pos.Item1, pos.Item2] == NodeState.GOAL;
    }

    public List<(int, int)> GetNeighbours((int, int) pos)
    {
        List<(int, int)> neighbours = new List<(int, int)>();
        int x = pos.Item1;
        int y = pos.Item2;

        if ((y + 1 < _height && y + 1 >= 0) && (x < _width && x >= 0))
        {
            neighbours.Add((x, y + 1));
        }

        if ((y - 1 < _height && y - 1 >= 0) && (x < _width && x >= 0))
        {
            neighbours.Add((x, y - 1));
        }

        if ((y < _height && y >= 0) && (x - 1 < _width && x - 1 >= 0))
        {
            neighbours.Add((x - 1, y));
        }

        if ((y < _height && y >= 0) && (x + 1 < _width && x + 1 >= 0))
        {
            neighbours.Add((x + 1, y));
        }

        return neighbours;
    }

    public void BuildGrid()
    {
        Notify(null, NotificationType.DESTROY_ALL);
        for (int x = 0; x < _grid.GetLength(0); x++)
        {
            for (int y = 0; y < _grid.GetLength(1); y++)
            {
                DrawTile(x, y);
            }
        }

        Debug.DrawLine(GetWorldPosition(0, _height), GetWorldPosition(_width, _height), Color.white, 100f, false);
        Debug.DrawLine(GetWorldPosition(_width, 0), GetWorldPosition(_width, _height), Color.white, 100f, false);
    }

    public IDictionary<(int, int), int> GenerateWeightsForPlayer()
    {
        int max = _grid.GetLength(0) * _grid.GetLength(1);
        IDictionary<(int, int), int> weights = new Dictionary<(int, int), int>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (IsWalkable((x, y)))
                    weights.Add((x, y), 0);
            }
        }

        foreach ((int, int ) goal in _goals)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (IsWalkable((x, y)))
                    {
                        NodePair nodePair = new NodePair((x, y), goal);
                        int dist = 0;
                        if (distances.ContainsKey(nodePair))
                        {
                            dist = distances[nodePair];
                        }
                        else
                        {
                            dist = BFS.GetDistance(goal, (x, y), this);
                            distances.Add(nodePair, dist);
                        }

                        if (dist == -1)
                            weights[(x, y)] = 0;
                        else
                            weights[(x, y)] += max - dist;
                    }
                }
            }
        }

        foreach (Ghost ghost in _ghosts)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (IsWalkable((x, y)))
                    {
                        NodePair nodePair = new NodePair((x, y), ghost.GetCurrentNode());
                        int dist = 0;
                        if (distances.ContainsKey(nodePair))
                        {
                            dist = distances[nodePair];
                        }
                        else
                        {
                            dist = BFS.GetDistance(ghost.GetCurrentNode(), (x, y), this);
                            distances.Add(nodePair, dist);
                        }

                        if (dist == -1)
                            weights[(x, y)] = 0;
                        else
                            weights[(x, y)] += dist - max;
                    }
                }
            }
        }

        return weights;
    }

    public IDictionary<(int, int), int> GenerateWeightsForGhost()
    {
        int max = _grid.GetLength(0) * _grid.GetLength(1);
        IDictionary<(int, int), int> weights = new Dictionary<(int, int), int>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                weights.Add((x, y), 0);
            }
        }

        foreach (Player player in _players.Where(x => !((Player) x).IsKilled).ToList())
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (IsWalkable((x, y)))
                    {
                        NodePair nodePair = new NodePair((x, y), player.GetCurrentNode());
                        int dist = 0;
                        if (distances.ContainsKey(nodePair))
                        {
                            dist = distances[nodePair];
                        }
                        else
                        {
                            dist = BFS.GetDistance(player.GetCurrentNode(), (x, y), this);
                            distances.Add(nodePair, dist);
                        }

                        if (dist == -1)
                            weights[(x, y)] = 0;
                        else
                            weights[(x, y)] += max - dist;
                    }
                }
            }
        }

        return weights;
    }

    public void FillGrid()
    {
        _grid = new NodeState[_width, _height];
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _grid[x, y] = NodeState.WALKABLE;
            }
        }
    }

    public List<(int, int)> GetWalkableNodes()
    {
        List<(int, int)> walkable = new List<(int, int)>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (IsWalkable((x, y)))
                {
                    walkable.Add((x, y));
                }
            }
        }

        return walkable;
    }

    public bool InGridRange((int, int) node)
    {
        return node.Item1 < _width && node.Item1 >= 0 && node.Item2 < _height && node.Item2 >= 0 &&
               IsWalkable(node);
    }

    public static IList<Vector3> ConvertPathFromNodeToVector(IList<(int, int)> path)
    {
        IList<Vector3> res = new Collection<Vector3>();
        foreach (var node in path)
        {
            res.Add(GetWorldPosition(node.Item1, node.Item2));
        }

        return res;
    }

    public static Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * _cellSize;
    }

    public static (int, int) GetGridPositon(Vector3 pos)
    {
        return ((int, int)) (pos.x / _cellSize, pos.y / _cellSize);
    }

    public bool EatGoal((int, int) node)
    {
        if (IsGoal(node))
        {
            return _goals.Remove(node);
        }

        return false;
    }

    #region private

    private void addPlayer((int, int) node)
    {
        if (!InGridRange(node))
            return;
        bool exist = false;

        foreach (Player player in _players)
        {
            exist = (player.GetCurrentNode() == node);
        }

        if (!exist)
            _players.Add(new Player(Instantiate(_characterPrefab), node));
    }

    private void addGhost((int, int) node)
    {
        if (!InGridRange(node))
            return;
        bool exist = false;

        foreach (Ghost ghost in _ghosts)
        {
            exist = (ghost.GetCurrentNode() == node);
        }

        if (!exist)
            _ghosts.Add(new Ghost(Instantiate(_ghostPrefab), node));
    }

    private bool IsGameOver()
    {
        return _players.Where(x => !((Player) x).IsKilled).ToList().Count == 0 || _goals.Count == 0;
    }

    private void SetValue(Vector2 worldPos, NodeState nodeState)
    {
        (int, int) gridPos = GetGridPositon(worldPos);
        int x = gridPos.Item1;
        int y = gridPos.Item2;
        if (x >= 0 && x < _width && y >= 0 && y < _height)
        {
            if (nodeState == NodeState.GOAL)
            {
                _goals.Add((x, y));
            }

            _grid[x, y] = nodeState;
            DrawTile(x, y);
        }
    }

    private void SetValue((int, int) node, NodeState nodeState)
    {
        int x = node.Item1;
        int y = node.Item2;
        if (x >= 0 && x < _width && y >= 0 && y < _height)
        {
            if (nodeState == NodeState.GOAL)
            {
                _goals.Add((x, y));
            }

            _grid[x, y] = nodeState;
            DrawTile(x, y);
        }
    }

    private void DrawTile(int x, int y)
    {
        Notify((x, y), NotificationType.DESTROY_ONE);
        GameObject tile = null;
        switch (_grid[x, y])
        {
            case NodeState.WALKABLE:
                tile = (GameObject) Instantiate(_walkableSprite, transform);
                break;
            case NodeState.BLOCKED:
                tile = (GameObject) Instantiate(_blockedSprite, transform);
                break;
            case NodeState.GOAL:
                tile = (GameObject) Instantiate(_goalSprite, transform);
                break;
        }

        if (tile != null)
        {
            tile.GetComponent<TileDestroyer>().SetPositon((x, y));
            tile.GetComponent<TileDestroyer>().SetSubject(this);
            tile.transform.position = new Vector2(x * _cellSize, y * _cellSize);
        }

        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f, false);
        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f, false);
    }

    private void DrawTile(int x, int y, GameObject sprite)
    {
        Notify((x, y), NotificationType.DESTROY_ONE);
        GameObject tile = tile = (GameObject) Instantiate(sprite, transform);
        tile.GetComponent<TileDestroyer>().SetPositon((x, y));
        tile.GetComponent<TileDestroyer>().SetSubject(this);
        tile.transform.position = new Vector2(x * _cellSize, y * _cellSize);
    }

    private void Reduce()
    {
        NodeState[,] temp = new NodeState[_width, _height];
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                temp[x, y] = _grid[x, y];
            }
        }

        _grid = temp;
    }

    private void ResizeGrid()
    {
        if (_width * _height > _grid.GetLength(0) * _grid.GetLength(1))
        {
            Enlarge();
        }
        else
        {
            Reduce();
        }
    }

    private void Enlarge()
    {
        NodeState[,] temp = new NodeState[_width, _height];
        for (int x = 0; x < _grid.GetLength(0); x++)
        {
            for (int y = 0; y < _grid.GetLength(1); y++)
            {
                temp[x, y] = _grid[x, y];
            }
        }

        for (int x = _grid.GetLength(0); x < _width; x++)
        {
            for (int y = _grid.GetLength(1); y < _height; y++)
            {
                temp[x, y] = NodeState.WALKABLE;
            }
        }

        _grid = temp;
    }

    #endregion

    public class NodePair
    {
        private (int, int) _a;
        private (int, int) _b;


        public NodePair((int, int) a, (int, int) b)
        {
            _a = a;
            _b = b;
        }

        private sealed class ABEqualityComparer : IEqualityComparer<NodePair>
        {
            public bool Equals(NodePair x, NodePair y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return (x._a.Equals(y._a) && x._b.Equals(y._b)) || (x._a.Equals(y._b) && x._b.Equals(y._a));
            }

            public int GetHashCode(NodePair obj)
            {
                unchecked
                {
                    return (obj._a.GetHashCode() * 397) ^ obj._b.GetHashCode();
                }
            }
        }

        public static IEqualityComparer<NodePair> ABComparer { get; } = new ABEqualityComparer();
    }

    public enum NodeState
    {
        WALKABLE,
        BLOCKED,
        GOAL
    }
}