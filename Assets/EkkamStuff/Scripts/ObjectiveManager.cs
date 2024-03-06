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

namespace Ekkam {
    public class ObjectiveManager : MonoBehaviour
    {
        public int currentObjectiveIndex = 0;
        public float objectiveTargetDistance;
        
        public int objectivesCompleted = 0;
        public int objectivesActive = 0;

        public Color objectiveCompletedColor = Color.green;
        public Color objectiveFailedColor = Color.red;

        public List<Objective> objectives = new List<Objective>();
        public List<Objective> activeObjectives = new List<Objective>();

        public List<ObjectiveCompletionAction> objectiveCompletionActions = new List<ObjectiveCompletionAction>();

        [SerializeField] GameObject objectiveUI;
        [SerializeField] GameObject objectiveItem;
        Player player;

        public Volume vignetteVolume;
        public Vignette vignette;
               
        void Start()
        {
            player = FindObjectOfType<Player>();

            // hide all objective targets if type is Reach
            foreach (Objective objective in objectives)
            {
                if (objective.objectiveTarget != null)
                {
                    objective.objectiveTarget.gameObject.SetActive(false);
                }
            }

            if (objectives.Count > 0)
            {
                objectives[currentObjectiveIndex].status = Objective.ObjectiveStatus.Active;
                AddObjectiveToUI(objectives[currentObjectiveIndex]);
                // activeObjectives.Add(objectives[currentObjectiveIndex]);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                // complete current objective
                for (int i = 0; i < objectives.Count; i++)
                {
                    if (objectives[i].status == Objective.ObjectiveStatus.Active)
                    {
                        CompleteObjective(objectives[i]);
                        CheckForCompletionActions(objectives.IndexOf(objectives[i]));
                        break;
                    }
                }
            }

            foreach (Objective objective in objectives)
            {
                if (objective.status != Objective.ObjectiveStatus.Active || objective.objectiveTarget == null) continue;

                var objectiveTargetRadius = 3.5f;
                objectiveTargetDistance = Vector3.Distance(player.transform.position, objective.objectiveTarget.transform.position);

                if (objective.objectiveType == Objective.ObjectiveType.Reach) // if objective is of type Reach ------------------------------------------
                {
                    if (objectiveTargetDistance < objectiveTargetRadius)
                    {
                        objectiveTargetDistance = Mathf.Infinity;
                        CompleteObjective(objective);
                    }
                    else
                    {
                        objective.objectiveTarget.gameObject.SetActive(true);
                    }
                }
                else if (objective.objectiveType == Objective.ObjectiveType.Interact) // if objective is of type Interact -------------------------------
                {
                    foreach (GameObject target in objective.objectiveMultiTargets)
                    {
                        if (target.GetComponent<Interactable>() == null) continue;
                        if (target.GetComponent<Interactable>().timesInteracted > 0)
                        {
                            CompleteObjective(objective);
                        }
                        else
                        {
                            objective.objectiveTarget.gameObject.SetActive(true);
                        }
                    }
                }
                else if (objective.objectiveType == Objective.ObjectiveType.Destroy) // if objective is of type Destroy ---------------------------------
                {
                    int numberOfTargetsToDestroy = objective.objectiveMultiTargets.Length;
                    int numberOfDestroyedTargets = 0;
                    foreach (GameObject target in objective.objectiveMultiTargets)
                    {
                        if (target == null || target.activeSelf == false)
                        {
                            numberOfDestroyedTargets++;
                        }
                    }

                    if (objectiveTargetDistance < objectiveTargetRadius)
                    {
                        objective.objectiveTarget.gameObject.SetActive(false);
                    }
                    else
                    {
                        objective.objectiveTarget.gameObject.SetActive(true);
                    }

                    if (numberOfDestroyedTargets == numberOfTargetsToDestroy)
                    {
                        CompleteObjective(objective);
                    }
                }
            }

        }

        public async void AddNextObjectives()
        {
            activeObjectives.Clear();
            bool autoAssignNextObjective = true;
            while (autoAssignNextObjective)
            {
                currentObjectiveIndex++;
                if (currentObjectiveIndex < objectives.Count)
                {
                    objectives[currentObjectiveIndex].status = Objective.ObjectiveStatus.Active;
                    AddObjectiveToUI(objectives[currentObjectiveIndex]);
                    // activeObjectives.Add(objectives[currentObjectiveIndex]);
                    if (currentObjectiveIndex < objectives.Count - 1)
                    {
                         autoAssignNextObjective = objectives[currentObjectiveIndex].autoAssignNextObjective;
                    }
                    await Task.Delay(1000);
                }
                else
                {
                    autoAssignNextObjective = false;
                }
            }
        }

        public void CompleteObjective(Objective objective, bool completedByPlayer = true)
        {
            if (objective.objectiveTarget != null) objective.objectiveTarget.gameObject.SetActive(false);
            objective.status = Objective.ObjectiveStatus.Completed;
            // activeObjectives.Remove(objective);

            if (completedByPlayer && objective.objectiveCategory == Objective.ObjectiveCategory.ShouldNotBeCompleted)
            {
                RemoveObjectiveFromUI(objective, false);
            }
            else
            {
                RemoveObjectiveFromUI(objective, true);
            }

            if (GetNumberOfObjectives(Objective.ObjectiveStatus.Active) < 1 && currentObjectiveIndex < objectives.Count - 1)
            {
                CheckForCompletionActions(objectives.IndexOf(objective));
                DetermineFreeWill(objective);
                AddNextObjectives();
            }
            else if (GetNumberOfObjectives(Objective.ObjectiveStatus.Completed) == objectives.Count)
            {
                print("All objectives completed!");
            }
            // Over here I'm checking if there are any active objectives that should not be completed
            // If there are, I'm auto completing them with success as the player did not complete them.
            // I hope this makes sense 😭
            else if (GetNumberOfObjectives(Objective.ObjectiveStatus.Active) > 0)
            {
                foreach (Objective obj in objectives)
                {
                    if (obj.status == Objective.ObjectiveStatus.Active)
                    {
                        if (obj.objectiveCategory == Objective.ObjectiveCategory.ShouldNotBeCompleted)
                        {
                            print("auto completing objective");
                            CompleteObjective(obj, false);
                        }
                    }
                }
            }
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

        void DetermineFreeWill(Objective completedObjective)
        {
            if (activeObjectives.Count > 1 && activeObjectives[0] == completedObjective)
            {
                player.freeWill += 10;
                StartCoroutine(PulseVignette(Color.red, 0.5f, 2.5f));
            }
            else if (activeObjectives.Count > 1)
            {
                player.freeWill -= 10;
                StartCoroutine(PulseVignette(Color.blue, 0.5f, 2.5f));
            }
        }

        public void CheckForCompletionActions(int completionIndex)
        {
            print("Checking for completion actions: " + completionIndex);
            foreach (ObjectiveCompletionAction action in objectiveCompletionActions)
            {
                if (action.objectiveIndexToComplete == completionIndex)
                {
                    switch (action.completionAction)
                    {
                        case ObjectiveCompletionAction.CompletionAction.Move:
                            StartCoroutine(MoveTarget(action.target, action.targetPositionOffset));
                            break;
                        case ObjectiveCompletionAction.CompletionAction.Enable:
                            action.target.SetActive(true);
                            break;
                        case ObjectiveCompletionAction.CompletionAction.Disable:
                            action.target.SetActive(false);
                            break;
                        case ObjectiveCompletionAction.CompletionAction.Destroy:
                            Destroy(action.target);
                            break;
                    }
                }
            }
        }

        IEnumerator MoveTarget(GameObject targetObj, Vector3 targetPositionOffset)
        {
            var targetPosition = targetObj.transform.localPosition + targetPositionOffset;
            float duration = 5f;
            yield return new WaitForSeconds(0.5f);
            Vector3 startPosition = targetObj.transform.localPosition;
            float startTime = Time.time;
            float endTime = startTime + duration;

            while (Time.time < endTime)
            {
                float t = (Time.time - startTime) / duration;
                targetObj.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
            targetObj.transform.localPosition = targetPosition;
        }

        public void AddObjectiveToUI(Objective objective)
        {
            GameObject objectiveUIItem = Instantiate(objectiveItem, objectiveUI.transform);
            objectiveUIItem.GetComponentInChildren<TextMeshProUGUI>().text = objective.objectiveText;
            objectiveUIItem.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
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

