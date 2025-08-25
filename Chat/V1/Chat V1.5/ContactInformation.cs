using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContactInformation : MonoBehaviour
{
    private List<string> ChatFileName = new List<string>();
    public int ChatFileIndex;
    public int MessageIndex;
    public Button ContactButton;
    public ContactMemories ContactMemories;

    public Image ContactIcon;
    public Text ContactName;
    public ContactData ContactData;

    public EnterMessage EnterMessage;
    public ContactInformation contactInformation;
    private int totalHistoryMessageCount;
    private void Start()
    {
        ContactButton.onClick.AddListener(OnButtonClicked);//���ĵ���¼�
        //������ҪJson����ϵͳ��չ
        ChatFileIndex = 0;

        ContactName.text =  ContactData.ContactName;
        ChatFileName = ContactData.MessageFileName;
        ContactIcon.sprite = LoadSpriteFromResources("Avatars", $"{ContactData.ContactIconFileName}");

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

    public void UpdateThisContact(bool isOverThisChatFile, int HistoryMessageIndex)
    {
        if (isOverThisChatFile == true)
        {
            ChatFileIndex++;
            totalHistoryMessageCount = totalHistoryMessageCount + HistoryMessageIndex;
            MessageIndex = 0;
            DebugMessage();
        }
        else
        {
            MessageIndex = HistoryMessageIndex;//����
            //���ڶԻ��ļ����ʱֱ��¼��totalHistoryMessageCount
            DebugMessage();//��������»������ͨ��totalHistoryMessageCount + MessageIndex�ķ�ʽ�õ���ȷ��ȫ���Ի���
        }
    }

    private void DebugMessage()
    {
        Debug.Log($"��ǰʹ�õ�" +
    $"{ChatFileIndex}���Ի��ļ�����ϵ��Ϊ" +
    $"{ContactName.text},��" +
    $"{totalHistoryMessageCount}�������¼�������������Ϊ" +
    $"{MessageIndex}");
    }

    private void OnButtonClicked()
    {
        bool isSelect = ContactMemories.UpdateContactSelected(contactInformation);
        if (isSelect == false)//�������ѡ�й�ֱ�ӷ���
            return;

        string NewChatFileName = GetStringByIndex(ChatFileIndex);
        EnterMessage.ClearAllChildren(EnterMessage.transform);
        EnterMessage.StartChat(NewChatFileName,contactInformation,ContactData,MessageIndex);//���߹����������Ի��������ļ�����data��message����
    }
    private string GetStringByIndex(int index)
    {
        // ��������Ƿ�Ϸ�������Խ�籨��
        if (index >= 0 && index < ChatFileName.Count)
        {
            return ChatFileName[index];
        }
        else
        {
            Debug.LogError("����������Χ��");
            return null;
        }
    }
    public string GetChatFileName()
    {
        string NewChatFileName = GetStringByIndex(ChatFileIndex);
        return NewChatFileName;
    }
}
