using System.Collections.Generic;
using UnityEngine;
using EasyTextEffects;


public class TextEffectManager : MonoBehaviour
{
    public List<string> effectNames;
    
    public void StartTextEffect(TextEffect textEffect)
    {
        textEffect.enabled = true;
        //textEffect.StartManualEffects();
    }

    public void StopTextEffect(TextEffect textEffect)
    {
        textEffect.enabled = false;
        textEffect.Refresh();
    }
}
