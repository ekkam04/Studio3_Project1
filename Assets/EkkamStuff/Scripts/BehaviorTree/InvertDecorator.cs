using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertDecorator : Node
{
    private Node child;

    public InvertDecorator(Node child)
    {
        this.child = child;
    }

    public override NodeState Evaluate()
    {
        var childState = child.Evaluate();
        if (childState == NodeState.Success) {
            state = NodeState.Failure;
        } else if (childState == NodeState.Failure) {
            state = NodeState.Success;
        } else {
            state = NodeState.Running;
        }
        return state;
    }
}
