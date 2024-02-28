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

        public List<Objective> objectives = new List<Objective>();
        public List<Objective> activeObjectives = new List<Objective>();

        public List<ObjectiveCompletionAction> objectiveCompletionActions = new List<ObjectiveCompletionAction>();

        [SerializeField] GameObject objectiveUI;
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
            }
        }

        void Update()
        {
            foreach (Objective objective in objectives)
            {
                if (objective.status != Objective.ObjectiveStatus.Active || objective.objectiveTarget == null) continue;

                var objectiveTargetRadius = 3.5f;
                objectiveTargetDistance = Vector3.Distance(player.transform.position, objective.objectiveTarget.transform.position);

                if (objective.objectiveType == Objective.ObjectiveType.Reach) // if objective is of type Reach ------------------------------------------
                {
                    if (objectiveTargetDistance < objectiveTargetRadius)
                    {
                        objective.objectiveTarget.gameObject.SetActive(false);
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
                            objective.objectiveTarget.gameObject.SetActive(false);
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
                        objective.objectiveTarget.gameObject.SetActive(false);
                        CompleteObjective(objective);
                    }
                }
            }

        }

        public void AddNextObjectives()
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
                    activeObjectives.Add(objectives[currentObjectiveIndex]);
                    if (currentObjectiveIndex < objectives.Count - 1)
                    {
                         autoAssignNextObjective = objectives[currentObjectiveIndex].autoAssignNextObjective;
                    }
                }
                else
                {
                    autoAssignNextObjective = false;
                }
            }
        }

        public void CompleteObjective(Objective objective)
        {
            objective.status = Objective.ObjectiveStatus.Completed;
            RemoveObjectiveFromUI(objective);

            if (GetNumberOfObjectives(Objective.ObjectiveStatus.Active) < 1 && currentObjectiveIndex < objectives.Count - 1)
            {
                CheckForCompletionActions();
                DetermineFreeWill(objective);
                AddNextObjectives();
            }
            else if (GetNumberOfObjectives(Objective.ObjectiveStatus.Completed) == objectives.Count)
            {
                print("All objectives completed!");
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
                StartCoroutine(PulseVignette(Color.blue, 0.5f, 2.5f));
            }
            else if (activeObjectives.Count > 1)
            {
                player.freeWill -= 10;
                StartCoroutine(PulseVignette(Color.red, 0.5f, 2.5f));
            }
        }

        public void CheckForCompletionActions()
        {
            foreach (ObjectiveCompletionAction action in objectiveCompletionActions)
            {
                if (action.objectiveIndexToComplete == currentObjectiveIndex)
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
            float duration = 3f;
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
            GameObject objectiveText = new GameObject();
            objectiveText.transform.SetParent(objectiveUI.transform);
            objectiveText.AddComponent<TextMeshProUGUI>();
            objectiveText.GetComponent<TextMeshProUGUI>().text = objective.objectiveText;
            objectiveText.GetComponent<TextMeshProUGUI>().fontSize = 36;
            objectiveText.GetComponent<TextMeshProUGUI>().rectTransform.sizeDelta = new Vector2(500, 50);
            objectiveText.transform.localScale = new Vector3(1, 1, 1);
            objective.objectiveUIText = objectiveText.GetComponent<TextMeshProUGUI>();
        }

        public async void RemoveObjectiveFromUI(Objective objective)
        {
            objective.objectiveUIText.color = Color.green;
            await Task.Delay(1000);
            objective.objectiveUIText.gameObject.SetActive(false);
            objective.objectiveUIText = null;
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

