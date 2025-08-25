using Newtonsoft.Json;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class EnterMessage : MonoBehaviour//2 Start Chat
{

    [Header("聊天记录")]
    public GameObject parentMessageObj;
    // 在Inspector中拖拽你的预制体
    public GameObject NPCMessagePrefab;
    public GameObject UserMessagePrefab;
    public Button EventButtonInThisChat;

    public RectTransform content; // 拖拽赋值：Content 的 RectTransform

    // 对话数据
    private DialogueSceneData currentDialogueData;
    private int currentDialogueIndex = 0;
    private bool isInDialogue = false; // 是否处于对话状态
    private bool isProcessingDialogue = false; // 是否正在处理对话（防止重复触发）
    private int MessageCount;
    private ContactData ContactInformation;
    private ContactInformation ContactChater;

    public float DebugStreamTextSpeed;
    public bool IsStreamText;
    private bool _lastActiveState;
    void Update()
    {
        bool currentActiveState = gameObject.activeSelf;// 获取当前激活状态
        if (currentActiveState != _lastActiveState 
            && isInDialogue == true 
            && isProcessingDialogue == true
            )
        {
            ExitMessageState();//防御性保存
            _lastActiveState = currentActiveState;//更新状态
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);//debug
    }

    private void Start()
    {
        EventButtonInThisChat.gameObject.SetActive(false);
        EventButtonInThisChat.onClick.AddListener(OnEventButtonClick);
        // 初始化状态（获取初始激活状态）
        _lastActiveState = gameObject.activeSelf;


        if (parentMessageObj == null)
            Debug.LogError("请指定对象！");
    }

    public void StartChat(string dialogueFileName,ContactInformation contactChater,ContactData SpeakerData,int MessageHistoryIndex)
    {

        ContactChater = contactChater;//初始化
        ContactInformation = SpeakerData;//初始化联系人信息
        currentDialogueData = DialogueLoader.LoadFromResources(dialogueFileName);

        if (currentDialogueData != null)
        {
            currentDialogueIndex = MessageHistoryIndex;
            if (currentDialogueIndex == 0)
            {
                NextMessage(currentDialogueIndex);//无历史记录则直接下一条
            }
            else
            {
                InstantiateHistoryMessage();
                //补齐历史记录
            }
        }
        else
        {
            Debug.LogError("没有正确传输对话Json");
        }
    }

    private void NextMessage(int UpdateDialogueIndex)
    {
        if (currentDialogueIndex >= currentDialogueData.dialogues.Count)//标志着过去的苦难过去了，新的斗争开始了
        { ExitMessageState();//逃离地球Online
            return; }   // 对话结束


        isInDialogue = true;
        isProcessingDialogue = true;//标志着代码发展进入了快速发展的新征程

        MessageCompany[] scripts = parentMessageObj.GetComponentsInChildren<MessageCompany>(includeInactive: true);//计算消息数量
        MessageCount = scripts.Length;//计算下一条信息的位置

        int MessageHight;
        if (MessageCount == 0)//避免为0情况
        {
            MessageHight = 90;
            Debug.Log($"新消息刷新在{MessageHight}高度");
        }
        else
        {
            MessageHight = 90 - MessageCount * 30;
            Debug.Log($"新消息刷新在{MessageHight}高度");
        }

        DialogueNode currentNode = currentDialogueData.dialogues[UpdateDialogueIndex];//获取消息从json

        GameObject instantiateMessage;
        float instantiateMessageX;//算计这是谁发的消息
        if (currentNode.speaker == "User")
        {
            instantiateMessage = UserMessagePrefab;
            instantiateMessageX = 50;
        }
        else
        {
            instantiateMessage = NPCMessagePrefab;
            instantiateMessageX = -50;
        }
        float StreamTextSpeed = currentNode.textSpeed * DebugStreamTextSpeed;//用调试乘数纠正速度

        GameObject NewMessageUser = Instantiate(instantiateMessage, parentMessageObj.transform);//实例化对象
        NewMessageUser.name = $"{currentNode.speaker}的消息";//命名
        RectTransform MessageTransform = NewMessageUser.GetComponent<RectTransform>();//获取位置组件

        if (MessageTransform != null) MessageTransform.anchoredPosition = new Vector2(instantiateMessageX, MessageHight);//设置位置
        else { Debug.LogError($"无法正确获取到{NewMessageUser.name}的RectTransform!"); }//报错
        //进入发展的新阶段，实现先代码带动后代码，创建高质量代码增长

        MessageCompany newMessageCompany = NewMessageUser.GetComponent<MessageCompany>();
        newMessageCompany.initializeMessage(currentNode.content,currentNode.speaker,currentNode.speakerAvatar,parentMessageObj, IsStreamText, StreamTextSpeed);//初始化
        //推进代码结构性改革
    }

    private void InstantiateHistoryMessage()
    {
        if (currentDialogueIndex <= 10)
        {
            // 加载0到currentDialogueIndex的所有消息（共currentDialogueIndex+1条）
            for (int i = currentDialogueIndex; i >= 0; i--)
            {
                MyMethod(i);
            }
            Debug.Log($"循环停止,执行{currentDialogueIndex + 1}次");
        }
        else
        {
            // 加载最近10条消息（从currentDialogueIndex-10到currentDialogueIndex-1）
            int start = currentDialogueIndex - 10;
            for (int i = currentDialogueIndex - 1; i >= start; i--)
            {
                MyMethod(i);
            }
            Debug.Log($"循环停止,执行10次");
        }
    }

    void MyMethod(int InstantiateHistoryMessageIndex)
    {
        NextMessage(InstantiateHistoryMessageIndex);
        Debug.Log("执行第 " + InstantiateHistoryMessageIndex + " 次");
    }

    public void NextDialogue()//外部调用
    { 
        DialogueNode currentNode = currentDialogueData.dialogues[currentDialogueIndex];// 处理当前对话的结束事件
        currentDialogueIndex++;// 进入下一条
        NextMessage(currentDialogueIndex);
        Debug.Log($"进入下一条对话，索引: {currentDialogueIndex}");
    }

    private void ExitMessageState()//需要在退出对话时调用
    {
        isInDialogue = false;
        isProcessingDialogue = false;
        if (currentDialogueIndex >= currentDialogueData.dialogues.Count)
        {
            ContactChater.UpdateThisContact(true, currentDialogueData.dialogues.Count);
        }
        else
        {
            Debug.Log("对话已结束");
            ContactChater.UpdateThisContact(false, currentDialogueIndex);//传输是否完结对话，以及当前进行的对话索引
        }
        EventButtonInThisChat.gameObject.SetActive(true);//_________激活按钮，用于继续下一个对话文件

        MessageCompany[] scripts = parentMessageObj.GetComponentsInChildren<MessageCompany>(includeInactive: true);//计算消息数量
        MessageCount = scripts.Length;//更新

        RectTransform ChatEventButtonTransform = EventButtonInThisChat.gameObject.GetComponent<RectTransform>();
        float ButtonHeight =  90 - MessageCount * 30;

        if (ChatEventButtonTransform != null) ChatEventButtonTransform.anchoredPosition = new Vector2(0, ButtonHeight);//设置位置
        else { Debug.LogError($"无法正确获取到{ChatEventButtonTransform.gameObject.name}的RectTransform!"); }//报错

    }

    public void ClearAllChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);// 删除子对象

        }
    }

    private void OnEventButtonClick()
    {
        currentDialogueIndex = 0;
        EventButtonInThisChat.gameObject.SetActive(false);
        currentDialogueData = DialogueLoader.LoadFromResources(ContactChater.GetChatFileName());//更新对话文件
        NextMessage(currentDialogueIndex);//从消息0开始
    }





}
