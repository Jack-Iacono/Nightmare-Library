using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    protected Collider interactCollider;

    [SerializeField]
    protected float interactRange = 5;
    protected bool inRange = false;
    
    protected enum InteractionType { CLICK, HIT }
    [SerializeField]
    protected List<InteractionType> interactionTypes = new List<InteractionType>();

    [SerializeField]
    protected LayerMask interactionLayers;

    public event EventHandler OnHit;
    public event EventHandler OnClick;

    // Update is called once per frame
    void Update()
    {
        if(
            inRange &&
            Input.GetKeyDown(PlayerController.keyInteract) && 
            PlayerController.ownerInstance.camCont.GetCameraSight(interactCollider)
        )
        {
            Click();
        }

        CheckRange();
    }
    private void CheckRange()
    {
        if(PlayerController.ownerInstance != null)
        {
            Collider[] col = Physics.OverlapSphere(transform.position, interactRange, interactionLayers);

            if (col.Length > 0)
            {
                bool playerFound = false;

                foreach (Collider c in col)
                {
                    if (c.gameObject == PlayerController.ownerInstance.gameObject)
                    {
                        if (!inRange)
                        {
                            inRange = true;
                            OnEnterRange();
                        }

                        playerFound = true;
                        break;
                    }
                }

                if (!playerFound && inRange)
                {
                    inRange = false;
                    OnExitRange();
                }
            }
            else if (inRange)
            {
                inRange = false;
                OnExitRange();
            }
        }
    }

    public virtual void Click()
    {
        Debug.Log("Click");
        OnClick?.Invoke(this, EventArgs.Empty);
    }
    public virtual void Hit()
    {
        Debug.Log("Hit");
        OnHit?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnEnterRange()
    {
        //Debug.Log("Enter Range");
    }
    protected virtual void OnExitRange()
    {
        //Debug.Log("Exit Range");
    }

    private void OnDrawGizmos()
    {
        if(inRange)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
