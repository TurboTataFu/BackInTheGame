using System.Collections.Generic;
using Newtonsoft.Json;

// �Ի��������ݣ��������������ĶԻ���Ϣ��
public class DialogueSceneData
{
    public string sceneId { get; set; } = "";
    public string background { get; set; } = "";
    public List<DialogueNode> dialogues { get; set; } = new List<DialogueNode>();
}

// �����Ի��ڵ㣨��չ�棩
public class DialogueNode
{
    public int id { get; set; }
    public string speaker { get; set; } = "";
    public string speakerAvatar { get; set; } = ""; // ͷ����Դ��
    public string content { get; set; } = "";

    // ��Ƶ����ֶ�
    public string voice { get; set; } = ""; // �����ļ����ƣ�����չ����
    public string sfx { get; set; } = "";   // �Ի�ʱ���ŵ���Ч����̾����Ц����

    // ������չ�ֶ�
    public string expression { get; set; } = "default"; // ����״̬
    public float textSpeed { get; set; } = 1.0f; // �ı��ٶȣ�1.0ΪĬ�ϣ�
    public bool autoPlayNext { get; set; } = false; // �Ƿ��Զ�������һ��
    public string eventOnDisplay { get; set; } = ""; // ��ʾʱ�������¼�
    public string eventOnFinish { get; set; } = ""; // ����ʱ�������¼�
    public List<ChoiceNode> choices { get; set; } = new List<ChoiceNode>(); // ������֧ѡ��
}

// ��֧ѡ���ࣨ���ֲ��䣩
public class ChoiceNode
{
    public string text { get; set; } = "";
    public int nextDialogueId { get; set; }
    public string unlockFlag { get; set; } = "";
}
