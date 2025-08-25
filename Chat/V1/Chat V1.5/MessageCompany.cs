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


    private Coroutine streamCoroutine; // ���ڿ�����ʽ��ʾ��Э��
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
            Debug.LogError("��ָ������");
        if (speakerName == null)
            Debug.LogError("��ָ������");
        if (Icon == null)
            Debug.LogError("��ָ������");

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
            Debug.LogError("��ָ������");
        MessageCompany[] scripts = parentObject.GetComponentsInChildren<MessageCompany>(includeInactive: true);

        MessageIndex = scripts.Length;
    }
    public Sprite LoadSpriteFromResources(string folderPath, string fileName)
    {
        string fullPath = $"{folderPath}/{fileName}";// ƴ��·��
        Sprite targetSprite = Resources.Load<Sprite>(fullPath);// ����Sprite

        if (targetSprite == null)
        {
            Debug.LogError($"δ�ҵ�Sprite{fullPath}");
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


    // ��ʽ��ʾ�ı���Э��
    private IEnumerator StreamTextCoroutine(Text targetText, string fullText, float ChatSpeed)
    {
        targetText.text = ""; // ��������ı�
        for (int i = 0; i < fullText.Length; i++)
        {
            targetText.text += fullText[i];
            yield return new WaitForSeconds(ChatSpeed); // �ȴ�ָ�����
        }
        streamCoroutine = null; // ��ɺ����Э������
    }
}
