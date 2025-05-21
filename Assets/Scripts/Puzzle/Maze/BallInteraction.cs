using System;
using UnityEngine;

public class BallInteraction : MonoBehaviour
{
     ButtonLogic buttonLogic;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void BallTouchButton(Collider collider)
    {
        buttonLogic = collider.GetComponent<ButtonLogic>();
        if (buttonLogic != null)
        {
            buttonLogic.BallOnButton();
        }
    }

    private void BallOffButton()
    {
        if (buttonLogic != null)
        {
            buttonLogic.OnButtonUnpressed();
            buttonLogic = null;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("MazeButton"))
        {
            BallTouchButton(other);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("MazeButton"))
        {
            BallOffButton();
        }
    }
}
