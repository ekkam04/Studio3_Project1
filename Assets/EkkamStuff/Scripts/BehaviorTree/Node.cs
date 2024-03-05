using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Node
{
    public enum NodeState { Running, Success, Failure }
    protected NodeState state;

    public NodeState State { get { return state; } }

    public abstract NodeState Evaluate();
}
