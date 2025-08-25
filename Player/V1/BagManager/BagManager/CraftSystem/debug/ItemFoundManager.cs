using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem.iOS;
using UnityEngine.Animations;
public class ItemFoundManager : MonoBehaviour
{
    public GameObject backpackObj;
    public Dictionary<string, int> backpackDataDict = new Dictionary<string, int>();//字典（键值对应）
    private void Start()
    {
        if (backpackObj == null)
        {
            Debug.LogError("没有指定背包");
            return;//报错阻止运行
        }

        FoundForItem();
        PrintDictForBackpack();
    }
    public void FoundForItem()
    {
        // 1. 清空现有数据（避免多次调用累积，根据需求决定是否保留）
        backpackDataDict.Clear();

        // 2. 获取背包下所有物品组件（只需要InventoryItemComponent，它关联了物品数据和数量）
        InventoryItemComponent[] allItemComponents = backpackObj.GetComponentsInChildren<InventoryItemComponent>();

        // 3. 检查组件数组是否有效
        if (allItemComponents == null || allItemComponents.Length == 0)
        {
            Debug.LogWarning("背包中没有找到物品组件");
            return;
        }

        // 4. 遍历所有物品组件，收集数据
        foreach (InventoryItemComponent itemComp in allItemComponents)
        {
            // 跳过无效组件（如未关联物品数据的情况）
            if (itemComp == null || itemComp.ItemData == null)
            {
                Debug.LogWarning("存在无效的物品组件");
                continue;
            }

            // 获取物品ID和当前数量（从组件关联的数据中获取）
            string itemId = itemComp.ItemData.itemID;
            int itemHeld = itemComp.CurrentCount;

            // 合并同类项（累加数量）
            if (backpackDataDict.ContainsKey(itemId))
            {
                backpackDataDict[itemId] += itemHeld;
            }
            else
            {
                backpackDataDict.Add(itemId, itemHeld);
            }
        }

        Debug.Log($"收集完成，共 {backpackDataDict.Count} 种物品");
        PrintDictForBackpack();
    }

    private void PrintDictForBackpack()//打印调试,输出代码执行结果
    {
        Debug.Log("\n===== 物品ID-数量对照表 =====");
        foreach (var pair in backpackDataDict)
        {
            Debug.Log($"ID: {pair.Key} → 数量: {pair.Value}");
        }
    }
}
