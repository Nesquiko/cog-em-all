using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    [SerializeField] private AudioSource soundFXObject;

    [SerializeField] private SoundFXDatabase soundFXDatabase;

    public void PlaySoundFXClip(SoundFXType type, Transform spawnTransform, float volume = 1f)
    {
        AudioClip audioClip = soundFXDatabase.GetRandomClip(type);
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }
}
