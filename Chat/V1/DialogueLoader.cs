using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class DialogueLoader
{
    /// <summary>
    /// 从Resources文件夹加载对话数据
    /// </summary>
    /// <param name="jsonFileName">JSON文件名（不含扩展名）</param>
    /// <returns>解析后的对话场景数据</returns>
    public static DialogueSceneData LoadFromResources(string jsonFileName)
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(jsonFileName);
        if (jsonAsset == null)
        {
            Debug.LogError($"未找到资源文件: {jsonFileName}");
            return null;
        }

        try
        {
            // 使用Newtonsoft.Json解析JSON
            return JsonConvert.DeserializeObject<DialogueSceneData>(jsonAsset.text);
        }
        catch (JsonException ex)
        {
            Debug.LogError($"JSON解析错误: {ex.Message}");
            return null;
        }
    }
}
