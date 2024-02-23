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
        public GameObject objectiveTarget;
        public GameObject[] objectiveMultiTargets;
        public TMP_Text objectiveUIText;
        public bool autoAssignNextObjective;
        public bool isCompleted;
    }
}
