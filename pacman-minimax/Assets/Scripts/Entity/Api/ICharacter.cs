using System.Collections.Generic;
using UnityEngine;

public interface ICharacter 
{
    (int, int) GetCurrentNode();
    void Go(Grid grid);

    void NextStep(int depth, Grid grid,
        IDictionary<(int, int), int> weights);
}