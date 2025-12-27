using UnityEngine;

public class SteamOnClick : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] steams;

    public void PuffSteam()
    {
        if (steams == null || steams.Length == 0) return;
        foreach (var steam in steams)
            if (steam != null) steam.Play();
    }
}
