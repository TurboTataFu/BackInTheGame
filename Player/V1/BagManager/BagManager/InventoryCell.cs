using UnityEngine;
using UnityEngine.UI;//单元格
using System;
public enum CellState
{
    Empty,
    Occupied,
    Blocked
}

[RequireComponent(typeof(Image))]
public class InventoryCell : MonoBehaviour
{
    [Header("状态配置")]
    [SerializeField] private Color _emptyColor = Color.white;
    [SerializeField] private Color _occupiedColor = Color.red;
    [SerializeField] private Color _blockedColor = Color.gray;

    [Header("调试信息")]
    [SerializeField] private Vector2Int _gridPosition;

    // 新增物品关联字段
    private InventoryItem _storedItem;
    private Image _background;
    public CellState _currentState = CellState.Empty;
    public InventoryItemComponent grid2ItemObj;//每个被占用的单元格对应的占有者

    // 公开属性
    public Vector2Int GridPosition => _gridPosition;
    public bool IsOccupied => _currentState == CellState.Occupied;
    public InventoryItem StoredItem => _storedItem; // 新增物品访问接口


    void Awake()
    {
        _background = GetComponent<Image>();
        UpdateVisual();
    }

    /// <summary>
    /// 初始化单元格（由InventoryGrid调用）
    /// </summary>
    public void Initialize(Vector2Int gridPos, CellState initialState = CellState.Empty)
    {
        _gridPosition = gridPos;
        name = $"Cell_{gridPos.x}_{gridPos.y}";
        SetState(initialState, null);
    }
    /// <summary>
    /// 设置状态并关联物品
    /// </summary>
    // 增强的SetState方法
    // 修正参数：接收场景中的物品实例组件，而非直接接收ScriptableObject
    public void SetState(CellState newState, InventoryItemComponent itemInstance)
    {
        _currentState = newState;
        grid2ItemObj = (newState == CellState.Occupied) ? itemInstance : null;// 关联场景中的物品实例（关键修复）
        _storedItem = (newState == CellState.Occupied && itemInstance != null)// 存储物品数据（ScriptableObject），用于访问配置信息
            ? itemInstance.ItemData  // 从实例中获取SO数据
            : null;
        UpdateVisual();  // 刷新单元格视觉状态（如高亮、图标等）
    }
    /// <summary>
    /// 强制清空单元格（谨慎使用）
    /// </summary>
    public void ForceClear()
    {
        _currentState = CellState.Empty;
        _storedItem = null;
        UpdateVisual();
    }
    private void UpdateVisual()
    {
        _background.color = _currentState switch
        {
            CellState.Empty => _emptyColor,
            CellState.Occupied => _occupiedColor,
            CellState.Blocked => _blockedColor,
            _ => Color.magenta // 错误状态提示
        };
    }

    // 添加获取物品的方法
    public InventoryItem TakeItem()
    {
        if (_currentState != CellState.Occupied) return null;

        InventoryItem item = _storedItem;
        _storedItem = null;
        _currentState = CellState.Empty;
        UpdateVisual();
        return item;
    }

    private void Update()
    {
        if(grid2ItemObj == null)
        {
            SetState(CellState.Empty, null);
        }
    }


}