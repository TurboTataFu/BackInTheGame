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

    [Header("��Ƶ����")]
    [SerializeField] private float globalVolume = 1f;
    [SerializeField] private Sound[] sounds;

    private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();
    private AudioSource bgmSource;
    private List<AudioSource> activeSfxSources = new List<AudioSource>();
    private GameObject audioPool;

    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        // ������ʼ��
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
        // ������Ƶ������
        audioPool = new GameObject("AudioPool");
        audioPool.transform.SetParent(transform);

        // ��ʼ��BGM��Դ
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;

        // ���������ֵ�
        foreach (var sound in sounds)
        {
            soundDictionary[sound.name.ToLower()] = sound;
        }
    }

    /// <summary>
    /// ������Ч��SFX��
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
            Debug.LogWarning($"δ�ҵ���Ч: {soundName}");
        }
    }

    /// <summary>
    /// ���ű������֣�BGM��
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
        // �ӳ���Ѱ�ҿ�����Դ
        foreach (var source in activeSfxSources)
        {
            if (!source.isPlaying) return source;
        }

        // ��������Դ
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

    // �������Ʒ���
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

    // ����/��������ʾ��
    public void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("GlobalVolume", globalVolume);
    }

    public void LoadAudioSettings()
    {
        SetGlobalVolume(PlayerPrefs.GetFloat("GlobalVolume", 1f));
    }
}