using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioResource menuMusic;
    [SerializeField] private AudioResource brassArmyMusic;
    [SerializeField] private AudioResource valveboundSeraphsMusic;
    [SerializeField] private AudioResource overpressureCollectiveMusic;

    private OperationDataDontDestroy operationData;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        DontDestroyOnLoad(Instance);

        operationData = OperationDataDontDestroy.GetOrReadDev();

        PlayMenuMusic();
    }

    public void PlayMenuMusic()
    {
        audioSource.resource = menuMusic;
        audioSource.Play();
    }

    public void PlayGameMusic()
    {
        Faction faction = operationData.Faction;

        audioSource.resource = faction switch
        {
            Faction.TheBrassArmy => brassArmyMusic,
            Faction.TheValveboundSeraphs => valveboundSeraphsMusic,
            Faction.OverpressureCollective => overpressureCollectiveMusic,
            _ => null,
        };

        audioSource.Play();
    }
}
