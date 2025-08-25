using UnityEngine;
using UnityEngine.UI;//��Ԫ��
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
    [Header("״̬����")]
    [SerializeField] private Color _emptyColor = Color.white;
    [SerializeField] private Color _occupiedColor = Color.red;
    [SerializeField] private Color _blockedColor = Color.gray;

    [Header("������Ϣ")]
    [SerializeField] private Vector2Int _gridPosition;

    // ������Ʒ�����ֶ�
    private InventoryItem _storedItem;
    private Image _background;
    public CellState _currentState = CellState.Empty;
    public InventoryItemComponent grid2ItemObj;//ÿ����ռ�õĵ�Ԫ���Ӧ��ռ����

    // ��������
    public Vector2Int GridPosition => _gridPosition;
    public bool IsOccupied => _currentState == CellState.Occupied;
    public InventoryItem StoredItem => _storedItem; // ������Ʒ���ʽӿ�


    void Awake()
    {
        _background = GetComponent<Image>();
        UpdateVisual();
    }

    /// <summary>
    /// ��ʼ����Ԫ����InventoryGrid���ã�
    /// </summary>
    public void Initialize(Vector2Int gridPos, CellState initialState = CellState.Empty)
    {
        _gridPosition = gridPos;
        name = $"Cell_{gridPos.x}_{gridPos.y}";
        SetState(initialState, null);
    }
    /// <summary>
    /// ����״̬��������Ʒ
    /// </summary>
    // ��ǿ��SetState����
    // �������������ճ����е���Ʒʵ�����������ֱ�ӽ���ScriptableObject
    public void SetState(CellState newState, InventoryItemComponent itemInstance)
    {
        _currentState = newState;
        grid2ItemObj = (newState == CellState.Occupied) ? itemInstance : null;// ���������е���Ʒʵ�����ؼ��޸���
        _storedItem = (newState == CellState.Occupied && itemInstance != null)// �洢��Ʒ���ݣ�ScriptableObject�������ڷ���������Ϣ
            ? itemInstance.ItemData  // ��ʵ���л�ȡSO����
            : null;
        UpdateVisual();  // ˢ�µ�Ԫ���Ӿ�״̬���������ͼ��ȣ�
    }
    /// <summary>
    /// ǿ����յ�Ԫ�񣨽���ʹ�ã�
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
            _ => Color.magenta // ����״̬��ʾ
        };
    }

    // ��ӻ�ȡ��Ʒ�ķ���
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