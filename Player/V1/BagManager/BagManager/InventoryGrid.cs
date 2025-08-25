using UnityEngine;
using System.Collections.Generic;

public class InventoryGrid : MonoBehaviour
{
    [Header("网格参数")]
    [SerializeField] public Vector2Int _gridSize = new Vector2Int(10, 8);
    [SerializeField] private float _cellSize = 50f;
    [SerializeField] private GameObject _cellPrefab;

    [Header("调试工具")]
    [SerializeField] private bool _showGridGizmos = true;

    public float CellSize => _cellSize;
    public Vector2Int GridSize => _gridSize;

    private RectTransform _rectTransform;
    private List<InventoryCell> _cells = new List<InventoryCell>();

    void Awake() => GenerateGrid();

    public Vector2Int BackPackSize(Vector2Int backPackSize)
    {
        return _gridSize;
    }

    private void GenerateGrid()
    {
        _rectTransform = GetComponent<RectTransform>();
        _rectTransform.sizeDelta = new Vector2(_gridSize.x * _cellSize, _gridSize.y * _cellSize);

        for (int y = 0; y < _gridSize.y; y++)
        {
            for (int x = 0; x < _gridSize.x; x++)
            {
                GameObject cell = Instantiate(_cellPrefab, transform);
                cell.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    x * _cellSize + _cellSize / 2,
                    -y * _cellSize - _cellSize / 2
                );

                InventoryCell cellComponent = cell.GetComponent<InventoryCell>();
                cellComponent.Initialize(new Vector2Int(x, y));
                _cells.Add(cellComponent);
            }
        }
    }

    public void SnapToGrid(RectTransform itemTransform, Vector2Int gridPos)
    {
        itemTransform.anchoredPosition = new Vector2(
            gridPos.x * _cellSize + _cellSize / 2,
            -gridPos.y * _cellSize - _cellSize / 2
        );
    }

    public bool CheckSpaceAvailability(Vector2Int position, Vector2Int size)
    {
        // 尺寸有效性检查
        if (size.x <= 0 || size.y <= 0) return false;

        // 边界预检测
        if (position.x < 0 || position.y < 0) return false;
        if (position.x + size.x > _gridSize.x || position.y + size.y > _gridSize.y) return false;

        // 合并后的单元格占用检测
        for (int y = position.y; y < position.y + size.y; y++)
        {
            for (int x = position.x; x < position.x + size.x; x++)
            {
                var cell = GetCell(x, y);
                if (cell == null)
                {
                    Debug.LogError($"[{gameObject.name}] 检查空间失败：单元格 ({x},{y}) 不存在");
                    return false;
                }
                if (cell.IsOccupied)
                {
                    Debug.LogWarning($"[{gameObject.name}] 检查空间失败：单元格 ({x},{y}) 已被占用！占用物品：{cell.StoredItem?.itemID ?? "未知物品"}");
                    return false;
                }
            }
        }
        return true;
    }



    // 新增：按物品实例移除所有占用单元格的方法
    public void RemoveItem(InventoryItemComponent item)
    {
        foreach (var cell in _cells)
        {
            if (cell.grid2ItemObj == item)
            {
                cell.SetState(CellState.Empty, null);
            }
            else
            {
                Debug.Log("FV4001- 正常的调试信息");
            }
        }
    }

    // 修改参数：接收场景中的物品实例组件，而非ScriptableObject
    public void PlaceItem(Vector2Int position, Vector2Int size, InventoryItemComponent itemInstance)
    {
        // 防御性检查：确保实例有效
        if (itemInstance == null)
        {
            Debug.LogError("无法放置物品：物品实例组件为null！");
            return;
        }

        // 遍历物品占用的所有单元格，绑定实例引用
        for (int y = position.y; y < position.y + size.y; y++)
        {
            for (int x = position.x; x < position.x + size.x; x++)
            {
                // 调用单元格的SetState，传入实例组件（关键修改）
                GetCell(x, y)?.SetState(CellState.Occupied, itemInstance);
            }
        }
    }


    public InventoryCell GetCell(int x, int y)
    {
        if (x < 0 || x >= _gridSize.x || y < 0 || y >= _gridSize.y) return null;
        return _cells[y * _gridSize.x + x];
    }

    /// 修复：验证物品是否确实占用了指定位置
    public bool VerifyItemOccupiesPosition(InventoryItemComponent item, Vector2Int position, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                int gridX = position.x + x;
                int gridY = position.y + y;
                var cell = GetCell(gridX, gridY); // 使用GetCell方法获取单元格

                // 检查单元格是否存在且被该物品占用
                if (cell == null || cell.StoredItem != item)
                    return false;
            }
        }
        return true;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!_showGridGizmos || !Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        Vector3 startPos = transform.position;

        // 绘制水平网格线
        for (int y = 0; y <= _gridSize.y; y++)
        {
            Vector3 lineStart = startPos + new Vector3(0, -y * _cellSize, 0);
            Vector3 lineEnd = lineStart + new Vector3(_gridSize.x * _cellSize, 0, 0);
            Gizmos.DrawLine(lineStart, lineEnd);
        }

        // 绘制垂直网格线
        for (int x = 0; x <= _gridSize.x; x++)
        {
            Vector3 lineStart = startPos + new Vector3(x * _cellSize, 0, 0);
            Vector3 lineEnd = lineStart + new Vector3(0, -_gridSize.y * _cellSize, 0);
            Gizmos.DrawLine(lineStart, lineEnd);
        }
    }
#endif
}
