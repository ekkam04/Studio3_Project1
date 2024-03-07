using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : Node
{
    protected List<Node> children = new List<Node>();

    public Sequence(List<Node> children)
    {
        this.children = children;
    }

    public override NodeState Evaluate()
    {
        bool isAnyChildRunning = false;
        foreach (Node node in children)
        {
            switch (node.Evaluate())
            {
                case NodeState.Failure:
                    state = NodeState.Failure;
                    return state;
                case NodeState.Success:
                    continue;
                case NodeState.Running:
                    isAnyChildRunning = true;
                    break;
                default:
                    state = NodeState.Success;
                    return state;
            }
        }

        state = isAnyChildRunning ? NodeState.Running : NodeState.Success;
        return state;
    }
}

