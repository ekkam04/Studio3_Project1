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
        Enable,
        Disable,
        Destroy,
    }
    public ActionToTake actionToTake;
    public Vector3 targetOffset;
    public float duration = 2f;
    
    public GameObject virtualCameraToTransitionTo;
    private float delayActionDuration = 1.5f;
    
    public delegate void OnActionComplete();
    public static event OnActionComplete onActionComplete;
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
            yield return new WaitForSeconds(delayActionDuration);
        }
        
        switch (actionToTake)
        {
            case ActionToTake.Move:
                StartCoroutine(Move());
                break;
            case ActionToTake.Rotate:
                transform.rotation = Quaternion.Euler(targetOffset); // Maybe use Quaternion.Lerp for smooth rotation in the future
                Invoke("HandleActionComplete", 0.1f);
                break;
            case ActionToTake.Scale:
                transform.localScale = targetOffset;
                Invoke("HandleActionComplete", 0.1f);
                break;
            case ActionToTake.Enable:
                gameObject.SetActive(true);
                Invoke("HandleActionComplete", 0.1f);
                break;
            case ActionToTake.Disable:
                gameObject.SetActive(false);
                Invoke("HandleActionComplete", 0.1f);
                break;
            case ActionToTake.Destroy:
                Destroy(gameObject);
                // ActionComplete event is handled in OnDestroy
                break;
        }
        
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

    private void OnDestroy()
    {
        HandleActionComplete();
    }

    void HandleActionComplete()
    {
        if (onActionComplete != null) onActionComplete();
        if (virtualCameraToTransitionTo != null) virtualCameraToTransitionTo.SetActive(false);
    }
    
}
