using System;
using System.Collections;
using System.Collections.Generic;
using Ekkam;
using UnityEngine;

public class Action : Signalable
{
    public enum ActionToTake
    {
        Move,
        Rotate,
        Scale,
        Disable,
        EnableChild,
        Destroy,
        RemoveTagFromInventory,
        AddCoins
    }
    [Header("Action Settings")]
    public ActionToTake actionToTake;
    private Vector3 originalTargetOffset;
    public Vector3 targetOffset;
    public Vector3[] sequentialTargetOffsets;
    public bool assignNextObjectiveOnActionComplete;
    
    private Vector3 startPosition;
    private Vector3 targetPosition;
    float timeElapsed;
    bool isMoving;
    
    [Header("Sequence Settings")]
    public int sequenceIndex = 0;
    public float duration = 2f;
    
    public GameObject virtualCameraToTransitionTo;
    private float delayActionDuration = 1.5f;
    
    [Header("Loop Settings")]
    public bool loop;
    public float loopDelay;
    private float loopTimer;
    
    [Header("Remove Tag From Inventory Settings")]
    public string tagToRemove;
    
    [Header("Events")]
    public int coinsToAdd = 0;
    
    public delegate void OnActionComplete();
    public static event OnActionComplete onActionComplete;
    
    void Start()
    {
        originalTargetOffset = targetOffset;
        loopTimer = duration + loopDelay; // So that the first action is taken immediately if loop is enabled
    }
    
    void Update()
    {
        if (loop)
        {
            loopTimer += Time.deltaTime;
            if (loopTimer >= duration + loopDelay)
            {
                loopTimer = 0;
                StartCoroutine(TakeAction());
            }
        }
        
        if (isMoving)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= duration)
            {
                isMoving = false;
                HandleActionComplete();
            }
        }
    }
    
    public override void Signal()
    {
        print(gameObject.name + " is taking action: " + actionToTake);
        StartCoroutine(TakeAction());
    }
    IEnumerator TakeAction()
    {
        if (virtualCameraToTransitionTo != null)
        {
            virtualCameraToTransitionTo.SetActive(true);
            GameManager.Instance.PauseGame();
            yield return new WaitForSeconds(delayActionDuration);
        }
        
        if (sequenceIndex > 0 && sequenceIndex <= sequentialTargetOffsets.Length)
        {
            targetOffset = sequentialTargetOffsets[sequenceIndex - 1];
        }
        else if (sequenceIndex > 0 && sequenceIndex > sequentialTargetOffsets.Length)
        {
            targetOffset = originalTargetOffset;
            sequenceIndex = 0;
        }
        
        switch (actionToTake)
        {
            case ActionToTake.Move:
                // StartCoroutine(Move());
                startPosition = transform.position;
                targetPosition = startPosition + targetOffset;
                timeElapsed = 0;
                isMoving = true;
                break;
            case ActionToTake.Rotate:
                // transform.rotation = Quaternion.Euler(targetOffset); // Maybe use Quaternion.Lerp for smooth rotation in the future
                // Invoke("HandleActionComplete", 0.1f);
                StartCoroutine(Rotate());
                break;
            case ActionToTake.Scale:
                transform.localScale = targetOffset;
                Invoke("HandleActionComplete", 0.1f);
                break;
            case ActionToTake.Disable:
                gameObject.SetActive(false);
                Invoke("HandleActionComplete", 1f);
                break;
            case ActionToTake.EnableChild:
                transform.GetChild(0).gameObject.SetActive(true);
                Invoke("HandleActionComplete", 1f);
                break;
            case ActionToTake.Destroy:
                Destroy(gameObject);
                // ActionComplete event is handled in OnDestroy
                break;
            case ActionToTake.RemoveTagFromInventory:
                var inventory = FindObjectOfType<Inventory>();
                inventory.RemoveItemByTag(tagToRemove);
                break;
            case ActionToTake.AddCoins:
                Player.Instance.coins += coinsToAdd;
                if (Player.Instance.coins < 0) Player.Instance.coins = 0;
                break;
        }
        
        sequenceIndex++;
    }
    
    IEnumerator Move()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + targetOffset;
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        HandleActionComplete();
    }
    
    IEnumerator Rotate()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(targetOffset);
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        HandleActionComplete();
    }

    private void OnDestroy()
    {
        HandleActionComplete();
    }

    void HandleActionComplete()
    {
        if (onActionComplete != null) onActionComplete();
        if (virtualCameraToTransitionTo != null)
        {
            GameManager.Instance.ResumeGame();
            virtualCameraToTransitionTo.SetActive(false);
        }
        if (assignNextObjectiveOnActionComplete)
        {
            FindObjectOfType<ObjectiveManager>().AddNextObjective();
        }
    }
    
}
