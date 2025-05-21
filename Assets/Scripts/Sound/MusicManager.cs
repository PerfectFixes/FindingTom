using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MusicTrack
{
    public string trackName;
    public AudioClip clip;
    public bool hasBeenPlayed = false;
    public float savedTime = 0f;
    [Range(0f, 1f)] public float targetVolume = 1f; // New: individual volume control
}

public class MusicManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioSource puzzleMusicSource;

    public AudioMixerGroup partyMixerGroup;
    
    [Header("Crossfade Settings")]
    [SerializeField] private float crossfadeDuration = 1.5f;
    [SerializeField] private AnimationCurve crossfadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Music Tracks")]
    [SerializeField] private MusicTrack backgroundMusic;
    [SerializeField] private MusicTrack partyMusic;
    [SerializeField] private List<MusicTrack> puzzleMusicTracks = new List<MusicTrack>();
    
    [Header("Performance Settings")]
    [SerializeField] private bool useOptimizedLoading = true;
    [SerializeField] private int preloadCount = 2; // Number of tracks to preload
    
    private bool isPlayingPuzzleMusic = false;
    private MusicTrack currentPuzzleTrack;
    private Coroutine crossfadeCoroutine;
    private Queue<MusicTrack> preloadQueue = new Queue<MusicTrack>();
    
    private void Start()
    {
        // Initialize audio sources with required settings
        InitializeAudioSources();
        
        // Preload the background music
        if (backgroundMusic.clip != null)
        {
            backgroundMusic.clip.LoadAudioData();
        }
        
        // Preload initial puzzle tracks
        if (useOptimizedLoading)
        {
            StartCoroutine(PreloadTracksAsync());
        }
        
        // Start with background music
        PlayBackgroundMusic();
    }
    public void PlayPartyMusic()
    {
        backgroundMusicSource.outputAudioMixerGroup = partyMixerGroup;
        crossfadeCoroutine = StartCoroutine(CrossfadeAudio(puzzleMusicSource, backgroundMusicSource, partyMusic));
    } 
    private void InitializeAudioSources()
    {
        if (backgroundMusicSource == null)
            backgroundMusicSource = gameObject.AddComponent<AudioSource>();
        if (puzzleMusicSource == null)
            puzzleMusicSource = gameObject.AddComponent<AudioSource>();
            
        // Configure audio sources for optimal performance
        ConfigureAudioSource(backgroundMusicSource);
        ConfigureAudioSource(puzzleMusicSource);
    }
    
    private void ConfigureAudioSource(AudioSource source)
    {
        source.loop = true;
        source.priority = 128; // Medium priority
        source.playOnAwake = false;
    }
    
    private IEnumerator PreloadTracksAsync()
    {
        for (int i = 0; i < Mathf.Min(preloadCount, puzzleMusicTracks.Count); i++)
        {
            if (puzzleMusicTracks[i].clip != null)
            {
                puzzleMusicTracks[i].clip.LoadAudioData();
                yield return null; // Wait one frame to avoid blocking
            }
        }
    }
    
    public void PlayBackgroundMusic()
    {
        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);
            
        if (isPlayingPuzzleMusic)
        {
            if (currentPuzzleTrack != null)
            {
                currentPuzzleTrack.savedTime = puzzleMusicSource.time;
            }
            crossfadeCoroutine = StartCoroutine(CrossfadeAudio(puzzleMusicSource, backgroundMusicSource, backgroundMusic));
        }
        else
        {
            SetupAndPlayTrack(backgroundMusicSource, backgroundMusic, true);
        }
        
        isPlayingPuzzleMusic = false;
    }
    
    public void PlayPuzzleMusic(string puzzleName)
    {
        MusicTrack puzzleTrack = puzzleMusicTracks.Find(track => track.trackName == puzzleName);
        
        if (puzzleTrack == null)
        {
            Debug.LogError($"No music track found for puzzle: {puzzleName}");
            return;
        }
        
        // Ensure the track is loaded
        if (useOptimizedLoading && puzzleTrack.clip.loadState != AudioDataLoadState.Loaded)
        {
            puzzleTrack.clip.LoadAudioData();
        }
        
        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);
            
        // Save the current background music position
        backgroundMusic.savedTime = backgroundMusicSource.time;
        
        currentPuzzleTrack = puzzleTrack;
        crossfadeCoroutine = StartCoroutine(CrossfadeAudio(backgroundMusicSource, puzzleMusicSource, puzzleTrack));
        isPlayingPuzzleMusic = true;
    }
    
    private void SetupAndPlayTrack(AudioSource source, MusicTrack track, bool immediate = false)
    {
        if (source.clip != track.clip)
        {
            source.clip = track.clip;
        }
        
        if (!track.hasBeenPlayed)
        {
            track.hasBeenPlayed = true;
            source.time = 0f;
        }
        else
        {
            source.time = track.savedTime;
        }
        
        if (immediate)
        {
            source.volume = track.targetVolume;
            source.Play();
        }
    }
    
    private IEnumerator CrossfadeAudio(AudioSource fadeOutSource, AudioSource fadeInSource, MusicTrack fadeInTrack)
    {
        float startVolume = fadeOutSource.volume;
        float targetVolume = fadeInTrack.targetVolume;
        
        // Setup fade-in track without playing yet
        SetupAndPlayTrack(fadeInSource, fadeInTrack);
        
        // Start playing but at zero volume
        fadeInSource.volume = 0f;
        fadeInSource.Play();
        
        float timeElapsed = 0f;
        
        while (timeElapsed < crossfadeDuration)
        {
            timeElapsed += Time.unscaledDeltaTime; // Use unscaled time for consistent crossfade even when game is paused
            float t = crossfadeCurve.Evaluate(timeElapsed / crossfadeDuration);
            
            fadeOutSource.volume = Mathf.Lerp(startVolume, 0f, t);
            fadeInSource.volume = Mathf.Lerp(0f, targetVolume, t);
            
            yield return null;
        }
        
        // Final state
        fadeOutSource.Pause();
        fadeOutSource.volume = startVolume; // Reset to original volume
        fadeInSource.volume = targetVolume;
        
        crossfadeCoroutine = null;
    }
    
    // Method to change target volume for a specific track
    public void SetTrackVolume(string trackName, float volume)
    {
        if (trackName == backgroundMusic.trackName)
        {
            backgroundMusic.targetVolume = Mathf.Clamp01(volume);
            if (!isPlayingPuzzleMusic)
            {
                backgroundMusicSource.volume = backgroundMusic.targetVolume;
            }
        }
        else
        {
            MusicTrack track = puzzleMusicTracks.Find(t => t.trackName == trackName);
            if (track != null)
            {
                track.targetVolume = Mathf.Clamp01(volume);
                if (currentPuzzleTrack == track && isPlayingPuzzleMusic)
                {
                    puzzleMusicSource.volume = track.targetVolume;
                }
            }
        }
    }
    
    // Preload a specific track (useful for upcoming puzzles)
    public void PreloadTrack(string trackName)
    {
        MusicTrack track = puzzleMusicTracks.Find(t => t.trackName == trackName);
        if (track != null && track.clip != null && track.clip.loadState != AudioDataLoadState.Loaded)
        {
            StartCoroutine(LoadTrackAsync(track));
        }
    }
    
    private IEnumerator LoadTrackAsync(MusicTrack track)
    {
        track.clip.LoadAudioData();
        while (track.clip.loadState != AudioDataLoadState.Loaded)
        {
            yield return null;
        }
    }
    
    // Optional: Add method to get status of a specific track
    public bool HasPuzzleTrackBeenPlayed(string puzzleName)
    {
        MusicTrack track = puzzleMusicTracks.Find(t => t.trackName == puzzleName);
        return track?.hasBeenPlayed ?? false;
    }
    
    private void OnDestroy()
    {
        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
        }
    }
}