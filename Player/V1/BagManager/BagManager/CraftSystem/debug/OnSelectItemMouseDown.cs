using UnityEngine;

public class OnSelectItemMouseDown : MonoBehaviour
{
    public CraftManager Manager;
    public InventoryItem InventoryItem;

    private void OnMouseDown()
    {
        Manager = GetComponent<CraftManager>();

        Manager.UpdateSelechedItem(InventoryItem);

        Debug.Log($" ƒ„—°‘Ò¡À{InventoryItem.ItemName}");
    }
}
