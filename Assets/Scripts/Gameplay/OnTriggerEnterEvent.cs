using System;
using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEnterEvent : MonoBehaviour
{
    [SerializeField] 
    private UnityEvent onEnter;    
    
    [SerializeField]
    private bool canInvoke = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (canInvoke)
            {
                onEnter?.Invoke();
            }
           
        }
    }

    public void SetCanInvokeTrue()
    {
        canInvoke = true;
    }
    public void SetInvokeOff()
    {
        canInvoke = false;
    }
}
