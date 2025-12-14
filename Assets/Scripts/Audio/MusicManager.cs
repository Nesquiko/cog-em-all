using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioResource menuMusic;
    [SerializeField] private AudioResource brassArmyMusic;
    [SerializeField] private AudioResource valveboundSeraphsMusic;
    [SerializeField] private AudioResource overpressureCollectiveMusic;

    private OperationDataDontDestroy operationData;

    private void Awake()
    {
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
