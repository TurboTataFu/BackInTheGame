using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;

public class ItemPlacer : MonoBehaviour
{
    private Dictionary<Vector2Int,bool> gridDict = new Dictionary<Vector2Int,bool>();
    public Dictionary<Vector2Int, InventoryCell> posToCellDict = new Dictionary<Vector2Int, InventoryCell>();

    private InventoryGrid backpackGrid;

    public GameObject inventoryGridObject; // 挂载 InventoryGrid 脚本的对象（负责网格逻辑）
    public GameObject itemsParent;        // 所有物品的父级对象（负责层级管理）

    public GameObject placerItem;
    private Vector2 placerItemSize;
    private void Start()
    {
        if (itemsParent == null)
        {
            Debug.LogError("未设置背包父级");
            return;
        }
        if (inventoryGridObject == null)
        {
            Debug.LogError("未设置网格管理器");
            return;
        }
        if (placerItem == null)
        {
            Debug.LogError("不知道要放置什么")
                ; return;
        }

        // 获取背包的InventoryGrid组件
        backpackGrid = inventoryGridObject.GetComponent<InventoryGrid>();
        if (backpackGrid == null)
        {
            Debug.LogError("背包上没有挂载InventoryGrid组件！");
            return;
        }
    }

    // 供按钮调用的公共方法，每次调用执行一次放置逻辑
    public void AddItemToBackpack()
    {
        GiveItem2Backpack();
    }

    private List<Vector2Int> GetActiveKeys()
    {
        List<Vector2Int> activeKeys = new List<Vector2Int>();
        foreach(var pair in gridDict)
        {
            if(pair.Value)
            {
                activeKeys.Add(pair.Key);
            }
        }
        return activeKeys;
    }

    private Vector2Int FindMinXyKeys(List<Vector2Int>activeKeys)
    {
        if(activeKeys.Count == 0)
        {
            Debug.Log("没有可用的格子");
            return new Vector2Int(-1,-1);
        }
        Vector2Int minKey = activeKeys[0];

        foreach(var key in activeKeys)
        {
            bool isSmaller = key.x < minKey.x;
            bool isXEqualAndYSmaller =(key.x == minKey.x) && (key.y < minKey.y);

            if(isSmaller || isXEqualAndYSmaller)
            {
                minKey = key;
            }
        }
        return minKey;

    }

    private List<Vector2Int> CalculateNewKeys(GameObject placerItem, Vector2Int minKey, InventoryGrid grid)
    {


        if (minKey.x == -1 || grid == null)
        {
            Debug.LogError("minKey无效或grid为null");
            return new List<Vector2Int>();
        }

        InventoryItemComponent itemComponent = placerItem.GetComponent<InventoryItemComponent>();
        if (itemComponent == null)
        {
            Debug.LogError("placerItem上没有InventoryItemComponent");
            return new List<Vector2Int> { new Vector2Int(-1, -1) };
        }
        Vector2Int originalSize = itemComponent.OriginalSize;

        List<Vector2Int> newKeys = new List<Vector2Int>();

        // 1. 生成基础方向位置，增加可用性检查
        // 左方向
        int leftX = minKey.x - originalSize.x;
        Vector2Int leftPos = new Vector2Int(leftX, minKey.y);
        if (leftPos.x >= 0 && grid.CheckSpaceAvailability(leftPos, originalSize)) // 新增可用性检查
        {
            newKeys.Add(leftPos);
        }

        // 右方向
        Vector2Int rightPos = new Vector2Int(minKey.x + 1, minKey.y);
        if (rightPos.x < grid._gridSize.x && grid.CheckSpaceAvailability(rightPos, originalSize)) // 新增
        {
            newKeys.Add(rightPos);
        }

        // 上方向
        Vector2Int upPos = new Vector2Int(minKey.x, minKey.y + 1);
        if (upPos.y < grid._gridSize.y && grid.CheckSpaceAvailability(upPos, originalSize)) // 新增
        {
            newKeys.Add(upPos);
        }

        // 下方向
        int downY = minKey.y - originalSize.y;
        Vector2Int downPos = new Vector2Int(minKey.x, downY);
        if (downPos.y >= 0 && grid.CheckSpaceAvailability(downPos, originalSize)) // 新增
        {
            newKeys.Add(downPos);
        }

        // 2. 大尺寸物品的斜向位置（1x1物品不会执行这里）
        if (originalSize.x > 2 && originalSize.y > 2)
        {
            List<Vector2Int> diagonalPositions = CheckDiagonalAreas(minKey, originalSize, grid);
            newKeys.AddRange(diagonalPositions);
        }

        // 3. 去重后，再次过滤无效位置（双重保险）
        newKeys = new List<Vector2Int>(new HashSet<Vector2Int>(newKeys));
        newKeys.RemoveAll(pos => pos.x < 0 || pos.y < 0 || pos.x >= grid._gridSize.x || pos.y >= grid._gridSize.y);

        // 4. 如果过滤后候选位置为空，直接使用minKey本身（它是可用格子）
        if (newKeys.Count == 0)
        {
            newKeys.Add(minKey);
            Debug.LogWarning("所有方向位置都无效，使用minKey作为放置位置");
        }

        return newKeys;
    }

    /// <summary>
    /// 检查大尺寸物品（x>2, y>2）的斜向区间格子是否可用
    /// </summary>
    /// <param name="minKey">基准坐标</param>
    /// <param name="itemSize">物品尺寸</param>
    /// <param name="grid">目标网格</param>
    /// <returns>所有可用的斜向起始坐标</returns>
    private List<Vector2Int> CheckDiagonalAreas(Vector2Int minKey, Vector2Int itemSize, InventoryGrid grid)
    {
        List<Vector2Int> validDiagonalPositions = new List<Vector2Int>();
        // 仅处理大尺寸物品（x>2且y>2）
        if (itemSize.x <= 2 || itemSize.y <= 2)
        {
            Debug.Log("物品尺寸小于等于2x2，无需检查斜向区间");
            return validDiagonalPositions;
        }
        // 安全检查：网格为空或基准坐标无效
        if (grid == null || minKey.x == -1)
        {
            return validDiagonalPositions;
        }
        // 定义斜向扩展范围（根据物品尺寸动态调整，确保覆盖足够区域）
        int expandRange = Mathf.Max(itemSize.x, itemSize.y) / 2;

        // 1. 右上方向（x递增，y递增）
        for (int xOffset = 1; xOffset <= expandRange; xOffset++)
        {
            for (int yOffset = 1; yOffset <= expandRange; yOffset++)
            {
                Vector2Int startPos = new Vector2Int(
                    minKey.x + xOffset,  // x向右偏移
                    minKey.y + yOffset   // y向上偏移
                );
                // 检查该位置是否能容纳物品
                if (grid.CheckSpaceAvailability(startPos, itemSize))
                {
                    validDiagonalPositions.Add(startPos);
                }
            }
        }

        // 2. 右下方向（x递增，y递减）
        for (int xOffset = 1; xOffset <= expandRange; xOffset++)
        {
            for (int yOffset = 1; yOffset <= expandRange; yOffset++)
            {
                Vector2Int startPos = new Vector2Int(
                    minKey.x + xOffset,    // x向右偏移
                    minKey.y - yOffset - (itemSize.y - 1)  // y向下偏移（预留物品高度空间）
                );
                if (grid.CheckSpaceAvailability(startPos, itemSize))
                {
                    validDiagonalPositions.Add(startPos);
                }
            }
        }

        // 3. 左上方向（x递减，y递增）
        for (int xOffset = 1; xOffset <= expandRange; xOffset++)
        {
            for (int yOffset = 1; yOffset <= expandRange; yOffset++)
            {
                Vector2Int startPos = new Vector2Int(
                    minKey.x - xOffset - (itemSize.x - 1),  // x向左偏移（预留物品宽度空间）
                    minKey.y + yOffset                     // y向上偏移
                );
                if (grid.CheckSpaceAvailability(startPos, itemSize))
                {
                    validDiagonalPositions.Add(startPos);
                }
            }
        }

        // 4. 左下方向（x递减，y递减）
        for (int xOffset = 1; xOffset <= expandRange; xOffset++)
        {
            for (int yOffset = 1; yOffset <= expandRange; yOffset++)
            {
                Vector2Int startPos = new Vector2Int(
                    minKey.x - xOffset - (itemSize.x - 1),  // x向左偏移
                    minKey.y - yOffset - (itemSize.y - 1)   // y向下偏移
                );
                if (grid.CheckSpaceAvailability(startPos, itemSize))
                {
                    validDiagonalPositions.Add(startPos);
                }
            }
        }

        return validDiagonalPositions;
    }

    private void GiveItem2Backpack()
    {
        InventoryCell[] allGrid = inventoryGridObject.GetComponentsInChildren<InventoryCell>(includeInactive: true);//InventoryGridObject是player背包系统中所有持有的Item的父级
        gridDict.Clear(); // 清空旧数据
        posToCellDict.Clear(); // 清空旧映射

        Debug.Log($"获取到的单元格总数：{allGrid.Length}");

        foreach (InventoryCell cell in allGrid)
        {
            // 跳过网格对象自身和物品父级（原有逻辑保留）
            if (cell.gameObject == inventoryGridObject)
            {
                Debug.Log($"跳过网格对象自身：{cell.name}");
                continue;
            }
            if (cell.gameObject == itemsParent)
                continue;

            Vector2Int cellPos = cell.GridPosition;


            bool isAvailable = (cell.grid2ItemObj == null);

            // 打印单元格状态（保留日志用于调试）
            Debug.Log($"单元格 {cellPos} 状态：grid2ItemObj是否为null={cell.grid2ItemObj == null} → 可用={isAvailable}");

            // 存入字典（原有逻辑保留）
            gridDict[cellPos] = isAvailable;
            posToCellDict[cellPos] = cell;
        }
        int availableCount = gridDict.Values.Count(v => v);
        Debug.Log($"gridDict中可用格子数量：{availableCount}");
        PlaceItemToBackpack(); // 初始化完成后，执行物品放置逻辑
    }

    private void PlaceItemToBackpack()
{
        InventoryItemComponent itemComponent = placerItem.GetComponent<InventoryItemComponent>();
        Vector2Int originalSize = itemComponent.OriginalSize;
        InventoryGrid backpackGrid = inventoryGridObject.GetComponent<InventoryGrid>();


        // 1. 获取所有可用格子（gridDict中value为true的键）
        List<Vector2Int> activeKeys = GetActiveKeys();
    if (activeKeys.Count == 0)
    {
        Debug.LogError("背包已满，无法放置物品！");
        return;
    }
    // 2. 找到可用格子中xy最小的坐标（优先左上角）

    Vector2Int minKey = FindMinXyKeys(activeKeys); // 注意：原方法返回值应为Vector2Int，需修正返回类型

        Debug.Log($"找到的minKey坐标：{minKey}，是否有效（x != -1）：{minKey.x != -1}"); // 新增日志
        if (minKey.x == -1)
        {
            Debug.LogError("未找到合适的起始格子！");
            return;
        }

        Debug.LogWarning($"placerItem 是否为null：{placerItem == null}");
        Debug.LogWarning($"minKey 是否有效：{minKey.x != -1}"); // minKey是结构体，不会为null，但需确认是否有效
        Debug.LogWarning($"backpackGrid 是否为null：{backpackGrid == null}");
        // 3. 计算该位置附近的候选放置位置
        List<Vector2Int> candidatePositions = CalculateNewKeys(placerItem, minKey, backpackGrid);
    if (candidatePositions.Count == 0 || candidatePositions[0].x == -1)
    {
        Debug.LogError("没有找到可放置的位置！");
        return;
    }

        // 4. 遍历候选位置，寻找第一个可用位置
        Vector2Int targetPos = Vector2Int.one * -1; // 初始化为无效值
        foreach (var pos in candidatePositions)
        {
            if (backpackGrid.CheckSpaceAvailability(pos, originalSize)) // 最终校验
            {
                targetPos = pos;
                break; // 找到第一个可用位置就退出
            }
        }

        // 如果没有找到可用位置，报错并返回
        if (targetPos.x == -1)
        {
            Debug.LogError("所有候选位置均被占用，无法放置物品！");
            return;
        }

        // 新增：通过坐标查找对应的单元格
        if (!posToCellDict.TryGetValue(targetPos, out InventoryCell targetCell))
        {
            Debug.LogError($"找不到坐标为 {targetPos} 的单元格！");
            return;
        }

        // 6. 实例化物品，设置父级
        GameObject placedItem = Instantiate(placerItem, itemsParent.transform);

        // 7. 设置物品UI位置和尺寸（你的原有代码）
        RectTransform itemRect = placedItem.GetComponent<RectTransform>();
        RectTransform cellRect = targetCell.GetComponent<RectTransform>();
        itemRect.anchoredPosition = cellRect.anchoredPosition;
        itemRect.sizeDelta = new Vector2(
            cellRect.sizeDelta.x * originalSize.x,
            cellRect.sizeDelta.y * originalSize.y
        );

        // 8. 获取实例化物品对应的 InventoryItem 实例

        InventoryItemComponent itemInstance = placerItem.GetComponent<InventoryItemComponent>();// 这是要绑定到单元格的物品数据

        // 9. 关键：调用目标单元格的 SetState 方法，将物品与单元格绑定
        // 此时单元格的 _storedItem 会被赋值为 itemToBind，完成绑定


        // 10. 其他原有逻辑（设置物品的网格位置、通知网格更新等）
        itemInstance.GridPosition = targetPos;
        backpackGrid.PlaceItem(targetPos, originalSize, itemInstance);


        Debug.Log($"物品已放置到单元格 {targetPos}，父级为 Backpack");
    }
}
