using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ghost : ICharacter
{
    private readonly GameObject _agentReflection;
    private (int, int) _curNode;
    private IList<(int, int)> _path;

    public Ghost(GameObject agentReflection, (int, int) curNode)
    {
        _agentReflection = agentReflection;
        _curNode = curNode;
        _path = new List<(int, int)>();
        _agentReflection.GetComponent<AgentReflection>()
            .SetStartPosition(Grid.GetWorldPosition(_curNode.Item1, _curNode.Item2));
        _agentReflection.GetComponent<AgentReflection>().Me = _agentReflection;
    }

    public (int, int) GetCurrentNode()
    {
        return _curNode;
    }

    public void NextStep(int depth, Grid grid, IDictionary<(int, int), int> weights)
    {
        _curNode = MiniMax.Minimax(depth, _curNode, true, grid, weights, 2);
        _path.Add(_curNode);
    }

    public void Go(Grid grid)
    {
        _agentReflection.GetComponent<AgentReflection>()
            .Go(Grid.ConvertPathFromNodeToVector(_path).Reverse().ToList(), grid, false, true);
        _path = new List<(int, int)>();
    }
}