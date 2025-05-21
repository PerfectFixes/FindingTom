using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class MusicRoomSwitcher : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioMixerGroup mainFloorGroup;
    public AudioMixerGroup dreamRoomGroup;
    public float fadeDuration = 1f;

    private Coroutine fadeCoroutine;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(SwitchGroupWithFade(dreamRoomGroup));
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(SwitchGroupWithFade(mainFloorGroup));
        }
    }

    private IEnumerator SwitchGroupWithFade(AudioMixerGroup newGroup)
    {
        float startVolume = musicSource.volume;

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.outputAudioMixerGroup = newGroup;

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = startVolume;
    }
}