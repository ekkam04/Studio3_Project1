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
    }
    public ActionToTake actionToTake;
    private Vector3 originalTargetOffset;
    public Vector3 targetOffset;
    public Vector3[] sequentialTargetOffsets;
    
    public int sequenceIndex = 0;
    public float duration = 2f;
    
    public GameObject virtualCameraToTransitionTo;
    private float delayActionDuration = 1.5f;
    
    public delegate void OnActionComplete();
    public static event OnActionComplete onActionComplete;
    
    void Start()
    {
        originalTargetOffset = targetOffset;
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
                StartCoroutine(Move());
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
    }
    
}
