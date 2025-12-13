using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance { get; private set; }

    [SerializeField] private AudioSource soundFXObject;

    [SerializeField] private SoundFXDatabase soundFXDatabase;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        DontDestroyOnLoad(Instance);
    }

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
