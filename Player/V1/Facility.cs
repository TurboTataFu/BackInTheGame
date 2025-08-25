using UnityEngine;

public class Facility : MonoBehaviour, IInteractable
{
    [Header("设施设置")]
    [SerializeField] private string facilityName = "设施";
    [SerializeField] private FacilityType facilityType = FacilityType.Generic;

    [Header("交互设置")]
    [SerializeField] private string interactionPrompt = "按E交互";
    [SerializeField] private float promptDisplayTime = 2f;

    private float promptTimer = 0f;
    private bool promptVisible = false;

    public void Interact(GameObject interactor)
    {
        switch (facilityType)
        {
            case FacilityType.Crafting:
                OpenCraftingUI(interactor);
                break;
            case FacilityType.Storage:
                OpenStorageUI(interactor);
                break;
            case FacilityType.Generic:
            default:
                Debug.Log($"与 {facilityName} 交互");
                break;
        }
    }

    private void OpenCraftingUI(GameObject interactor)
    {
        Debug.Log($"打开 {facilityName} 的制作界面");
        // 实际应实现打开制作界面的逻辑
    }

    private void OpenStorageUI(GameObject interactor)
    {
        Debug.Log($"打开 {facilityName} 的存储界面");
        if (InventoryManager.Instance != null)
        {
            // 这里可以实现显示背包内容的逻辑
            foreach (var item in InventoryManager.Instance.items)
            {
                Debug.Log($"背包中有物品: {item.itemID}, 数量: {item.CurrentStack}");
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

public enum FacilityType
{
    Generic,
    Crafting,
    Storage,
    Vendor
}
