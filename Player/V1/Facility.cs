using UnityEngine;

public class Facility : MonoBehaviour, IInteractable
{
    [Header("��ʩ����")]
    [SerializeField] private string facilityName = "��ʩ";
    [SerializeField] private FacilityType facilityType = FacilityType.Generic;

    [Header("��������")]
    [SerializeField] private string interactionPrompt = "��E����";
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
                Debug.Log($"�� {facilityName} ����");
                break;
        }
    }

    private void OpenCraftingUI(GameObject interactor)
    {
        Debug.Log($"�� {facilityName} ����������");
        // ʵ��Ӧʵ�ִ�����������߼�
    }

    private void OpenStorageUI(GameObject interactor)
    {
        Debug.Log($"�� {facilityName} �Ĵ洢����");
        if (InventoryManager.Instance != null)
        {
            // �������ʵ����ʾ�������ݵ��߼�
            foreach (var item in InventoryManager.Instance.items)
            {
                Debug.Log($"����������Ʒ: {item.itemID}, ����: {item.CurrentStack}");
            }
        }
        else
        {
            Debug.Log("δ�ҵ�����������");
        }
    }

    public void OnStartHover()
    {
        promptVisible = true;
        promptTimer = promptDisplayTime;
        Debug.Log($"��ʾ��ʾ: {interactionPrompt}");
    }

    public void OnEndHover()
    {
        promptVisible = false;
        Debug.Log("������ʾ");
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
