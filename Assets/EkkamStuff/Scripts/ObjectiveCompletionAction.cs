using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    [System.Serializable]
    public class ObjectiveCompletionAction
    {
        public int objectiveIndexToComplete;
        public enum CompletionAction
        {
            Move,
            Enable,
            Disable,
            Destroy,
        }
        public CompletionAction completionAction;
        public GameObject target;
        public Vector3 targetPositionOffset;
    }
}
