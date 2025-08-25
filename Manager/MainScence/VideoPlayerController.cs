using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainController : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string targetScene;

    void Start()
    {
        playButton.onClick.AddListener(PlayAndSwitch);
    }

    public void PlayAndSwitch()
    {
        videoPlayer.Play();
        playButton.gameObject.SetActive(false);

        // 修复类型错误的关键行
        Invoke("SwitchScene", (float)videoPlayer.clip.length);
    }

    public void SwitchManually()
    {
        SceneManager.LoadScene(targetScene);
    }

    void SwitchScene()
    {
        SceneManager.LoadScene(targetScene);
    }
}