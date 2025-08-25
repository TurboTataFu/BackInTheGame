using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<InventoryItem> items = new List<InventoryItem>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool AddItem(int itemID, int itemCount)
    {
        // ����򵥼�����ӳɹ���ʵ������Ҫ������Ʒ�ѵ����߼�
        InventoryItem newItem = new InventoryItem();
        newItem.itemID = itemID.ToString();
        newItem.CurrentStack = itemCount;
        items.Add(newItem);
        return true;
    }
}