using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class DialogueLoader
{
    /// <summary>
    /// ��Resources�ļ��м��ضԻ�����
    /// </summary>
    /// <param name="jsonFileName">JSON�ļ�����������չ����</param>
    /// <returns>������ĶԻ���������</returns>
    public static DialogueSceneData LoadFromResources(string jsonFileName)
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(jsonFileName);
        if (jsonAsset == null)
        {
            Debug.LogError($"δ�ҵ���Դ�ļ�: {jsonFileName}");
            return null;
        }

        try
        {
            // ʹ��Newtonsoft.Json����JSON
            return JsonConvert.DeserializeObject<DialogueSceneData>(jsonAsset.text);
        }
        catch (JsonException ex)
        {
            Debug.LogError($"JSON��������: {ex.Message}");
            return null;
        }
    }
}
