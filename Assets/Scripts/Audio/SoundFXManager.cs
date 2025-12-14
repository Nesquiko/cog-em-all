using System.Collections;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    [SerializeField] private AudioSource soundFXObject;
    [SerializeField] private SoundFXDatabase soundFXDatabase;

    public void PlaySoundFXClip(SoundFXType type, Transform spawnTransform, float volume = 1f)
    {
        AudioClip audioClip = soundFXDatabase.GetRandomClip(type);
        if (audioClip == null) return;
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.loop = false;
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }

    public AudioSource PlayLoopedSoundFX(SoundFXType type, Transform spawnTransform, float volume = 1f)
    {
        AudioClip audioClip = soundFXDatabase.GetRandomClip(type);
        if (audioClip == null) return null;
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.Play();

        return audioSource;
    }

    public void StopSoundFX(AudioSource source)
    {
        if (source == null) return;

        source.Stop();
        Destroy(source.gameObject);
    }

    public void StopSoundFX(AudioSource source, float fadeOutDuration)
    {
        if (source == null) return;
        StartCoroutine(FadeAndStop(source, fadeOutDuration));
    }

    private IEnumerator FadeAndStop(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float time = 0f;

        while (time < duration && source != null)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }

        if (source != null)
        {
            source.Stop();
            Destroy(source.gameObject);
        }
    }
}
