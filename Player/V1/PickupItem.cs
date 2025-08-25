using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("物品信息")]
    [SerializeField] private string itemName = "物品";
    [SerializeField] private int itemID = 0;
    [SerializeField] private int itemCount = 1;

    [Header("交互设置")]
    [SerializeField] private string interactionPrompt = "按E拾取";
    [SerializeField] private float promptDisplayTime = 2f;

    private float promptTimer = 0f;
    private bool promptVisible = false;

    public void Interact(GameObject interactor)
    {
        if (InventoryManager.Instance != null)
        {
            bool success = InventoryManager.Instance.AddItem(itemID, itemCount);
            if (success)
            {
                Debug.Log($"拾取了 {itemCount} 个 {itemName}");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("无法拾取物品，背包已满");
            }
        }
        else
        {
            Debug.Log("未找到背包管理器");
        }
    }

    public void OnStartHover()
    {
        promptVisible = true;
        promptTimer = promptDisplayTime;
        Debug.Log($"显示提示: {interactionPrompt}");
    }

    public void OnEndHover()
    {
        promptVisible = false;
        Debug.Log("隐藏提示");
    }

    private void Update()
    {
        if (promptVisible)
        {
            promptTimer -= Time.deltaTime;
            if (promptTimer <= 0)
            {
                promptVisible = false;
            }
        }
    }
}