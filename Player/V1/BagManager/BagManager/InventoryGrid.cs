using UnityEngine;
using System.Collections.Generic;

public class InventoryGrid : MonoBehaviour
{
    [Header("�������")]
    [SerializeField] public Vector2Int _gridSize = new Vector2Int(10, 8);
    [SerializeField] private float _cellSize = 50f;
    [SerializeField] private GameObject _cellPrefab;

    [Header("���Թ���")]
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
        // �ߴ���Ч�Լ��
        if (size.x <= 0 || size.y <= 0) return false;

        // �߽�Ԥ���
        if (position.x < 0 || position.y < 0) return false;
        if (position.x + size.x > _gridSize.x || position.y + size.y > _gridSize.y) return false;

        // �ϲ���ĵ�Ԫ��ռ�ü��
        for (int y = position.y; y < position.y + size.y; y++)
        {
            for (int x = position.x; x < position.x + size.x; x++)
            {
                var cell = GetCell(x, y);
                if (cell == null)
                {
                    Debug.LogError($"[{gameObject.name}] ���ռ�ʧ�ܣ���Ԫ�� ({x},{y}) ������");
                    return false;
                }
                if (cell.IsOccupied)
                {
                    Debug.LogWarning($"[{gameObject.name}] ���ռ�ʧ�ܣ���Ԫ�� ({x},{y}) �ѱ�ռ�ã�ռ����Ʒ��{cell.StoredItem?.itemID ?? "δ֪��Ʒ"}");
                    return false;
                }
            }
        }
        return true;
    }



    // ����������Ʒʵ���Ƴ�����ռ�õ�Ԫ��ķ���
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
                Debug.Log("FV4001- �����ĵ�����Ϣ");
            }
        }
    }

    // �޸Ĳ��������ճ����е���Ʒʵ�����������ScriptableObject
    public void PlaceItem(Vector2Int position, Vector2Int size, InventoryItemComponent itemInstance)
    {
        // �����Լ�飺ȷ��ʵ����Ч
        if (itemInstance == null)
        {
            Debug.LogError("�޷�������Ʒ����Ʒʵ�����Ϊnull��");
            return;
        }

        // ������Ʒռ�õ����е�Ԫ�񣬰�ʵ������
        for (int y = position.y; y < position.y + size.y; y++)
        {
            for (int x = position.x; x < position.x + size.x; x++)
            {
                // ���õ�Ԫ���SetState������ʵ��������ؼ��޸ģ�
                GetCell(x, y)?.SetState(CellState.Occupied, itemInstance);
            }
        }
    }


    public InventoryCell GetCell(int x, int y)
    {
        if (x < 0 || x >= _gridSize.x || y < 0 || y >= _gridSize.y) return null;
        return _cells[y * _gridSize.x + x];
    }

    /// �޸�����֤��Ʒ�Ƿ�ȷʵռ����ָ��λ��
    public bool VerifyItemOccupiesPosition(InventoryItemComponent item, Vector2Int position, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                int gridX = position.x + x;
                int gridY = position.y + y;
                var cell = GetCell(gridX, gridY); // ʹ��GetCell������ȡ��Ԫ��

                // ��鵥Ԫ���Ƿ�����ұ�����Ʒռ��
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

        // ����ˮƽ������
        for (int y = 0; y <= _gridSize.y; y++)
        {
            Vector3 lineStart = startPos + new Vector3(0, -y * _cellSize, 0);
            Vector3 lineEnd = lineStart + new Vector3(_gridSize.x * _cellSize, 0, 0);
            Gizmos.DrawLine(lineStart, lineEnd);
        }

        // ���ƴ�ֱ������
        for (int x = 0; x <= _gridSize.x; x++)
        {
            Vector3 lineStart = startPos + new Vector3(x * _cellSize, 0, 0);
            Vector3 lineEnd = lineStart + new Vector3(0, -_gridSize.y * _cellSize, 0);
            Gizmos.DrawLine(lineStart, lineEnd);
        }
    }
#endif
}
