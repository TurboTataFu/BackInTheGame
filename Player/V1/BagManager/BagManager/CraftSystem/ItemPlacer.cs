using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;

public class ItemPlacer : MonoBehaviour
{
    private Dictionary<Vector2Int,bool> gridDict = new Dictionary<Vector2Int,bool>();
    public Dictionary<Vector2Int, InventoryCell> posToCellDict = new Dictionary<Vector2Int, InventoryCell>();

    private InventoryGrid backpackGrid;

    public GameObject inventoryGridObject; // ���� InventoryGrid �ű��Ķ��󣨸��������߼���
    public GameObject itemsParent;        // ������Ʒ�ĸ������󣨸���㼶����

    public GameObject placerItem;
    private Vector2 placerItemSize;
    private void Start()
    {
        if (itemsParent == null)
        {
            Debug.LogError("δ���ñ�������");
            return;
        }
        if (inventoryGridObject == null)
        {
            Debug.LogError("δ�������������");
            return;
        }
        if (placerItem == null)
        {
            Debug.LogError("��֪��Ҫ����ʲô")
                ; return;
        }

        // ��ȡ������InventoryGrid���
        backpackGrid = inventoryGridObject.GetComponent<InventoryGrid>();
        if (backpackGrid == null)
        {
            Debug.LogError("������û�й���InventoryGrid�����");
            return;
        }
    }

    // ����ť���õĹ���������ÿ�ε���ִ��һ�η����߼�
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
            Debug.Log("û�п��õĸ���");
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
            Debug.LogError("minKey��Ч��gridΪnull");
            return new List<Vector2Int>();
        }

        InventoryItemComponent itemComponent = placerItem.GetComponent<InventoryItemComponent>();
        if (itemComponent == null)
        {
            Debug.LogError("placerItem��û��InventoryItemComponent");
            return new List<Vector2Int> { new Vector2Int(-1, -1) };
        }
        Vector2Int originalSize = itemComponent.OriginalSize;

        List<Vector2Int> newKeys = new List<Vector2Int>();

        // 1. ���ɻ�������λ�ã����ӿ����Լ��
        // ����
        int leftX = minKey.x - originalSize.x;
        Vector2Int leftPos = new Vector2Int(leftX, minKey.y);
        if (leftPos.x >= 0 && grid.CheckSpaceAvailability(leftPos, originalSize)) // ���������Լ��
        {
            newKeys.Add(leftPos);
        }

        // �ҷ���
        Vector2Int rightPos = new Vector2Int(minKey.x + 1, minKey.y);
        if (rightPos.x < grid._gridSize.x && grid.CheckSpaceAvailability(rightPos, originalSize)) // ����
        {
            newKeys.Add(rightPos);
        }

        // �Ϸ���
        Vector2Int upPos = new Vector2Int(minKey.x, minKey.y + 1);
        if (upPos.y < grid._gridSize.y && grid.CheckSpaceAvailability(upPos, originalSize)) // ����
        {
            newKeys.Add(upPos);
        }

        // �·���
        int downY = minKey.y - originalSize.y;
        Vector2Int downPos = new Vector2Int(minKey.x, downY);
        if (downPos.y >= 0 && grid.CheckSpaceAvailability(downPos, originalSize)) // ����
        {
            newKeys.Add(downPos);
        }

        // 2. ��ߴ���Ʒ��б��λ�ã�1x1��Ʒ����ִ�����
        if (originalSize.x > 2 && originalSize.y > 2)
        {
            List<Vector2Int> diagonalPositions = CheckDiagonalAreas(minKey, originalSize, grid);
            newKeys.AddRange(diagonalPositions);
        }

        // 3. ȥ�غ��ٴι�����Чλ�ã�˫�ر��գ�
        newKeys = new List<Vector2Int>(new HashSet<Vector2Int>(newKeys));
        newKeys.RemoveAll(pos => pos.x < 0 || pos.y < 0 || pos.x >= grid._gridSize.x || pos.y >= grid._gridSize.y);

        // 4. ������˺��ѡλ��Ϊ�գ�ֱ��ʹ��minKey�������ǿ��ø��ӣ�
        if (newKeys.Count == 0)
        {
            newKeys.Add(minKey);
            Debug.LogWarning("���з���λ�ö���Ч��ʹ��minKey��Ϊ����λ��");
        }

        return newKeys;
    }

    /// <summary>
    /// ����ߴ���Ʒ��x>2, y>2����б����������Ƿ����
    /// </summary>
    /// <param name="minKey">��׼����</param>
    /// <param name="itemSize">��Ʒ�ߴ�</param>
    /// <param name="grid">Ŀ������</param>
    /// <returns>���п��õ�б����ʼ����</returns>
    private List<Vector2Int> CheckDiagonalAreas(Vector2Int minKey, Vector2Int itemSize, InventoryGrid grid)
    {
        List<Vector2Int> validDiagonalPositions = new List<Vector2Int>();
        // �������ߴ���Ʒ��x>2��y>2��
        if (itemSize.x <= 2 || itemSize.y <= 2)
        {
            Debug.Log("��Ʒ�ߴ�С�ڵ���2x2��������б������");
            return validDiagonalPositions;
        }
        // ��ȫ��飺����Ϊ�ջ��׼������Ч
        if (grid == null || minKey.x == -1)
        {
            return validDiagonalPositions;
        }
        // ����б����չ��Χ��������Ʒ�ߴ綯̬������ȷ�������㹻����
        int expandRange = Mathf.Max(itemSize.x, itemSize.y) / 2;

        // 1. ���Ϸ���x������y������
        for (int xOffset = 1; xOffset <= expandRange; xOffset++)
        {
            for (int yOffset = 1; yOffset <= expandRange; yOffset++)
            {
                Vector2Int startPos = new Vector2Int(
                    minKey.x + xOffset,  // x����ƫ��
                    minKey.y + yOffset   // y����ƫ��
                );
                // ����λ���Ƿ���������Ʒ
                if (grid.CheckSpaceAvailability(startPos, itemSize))
                {
                    validDiagonalPositions.Add(startPos);
                }
            }
        }

        // 2. ���·���x������y�ݼ���
        for (int xOffset = 1; xOffset <= expandRange; xOffset++)
        {
            for (int yOffset = 1; yOffset <= expandRange; yOffset++)
            {
                Vector2Int startPos = new Vector2Int(
                    minKey.x + xOffset,    // x����ƫ��
                    minKey.y - yOffset - (itemSize.y - 1)  // y����ƫ�ƣ�Ԥ����Ʒ�߶ȿռ䣩
                );
                if (grid.CheckSpaceAvailability(startPos, itemSize))
                {
                    validDiagonalPositions.Add(startPos);
                }
            }
        }

        // 3. ���Ϸ���x�ݼ���y������
        for (int xOffset = 1; xOffset <= expandRange; xOffset++)
        {
            for (int yOffset = 1; yOffset <= expandRange; yOffset++)
            {
                Vector2Int startPos = new Vector2Int(
                    minKey.x - xOffset - (itemSize.x - 1),  // x����ƫ�ƣ�Ԥ����Ʒ��ȿռ䣩
                    minKey.y + yOffset                     // y����ƫ��
                );
                if (grid.CheckSpaceAvailability(startPos, itemSize))
                {
                    validDiagonalPositions.Add(startPos);
                }
            }
        }

        // 4. ���·���x�ݼ���y�ݼ���
        for (int xOffset = 1; xOffset <= expandRange; xOffset++)
        {
            for (int yOffset = 1; yOffset <= expandRange; yOffset++)
            {
                Vector2Int startPos = new Vector2Int(
                    minKey.x - xOffset - (itemSize.x - 1),  // x����ƫ��
                    minKey.y - yOffset - (itemSize.y - 1)   // y����ƫ��
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
        InventoryCell[] allGrid = inventoryGridObject.GetComponentsInChildren<InventoryCell>(includeInactive: true);//InventoryGridObject��player����ϵͳ�����г��е�Item�ĸ���
        gridDict.Clear(); // ��վ�����
        posToCellDict.Clear(); // ��վ�ӳ��

        Debug.Log($"��ȡ���ĵ�Ԫ��������{allGrid.Length}");

        foreach (InventoryCell cell in allGrid)
        {
            // ������������������Ʒ������ԭ���߼�������
            if (cell.gameObject == inventoryGridObject)
            {
                Debug.Log($"���������������{cell.name}");
                continue;
            }
            if (cell.gameObject == itemsParent)
                continue;

            Vector2Int cellPos = cell.GridPosition;


            bool isAvailable = (cell.grid2ItemObj == null);

            // ��ӡ��Ԫ��״̬��������־���ڵ��ԣ�
            Debug.Log($"��Ԫ�� {cellPos} ״̬��grid2ItemObj�Ƿ�Ϊnull={cell.grid2ItemObj == null} �� ����={isAvailable}");

            // �����ֵ䣨ԭ���߼�������
            gridDict[cellPos] = isAvailable;
            posToCellDict[cellPos] = cell;
        }
        int availableCount = gridDict.Values.Count(v => v);
        Debug.Log($"gridDict�п��ø���������{availableCount}");
        PlaceItemToBackpack(); // ��ʼ����ɺ�ִ����Ʒ�����߼�
    }

    private void PlaceItemToBackpack()
{
        InventoryItemComponent itemComponent = placerItem.GetComponent<InventoryItemComponent>();
        Vector2Int originalSize = itemComponent.OriginalSize;
        InventoryGrid backpackGrid = inventoryGridObject.GetComponent<InventoryGrid>();


        // 1. ��ȡ���п��ø��ӣ�gridDict��valueΪtrue�ļ���
        List<Vector2Int> activeKeys = GetActiveKeys();
    if (activeKeys.Count == 0)
    {
        Debug.LogError("�����������޷�������Ʒ��");
        return;
    }
    // 2. �ҵ����ø�����xy��С�����꣨�������Ͻǣ�

    Vector2Int minKey = FindMinXyKeys(activeKeys); // ע�⣺ԭ��������ֵӦΪVector2Int����������������

        Debug.Log($"�ҵ���minKey���꣺{minKey}���Ƿ���Ч��x != -1����{minKey.x != -1}"); // ������־
        if (minKey.x == -1)
        {
            Debug.LogError("δ�ҵ����ʵ���ʼ���ӣ�");
            return;
        }

        Debug.LogWarning($"placerItem �Ƿ�Ϊnull��{placerItem == null}");
        Debug.LogWarning($"minKey �Ƿ���Ч��{minKey.x != -1}"); // minKey�ǽṹ�壬����Ϊnull������ȷ���Ƿ���Ч
        Debug.LogWarning($"backpackGrid �Ƿ�Ϊnull��{backpackGrid == null}");
        // 3. �����λ�ø����ĺ�ѡ����λ��
        List<Vector2Int> candidatePositions = CalculateNewKeys(placerItem, minKey, backpackGrid);
    if (candidatePositions.Count == 0 || candidatePositions[0].x == -1)
    {
        Debug.LogError("û���ҵ��ɷ��õ�λ�ã�");
        return;
    }

        // 4. ������ѡλ�ã�Ѱ�ҵ�һ������λ��
        Vector2Int targetPos = Vector2Int.one * -1; // ��ʼ��Ϊ��Чֵ
        foreach (var pos in candidatePositions)
        {
            if (backpackGrid.CheckSpaceAvailability(pos, originalSize)) // ����У��
            {
                targetPos = pos;
                break; // �ҵ���һ������λ�þ��˳�
            }
        }

        // ���û���ҵ�����λ�ã���������
        if (targetPos.x == -1)
        {
            Debug.LogError("���к�ѡλ�þ���ռ�ã��޷�������Ʒ��");
            return;
        }

        // ������ͨ��������Ҷ�Ӧ�ĵ�Ԫ��
        if (!posToCellDict.TryGetValue(targetPos, out InventoryCell targetCell))
        {
            Debug.LogError($"�Ҳ�������Ϊ {targetPos} �ĵ�Ԫ��");
            return;
        }

        // 6. ʵ������Ʒ�����ø���
        GameObject placedItem = Instantiate(placerItem, itemsParent.transform);

        // 7. ������ƷUIλ�úͳߴ磨���ԭ�д��룩
        RectTransform itemRect = placedItem.GetComponent<RectTransform>();
        RectTransform cellRect = targetCell.GetComponent<RectTransform>();
        itemRect.anchoredPosition = cellRect.anchoredPosition;
        itemRect.sizeDelta = new Vector2(
            cellRect.sizeDelta.x * originalSize.x,
            cellRect.sizeDelta.y * originalSize.y
        );

        // 8. ��ȡʵ������Ʒ��Ӧ�� InventoryItem ʵ��

        InventoryItemComponent itemInstance = placerItem.GetComponent<InventoryItemComponent>();// ����Ҫ�󶨵���Ԫ�����Ʒ����

        // 9. �ؼ�������Ŀ�굥Ԫ��� SetState ����������Ʒ�뵥Ԫ���
        // ��ʱ��Ԫ��� _storedItem �ᱻ��ֵΪ itemToBind����ɰ�


        // 10. ����ԭ���߼���������Ʒ������λ�á�֪ͨ������µȣ�
        itemInstance.GridPosition = targetPos;
        backpackGrid.PlaceItem(targetPos, originalSize, itemInstance);


        Debug.Log($"��Ʒ�ѷ��õ���Ԫ�� {targetPos}������Ϊ Backpack");
    }
}
