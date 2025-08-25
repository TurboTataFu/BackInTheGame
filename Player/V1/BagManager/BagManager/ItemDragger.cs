using UnityEngine;
using UnityEngine.EventSystems;//��ק������

public class ItemDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform _rt;
    private Vector2 _originalPosition;
    private InventoryGrid _currentGrid;

    private Vector2Int _dragStartGridPos; // ��¼��ק��ʼʱ����Ʒλ��
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

        // ��������¼��ק��ʼʱ����Ʒ����λ��
        var itemComponent = GetComponent<InventoryItemComponent>();
        if (itemComponent != null && itemComponent.ItemData != null)
        {
            _dragStartGridPos = itemComponent.ItemData.GridPosition;
        }
    }

    // ��ȷʵ��IDragHandler�ӿ�
    // �޸ĺ�� OnDrag ����
    public void OnDrag(PointerEventData eventData)
    {
        if (_currentGrid == null) return;

        // ��������Ļ����ת��Ϊ UI ��������
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _currentGrid.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPos
        );

        // ֱ������λ�ã����� delta �ۼ����
        _rt.anchoredPosition = localPos;
    }

    // ItemDragger.cs �е� OnEndDrag ����
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_currentGrid == null) return;

        InventoryItemComponent itemInstance = GetComponent<InventoryItemComponent>();
        if (itemInstance == null)
        {
            Debug.LogError("InventoryItemComponent missing!", this);
            return;
        }


       // Vector2Int originalGridPos = item.GridPosition;����ԭʼλ�����ڻ���
        // ����ԭʼλ�����ڻ��ˣ�ʹ����ק��ʼʱ��¼��λ�ã�
        Vector2Int originalGridPos = _dragStartGridPos;

        // �ؼ��޸�����֤λ�����ڵ�ǰ��Ʒ�����Ƴ�

            _currentGrid.RemoveItem(itemInstance); // ����Ʒ�Ƴ����ǰ�λ��


        // ����������λ��
        Vector2Int gridPos = CalculateGridPosition(itemInstance);

        // �����λ���Ƿ����
        if (_currentGrid.CheckSpaceAvailability(gridPos, itemInstance.OriginalSize))
        {
            // ��������ռ��״̬
            _currentGrid.PlaceItem(gridPos, itemInstance.OriginalSize, itemInstance);

            // ���뵽���񲢸�����Ʒ����
            _currentGrid.SnapToGrid(_rt, gridPos);
            itemInstance.GridPosition = gridPos;










            // �����Ʒ����ת״̬��Ҫ��������
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
            // ���ԭλ���Ƿ��Կ���
            if (_currentGrid.CheckSpaceAvailability(originalGridPos, itemInstance.OriginalSize))
            {
                _currentGrid.PlaceItem(originalGridPos, itemInstance.OriginalSize, itemInstance);
                _rt.anchoredPosition = _originalPosition;
            }
            else
            {
                Debug.LogWarning("ԭλ���ѱ�ռ�ã���Ʒ�޷��Żأ�");
                // ��ѡ��Ѱ����λ�û������
                // �˴���ʱǿ�ƷŻأ�������Ҫ���⴦��
                _currentGrid.PlaceItem(originalGridPos, itemInstance.OriginalSize, itemInstance);
                _rt.anchoredPosition = _originalPosition;
            }
        }
    }

    // �Ż���CalculateGridPosition����
    private Vector2Int CalculateGridPosition(InventoryItemComponent item)
    {
        if (_currentGrid == null || item == null) return Vector2Int.zero;

        float cellSize = _currentGrid.CellSize;
        Vector2 localPos = _rt.localPosition;

        // ���Ӱ뵥Ԫ��ƫ����ʵ�����ĵ����
        int gridX = Mathf.FloorToInt((localPos.x + cellSize / 2) / cellSize);
        int gridY = Mathf.FloorToInt((-localPos.y + cellSize / 2) / cellSize);

        // ������Ʒ�ߴ�ı߽�����
        int maxX = Mathf.Max(_currentGrid.GridSize.x - item.OriginalSize.x, 0);
        int maxY = Mathf.Max(_currentGrid.GridSize.y - item.OriginalSize.y, 0);

        gridX = Mathf.Clamp(gridX, 0, maxX);
        gridY = Mathf.Clamp(gridY, 0, maxY);

        return new Vector2Int(gridX, gridY);
    }

    // �Ľ���RotateItem����
    public void RotateItem()
    {
        InventoryItemComponent itemComponent = GetComponent<InventoryItemComponent>();
        if (itemComponent == null) return;

        InventoryItem item = itemComponent.ItemData;
        if (!item.CanRotate) return;

        // ������ģ�ʹ�����ת
        item.IsRotated = !item.IsRotated;

        // ������ʾ�ߴ�
        itemComponent._originalSize = new Vector2Int(item.Size.y, item.Size.x);
        itemComponent.UpdateVisualSize(_currentGrid);

        // �Ӿ���ת�������Ҫ��
        _rt.rotation = Quaternion.Euler(0, 0, item.IsRotated ? 90 : 0);
    }
}