using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveToCredits : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private string creditsGameSceneName = "Credits";
    [SerializeField] private Object creditsGameScene; // For inspector reference
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void GoToCredits()
    {
        if (creditsGameScene != null)
        {
            SceneManager.LoadScene(creditsGameScene.name);
        }
        else
        {
            SceneManager.LoadScene(creditsGameSceneName); 
        }
       
    }
}
