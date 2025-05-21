using System;
using UnityEngine;

public class SetObjectOnOrOff : MonoBehaviour
{
    [SerializeField]
    private bool isObjectOn = false;

    private void Awake()
    {
        SetObjectState(isObjectOn);
    }
    public void SetObjectState(bool state)
    {
        gameObject.SetActive(state);
    }
}
