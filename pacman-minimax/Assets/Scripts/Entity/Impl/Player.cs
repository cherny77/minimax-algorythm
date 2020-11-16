using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : ICharacter
{
    private readonly GameObject _agentReflection;
    private int _score;
    private (int, int) _curNode;
    private IList<(int, int)> _path;
    private bool _isKilled = false;


    public Player(GameObject agentReflection, (int, int) curNode)
    {
        _agentReflection = agentReflection;
        _path = new List<(int, int)>();
        _curNode = curNode;
        _agentReflection.GetComponent<AgentReflection>()
            .SetStartPosition(Grid.GetWorldPosition(_curNode.Item1, _curNode.Item2));
        _agentReflection.GetComponent<AgentReflection>().Me = _agentReflection;
    }


    public (int, int) GetCurrentNode()
    {
        return _curNode;
    }

    public bool IsKilled
    {
        get => _isKilled;
        set => _isKilled = value;
    }

    public void NextStep(int depth, Grid grid, IDictionary<(int, int), int> weights)
    {
        _curNode = MiniMax.Minimax(depth, _curNode, true, grid, weights, 1);
        _path.Add(_curNode);
        if (grid.EatGoal(_curNode)) _score++;
    }

    public void Go(Grid grid)
    {
        _agentReflection.GetComponent<AgentReflection>().Go(Grid.ConvertPathFromNodeToVector(_path).Reverse().ToList(), grid, true, _isKilled);
        _path = new List<(int, int)>();
    }
}