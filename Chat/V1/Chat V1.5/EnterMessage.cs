using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class EnterMessage : MonoBehaviour//2 Start Chat
{
    //滚动视图UI元素好像与RealToon着色器冲突，会出现着色异常

    [Header("聊天记录")]
    public GameObject parentMessageObj;
    // 在Inspector中拖拽你的预制体
    public GameObject NPCMessagePrefab;
    public GameObject UserMessagePrefab;
    public Button EventButtonInThisChat;
    // 存储需要保留的对象,
    private static GameObject _objectToKeep;

    public int multiplierDebugRectHeight;//用于调试滚动视图的大小
    public int addendDebugRectHeight;//用于调试滚动视图的大小

    public RectTransform content; // 拖拽赋值：Content 的 RectTransform

    // 对话数据
    private DialogueSceneData currentDialogueData;
    private int currentDialogueIndex = 0;
    private bool isInDialogue = false; // 是否处于对话状态
    private bool isProcessingDialogue = false; // 是否正在处理对话（防止重复触发）
    private int MessageCount;
    private ContactData ContactInformation;
    private ContactInformation ContactChater;

    private RectTransform ContentRectTransform;

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
        ContentRectTransform = GetComponent<RectTransform>();
        _objectToKeep = EventButtonInThisChat.gameObject;//开始的时候获取按钮对象，它将不会因为消息更新而被删除
        EventButtonInThisChat.gameObject.SetActive(false);//设置按钮状态为关闭，防止误点
        EventButtonInThisChat.onClick.AddListener(OnEventButtonClick);//添加监听以便后续事件
        // 初始化状态（获取初始激活状态）
        _lastActiveState = gameObject.activeSelf;


        if (parentMessageObj == null)
            Debug.LogError("请指定对象！");
        if (EventButtonInThisChat == null)
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
                //补齐历史记录，这里可能功能不全，需要进行改进
            }
        }
        else
        {
            Debug.LogError("没有正确传输对话Json");
        }
    }

    private void NextMessage(int UpdateDialogueIndex)
    {
        bool isOver = isThisChatOver();
        if (isOver)
        {
            return;
        }


        isInDialogue = true;
        isProcessingDialogue = true;//标志开始对话，为后续的其他功能留扩展余地

        MessageCompany[] scripts = parentMessageObj.GetComponentsInChildren<MessageCompany>(includeInactive: true);//计算消息数量
        MessageCount = scripts.Length;//计算下一条信息的位置

        int MessageHight;
        if (MessageCount == 0)//避免为0情况
        {
            MessageHight = 0;
            Debug.Log($"新消息刷新在{MessageHight}高度");
        }
        else
        {
            MessageHight = MessageCount * -30;
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

        Vector2 newSize = ContentRectTransform.sizeDelta;//开始修改面板视图的高度尺寸
        newSize.y = MessageCount * multiplierDebugRectHeight + addendDebugRectHeight; // 修改Y值（高度）
        ContentRectTransform.sizeDelta = newSize;


        RectTransform messageParentRectTranform = parentMessageObj.GetComponent<RectTransform>();//修改message消息的高度，不修改会出现高度错误
        Vector2 newPosition = messageParentRectTranform.anchoredPosition;

        newPosition.y = newSize.y / 2 ; // 基于滚动视图面板的高度尺寸修改message的Y轴位置
        messageParentRectTranform.anchoredPosition = newPosition;

        MessageCompany newMessageCompany = NewMessageUser.GetComponent<MessageCompany>();
        newMessageCompany.initializeMessage(currentNode.content,currentNode.speaker,currentNode.speakerAvatar,parentMessageObj, IsStreamText, StreamTextSpeed);//初始化
        //推进代码结构性改革

        currentDialogueIndex++;//这里是对Json对话信息的索引加一，下一条信息
    }

    private void InstantiateHistoryMessage()//这里可能不包括跨json文件加载的功能
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

    private bool isThisChatOver()
    {
        if (currentDialogueIndex >= currentDialogueData.dialogues.Count)//标志着过去的苦难过去了，新的斗争开始了 copy1
        {
            ExitMessageState();//逃离地球Online
            return true;
        }                      // 对话结束
            return false;
    }

    public void NextDialogue()//外部调用
    {
        bool isOver = isThisChatOver();
        if (isOver)
        { 
            return;
        }

        DialogueNode currentNode = currentDialogueData.dialogues[currentDialogueIndex];// 处理当前对话的结束事件
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
        EventButtonInThisChat.gameObject.SetActive(true);//_________激活按钮，用于继续下一个对话文件（可能为了剧情进度需要限制手段）

        MessageCompany[] scripts = parentMessageObj.GetComponentsInChildren<MessageCompany>(includeInactive: true);//计算消息数量
        MessageCount = scripts.Length;//更新

        RectTransform ChatEventButtonTransform = EventButtonInThisChat.gameObject.GetComponent<RectTransform>();

        float ButtonHeight = MessageCount * -30;//Button的高度计算

        if (ChatEventButtonTransform != null) ChatEventButtonTransform.anchoredPosition = new Vector2(0, ButtonHeight);//设置位置
        else { Debug.LogError($"无法正确获取到{ChatEventButtonTransform.gameObject.name}的RectTransform!"); }//报错

    }

    public static void ClearAllChildren(Transform parent)
    {
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;

            // 使用内部全局变量判断是否需要保留
            if (child == _objectToKeep)
                continue;

            Destroy(child);
        }
    }
    private void OnEventButtonClick()
    {
        currentDialogueIndex = 0;//重置index
        EventButtonInThisChat.gameObject.SetActive(false);
        currentDialogueData = DialogueLoader.LoadFromResources(ContactChater.GetChatFileName());//更新对话文件
        NextMessage(currentDialogueIndex);//从消息0开始
    }
}
