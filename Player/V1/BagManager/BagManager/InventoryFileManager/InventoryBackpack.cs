using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="New Backpack",menuName = "Inventory/New Backpack")]
public class InventoryBackpack : ScriptableObject
{
    public List<InventoryItem> Itmelist = new List<InventoryItem>();
}
