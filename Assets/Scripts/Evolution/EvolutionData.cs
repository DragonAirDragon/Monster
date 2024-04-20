using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EvolutionData : MonoBehaviour
{ 
    public List<EvolutionNode> startingNodes;

    public EvolutionNode GetRandomStartNode()
    {
        return startingNodes[Random.Range(0, startingNodes.Count)];
    }

    public EvolutionNode GetNodeUsingName(string enterName)
    {
        foreach (var node in startingNodes)
        {
            if (node.name == enterName)
                return node;
        }

        return null;
    }
    
}
