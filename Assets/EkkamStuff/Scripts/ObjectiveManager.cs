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
        private float objectiveWaypointDistance = 3.5f;
        
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

        public Volume vignetteVolume;
        public Vignette vignette;
               
        void Start()
        {
            player = FindObjectOfType<Player>();
            gameManager = FindObjectOfType<GameManager>();

            // hide all objective targets if type is Reach
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

            if (objectives.Count > 0)
            {
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
            }
            
            SkipTasks(currentObjectiveIndex);
        }

        void Update()
        {
            foreach (Objective objective in objectives)
            {
                if (objective.status != Objective.ObjectiveStatus.Active) continue;
                
                foreach (Objective objectivesThatFailThisObjective in objective.objectivesThatFailThisObjective)
                {
                    CheckObjectiveCompletion(objectivesThatFailThisObjective);
                    // if (objectivesThatFailThisObjective.status != Objective.ObjectiveStatus.Active)
                    // {
                    //     // CompleteObjective(objective, false);
                    //     objective.objectiveMessedUp = true;
                    //     // return;
                    // }
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
            }
            else if (objective.objectiveType == Objective.ObjectiveType.Interact) // if objective is of type Interact -------------------------------
            {
                foreach (GameObject target in objective.objectiveTargets)
                {
                    if (target.GetComponent<Interactable>() != null && target.GetComponent<Interactable>().timesInteracted > 0)
                    {
                        CompleteObjective(objective, !objective.objectiveMessedUp);
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
                }

                if (numberOfDestroyedTargets == numberOfTargetsToDestroy)
                {
                    CompleteObjective(objective, !objective.objectiveMessedUp);
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
                }
            }
        }

        public void CompleteObjective(Objective objective, bool wasSuccessful)
        {
            print("Objective completed: " + objective.objectiveText + " - " + wasSuccessful);
            if (wasSuccessful)
            {
                objective.status = Objective.ObjectiveStatus.Completed;
            }
            else
            {
                objective.status = Objective.ObjectiveStatus.Failed;
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
                signal.Signal();
            }
            
            if (!objectives.Contains(objective)) return; // Only main objectives should progress the story
            
            if (currentObjectiveIndex < objectives.Count - 1)
            {
                currentObjectiveIndex++;
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
            }
            else
            {
                print("All objectives completed");
            }
        }
        
        // public void FailObjective(Objective objective)
        // {
        //     objective.status = Objective.ObjectiveStatus.Failed;
        //     RemoveObjectiveFromUI(objective, false);
        //     
        //     foreach (Objective objectivesThatFailThisObjective in objective.objectivesThatFailThisObjective)
        //     {
        //         if (objectivesThatFailThisObjective.status == Objective.ObjectiveStatus.Completed) continue;
        //         objectivesThatFailThisObjective.status = Objective.ObjectiveStatus.Failed;
        //         RemoveObjectiveFromUI(objectivesThatFailThisObjective, false);
        //     }
        // }

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

        void DetermineFreeWill(Objective completedObjective)
        {
            
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
                print("Skipping task " + i);
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

        IEnumerator PulseVignette(Color color, float fadeInDuration, float fadeOutDuration)
        {
            // set intensity from 0 to 0.5 and back to 0
            vignetteVolume.profile.TryGet(out vignette);
            vignette.color.value = color;
            float startTime = Time.time;
            float endTime = startTime + fadeInDuration;
            while (Time.time < endTime)
            {
                float t = (Time.time - startTime) / fadeInDuration;
                vignette.intensity.value = Mathf.Lerp(0, 0.5f, t);
                yield return null;
            }
            startTime = Time.time;
            endTime = startTime + fadeOutDuration;
            while (Time.time < endTime)
            {
                float t = (Time.time - startTime) / fadeOutDuration;
                vignette.intensity.value = Mathf.Lerp(0.5f, 0, t);
                yield return null;
            }
            vignette.intensity.value = 0;

        }
    }
}

