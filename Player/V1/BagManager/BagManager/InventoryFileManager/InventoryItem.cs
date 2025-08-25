using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName ="New Itme",menuName ="Inventory/New Itme")]
public class InventoryItem:ScriptableObject
{
    public ItemType itemType; // 新增类型字段
    public string itemID; // 使用小写
    public GameObject Prefab; // 添加Prefab字段
    public string ItemName;
    public Vector2Int Size;


    public Sprite Icon;
    public int MaxStack = 1;
    public int CurrentStack = 1;
    public bool CanRotate = true;

    public List<string> craftItem = new List<string>();

    [HideInInspector] public Vector2Int GridPosition;
    [HideInInspector] public bool IsRotated;

}