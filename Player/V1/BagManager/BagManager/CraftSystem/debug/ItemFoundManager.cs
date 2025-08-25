using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem.iOS;
using UnityEngine.Animations;
public class ItemFoundManager : MonoBehaviour
{
    public GameObject backpackObj;
    public Dictionary<string, int> backpackDataDict = new Dictionary<string, int>();//�ֵ䣨��ֵ��Ӧ��
    private void Start()
    {
        if (backpackObj == null)
        {
            Debug.LogError("û��ָ������");
            return;//������ֹ����
        }

        FoundForItem();
        PrintDictForBackpack();
    }
    public void FoundForItem()
    {
        // 1. ����������ݣ������ε����ۻ���������������Ƿ�����
        backpackDataDict.Clear();

        // 2. ��ȡ������������Ʒ�����ֻ��ҪInventoryItemComponent������������Ʒ���ݺ�������
        InventoryItemComponent[] allItemComponents = backpackObj.GetComponentsInChildren<InventoryItemComponent>();

        // 3. �����������Ƿ���Ч
        if (allItemComponents == null || allItemComponents.Length == 0)
        {
            Debug.LogWarning("������û���ҵ���Ʒ���");
            return;
        }

        // 4. ����������Ʒ������ռ�����
        foreach (InventoryItemComponent itemComp in allItemComponents)
        {
            // ������Ч�������δ������Ʒ���ݵ������
            if (itemComp == null || itemComp.ItemData == null)
            {
                Debug.LogWarning("������Ч����Ʒ���");
                continue;
            }

            // ��ȡ��ƷID�͵�ǰ����������������������л�ȡ��
            string itemId = itemComp.ItemData.itemID;
            int itemHeld = itemComp.CurrentCount;

            // �ϲ�ͬ����ۼ�������
            if (backpackDataDict.ContainsKey(itemId))
            {
                backpackDataDict[itemId] += itemHeld;
            }
            else
            {
                backpackDataDict.Add(itemId, itemHeld);
            }
        }

        Debug.Log($"�ռ���ɣ��� {backpackDataDict.Count} ����Ʒ");
        PrintDictForBackpack();
    }

    private void PrintDictForBackpack()//��ӡ����,�������ִ�н��
    {
        Debug.Log("\n===== ��ƷID-�������ձ� =====");
        foreach (var pair in backpackDataDict)
        {
            Debug.Log($"ID: {pair.Key} �� ����: {pair.Value}");
        }
    }
}
