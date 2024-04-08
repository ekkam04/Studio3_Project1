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
            Power,
            Destroy,
            Reach,
            Sequence,
            DamageAnyEnemy,
        }
        public ObjectiveType objectiveType;
    
        public GameObject objectiveWaypoint;
        public GameObject[] objectiveTargets;
    
        public Objective[] objectiveSequence;
        public bool objectiveMessedUp;
        
        public Objective[] objectivesThatFailThisObjective;
    
        public GameObject objectiveUIItem;
        
        public Signalable[] completionSignals;
        public string completionActionKey;
        
        public bool doNotAssignNextObjectiveOnCompletion;
        
        public enum ObjectiveStatus
        {
            Inactive,
            Active,
            Completed,
            KindaCompleted,
            Failed
        }
        public ObjectiveStatus status;
    }
}
