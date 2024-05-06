using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using UnityEngine.Rendering.Universal;
using QFSW.QC;

namespace Ekkam {
    public class ObjectiveManager : MonoBehaviour
    {
        private GameManager gameManager;
        public int currentObjectiveIndex = 0;
        private float objectiveWaypointDistance = 1.5f;
        
        // public int objectivesCompleted = 0;
        // public int objectivesActive = 0;

        public Color objectiveCompletedColor = Color.green;
        public Color objectiveFailedColor = Color.red;

        public List<Objective> objectives = new List<Objective>();
        // public List<int> activeObjectiveIndices = new List<int>();

        public List<ObjectiveCompletionAction> objectiveCompletionActions = new List<ObjectiveCompletionAction>();

        [SerializeField] GameObject objectiveUI;
        [SerializeField] GameObject objectiveItem;
        Player player;
        public bool playerDamagedEnemyCheck = false;
        public bool playerObservedColliderCheck = false;
        private bool mousePosition3DObserverCheck = false;
        
        public delegate void OnObjectiveComplete(string completionActionKey);
        public static event OnObjectiveComplete onObjectiveComplete;
               
        void Start()
        {
            player = FindObjectOfType<Player>();
            gameManager = FindObjectOfType<GameManager>();
            
            HideAllObjectiveMarkers();
        }
        
        public void InitializeFromCurrentIndex()
        {
            SkipTasks(currentObjectiveIndex);
            
            if (objectives.Count > 0)
            {
                AddNextObjective();
            }
        }

        void Update()
        {
            foreach (Objective objective in objectives)
            {
                if (objective.status != Objective.ObjectiveStatus.Active) continue;
                
                foreach (Objective objectivesThatFailThisObjective in objective.objectivesThatFailThisObjective)
                {
                    CheckObjectiveCompletion(objectivesThatFailThisObjective);
                    if (objectivesThatFailThisObjective.status != Objective.ObjectiveStatus.Active)
                    {
                        // CompleteObjective(objective, false);
                        objective.objectiveMessedUp = true;
                        // return;
                    }
                }
                
                if (objective.objectiveType == Objective.ObjectiveType.Sequence)
                {
                    foreach (Objective sequenceObjective in objective.objectiveSequence)
                    {
                        if (sequenceObjective.status != Objective.ObjectiveStatus.Active) continue;
                        CheckObjectiveCompletion(sequenceObjective, true, objective);
                    }
                }
                
                CheckObjectiveCompletion(objective);
                
            }

        }
        
        void CheckObjectiveCompletion(Objective objective, bool isSequential = false, Objective parentObjective = null)
        {
            var distance = Mathf.Infinity;
            if (objective.objectiveWaypoint != null)
            {
                distance = Vector3.Distance(player.transform.position, objective.objectiveWaypoint.transform.position);
                if (distance < objectiveWaypointDistance)
                {
                    objective.objectiveWaypoint.SetActive(false);
                }
                else
                {
                    objective.objectiveWaypoint.SetActive(true);
                }
            }

            if (objective.objectiveType == Objective.ObjectiveType.Reach) // if objective is of type Reach ------------------------------------------
            {
                if (distance < objectiveWaypointDistance)
                {
                    CompleteObjective(objective, !objective.objectiveMessedUp);
                    CheckSequence(isSequential, objective, parentObjective);
                }
            }
            else if (objective.objectiveType == Objective.ObjectiveType.Interact) // if objective is of type Interact -------------------------------
            {
                foreach (GameObject target in objective.objectiveTargets)
                {
                    if (target.GetComponent<Interactable>() != null && target.GetComponent<Interactable>().timesInteracted > 0)
                    {
                        CompleteObjective(objective, !objective.objectiveMessedUp);
                        CheckSequence(isSequential, objective, parentObjective);
                    }
                }
            }
            else if (objective.objectiveType == Objective.ObjectiveType.Power) // if objective is of type Power -------------------------------
            {
                foreach (GameObject target in objective.objectiveTargets)
                {
                    if (target.GetComponent<Wire>() != null && target.GetComponent<Wire>().isPowered)
                    {
                        CompleteObjective(objective, !objective.objectiveMessedUp);
                        CheckSequence(isSequential, objective, parentObjective);
                    }
                }
            }
            else if (objective.objectiveType == Objective.ObjectiveType.Destroy) // if objective is of type Destroy ---------------------------------
            {
                int numberOfTargetsToDestroy = objective.objectiveTargets.Length;
                int numberOfDestroyedTargets = 0;
                foreach (GameObject target in objective.objectiveTargets)
                {
                    if (target == null || target.activeSelf == false)
                    {
                        numberOfDestroyedTargets++;
                    }
                    else if (
                        target.GetComponent<Interactable>() != null
                        && target.GetComponent<Interactable>().interactionAction == Interactable.InteractionAction.DamageCrystal
                        && target.GetComponent<Interactable>().isBroken == true
                    )
                    {
                        numberOfDestroyedTargets++;
                    }
                }

                if (numberOfDestroyedTargets == numberOfTargetsToDestroy)
                {
                    CompleteObjective(objective, !objective.objectiveMessedUp);
                    CheckSequence(isSequential, objective, parentObjective);
                }
            }
            else if (objective.objectiveType == Objective.ObjectiveType.Sequence)
            {
                bool sequenceCompleted = true;
                foreach (Objective sequenceObjective in objective.objectiveSequence)
                {
                    if (sequenceObjective.status == Objective.ObjectiveStatus.Active)
                    {
                        sequenceCompleted = false;
                        break;
                    }
                }
                
                if (sequenceCompleted)
                {
                    CompleteObjective(objective, !objective.objectiveMessedUp);
                    CheckSequence(isSequential, objective, parentObjective);
                }
            }
            else if (objective.objectiveType == Objective.ObjectiveType.DamageAnyEnemy)
            {
                if (playerDamagedEnemyCheck)
                {
                    CompleteObjective(objective, !objective.objectiveMessedUp);
                    CheckSequence(isSequential, objective, parentObjective);
                    playerDamagedEnemyCheck = false;
                }
            }
            else if (objective.objectiveType == Objective.ObjectiveType.ObserveCollider)
            {
                if (!mousePosition3DObserverCheck)
                {
                    FindObjectOfType<MousePosition3D>().colliderToObserve = objective.objectiveTargets[0].GetComponent<Collider>();
                    FindObjectOfType<MousePosition3D>().observeCollider = true;
                    mousePosition3DObserverCheck = true;
                }
                if (playerObservedColliderCheck)
                {
                    CompleteObjective(objective, !objective.objectiveMessedUp);
                    CheckSequence(isSequential, objective, parentObjective);
                    playerObservedColliderCheck = false;
                }
            }
            else if (objective.objectiveType == Objective.ObjectiveType.ReachInCar)
            {
                if (distance < (objectiveWaypointDistance + 1f) && player.isDriving)
                {
                    CompleteObjective(objective, !objective.objectiveMessedUp);
                    CheckSequence(isSequential, objective, parentObjective);
                }
            }
        }

        private void CheckSequence(bool isSequential, Objective objective, Objective parentObjective = null)
        {
            if (isSequential && parentObjective != null)
            {
                int sequenceObjectiveIndex = Array.IndexOf(parentObjective.objectiveSequence, objective);
                int completedSequenceObjectives = 0;
                foreach (Objective sequenceObjective in parentObjective.objectiveSequence)
                {
                    if (sequenceObjective.status != Objective.ObjectiveStatus.Active)
                    {
                        completedSequenceObjectives++;
                    }
                }
                int remainingSequenceObjectives = parentObjective.objectiveSequence.Length - completedSequenceObjectives;
                        
                print("completedSequenceObjectives: " + completedSequenceObjectives);
                if (remainingSequenceObjectives > 0)
                {
                    if (sequenceObjectiveIndex == completedSequenceObjectives - 1)
                    {
                        print("Sequence maintained");
                        parentObjective.objectiveMessedUp = false;
                    }
                    else
                    {
                        print("Sequence not maintained");
                        parentObjective.objectiveMessedUp = true;
                    }
                }
                print("Sequence maintained? " + !parentObjective.objectiveMessedUp);
            }
        }

        public void CompleteObjective(Objective objective, bool wasSuccessful)
        {
            print("Objective completed: " + objective.objectiveText + " - " + wasSuccessful);
            if (wasSuccessful)
            {
                objective.status = Objective.ObjectiveStatus.Completed;
                SoundManager.Instance.PlaySound("objective-success");
            }
            else
            {
                objective.status = Objective.ObjectiveStatus.Failed;
                SoundManager.Instance.PlaySound("objective-fail");
            }
            
            RemoveObjectiveFromUI(objective, wasSuccessful);
            
            foreach (Objective objectivesThatFailThisObjective in objective.objectivesThatFailThisObjective)
            {
                if (objectivesThatFailThisObjective.status == Objective.ObjectiveStatus.Completed) continue;
                objectivesThatFailThisObjective.status = Objective.ObjectiveStatus.Failed;
                RemoveObjectiveFromUI(objectivesThatFailThisObjective, wasSuccessful);
            }
            
            foreach (Signalable signal in objective.completionSignals)
            {
                if (signal == null) continue;
                signal.Signal();
            }
            
            if (onObjectiveComplete != null && objective.completionActionKey != "")
            {
                onObjectiveComplete.Invoke(objective.completionActionKey);
            }
            
            HideAllObjectiveMarkers();
            
            if (wasSuccessful && objective.completionDialogs.Count > 0)
            {
                gameManager.PlayDroneDialog(objective.completionDialogs);
                objective.doNotAssignNextObjectiveOnCompletion = true;
            }
            else if (!wasSuccessful && objective.failedDialogs.Count > 0)
            {
                gameManager.PlayDroneDialog(objective.failedDialogs);
                objective.doNotAssignNextObjectiveOnCompletion = true;
            }
            
            if (!objectives.Contains(objective)) return; // Only main objectives should progress the story
            
            // Determine freewill change if defiance was possible
            if (objective.objectivesThatFailThisObjective.Length > 0 || objective.objectiveSequence.Length > 0)
            {
                if (wasSuccessful)
                {
                    Player.Instance.freeWill -= 10f;
                }
                else
                {
                    Player.Instance.freeWill += 10f;
                }
            }
            
            if (currentObjectiveIndex < objectives.Count - 1)
            {
                currentObjectiveIndex++;
                if (!objective.doNotAssignNextObjectiveOnCompletion)
                {
                    AddNextObjective();
                }
            }
            else
            {
                print("All objectives completed");
            }
        }
        
        public void AddNextObjective()
        {
            playerDamagedEnemyCheck = false;
            playerObservedColliderCheck = false;
            objectives[currentObjectiveIndex].status = Objective.ObjectiveStatus.Active;
            AddObjectiveToUI(objectives[currentObjectiveIndex]);
            foreach (Objective objectivesThatFailThisObjective in objectives[currentObjectiveIndex].objectivesThatFailThisObjective)
            {
                objectivesThatFailThisObjective.status = Objective.ObjectiveStatus.Active;
                objectivesThatFailThisObjective.objectiveMessedUp = true;
                AddObjectiveToUI(objectivesThatFailThisObjective, 40f);
            }
            foreach (Objective sequenceObjective in objectives[currentObjectiveIndex].objectiveSequence)
            {
                sequenceObjective.status = Objective.ObjectiveStatus.Active;
                AddObjectiveToUI(sequenceObjective, 40f);
            }
            SoundManager.Instance.PlaySound("objective-assign");
        }

        int GetNumberOfObjectives(Objective.ObjectiveStatus status)
        {
            int numberOfObjectives = 0;
            foreach (Objective objective in objectives)
            {
                if (objective.status == status)
                {
                    numberOfObjectives++;
                }
            }
            return numberOfObjectives;
        }
        
        [Command("skip-tasks")]
        public async void SkipTasks(int numberOfTasksToSkip)
        {
            int firstActiveObjectiveIndex = 0;
            for (int i = 0; i < objectives.Count; i++)
            {
                if (objectives[i].status == Objective.ObjectiveStatus.Active)
                {
                    firstActiveObjectiveIndex = i;
                    print("First active objective index: " + firstActiveObjectiveIndex);
                    break;
                }
            }
            
            for (int i = firstActiveObjectiveIndex; i < numberOfTasksToSkip; i++)
            {
                // print("Skipping task " + i);
                if (objectives[i].status == Objective.ObjectiveStatus.Active)
                {
                    foreach (Objective objectivesThatFailThisObjective in objectives[i].objectivesThatFailThisObjective)
                    {
                        objectivesThatFailThisObjective.status = Objective.ObjectiveStatus.Failed;
                        RemoveObjectiveFromUI(objectivesThatFailThisObjective, false);
                    }
                    foreach(Objective sequenceObjective in objectives[i].objectiveSequence)
                    {
                        if (sequenceObjective.status == Objective.ObjectiveStatus.Active)
                        {
                            sequenceObjective.status = Objective.ObjectiveStatus.Completed;
                            RemoveObjectiveFromUI(sequenceObjective, true);
                        }
                    }
                    CompleteObjective(objectives[i], true);
                    await Task.Delay(200);
                }
            }
        }

        public void AddObjectiveToUI(Objective objective, float leftOffset = 0f)
        {
            GameObject objectiveUIItem = Instantiate(objectiveItem, objectiveUI.transform);
            
            objectiveUIItem.GetComponentInChildren<TextMeshProUGUI>().text = objective.objectiveText;
            objectiveUIItem.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            
            objectiveUIItem.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(leftOffset, 0);
            
            objective.objectiveUIItem = objectiveUIItem;
            objectiveUIItem.GetComponentInChildren<Animator>().SetBool("Active", false);
        }

        public async void RemoveObjectiveFromUI(Objective objective, bool wasSuccessful)
        {
            objective.objectiveUIItem.GetComponentInChildren<Animator>().SetBool("Active", true);
            if (wasSuccessful)
            {
                objective.objectiveUIItem.GetComponentInChildren<TextMeshProUGUI>().color = objectiveCompletedColor;
            }
            else
            {
                objective.objectiveUIItem.GetComponentInChildren<TextMeshProUGUI>().color = objectiveFailedColor;
            }
            await Task.Delay(2000);
            objective.objectiveUIItem.SetActive(false);
        }

        public void HideAllObjectiveMarkers()
        {
            foreach (Objective objective in objectives)
            {
                if (objective.objectiveWaypoint != null)
                {
                    objective.objectiveWaypoint.gameObject.SetActive(false);
                }

                foreach (Objective sequenceObjective in objective.objectiveSequence)
                {
                    if (sequenceObjective.objectiveWaypoint != null)
                    {
                        sequenceObjective.objectiveWaypoint.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}

