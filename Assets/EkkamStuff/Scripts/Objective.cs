using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ekkam {
    [System.Serializable]
    public class Objective
    {
        public string objectiveText;
        public enum ObjectiveType
        {
            Collect,
            Interact,
            Destroy,
            Reach,
            Sequence,
        }
        public ObjectiveType objectiveType;
    
        public GameObject objectiveWaypoint;
        public GameObject[] objectiveTargets;
    
        public Objective[] objectiveSequence;
        public bool sequenceNotMaintained;
        public Objective[] objectivesThatFailThisObjective;
    
        public GameObject objectiveUIItem;
        
        public Signalable[] completionSignals;
        public enum ObjectiveStatus
        {
            Inactive,
            Active,
            Completed,
            Failed
        }
        public ObjectiveStatus status;
    }
}
