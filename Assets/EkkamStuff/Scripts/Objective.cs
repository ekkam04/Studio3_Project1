using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ekkam {
    [System.Serializable]
    public class Objective
    {
        [Header("Objective Settings")]
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
            ObserveCollider
        }
        public ObjectiveType objectiveType;
    
        public GameObject objectiveWaypoint;
        public GameObject[] objectiveTargets;
    
        public Objective[] objectiveSequence;
        public bool objectiveMessedUp;
        
        public Objective[] objectivesThatFailThisObjective;
    
        public GameObject objectiveUIItem;
        
        [Header("Completion Settings")]
        public Signalable[] completionSignals;
        public string completionActionKey;
        public List<Dialog> completionDialogs;
        public List<Dialog> failedDialogs;
        public bool doNotAssignNextObjectiveOnCompletion;
        
        public enum ObjectiveStatus
        {
            Inactive,
            Active,
            Completed,
            KindaCompleted,
            Failed
        }
        [Header("Objective Status")]
        public ObjectiveStatus status;
    }
}
