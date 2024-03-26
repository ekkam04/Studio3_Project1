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
    
    public delegate void OnActionComplete();
    public static event OnActionComplete onActionComplete;
    public override void Signal()
    {
        print(gameObject.name + " is taking action: " + actionToTake);
        switch (actionToTake)
        {
            case ActionToTake.Move:
                StartCoroutine(Move());
                break;
            case ActionToTake.Rotate:
                transform.rotation = Quaternion.Euler(targetOffset); // Maybe use Quaternion.Lerp for smooth rotation in the future
                if (onActionComplete != null) onActionComplete();
                break;
            case ActionToTake.Scale:
                transform.localScale = targetOffset;
                if (onActionComplete != null) onActionComplete();
                break;
            case ActionToTake.Enable:
                gameObject.SetActive(true);
                if (onActionComplete != null) onActionComplete();
                break;
            case ActionToTake.Disable:
                gameObject.SetActive(false);
                if (onActionComplete != null) onActionComplete();
                break;
            case ActionToTake.Destroy:
                Destroy(gameObject);
                if (onActionComplete != null) onActionComplete();
                break;
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
            if (onActionComplete != null) onActionComplete();
        }
    }
}
