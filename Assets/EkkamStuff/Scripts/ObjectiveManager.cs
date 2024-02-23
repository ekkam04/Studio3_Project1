using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

namespace Ekkam {
    public class ObjectiveManager : MonoBehaviour
    {
        public int currentObjectiveIndex = 0;
        public float objectiveTargetDistance;

        public List<Objective> objectives = new List<Objective>();
        public List<Objective> completedObjectives = new List<Objective>();
        public List<Objective> activeObjectives = new List<Objective>();

        public List<ObjectiveCompletionAction> objectiveCompletionActions = new List<ObjectiveCompletionAction>();

        [SerializeField] GameObject objectiveUI;
        Player player;
               
        void Start()
        {
            player = FindObjectOfType<Player>();

            // hide all objective targets if type is Reach
            foreach (Objective objective in objectives)
            {
                if (objective.objectiveType == Objective.ObjectiveType.Reach)
                {
                    objective.objectiveTarget.gameObject.SetActive(false);
                }
            }

            if (objectives.Count > 0)
            {
                activeObjectives.Add(objectives[currentObjectiveIndex]);
                AddObjectiveToUI(objectives[currentObjectiveIndex]);
            }
        }

        void Update()
        {

            List<Objective> completedObjectives = new List<Objective>();
            foreach (Objective objective in activeObjectives)
            {
                if (objective.isCompleted)
                {
                    completedObjectives.Add(objective);
                    RemoveObjectiveFromUI(objective);
                }
            }

            if (activeObjectives.Count < 2 && completedObjectives.Count > 0 && currentObjectiveIndex < objectives.Count - 1)
            {
                CheckForObjectiveCompletion();
                AddNextObjective();
            }
            else if (activeObjectives.Count < 1)
            {
                print("All objectives completed!");
            }

            foreach (Objective objective in completedObjectives)
            {
                CompleteObjective(objective);
            }
            

            foreach (Objective objective in activeObjectives)
            {
                if (objective.objectiveTarget == null) continue;

                objectiveTargetDistance = Vector3.Distance(player.transform.position, objective.objectiveTarget.transform.position);


                if (objective.objectiveType == Objective.ObjectiveType.Reach)
                {
                    if (objectiveTargetDistance < 3.5f)
                    {
                        objective.objectiveTarget.gameObject.SetActive(false);
                        objectiveTargetDistance = Mathf.Infinity;
                        objective.isCompleted = true;
                    }
                    else
                    {
                        objective.objectiveTarget.gameObject.SetActive(true);
                    }
                }
                else if (objective.objectiveType == Objective.ObjectiveType.Interact)
                {
                    if (objective.objectiveTarget.GetComponent<Interactable>().timesInteracted > 0)
                    {
                        objective.isCompleted = true;
                    }
                }
                else if (objective.objectiveType == Objective.ObjectiveType.Destroy)
                {
                    int numberOfTargetsToDestroy = objective.objectiveMultiTargets.Length;
                    int numberOfDestroyedTargets = 0;
                    foreach (GameObject target in objective.objectiveMultiTargets)
                    {
                        if (target == null)
                        {
                            numberOfDestroyedTargets++;
                        }
                    }
                    if (numberOfDestroyedTargets == numberOfTargetsToDestroy)
                    {
                        objective.isCompleted = true;
                    }
                }
            }
        }

        public void AddNextObjective()
        {
            bool autoAssignNextObjective = true;
            while (autoAssignNextObjective)
            {
                currentObjectiveIndex++;
                activeObjectives.Add(objectives[currentObjectiveIndex]);
                AddObjectiveToUI(objectives[currentObjectiveIndex]);
                if (currentObjectiveIndex < objectives.Count - 1)
                {
                     autoAssignNextObjective = objectives[currentObjectiveIndex].autoAssignNextObjective;
                }
            }
        }

        public void CompleteObjective(Objective objective)
        {
            completedObjectives.Add(objective);
            activeObjectives.Remove(objective);
        }

        public void CheckForObjectiveCompletion()
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
            // set color to green
            objective.objectiveUIText.color = Color.green;
            await Task.Delay(1500);
            Destroy(objective.objectiveUIText.gameObject);
        }
    }
}

