using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CraftManager : MonoBehaviour
{
    public GameObject debug;
    public GameObject backpackObj;
    public Image icon;
    public Text itemName;
    private Dictionary<string, int> idCountDict = new Dictionary<string, int>();//��������
    private InventoryItem CraftGiveItem;
    public ItemPlacer Placer;//������

    // ���԰�ť���
    public void onDebugButtonDown()
    {
        Debug.Log("=== ��ʼִ��onDebugButtonDown ===");
        InventoryItemComponent item = debug.GetComponent<InventoryItemComponent>();

        if (item == null)
        {
            Debug.LogError("debug������δ�ҵ�InventoryItemComponent���");
            return;
        }
        if (item.ItemData == null)
        {
            Debug.LogError("debug�����ItemDataΪnull");
            return;
        }

        Debug.Log($"��ȡ��������Ʒ: {item.ItemData.ItemName} (ID: {item.ItemID})");
        UpdateSelechedItem(item.ItemData);
        OnCraftButton();
        Debug.Log("=== ����ִ��onDebugButtonDown ===");
    }

    // ����ѡ����Ʒ��Ϣ
    public void UpdateSelechedItem(InventoryItem SelechedItem)
    {
        Debug.Log($"=== ��ʼ����ѡ����Ʒ: {SelechedItem?.ItemName ?? "null"} ===");
        idCountDict.Clear();

        if (SelechedItem == null)
        {
            Debug.LogError("UpdateSelechedItem: ѡ����ƷΪnull");
            return;
        }

        // ����UI��ʾ
        icon.sprite = SelechedItem.Icon;
        itemName.text = SelechedItem.ItemName;
        Debug.Log($"����UI��ʾ: ��Ʒ����={SelechedItem.ItemName}, ͼ��={(SelechedItem.Icon != null ? "����" : "������")}");

        // ת���䷽�ֵ�
        idCountDict = ConvertToIdCountDict(SelechedItem.craftItem);
        CraftGiveItem = SelechedItem;

        // ��ӡ�䷽����
        Debug.Log($"�����䷽���� (��{idCountDict.Count}�ֲ���):");
        foreach (var pair in idCountDict)
        {
            Debug.Log($"  - ����ID: {pair.Key}, ��������: {pair.Value}");
        }
        Debug.Log($"=== ��ɸ���ѡ����Ʒ ===");
    }

    // ������ť���
    public void OnCraftButton()
    {
        Debug.Log("=== ��ʼִ��OnCraftButton ===");

        // �����������Ƿ���Ч
        if (idCountDict == null || idCountDict.Count == 0)
        {
            Debug.LogWarning("OnCraftButton: �䷽�ֵ�Ϊ��");
            return;
        }
        if (CraftGiveItem == null)
        {
            Debug.LogWarning("OnCraftButton: Ŀ��������ƷΪnull");
            return;
        }
        if (backpackObj == null)
        {
            Debug.LogError("OnCraftButton: backpackObjδ��ֵ");
            return;
        }

        // ��ȡ��Ʒ������
        ItemFoundManager itemFoundManager = GetComponent<ItemFoundManager>();
        if (itemFoundManager == null)
        {
            Debug.LogError("OnCraftButton: δ�ҵ�ItemFoundManager���");
            return;
        }

        // ���ұ�����Ʒ
        itemFoundManager.FoundForItem();
        Dictionary<string, int> backpackItemHeld = itemFoundManager.backpackDataDict;

        if (backpackItemHeld == null)
        {
            Debug.LogError("OnCraftButton: ���������ֵ�Ϊnull");
            return;
        }

        // ��ӡ��ǰ����������Ʒ
        Debug.Log($"��ǰ������Ʒ (��{backpackItemHeld.Count}��):");
        foreach (var pair in backpackItemHeld)
        {
            Debug.Log($"  - ��ƷID: {pair.Key}, ��������: {pair.Value}");
        }

        // ����Ƿ�������������
        bool canOrNotCraft = CheckBackPack2Craft(backpackItemHeld, idCountDict);
        Debug.Log($"�������������: {(canOrNotCraft ? "����" : "������")}");
        if (!canOrNotCraft)
        {
            Debug.Log("OnCraftButton: ������������������ֹ����");
            return;
        }

        // ɸѡƥ�����Ʒ����
        List<GameObject> matchingObjects = new List<GameObject>();
        InventoryItemComponent[] allItem = backpackObj.GetComponentsInChildren<InventoryItemComponent>(true); // �������ö���
        Debug.Log($"�ӱ������ҵ�{allItem.Length}����Ʒ���");

        foreach (InventoryItemComponent component in allItem)
        {
            // �ȼ�������ItemData�Ƿ���Ч���ؼ��������жϣ�
            if (component == null)
            {
                Debug.LogWarning("���ֿյ�InventoryItemComponent���������");
                continue;
            }
            if (component.ItemData == null)
            {
                Debug.LogWarning($"��Ʒ����{component.gameObject.name}��ItemDataΪnull������");
                continue;
            }

            // �����޸ģ�ʹ��ScriptableObject�ϵĹ̶�itemID����ƥ��
            string fixedItemId = component.ItemData.itemID;
            if (backpackItemHeld.ContainsKey(fixedItemId))
            {
                matchingObjects.Add(component.gameObject);
                Debug.Log($"ƥ�䵽��Ʒ: �̶�ID={fixedItemId}, ��������={component.gameObject.name}");
            }
        }
        Debug.Log($"��ƥ�䵽{matchingObjects.Count}��������������Ʒ����");

        // ������Ʒ����
        ProcessItems(idCountDict, matchingObjects);
        Debug.Log("=== ����ִ��OnCraftButton ===");
    }

    // ������Ʒ�����߼�
    private void ProcessItems(Dictionary<string, int> demandDict, List<GameObject> itemObjects)
    {
        Debug.Log("=== ��ʼ������Ʒ���� ===");
        Dictionary<string, int> tempDemand = new Dictionary<string, int>(demandDict);

        // ��ӡ��ʼ����
        Debug.Log("��ʼ��������:");
        foreach (var pair in tempDemand)
        {
            Debug.Log($"  - {pair.Key}: ��Ҫ{pair.Value}��");
        }

        foreach (var key in new List<string>(tempDemand.Keys))
        {
            int currentDemand = tempDemand[key];
            if (currentDemand <= 0)
            {
                Debug.Log($"����{key}���������㣬��������");
                continue;
            }

            // ɸѡƥ�����Ʒ
            List<GameObject> matchedObjects = itemObjects.FindAll(obj =>
            {
                var holder = obj.GetComponent<InventoryItemComponent>();
                return holder != null && holder.ItemData.itemID == key;
            });
            Debug.Log($"Ϊ����{key}�ҵ�{matchedObjects.Count}��ƥ����Ʒ");

            // ����ÿ��ƥ����Ʒ
            foreach (var obj in matchedObjects)
            {
                if (currentDemand <= 0) break;

                InventoryItemComponent holder = obj.GetComponent<InventoryItemComponent>();
                if (holder == null)
                {
                    Debug.LogWarning($"��Ʒ����{obj.name}��δ�ҵ�InventoryItemComponent");
                    continue;
                }

                int hold = holder.CurrentCount;
                Debug.Log($"������Ʒ: ID={holder.ItemData.itemID}, ��������={hold}, ��ǰ����={currentDemand}");

                if (hold < currentDemand)
                {
                    currentDemand -= hold;
                    Destroy(obj);
                    Debug.Log($"  ������Ʒ{obj.name}��ʣ������: {currentDemand}");
                }
                else if (hold > currentDemand)
                {
                    holder.CurrentCount = hold - currentDemand;
                    currentDemand = 0;
                    Debug.Log($"  ������Ʒ������{holder.CurrentCount}������������");
                }
                else
                {
                    Destroy(obj);
                    currentDemand = 0;
                    Debug.Log($"  ������Ʒ{obj.name}������������");
                }
            }

            tempDemand[key] = currentDemand;
            Debug.Log($"����{key}������ϣ�����ʣ������: {currentDemand}");
        }

        // ����Ƿ�������������
        bool allMet = tempDemand.Values.All(d => d <= 0);
        Debug.Log($"���в��ϴ�����ϣ��Ƿ�ȫ����������: {allMet}");

        if (allMet)
        {
            Debug.Log("��ʼ����������ɵ���Ʒ");
            if (Placer == null)
            {
                Debug.LogError("Placerδ��ֵ���޷�������Ʒ");
                return;
            }
            Placer.AddItemToBackpack();
        }
        Debug.Log("=== ����������Ʒ���� ===");
    }

    // ��鱳���Ƿ�������������
    public static bool CheckBackPack2Craft(
        Dictionary<string, int> aDict,//����Dict
        Dictionary<string, int> bDict)//�����䷽Dict
    {
        Debug.Log("=== ��ʼ����������� ===");

        if (bDict == null || bDict.Count == 0)
        {
            Debug.Log("�䷽Ϊ�գ�������ϼ�������");
            return true;
        }

        if (aDict == null || aDict.Count == 0)
        {
            Debug.LogError("����Ϊ�գ����䷽��Ҫ���ϣ��޷�����");
            return false;
        }

        foreach (var pair in bDict)
        {
            string key = pair.Key;
            int bValue = pair.Value;

            if (!aDict.TryGetValue(key, out int aValue))
            {
                Debug.LogError($"������ȱ�ٱ�Ҫ����: {key}");
                return false;
            }

            if (aValue < bValue) // ע�⣺ԭ������aValue <= bValue����������Ϊ<�������㹻���ɣ�
            {
                Debug.LogError($"����{key}��������: ����{aValue}����Ҫ{bValue}");
                return false;
            }

            Debug.Log($"����{key}��������: ����{aValue} >= ��Ҫ{bValue}");
        }

        Debug.Log("=== ���в��Ͼ������������� ===");
        return true;
    }

    // ת��ID�б�Ϊ�����ֵ�
    private static Dictionary<string, int> ConvertToIdCountDict(List<string> idList)
    {
        Debug.Log("=== ��ʼת��ID�б�Ϊ�����ֵ� ===");
        Dictionary<string, int> idCountDict = new Dictionary<string, int>();

        if (idList == null || idList.Count == 0)
        {
            Debug.Log("�����ID�б�Ϊ��");
            return idCountDict;
        }

        Debug.Log($"�����ID�б�{idList.Count}��Ԫ��: {string.Join(",", idList)}");
        foreach (string id in idList)
        {
            if (idCountDict.ContainsKey(id))
            {
                idCountDict[id]++;
            }
            else
            {
                idCountDict.Add(id, 1);
            }
        }

        // ��ӡת�����
        Debug.Log("ת����ļ����ֵ�:");
        foreach (var pair in idCountDict)
        {
            Debug.Log($"  - {pair.Key}: {pair.Value}��");
        }
        Debug.Log("=== ���ת��ID�б� ===");
        return idCountDict;
    }
}
