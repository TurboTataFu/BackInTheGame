// InventoryItemComponent.cs
using System;
using System.Collections.Generic;
using UnityEngine;//��Ʒʵ��


public class InventoryItemComponent : MonoBehaviour
{


    public string ItemID; // ������̬����
    public int CurrentCount; // ��ǰ��������
    public List<Vector2Int> GridOccupied = new List<Vector2Int>();
    public bool isRotate; 

    // ��� GridPosition ����
    public Vector2Int GridPosition
    {
        get => _itemData.GridPosition;
        set => _itemData.GridPosition = value;
    }

    public bool stackable = false;//�Ƿ�ɶѵ�

    // InventoryItemComponent.cs �޸�
    public ItemType itemType; // �滻ԭ���� public float itemType11

    public string ItemName = "δ����";
    [TextArea]
    public string Title = "����";


    public void UpdateItemCount2Destroy()    //�������ľ�ʱɾ��Item
    {
        if (CurrentCount == 0)
            Destroy(gameObject); 
    }

    // InventoryItemComponent.cs ��������
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
    public Vector2Int OriginalSize => _originalSize; // �������
    // �����ߴ���·���
    // ����������������
    [System.NonSerialized]
    public string CurrentContainerType;

    // ����ID���ɱ��
    public bool IsMainInstance = true;


    // �޸�InventoryItemComponent.cs�е�RotateItem����
    // �Ľ���RotateItem����
    public void RotateItem()
    {
        if (!ItemData.CanRotate) return;

        // ����ԭʼ�ߴ��¼
        _originalSize = new Vector2Int(_originalSize.y, _originalSize.x);

        // ������ʾ�ߴ�
        InventoryGrid grid = GetComponentInParent<InventoryGrid>();
        if (grid != null)
        {
            UpdateVisualSize(grid);
        }

        // ������ת״̬****
        isRotate = !isRotate;
    }

    // �Ż����UpdateVisualSize����
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

    // ����ԭʼ�ߴ�ķ�������
    // ���������֤
    public InventoryItem ItemData
    {
        get
        {
            if (_itemData == null)
            {
                Debug.LogWarning("��Ʒ����δ��ʼ��", this);
                return new InventoryItem();
            }
            return _itemData;
        }
    }

    // �Ż����Initialize����
    public void Initialize(InventoryItem data, InventoryGrid grid)
    {
        if (grid == null)
        {
            Debug.LogError("�����ṩ��Ч�� InventoryGrid ����", this);
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