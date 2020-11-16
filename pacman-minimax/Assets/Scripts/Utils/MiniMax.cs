using System;
using System.Collections.Generic;
using System.Linq;

public class MiniMax
{
    private static readonly Random random = new Random();

    public static (int, int) Minimax(int depth, (int, int) node, bool isMax, Grid grid,
        IDictionary<(int, int), int> weights, int entity) 
    {
        if (depth == 0)
            return node;

        IList<(int, int)> freeNodes = grid.GetNeighbours(node).Where(x => !grid.IsBlocked(x)).ToList();
        if (isMax)
        {
            IDictionary<(int, int), int> vals = new Dictionary<(int, int), int>();
            foreach ((int, int) n in freeNodes)
            {
                vals.Add(n, weights[Minimax(depth - 1, n, false, grid, weights, entity)]);
            }

            int maxVal = vals.Values.Max();
            
            IList<(int, int)> possVals = new List<(int, int)>();
            foreach ((int, int) n in freeNodes)
            {
                if (vals[n] == maxVal) possVals.Add(n);
            }

            return GetRandomElement(possVals);
        }
       
        else
        {
            IDictionary<(int, int), int> vals = new Dictionary<(int, int), int>();
            foreach ((int, int) n in freeNodes)
            {
                vals.Add(n, weights[Minimax(depth - 1, n, true, grid, weights, entity)]);
            }

            int minVal = vals.Values.Min();
            IList<(int, int)> possVals = new List<(int, int)>();
            foreach ((int, int) n in freeNodes)
            {
                if (vals[n] == minVal) possVals.Add(n);
            }
            return GetRandomElement(possVals);
        }
    }

    private static (int,int)  GetRandomElement(IList<(int,int)> vals)
    {
        int index = random.Next(vals.Count);
        return vals[index];
    }
}