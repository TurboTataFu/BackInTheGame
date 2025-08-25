using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CraftManager : MonoBehaviour
{
    public GameObject debug;
    public GameObject backpackObj;
    public Image icon;
    public Text itemName;
    private Dictionary<string, int> idCountDict = new Dictionary<string, int>();//制作材料
    private InventoryItem CraftGiveItem;
    public ItemPlacer Placer;//放置器

    // 调试按钮点击
    public void onDebugButtonDown()
    {
        Debug.Log("=== 开始执行onDebugButtonDown ===");
        InventoryItemComponent item = debug.GetComponent<InventoryItemComponent>();

        if (item == null)
        {
            Debug.LogError("debug对象上未找到InventoryItemComponent组件");
            return;
        }
        if (item.ItemData == null)
        {
            Debug.LogError("debug对象的ItemData为null");
            return;
        }

        Debug.Log($"获取到调试物品: {item.ItemData.ItemName} (ID: {item.ItemID})");
        UpdateSelechedItem(item.ItemData);
        OnCraftButton();
        Debug.Log("=== 结束执行onDebugButtonDown ===");
    }

    // 更新选中物品信息
    public void UpdateSelechedItem(InventoryItem SelechedItem)
    {
        Debug.Log($"=== 开始更新选中物品: {SelechedItem?.ItemName ?? "null"} ===");
        idCountDict.Clear();

        if (SelechedItem == null)
        {
            Debug.LogError("UpdateSelechedItem: 选中物品为null");
            return;
        }

        // 更新UI显示
        icon.sprite = SelechedItem.Icon;
        itemName.text = SelechedItem.ItemName;
        Debug.Log($"更新UI显示: 物品名称={SelechedItem.ItemName}, 图标={(SelechedItem.Icon != null ? "存在" : "不存在")}");

        // 转换配方字典
        idCountDict = ConvertToIdCountDict(SelechedItem.craftItem);
        CraftGiveItem = SelechedItem;

        // 打印配方详情
        Debug.Log($"制作配方详情 (共{idCountDict.Count}种材料):");
        foreach (var pair in idCountDict)
        {
            Debug.Log($"  - 材料ID: {pair.Key}, 所需数量: {pair.Value}");
        }
        Debug.Log($"=== 完成更新选中物品 ===");
    }

    // 制作按钮点击
    public void OnCraftButton()
    {
        Debug.Log("=== 开始执行OnCraftButton ===");

        // 检查基础数据是否有效
        if (idCountDict == null || idCountDict.Count == 0)
        {
            Debug.LogWarning("OnCraftButton: 配方字典为空");
            return;
        }
        if (CraftGiveItem == null)
        {
            Debug.LogWarning("OnCraftButton: 目标制作物品为null");
            return;
        }
        if (backpackObj == null)
        {
            Debug.LogError("OnCraftButton: backpackObj未赋值");
            return;
        }

        // 获取物品管理器
        ItemFoundManager itemFoundManager = GetComponent<ItemFoundManager>();
        if (itemFoundManager == null)
        {
            Debug.LogError("OnCraftButton: 未找到ItemFoundManager组件");
            return;
        }

        // 查找背包物品
        itemFoundManager.FoundForItem();
        Dictionary<string, int> backpackItemHeld = itemFoundManager.backpackDataDict;

        if (backpackItemHeld == null)
        {
            Debug.LogError("OnCraftButton: 背包数据字典为null");
            return;
        }

        // 打印当前背包持有物品
        Debug.Log($"当前背包物品 (共{backpackItemHeld.Count}种):");
        foreach (var pair in backpackItemHeld)
        {
            Debug.Log($"  - 物品ID: {pair.Key}, 持有数量: {pair.Value}");
        }

        // 检查是否满足制作条件
        bool canOrNotCraft = CheckBackPack2Craft(backpackItemHeld, idCountDict);
        Debug.Log($"制作条件检查结果: {(canOrNotCraft ? "满足" : "不满足")}");
        if (!canOrNotCraft)
        {
            Debug.Log("OnCraftButton: 不满足制作条件，终止流程");
            return;
        }

        // 筛选匹配的物品对象
        List<GameObject> matchingObjects = new List<GameObject>();
        InventoryItemComponent[] allItem = backpackObj.GetComponentsInChildren<InventoryItemComponent>(true); // 包含禁用对象
        Debug.Log($"从背包中找到{allItem.Length}个物品组件");

        foreach (InventoryItemComponent component in allItem)
        {
            // 先检查组件和ItemData是否有效（关键防御性判断）
            if (component == null)
            {
                Debug.LogWarning("发现空的InventoryItemComponent组件，跳过");
                continue;
            }
            if (component.ItemData == null)
            {
                Debug.LogWarning($"物品对象{component.gameObject.name}的ItemData为null，跳过");
                continue;
            }

            // 核心修改：使用ScriptableObject上的固定itemID进行匹配
            string fixedItemId = component.ItemData.itemID;
            if (backpackItemHeld.ContainsKey(fixedItemId))
            {
                matchingObjects.Add(component.gameObject);
                Debug.Log($"匹配到物品: 固定ID={fixedItemId}, 对象名称={component.gameObject.name}");
            }
        }
        Debug.Log($"共匹配到{matchingObjects.Count}个符合条件的物品对象");

        // 处理物品消耗
        ProcessItems(idCountDict, matchingObjects);
        Debug.Log("=== 结束执行OnCraftButton ===");
    }

    // 处理物品消耗逻辑
    private void ProcessItems(Dictionary<string, int> demandDict, List<GameObject> itemObjects)
    {
        Debug.Log("=== 开始处理物品消耗 ===");
        Dictionary<string, int> tempDemand = new Dictionary<string, int>(demandDict);

        // 打印初始需求
        Debug.Log("初始制作需求:");
        foreach (var pair in tempDemand)
        {
            Debug.Log($"  - {pair.Key}: 需要{pair.Value}个");
        }

        foreach (var key in new List<string>(tempDemand.Keys))
        {
            int currentDemand = tempDemand[key];
            if (currentDemand <= 0)
            {
                Debug.Log($"材料{key}需求已满足，跳过处理");
                continue;
            }

            // 筛选匹配的物品
            List<GameObject> matchedObjects = itemObjects.FindAll(obj =>
            {
                var holder = obj.GetComponent<InventoryItemComponent>();
                return holder != null && holder.ItemData.itemID == key;
            });
            Debug.Log($"为材料{key}找到{matchedObjects.Count}个匹配物品");

            // 处理每个匹配物品
            foreach (var obj in matchedObjects)
            {
                if (currentDemand <= 0) break;

                InventoryItemComponent holder = obj.GetComponent<InventoryItemComponent>();
                if (holder == null)
                {
                    Debug.LogWarning($"物品对象{obj.name}上未找到InventoryItemComponent");
                    continue;
                }

                int hold = holder.CurrentCount;
                Debug.Log($"处理物品: ID={holder.ItemData.itemID}, 持有数量={hold}, 当前仍需={currentDemand}");

                if (hold < currentDemand)
                {
                    currentDemand -= hold;
                    Destroy(obj);
                    Debug.Log($"  销毁物品{obj.name}，剩余需求: {currentDemand}");
                }
                else if (hold > currentDemand)
                {
                    holder.CurrentCount = hold - currentDemand;
                    currentDemand = 0;
                    Debug.Log($"  减少物品数量至{holder.CurrentCount}，需求已满足");
                }
                else
                {
                    Destroy(obj);
                    currentDemand = 0;
                    Debug.Log($"  销毁物品{obj.name}，需求已满足");
                }
            }

            tempDemand[key] = currentDemand;
            Debug.Log($"材料{key}处理完毕，最终剩余需求: {currentDemand}");
        }

        // 检查是否所有需求都满足
        bool allMet = tempDemand.Values.All(d => d <= 0);
        Debug.Log($"所有材料处理完毕，是否全部满足需求: {allMet}");

        if (allMet)
        {
            Debug.Log("开始放置制作完成的物品");
            if (Placer == null)
            {
                Debug.LogError("Placer未赋值，无法放置物品");
                return;
            }
            Placer.AddItemToBackpack();
        }
        Debug.Log("=== 结束处理物品消耗 ===");
    }

    // 检查背包是否满足制作条件
    public static bool CheckBackPack2Craft(
        Dictionary<string, int> aDict,//背包Dict
        Dictionary<string, int> bDict)//制作配方Dict
    {
        Debug.Log("=== 开始检查制作条件 ===");

        if (bDict == null || bDict.Count == 0)
        {
            Debug.Log("配方为空，无需材料即可制作");
            return true;
        }

        if (aDict == null || aDict.Count == 0)
        {
            Debug.LogError("背包为空，但配方需要材料，无法制作");
            return false;
        }

        foreach (var pair in bDict)
        {
            string key = pair.Key;
            int bValue = pair.Value;

            if (!aDict.TryGetValue(key, out int aValue))
            {
                Debug.LogError($"背包中缺少必要材料: {key}");
                return false;
            }

            if (aValue < bValue) // 注意：原代码是aValue <= bValue，这里修正为<（数量足够即可）
            {
                Debug.LogError($"材料{key}数量不足: 持有{aValue}，需要{bValue}");
                return false;
            }

            Debug.Log($"材料{key}满足条件: 持有{aValue} >= 需要{bValue}");
        }

        Debug.Log("=== 所有材料均满足制作条件 ===");
        return true;
    }

    // 转换ID列表为计数字典
    private static Dictionary<string, int> ConvertToIdCountDict(List<string> idList)
    {
        Debug.Log("=== 开始转换ID列表为计数字典 ===");
        Dictionary<string, int> idCountDict = new Dictionary<string, int>();

        if (idList == null || idList.Count == 0)
        {
            Debug.Log("输入的ID列表为空");
            return idCountDict;
        }

        Debug.Log($"输入的ID列表共{idList.Count}个元素: {string.Join(",", idList)}");
        foreach (string id in idList)
        {
            if (idCountDict.ContainsKey(id))
            {
                idCountDict[id]++;
            }
            else
            {
                idCountDict.Add(id, 1);
            }
        }

        // 打印转换结果
        Debug.Log("转换后的计数字典:");
        foreach (var pair in idCountDict)
        {
            Debug.Log($"  - {pair.Key}: {pair.Value}次");
        }
        Debug.Log("=== 完成转换ID列表 ===");
        return idCountDict;
    }
}
