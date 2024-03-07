using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ekkam {
    [System.Serializable]
    public class Objective
    {
        public string objectiveText;
        // public string objectiveDescription;
        public enum ObjectiveType
        {
            Collect,
            Interact,
            Destroy,
            Reach
        }
        public ObjectiveType objectiveType;
        public enum ObjectiveCategory
        {
            ShouldBeCompleted,
            ShouldNotBeCompleted
        }
        public ObjectiveCategory objectiveCategory;
        public GameObject objectiveTarget;
        public GameObject[] objectiveMultiTargets;
        public GameObject objectiveUIItem;
        public bool autoAssignNextObjective;
        public enum ObjectiveStatus
        {
            Inactive,
            Active,
            Completed,
            Failed
        }
        public ObjectiveStatus status;
        // public bool isActive;
        // public bool isCompleted;
    }
}
