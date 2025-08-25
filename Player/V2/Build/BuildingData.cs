using UnityEngine;

// 建筑数据结构（存储单种建筑信息）
[System.Serializable]
public class BuildingData
{
    [Tooltip("建筑分类（如'住宅'、'工业'）")]
    public string category;
    [Tooltip("建筑唯一标识（同一分类内不可重复）")]
    public string id;
    [Tooltip("建筑预制件")]
    public GameObject prefab;
    [Tooltip("建筑图标（用于按钮显示）")]
    public Sprite icon;

    // 验证数据是否完整
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(category) &&
               !string.IsNullOrEmpty(id) &&
               prefab != null;
    }

    // 显示名称（用于编辑器）
    public string DisplayName => $"{category}/{id}";
}
