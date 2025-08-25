using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MessageCompany : MonoBehaviour
{
    public Text ChatSpace;
    public Text speakerName;
    public Image Icon;
    public int MessageIndex;


    private Coroutine streamCoroutine; // 用于控制流式显示的协程
    public void initializeMessage(
        string ChatContent,
        string SpeakerName,
        string SpeakerAvatar,
        GameObject parentObject,
        bool IsStreamText,
        float TextSpeed
        )
    {
        if (ChatSpace == null)
            Debug.LogError("请指定对象！");
        if (speakerName == null)
            Debug.LogError("请指定对象！");
        if (Icon == null)
            Debug.LogError("请指定对象！");

        ChatSpace.text = ChatContent;

        if (IsStreamText)
        {
            streamCoroutine = StartCoroutine(StreamTextCoroutine(ChatSpace, ChatContent, TextSpeed));
        }
        else
        {
            ShowAllText(ChatSpace, ChatContent);
        }

        Icon.sprite = LoadSpriteFromResources("Avatars", $"{SpeakerAvatar}");

        if (parentObject == null)
            Debug.LogError("请指定对象！");
        MessageCompany[] scripts = parentObject.GetComponentsInChildren<MessageCompany>(includeInactive: true);

        MessageIndex = scripts.Length;
    }
    public Sprite LoadSpriteFromResources(string folderPath, string fileName)
    {
        string fullPath = $"{folderPath}/{fileName}";// 拼接路径
        Sprite targetSprite = Resources.Load<Sprite>(fullPath);// 加载Sprite

        if (targetSprite == null)
        {
            Debug.LogError($"未找到Sprite{fullPath}");
            return null;
        }
        return targetSprite;
    }
    public void ShowAllText(Text targetText, string fullText)
    {
        if (streamCoroutine != null)
            StopCoroutine(streamCoroutine);

        targetText.text = fullText;
    }


    // 流式显示文本的协程
    private IEnumerator StreamTextCoroutine(Text targetText, string fullText, float ChatSpeed)
    {
        targetText.text = ""; // 清空现有文本
        for (int i = 0; i < fullText.Length; i++)
        {
            targetText.text += fullText[i];
            yield return new WaitForSeconds(ChatSpeed); // 等待指定间隔
        }
        streamCoroutine = null; // 完成后清空协程引用
    }
}
