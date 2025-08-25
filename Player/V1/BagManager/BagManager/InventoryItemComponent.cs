// InventoryItemComponent.cs
using System;
using System.Collections.Generic;
using UnityEngine;//物品实例


public class InventoryItemComponent : MonoBehaviour
{


    public string ItemID; // 关联静态数据
    public int CurrentCount; // 当前持有数量
    public List<Vector2Int> GridOccupied = new List<Vector2Int>();
    public bool isRotate; 

    // 添加 GridPosition 属性
    public Vector2Int GridPosition
    {
        get => _itemData.GridPosition;
        set => _itemData.GridPosition = value;
    }

    public bool stackable = false;//是否可堆叠

    // InventoryItemComponent.cs 修改
    public ItemType itemType; // 替换原来的 public float itemType11

    public string ItemName = "未命名";
    [TextArea]
    public string Title = "介绍";


    public void UpdateItemCount2Destroy()    //当数量耗尽时删除Item
    {
        if (CurrentCount == 0)
            Destroy(gameObject); 
    }

    // InventoryItemComponent.cs 新增属性
    public Vector2Int CurrentDisplaySize
    {
        get
        {
            return _itemData.IsRotated ?
                new Vector2Int(_originalSize.y, _originalSize.x) :
                _originalSize;
        }
    }

    [SerializeField] public InventoryItem _itemData;
    public Vector2Int _originalSize;
    public Vector2Int OriginalSize => _originalSize; // 添加这行
    // 新增尺寸更新方法
    // 新增容器类型属性
    [System.NonSerialized]
    public string CurrentContainerType;

    // 新增ID生成标记
    public bool IsMainInstance = true;


    // 修改InventoryItemComponent.cs中的RotateItem方法
    // 改进的RotateItem方法
    public void RotateItem()
    {
        if (!ItemData.CanRotate) return;

        // 交换原始尺寸记录
        _originalSize = new Vector2Int(_originalSize.y, _originalSize.x);

        // 更新显示尺寸
        InventoryGrid grid = GetComponentInParent<InventoryGrid>();
        if (grid != null)
        {
            UpdateVisualSize(grid);
        }

        // 更新旋转状态****
        isRotate = !isRotate;
    }

    // 优化后的UpdateVisualSize方法
    public void UpdateVisualSize(InventoryGrid grid)
    {
        if (grid == null) return;

        RectTransform rt = GetComponent<RectTransform>();
        Vector2Int displaySize = _itemData.IsRotated ?
            new Vector2Int(_originalSize.y, _originalSize.x) :
            _originalSize;

        rt.sizeDelta = new Vector2(
            displaySize.x * grid.CellSize,
            displaySize.y * grid.CellSize
        );
    }

    // 保持原始尺寸的访问属性
    // 添加属性验证
    public InventoryItem ItemData
    {
        get
        {
            if (_itemData == null)
            {
                Debug.LogWarning("物品数据未初始化", this);
                return new InventoryItem();
            }
            return _itemData;
        }
    }

    // 优化后的Initialize方法
    public void Initialize(InventoryItem data, InventoryGrid grid)
    {
        if (grid == null)
        {
            Debug.LogError("必须提供有效的 InventoryGrid 引用", this);
            return;
        }

        _itemData = data;
        _originalSize = data.Size;
        UpdateVisualSize(grid);
    }

    private void Update()
    {

    }
}