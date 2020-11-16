using System.Collections.Generic;

public class BFS
{
    public static int GetDistance((int, int) from, (int, int) to, Grid grid)
    {
        if (from == to)
        {
            return 0;
        }
        IDictionary<(int, int), (int, int)> nodeParents = new Dictionary<(int, int), (int, int)>();
        Queue<(int, int)> queue = new Queue<(int, int)>();
        HashSet<(int, int)> exploredNodes = new HashSet<(int, int)>();
        queue.Enqueue(from);

        while (queue.Count != 0)
        {
            (int, int) currentNode = queue.Dequeue();
            if (currentNode == to)
            {
                return GetPath(nodeParents, from, to).Count;
            }

            IList<(int, int)> nodes = grid.GetNeighbours(currentNode);

            foreach ((int, int) node in nodes)
            {
                if (!exploredNodes.Contains(node) && grid.IsWalkable(node))
                {
                    exploredNodes.Add(node);
                    nodeParents.Add(node, currentNode);
                    queue.Enqueue(node);
                }
            }
        }

        return -1;
    }

    private static IList<(int, int)> GetPath(IDictionary<(int, int), (int, int)> nodeParents, (int, int) from,
        (int, int) to)
    {
        IList<(int, int)> path = new List<(int, int)>();
        (int, int) curr = to;
        while (curr != from)
        {
            path.Add(curr);
            curr = nodeParents[curr];
        }

        path.Add(from);
        return path;
    }
}