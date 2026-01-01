using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class SoundManagersDontDestroy : MonoBehaviour
{
    private static SoundManagersDontDestroy dontDestroyInstance;

    [SerializeField] private SoundFXManager soundFXManager;
    public SoundFXManager SoundFX => soundFXManager;

    [SerializeField] private MusicManager musicManager;
    public MusicManager Music => musicManager;

    [SerializeField] private SoundMixerManager soundMixerManager;
    public SoundMixerManager Mixer => soundMixerManager;

    private void Awake()
    {
        if (dontDestroyInstance != null && dontDestroyInstance != this)
        {
            Destroy(gameObject);
            return;
        }
        dontDestroyInstance = this;
        DontDestroyOnLoad(gameObject);
    }

    private const string PrefabPath = "Assets/Prefabs/Audio/SoundManagers.prefab";
    public static SoundManagersDontDestroy GerOrCreate()
    {
        if (dontDestroyInstance != null) return dontDestroyInstance;

        dontDestroyInstance = FindFirstObjectByType<SoundManagersDontDestroy>();
        if (dontDestroyInstance != null) return dontDestroyInstance;

#if UNITY_EDITOR
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.IsNotNull(prefab, $"SoundManagersDontDestroy prefab not found at '{PrefabPath}'. Fix the path or create the prefab.");

        var instance = Instantiate(prefab);
        Assert.IsNotNull(instance, "Failed to instantiate SoundManagersDontDestroy prefab");

        var managers = instance.GetComponent<SoundManagersDontDestroy>();
        Assert.IsNotNull(managers, "SoundManagersDontDestroy prefab does not contain an the script component");
        return managers;

#else
        Debug.LogWarning("trying to load sound managers, they weren't in the dont destroy context");
        return null;
#endif
    }
}
