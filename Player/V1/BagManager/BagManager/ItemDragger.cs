using UnityEngine;
using UnityEngine.EventSystems;//拖拽控制器

public class ItemDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform _rt;
    private Vector2 _originalPosition;
    private InventoryGrid _currentGrid;

    private Vector2Int _dragStartGridPos; // 记录拖拽开始时的物品位置
    void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalPosition = _rt.anchoredPosition;
        _currentGrid = GetComponentInParent<InventoryGrid>();

        if (_currentGrid == null)
        {
            Debug.LogError("Parent InventoryGrid not found!", this);
            return;
        }

        // 新增：记录拖拽开始时的物品网格位置
        var itemComponent = GetComponent<InventoryItemComponent>();
        if (itemComponent != null && itemComponent.ItemData != null)
        {
            _dragStartGridPos = itemComponent.ItemData.GridPosition;
        }
    }

    // 正确实现IDragHandler接口
    // 修改后的 OnDrag 方法
    public void OnDrag(PointerEventData eventData)
    {
        if (_currentGrid == null) return;

        // 将鼠标的屏幕坐标转换为 UI 本地坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _currentGrid.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPos
        );

        // 直接设置位置，避免 delta 累加误差
        _rt.anchoredPosition = localPos;
    }

    // ItemDragger.cs 中的 OnEndDrag 方法
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_currentGrid == null) return;

        InventoryItemComponent itemInstance = GetComponent<InventoryItemComponent>();
        if (itemInstance == null)
        {
            Debug.LogError("InventoryItemComponent missing!", this);
            return;
        }


       // Vector2Int originalGridPos = item.GridPosition;保存原始位置用于回退
        // 保存原始位置用于回退（使用拖拽开始时记录的位置）
        Vector2Int originalGridPos = _dragStartGridPos;

        // 关键修复：验证位置属于当前物品后再移除

            _currentGrid.RemoveItem(itemInstance); // 按物品移除而非按位置


        // 计算新网格位置
        Vector2Int gridPos = CalculateGridPosition(itemInstance);

        // 检查新位置是否可用
        if (_currentGrid.CheckSpaceAvailability(gridPos, itemInstance.OriginalSize))
        {
            // 更新网格占用状态
            _currentGrid.PlaceItem(gridPos, itemInstance.OriginalSize, itemInstance);

            // 对齐到网格并更新物品数据
            _currentGrid.SnapToGrid(_rt, gridPos);
            itemInstance.GridPosition = gridPos;










            // 如果物品是旋转状态需要修正坐标
            if (itemInstance.isRotate)
            {
                _rt.anchoredPosition += new Vector2(
                    (itemInstance.OriginalSize.y - itemInstance.OriginalSize.x) * _currentGrid.CellSize / 2,
                    (itemInstance.OriginalSize.x - itemInstance.OriginalSize.y) * _currentGrid.CellSize / 2
                );
            }
        }
        else
        {
            // 检查原位置是否仍可用
            if (_currentGrid.CheckSpaceAvailability(originalGridPos, itemInstance.OriginalSize))
            {
                _currentGrid.PlaceItem(originalGridPos, itemInstance.OriginalSize, itemInstance);
                _rt.anchoredPosition = _originalPosition;
            }
            else
            {
                Debug.LogWarning("原位置已被占用，物品无法放回！");
                // 可选：寻找新位置或处理错误
                // 此处暂时强制放回，可能需要额外处理
                _currentGrid.PlaceItem(originalGridPos, itemInstance.OriginalSize, itemInstance);
                _rt.anchoredPosition = _originalPosition;
            }
        }
    }

    // 优化的CalculateGridPosition方法
    private Vector2Int CalculateGridPosition(InventoryItemComponent item)
    {
        if (_currentGrid == null || item == null) return Vector2Int.zero;

        float cellSize = _currentGrid.CellSize;
        Vector2 localPos = _rt.localPosition;

        // 增加半单元格偏移以实现中心点对齐
        int gridX = Mathf.FloorToInt((localPos.x + cellSize / 2) / cellSize);
        int gridY = Mathf.FloorToInt((-localPos.y + cellSize / 2) / cellSize);

        // 考虑物品尺寸的边界限制
        int maxX = Mathf.Max(_currentGrid.GridSize.x - item.OriginalSize.x, 0);
        int maxY = Mathf.Max(_currentGrid.GridSize.y - item.OriginalSize.y, 0);

        gridX = Mathf.Clamp(gridX, 0, maxX);
        gridY = Mathf.Clamp(gridY, 0, maxY);

        return new Vector2Int(gridX, gridY);
    }

    // 改进的RotateItem方法
    public void RotateItem()
    {
        InventoryItemComponent itemComponent = GetComponent<InventoryItemComponent>();
        if (itemComponent == null) return;

        InventoryItem item = itemComponent.ItemData;
        if (!item.CanRotate) return;

        // 由数据模型处理旋转
        item.IsRotated = !item.IsRotated;

        // 更新显示尺寸
        itemComponent._originalSize = new Vector2Int(item.Size.y, item.Size.x);
        itemComponent.UpdateVisualSize(_currentGrid);

        // 视觉旋转（如果需要）
        _rt.rotation = Quaternion.Euler(0, 0, item.IsRotated ? 90 : 0);
    }
}