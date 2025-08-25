using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Newtonsoft.Json;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI引用")]
    [Tooltip("显示说话者名字的文本组件")]
    public Text speakerText;
    [Tooltip("显示对话内容的文本组件")]
    public Text contentText;
    [Tooltip("角色头像显示组件")]
    public Image speakerAvatar;
    [Tooltip("背景显示组件")]
    public Image backgroundImage;

    [Header("资源路径")]
    [Tooltip("头像资源存放路径（Resources下）")]
    public string avatarResourcePath = "Avatars/";
    [Tooltip("背景资源存放路径（Resources下）")]
    public string backgroundResourcePath = "Backgrounds/";


    

    // 对话数据
    private DialogueSceneData currentDialogueData;
    private int currentDialogueIndex = 0;
    private bool isInDialogue = false; // 是否处于对话状态
    private bool isProcessingDialogue = false; // 是否正在处理对话（防止重复触发）


    public GameObject objectToEnable;
    private Coroutine autoPlayCoroutine;
    // 公开的初始化方法（供外部调用）
    public void Initialize(string dialogueFileName)//isChat为True时为聊天对话//False为Galgame图片小说对话
    {
        // 加载对话数据
        currentDialogueData = DialogueLoader.LoadFromResources(dialogueFileName);

        if (currentDialogueData != null)
        {
            EnterDialogueState();
            // 加载初始背景
            SetBackground(currentDialogueData.background);
            // 显示第一条对话
            currentDialogueIndex = 0;

            ShowCurrentDialogue();
 
            
        }
        else
        {
            Debug.LogError("对话初始化失败，数据为空");
        }
    }

    // 进入对话状态
    private void EnterDialogueState()
    {
        isInDialogue = true;
        // 显示对话UI
        speakerText.gameObject.SetActive(true);
        contentText.gameObject.SetActive(true);
        speakerAvatar.gameObject.SetActive(true);
        // 暂停游戏时间（可选）
        Time.timeScale = 0;
    }

    // 显示当前对话
    private void ShowCurrentDialogue()
    {
        if (currentDialogueIndex >= currentDialogueData.dialogues.Count)
        {
            // 对话结束
            ExitDialogueState();
            return;
        }

        isProcessingDialogue = true;
        DialogueNode currentNode = currentDialogueData.dialogues[currentDialogueIndex];

        // 更新UI显示
        speakerText.text = currentNode.speaker;
        LoadAvatar(currentNode.speakerAvatar);

        // 清空文本并开始显示（这里简化处理，实际可实现逐字显示）
        contentText.text = currentNode.content;

        // 处理"显示时触发的事件"
        ProcessEvent(currentNode.eventOnDisplay);

        // 如果设置了自动播放下一条，延迟后自动切换
        // 修改ShowCurrentDialogue中启动协程的代码
        if (currentNode.autoPlayNext)
        {
            float delay = currentNode.content.Length * 0.1f * (1 / currentNode.textSpeed);
            // 记录协程引用，方便后续中断
            autoPlayCoroutine = StartCoroutine(AutoPlayNext(delay));
        }
        else
        {
            isProcessingDialogue = false;
        }

    }

    // 自动播放下一条对话的协程
    private IEnumerator AutoPlayNext(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // 用Realtime不受Time.timeScale影响
        NextDialogue();
    }

    // 处理事件字符串（格式："事件类型:参数"）
    private void ProcessEvent(string eventString)
    {
        if (string.IsNullOrEmpty(eventString)) return;

        string[] eventParts = eventString.Split(':');
        if (eventParts.Length < 1) return;

        switch (eventParts[0])
        {
            case "play_sound":
                if (eventParts.Length > 1)
                    PlaySound(eventParts[1]);
                break;
            case "change_background":
                if (eventParts.Length > 1)
                    SetBackground(eventParts[1]);
                break;
            case "show_emoji":
                if (eventParts.Length > 1)
                    ShowEmoji(eventParts[1]);
                break;
            case "exit_dialogue":
                ExitDialogueState();
                break;
            default:
                Debug.LogWarning($"未处理的事件类型: {eventParts[0]}");
                break;
        }
    }

    // 下一条对话（公开方法，可被UI按钮调用）
    public void NextDialogue()
    {
        // 调试日志：跟踪当前状态
        Debug.Log($"NextDialogue触发 - isInDialogue: {isInDialogue}, isProcessingDialogue: {isProcessingDialogue}");

        if (!isInDialogue || isProcessingDialogue)
        {
            Debug.LogWarning("NextDialogue被阻塞 - 状态异常");
            return;
        }

        // 处理当前对话的结束事件
        DialogueNode currentNode = currentDialogueData.dialogues[currentDialogueIndex];
        ProcessEvent(currentNode.eventOnFinish);

        // 进入下一条
        currentDialogueIndex++;
        Debug.Log($"进入下一条对话，索引: {currentDialogueIndex}");
        ShowCurrentDialogue();
    }

    // 退出对话状态
    public void ExitDialogueState()
    {
        isInDialogue = false;
        isProcessingDialogue = false;

        // 隐藏对话UI
        speakerText.gameObject.SetActive(false);
        contentText.gameObject.SetActive(false);
        speakerAvatar.gameObject.SetActive(false);
        objectToEnable.SetActive(false);
        // 恢复游戏时间
        Time.timeScale = 1;

        // 触发对话结束事件（可用于通知其他系统）
        OnDialogueExit();
    }

    // 对话结束时的回调（可扩展）
    private void OnDialogueExit()
    {
        Debug.Log("对话已结束");
        // 这里可以添加对话结束后的逻辑，如触发剧情、返回主菜单等
    }

    // 加载角色头像
    private void LoadAvatar(string avatarName)
    {
        if (string.IsNullOrEmpty(avatarName))
        {
            speakerAvatar.enabled = false;
            return;
        }

        Sprite avatar = Resources.Load<Sprite>($"{avatarResourcePath}{avatarName}");
        if (avatar != null)
        {
            speakerAvatar.sprite = avatar;
            speakerAvatar.enabled = true;
        }
        else
        {
            Debug.LogWarning($"未找到头像资源: {avatarName}");
            speakerAvatar.enabled = false;
        }
    }

    // 设置背景
    // 设置背景（添加状态重置逻辑）
    private void SetBackground(string backgroundName)
    {
        if (backgroundImage == null) return;

        Sprite bg = Resources.Load<Sprite>($"{backgroundResourcePath}{backgroundName}");
        if (bg != null)
        {
            backgroundImage.sprite = bg;
        }
        else
        {
            Debug.LogWarning($"未找到背景资源: {backgroundName}");
        }

        // 关键：处理背景切换时的状态重置（与PlaySound/ShowEmoji保持一致）
        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }
        if (isProcessingDialogue)
        {
            isProcessingDialogue = false;
        }
    }

    // 播放音效（示例方法，需结合实际音频系统）
    private void PlaySound(string soundName)
    {
        // 实际项目中这里会调用音频管理器播放对应音效
        Debug.Log($"播放音效: {soundName}");

        // 若正在自动播放，先终止协程
        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }
        // 无论是否在处理中，强制尝试进入下一条（重置状态）
        if (isProcessingDialogue)
        {
            isProcessingDialogue = false;
        }
    }

    // 显示表情符号（示例方法）
    private void ShowEmoji(string emojiName)
    {
        // 实际项目中这里会显示对应表情UI
        Debug.Log($"显示表情: {emojiName}");

        // 若正在自动播放，先终止协程
        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }
        // 无论是否在处理中，强制尝试进入下一条（重置状态）
        if (isProcessingDialogue)
        {
            isProcessingDialogue = false;
        }
    }


    // 监听空格键输入
    private void Update()
    {
        if (isInDialogue && Input.GetKeyDown(KeyCode.Space))
        {
            // 无论当前是否在处理中，强制终止自动播放并重置状态
            if (autoPlayCoroutine != null)
            {
                StopCoroutine(autoPlayCoroutine);
                autoPlayCoroutine = null;
            }
            if (isProcessingDialogue)
            {
                isProcessingDialogue = false;
            }
            NextDialogue(); // 此时NextDialogue的条件会通过（因为isProcessingDialogue已设为false）
        }

        // Esc退出逻辑不变
        if (isInDialogue && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitDialogueState();
        }
    }
}
