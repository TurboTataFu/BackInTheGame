using System.Collections.Generic;
using Newtonsoft.Json;

// 对话场景数据（包含整个场景的对话信息）
public class DialogueSceneData
{
    public string sceneId { get; set; } = "";
    public string background { get; set; } = "";
    public List<DialogueNode> dialogues { get; set; } = new List<DialogueNode>();
}

// 单条对话节点（扩展版）
public class DialogueNode
{
    public int id { get; set; }
    public string speaker { get; set; } = "";
    public string speakerAvatar { get; set; } = ""; // 头像资源名
    public string content { get; set; } = "";

    // 音频相关字段
    public string voice { get; set; } = ""; // 语音文件名称（无扩展名）
    public string sfx { get; set; } = "";   // 对话时播放的音效（如叹气、笑声）

    // 新增扩展字段
    public string expression { get; set; } = "default"; // 表情状态
    public float textSpeed { get; set; } = 1.0f; // 文本速度（1.0为默认）
    public bool autoPlayNext { get; set; } = false; // 是否自动播放下一条
    public string eventOnDisplay { get; set; } = ""; // 显示时触发的事件
    public string eventOnFinish { get; set; } = ""; // 结束时触发的事件
    public List<ChoiceNode> choices { get; set; } = new List<ChoiceNode>(); // 保留分支选项
}

// 分支选项类（保持不变）
public class ChoiceNode
{
    public string text { get; set; } = "";
    public int nextDialogueId { get; set; }
    public string unlockFlag { get; set; } = "";
}
