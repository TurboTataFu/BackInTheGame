using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop = false;
    }

    [Header("音频设置")]
    [SerializeField] private float globalVolume = 1f;
    [SerializeField] private Sound[] sounds;

    private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();
    private AudioSource bgmSource;
    private List<AudioSource> activeSfxSources = new List<AudioSource>();
    private GameObject audioPool;

    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Initialize()
    {
        // 创建音频池容器
        audioPool = new GameObject("AudioPool");
        audioPool.transform.SetParent(transform);

        // 初始化BGM音源
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;

        // 建立声音字典
        foreach (var sound in sounds)
        {
            soundDictionary[sound.name.ToLower()] = sound;
        }
    }

    /// <summary>
    /// 播放音效（SFX）
    /// </summary>
    public void PlaySFX(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName.ToLower(), out Sound sound))
        {
            var source = GetAvailableSource();
            source.clip = sound.clip;
            source.volume = sound.volume * globalVolume;
            source.loop = sound.loop;
            source.Play();

            if (!sound.loop)
            {
                StartCoroutine(ReturnToPool(source));
            }
        }
        else
        {
            Debug.LogWarning($"未找到音效: {soundName}");
        }
    }

    /// <summary>
    /// 播放背景音乐（BGM）
    /// </summary>
    public void PlayBGM(string musicName)
    {
        if (soundDictionary.TryGetValue(musicName.ToLower(), out Sound music))
        {
            bgmSource.clip = music.clip;
            bgmSource.volume = music.volume * globalVolume;
            bgmSource.loop = music.loop;
            bgmSource.Play();
        }
    }

    private AudioSource GetAvailableSource()
    {
        // 从池中寻找可用音源
        foreach (var source in activeSfxSources)
        {
            if (!source.isPlaying) return source;
        }

        // 创建新音源
        var newSource = audioPool.AddComponent<AudioSource>();
        activeSfxSources.Add(newSource);
        return newSource;
    }

    private System.Collections.IEnumerator ReturnToPool(AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length);
        source.Stop();
        source.clip = null;
    }

    // 音量控制方法
    public void SetGlobalVolume(float volume)
    {
        globalVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    private void UpdateAllVolumes()
    {
        bgmSource.volume = soundDictionary[bgmSource.clip.name.ToLower()].volume * globalVolume;

        foreach (var source in activeSfxSources)
        {
            if (source.isPlaying && source.clip != null)
            {
                soundDictionary.TryGetValue(source.clip.name.ToLower(), out Sound sound);
                source.volume = (sound?.volume ?? 1f) * globalVolume;
            }
        }
    }

    // 保存/加载设置示例
    public void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("GlobalVolume", globalVolume);
    }

    public void LoadAudioSettings()
    {
        SetGlobalVolume(PlayerPrefs.GetFloat("GlobalVolume", 1f));
    }
}