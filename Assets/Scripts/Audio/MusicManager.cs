using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioResource menuMusic;
    [SerializeField] private AudioResource brassArmyMusic;
    [SerializeField] private AudioResource valveboundSeraphsMusic;
    [SerializeField] private AudioResource overpressureCollectiveMusic;


    private void Awake()
    {
        PlayMenuMusic();
    }

    public void PlayMenuMusic()
    {
        audioSource.resource = menuMusic;
        audioSource.Play();
    }

    public void PlayGameMusic()
    {
        // DO NOT move getting this OperationDataDontDestroy, if it is called from elsewhere it might create a DEV
        // operation data, even though it is normal game
        Faction faction = OperationDataDontDestroy.GetOrReadDev().Faction;

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
