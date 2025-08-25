using Newtonsoft.Json;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class EnterMessage : MonoBehaviour//2 Start Chat
{

    [Header("�����¼")]
    public GameObject parentMessageObj;
    // ��Inspector����ק���Ԥ����
    public GameObject NPCMessagePrefab;
    public GameObject UserMessagePrefab;
    public Button EventButtonInThisChat;

    public RectTransform content; // ��ק��ֵ��Content �� RectTransform

    // �Ի�����
    private DialogueSceneData currentDialogueData;
    private int currentDialogueIndex = 0;
    private bool isInDialogue = false; // �Ƿ��ڶԻ�״̬
    private bool isProcessingDialogue = false; // �Ƿ����ڴ���Ի�����ֹ�ظ�������
    private int MessageCount;
    private ContactData ContactInformation;
    private ContactInformation ContactChater;

    public float DebugStreamTextSpeed;
    public bool IsStreamText;
    private bool _lastActiveState;
    void Update()
    {
        bool currentActiveState = gameObject.activeSelf;// ��ȡ��ǰ����״̬
        if (currentActiveState != _lastActiveState 
            && isInDialogue == true 
            && isProcessingDialogue == true
            )
        {
            ExitMessageState();//�����Ա���
            _lastActiveState = currentActiveState;//����״̬
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);//debug
    }

    private void Start()
    {
        EventButtonInThisChat.gameObject.SetActive(false);
        EventButtonInThisChat.onClick.AddListener(OnEventButtonClick);
        // ��ʼ��״̬����ȡ��ʼ����״̬��
        _lastActiveState = gameObject.activeSelf;


        if (parentMessageObj == null)
            Debug.LogError("��ָ������");
    }

    public void StartChat(string dialogueFileName,ContactInformation contactChater,ContactData SpeakerData,int MessageHistoryIndex)
    {

        ContactChater = contactChater;//��ʼ��
        ContactInformation = SpeakerData;//��ʼ����ϵ����Ϣ
        currentDialogueData = DialogueLoader.LoadFromResources(dialogueFileName);

        if (currentDialogueData != null)
        {
            currentDialogueIndex = MessageHistoryIndex;
            if (currentDialogueIndex == 0)
            {
                NextMessage(currentDialogueIndex);//����ʷ��¼��ֱ����һ��
            }
            else
            {
                InstantiateHistoryMessage();
                //������ʷ��¼
            }
        }
        else
        {
            Debug.LogError("û����ȷ����Ի�Json");
        }
    }

    private void NextMessage(int UpdateDialogueIndex)
    {
        if (currentDialogueIndex >= currentDialogueData.dialogues.Count)//��־�Ź�ȥ�Ŀ��ѹ�ȥ�ˣ��µĶ�����ʼ��
        { ExitMessageState();//�������Online
            return; }   // �Ի�����


        isInDialogue = true;
        isProcessingDialogue = true;//��־�Ŵ��뷢չ�����˿��ٷ�չ��������

        MessageCompany[] scripts = parentMessageObj.GetComponentsInChildren<MessageCompany>(includeInactive: true);//������Ϣ����
        MessageCount = scripts.Length;//������һ����Ϣ��λ��

        int MessageHight;
        if (MessageCount == 0)//����Ϊ0���
        {
            MessageHight = 90;
            Debug.Log($"����Ϣˢ����{MessageHight}�߶�");
        }
        else
        {
            MessageHight = 90 - MessageCount * 30;
            Debug.Log($"����Ϣˢ����{MessageHight}�߶�");
        }

        DialogueNode currentNode = currentDialogueData.dialogues[UpdateDialogueIndex];//��ȡ��Ϣ��json

        GameObject instantiateMessage;
        float instantiateMessageX;//�������˭������Ϣ
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
        float StreamTextSpeed = currentNode.textSpeed * DebugStreamTextSpeed;//�õ��Գ��������ٶ�

        GameObject NewMessageUser = Instantiate(instantiateMessage, parentMessageObj.transform);//ʵ��������
        NewMessageUser.name = $"{currentNode.speaker}����Ϣ";//����
        RectTransform MessageTransform = NewMessageUser.GetComponent<RectTransform>();//��ȡλ�����

        if (MessageTransform != null) MessageTransform.anchoredPosition = new Vector2(instantiateMessageX, MessageHight);//����λ��
        else { Debug.LogError($"�޷���ȷ��ȡ��{NewMessageUser.name}��RectTransform!"); }//����
        //���뷢չ���½׶Σ�ʵ���ȴ����������룬������������������

        MessageCompany newMessageCompany = NewMessageUser.GetComponent<MessageCompany>();
        newMessageCompany.initializeMessage(currentNode.content,currentNode.speaker,currentNode.speakerAvatar,parentMessageObj, IsStreamText, StreamTextSpeed);//��ʼ��
        //�ƽ�����ṹ�Ըĸ�
    }

    private void InstantiateHistoryMessage()
    {
        if (currentDialogueIndex <= 10)
        {
            // ����0��currentDialogueIndex��������Ϣ����currentDialogueIndex+1����
            for (int i = currentDialogueIndex; i >= 0; i--)
            {
                MyMethod(i);
            }
            Debug.Log($"ѭ��ֹͣ,ִ��{currentDialogueIndex + 1}��");
        }
        else
        {
            // �������10����Ϣ����currentDialogueIndex-10��currentDialogueIndex-1��
            int start = currentDialogueIndex - 10;
            for (int i = currentDialogueIndex - 1; i >= start; i--)
            {
                MyMethod(i);
            }
            Debug.Log($"ѭ��ֹͣ,ִ��10��");
        }
    }

    void MyMethod(int InstantiateHistoryMessageIndex)
    {
        NextMessage(InstantiateHistoryMessageIndex);
        Debug.Log("ִ�е� " + InstantiateHistoryMessageIndex + " ��");
    }

    public void NextDialogue()//�ⲿ����
    { 
        DialogueNode currentNode = currentDialogueData.dialogues[currentDialogueIndex];// ����ǰ�Ի��Ľ����¼�
        currentDialogueIndex++;// ������һ��
        NextMessage(currentDialogueIndex);
        Debug.Log($"������һ���Ի�������: {currentDialogueIndex}");
    }

    private void ExitMessageState()//��Ҫ���˳��Ի�ʱ����
    {
        isInDialogue = false;
        isProcessingDialogue = false;
        if (currentDialogueIndex >= currentDialogueData.dialogues.Count)
        {
            ContactChater.UpdateThisContact(true, currentDialogueData.dialogues.Count);
        }
        else
        {
            Debug.Log("�Ի��ѽ���");
            ContactChater.UpdateThisContact(false, currentDialogueIndex);//�����Ƿ����Ի����Լ���ǰ���еĶԻ�����
        }
        EventButtonInThisChat.gameObject.SetActive(true);//_________���ť�����ڼ�����һ���Ի��ļ�

        MessageCompany[] scripts = parentMessageObj.GetComponentsInChildren<MessageCompany>(includeInactive: true);//������Ϣ����
        MessageCount = scripts.Length;//����

        RectTransform ChatEventButtonTransform = EventButtonInThisChat.gameObject.GetComponent<RectTransform>();
        float ButtonHeight =  90 - MessageCount * 30;

        if (ChatEventButtonTransform != null) ChatEventButtonTransform.anchoredPosition = new Vector2(0, ButtonHeight);//����λ��
        else { Debug.LogError($"�޷���ȷ��ȡ��{ChatEventButtonTransform.gameObject.name}��RectTransform!"); }//����

    }

    public void ClearAllChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);// ɾ���Ӷ���

        }
    }

    private void OnEventButtonClick()
    {
        currentDialogueIndex = 0;
        EventButtonInThisChat.gameObject.SetActive(false);
        currentDialogueData = DialogueLoader.LoadFromResources(ContactChater.GetChatFileName());//���¶Ի��ļ�
        NextMessage(currentDialogueIndex);//����Ϣ0��ʼ
    }





}
