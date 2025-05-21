using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource), typeof(BoxCollider))]
public class RoomMusicZone : MonoBehaviour
{
    public float fadeDuration = 1f;
    public float maxVolume = 1f;
    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0f;
        audioSource.loop = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!audioSource.isPlaying)
                audioSource.Play();

            StartFade(maxVolume); // Fade in to full volume
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartFade(0f); // Fade out
        }
    }
    public void StartFade(float targetVolume)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeVolume(targetVolume));
    }
    private IEnumerator FadeVolume(float targetVolume)
    {
        float startVolume = audioSource.volume;
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timeElapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;

        if (targetVolume == 0f)
            audioSource.Stop();
    }
}