using System;
using UnityEngine;
using Unity.Cinemachine; // זה ה־namespace החדש ב־Cinemachine 4

public class CutsceneEnder : MonoBehaviour
{
    public GameObject fakePlayer;
    public GameObject realPlayer;
    public GameObject tom;
    public GameObject animationCamera;

    public CinemachineCamera vcamIntro;
    public CinemachineCamera vcamPlayer;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnCutsceneEnd()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        tom.SetActive(false);
        // מפעילים את השחקן האמיתי
        realPlayer.SetActive(true);

        // עושים מעבר למצלמה של השחקן
        vcamIntro.Priority = 10;
        vcamPlayer.Priority = 20;

        // מכבים את הדמות של האנימציה
        fakePlayer.SetActive(false);
        animationCamera.SetActive(false);
    }
}
