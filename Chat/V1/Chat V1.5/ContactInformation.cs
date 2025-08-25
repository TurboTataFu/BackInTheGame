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
        ContactButton.onClick.AddListener(OnButtonClicked);//订阅点击事件
        //可能需要Json保存系统扩展
        ChatFileIndex = 0;

        ContactName.text =  ContactData.ContactName;
        ChatFileName = ContactData.MessageFileName;
        ContactIcon.sprite = LoadSpriteFromResources("Avatars", $"{ContactData.ContactIconFileName}");

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
            MessageIndex = HistoryMessageIndex;//更新
            //仅在对话文件完成时直接录入totalHistoryMessageCount
            DebugMessage();//这种情况下或许可以通过totalHistoryMessageCount + MessageIndex的方式得到正确的全部对话数
        }
    }

    private void DebugMessage()
    {
        Debug.Log($"当前使用第" +
    $"{ChatFileIndex}个对话文件，联系人为" +
    $"{ContactName.text},共" +
    $"{totalHistoryMessageCount}条聊天记录，聊天记里索引为" +
    $"{MessageIndex}");
    }

    private void OnButtonClicked()
    {
        bool isSelect = ContactMemories.UpdateContactSelected(contactInformation);
        if (isSelect == false)//如果曾经选中过直接返回
            return;

        string NewChatFileName = GetStringByIndex(ChatFileIndex);
        EnterMessage.ClearAllChildren(EnterMessage.transform);
        EnterMessage.StartChat(NewChatFileName,contactInformation,ContactData,MessageIndex);//告诉管理器启动对话，传输文件名、data、message索引
    }
    private string GetStringByIndex(int index)
    {
        // 检查索引是否合法（避免越界报错）
        if (index >= 0 && index < ChatFileName.Count)
        {
            return ChatFileName[index];
        }
        else
        {
            Debug.LogError("索引超出范围！");
            return null;
        }
    }
    public string GetChatFileName()
    {
        string NewChatFileName = GetStringByIndex(ChatFileIndex);
        return NewChatFileName;
    }
}
